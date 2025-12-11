using System.Security.Cryptography;
using System.Text;
using DeviceApi.Data;
using DeviceApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeviceApi.Controllers
{
    // 管理者ユーザーの作成・更新・削除を行うAPIコントローラー
    [Route("api/[controller]")]
    [ApiController]
    public class AdminUsersController : ControllerBase
    {
        private readonly DeviceDbContext _context;

        // DbContextをDIで受け取るコンストラクター
        public AdminUsersController(DeviceDbContext context)
        {
            _context = context;
        }

        // 管理者ユーザーの一覧を取得
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _context.AdminUsers
                .OrderBy(u => u.Id)
                .Select(u => new
                {
                    id = u.Id,
                    username = u.Username,
                    role = string.IsNullOrWhiteSpace(u.Role) ? "admin" : u.Role,
                    createdAt = u.CreatedAt
                })
                .ToList();

            return Ok(users);
        }

        // 管理者ユーザーを新規作成
        [HttpPost]
        public IActionResult Create([FromBody] CreateAdminUserRequest request)
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

            if (_context.AdminUsers.Any(u => u.Username == request.Username))
            {
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status409Conflict,
                    ContentType = "text/plain; charset=utf-8",
                    Content = "このユーザー名は既に使用されています。"
                };
            }

            var user = new AdminUser
            {
                Username = request.Username,
                PasswordHash = ComputeSha256(request.Password),
                Role = string.IsNullOrWhiteSpace(request.Role) ? "admin" : request.Role,
                CreatedAt = DateTime.UtcNow
            };

            _context.AdminUsers.Add(user);
            _context.SaveChanges();

            return Ok(new
            {
                id = user.Id,
                username = user.Username,
                role = user.Role,
                createdAt = user.CreatedAt
            });
        }

        // 管理者ユーザーを更新（ロールやパスワード）
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UpdateAdminUserRequest request)
        {
            var user = _context.AdminUsers.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    ContentType = "text/plain; charset=utf-8",
                    Content = "ユーザーが見つかりません。"
                };
            }

            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                user.Role = request.Role;
            }

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                user.PasswordHash = ComputeSha256(request.Password);
            }

            _context.SaveChanges();

            return Ok(new
            {
                id = user.Id,
                username = user.Username,
                role = user.Role,
                createdAt = user.CreatedAt
            });
        }

        // 管理者ユーザーを削除（root admin は除外）
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var user = _context.AdminUsers.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    ContentType = "text/plain; charset=utf-8",
                    Content = "ユーザーが見つかりません。"
                };
            }

            if (user.Username.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ContentType = "text/plain; charset=utf-8",
                    Content = "ルート管理者ユーザーは削除できません。"
                };
            }

            _context.AdminUsers.Remove(user);
            _context.SaveChanges();

            return new ContentResult
            {
                StatusCode = StatusCodes.Status200OK,
                ContentType = "text/plain; charset=utf-8",
                Content = "ユーザーを削除しました。"
            };
        }

        // パスワードをSHA256でハッシュ化
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
