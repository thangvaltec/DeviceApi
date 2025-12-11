using System.Security.Cryptography;
using System.Text;
using DeviceApi.Data;
using DeviceApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeviceApi.Controllers
{
    // 管理者ユーザーの認証（ログイン）を扱うAPIコントローラー
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DeviceDbContext _context;

        // DbContextをDIで受け取るコンストラクター
        public AuthController(DeviceDbContext context)
        {
            _context = context;
        }

        // ログイン処理：ユーザー名とパスワードを検証
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ContentType = "text/plain; charset=utf-8",
                    Content = "ユーザー名とパスワードは必須です。"
                };
            }

            var user = _context.AdminUsers.FirstOrDefault(u => u.Username == request.Username);
            if (user == null)
            {
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    ContentType = "text/plain; charset=utf-8",
                    Content = "ユーザー名またはパスワードが違います。"
                };
            }

            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    ContentType = "text/plain; charset=utf-8",
                    Content = "ユーザー名またはパスワードが違います。"
                };
            }

            return Ok(new
            {
                username = user.Username,
                role = string.IsNullOrWhiteSpace(user.Role) ? "admin" : user.Role
            });
        }

        // 入力パスワードが保存済みハッシュ（またはプレーン文字列）と一致するか確認
        private static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(storedHash))
            {
                return false;
            }

            // SHA256ハッシュ（16進）として保存されている場合
            var sha256 = ComputeSha256(password);
            if (string.Equals(storedHash, sha256, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // レガシーデータ用に平文との比較も許容
            return string.Equals(storedHash, password);
        }

        private static string ComputeSha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
