using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DeviceApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DeviceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ContractClientDbContext _masterContext;
        private readonly ContractClientDbContextFactory _contractClientDbFactory;
        private readonly IConfiguration _configuration;

        // DbContextをDIで受け取るコンストラクター
        public AuthController(
            ContractClientDbContext masterContext,
            ContractClientDbContextFactory contractClientDbFactory,
            IConfiguration configuration)
        {
            _masterContext = masterContext;
            _contractClientDbFactory = contractClientDbFactory;
            _configuration = configuration;
        }

        public class LoginRequest
        {
            public string ContractClientCd { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {

            var contractClient = _masterContext.ContractClient.FirstOrDefault(u => u.ContractClientCd == request.ContractClientCd);
            if (contractClient == null)
            {
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    ContentType = "text/plain; charset=utf-8",
                    Content = "契約コードが存在しません。"
                };
            }
            else
            {
                // DEBUG: 入力値と masterDB から取得した契約コードを出力
                Console.WriteLine($"[DEBUG] AuthController request.ContractClientCd: {request.ContractClientCd}");
                Console.WriteLine($"[DEBUG] AuthController master contractClient.ContractClientCd: {contractClient.ContractClientCd}");

                // ① appsettings.json から接続文字列を取得して Database 部分を置換
                var connStringTemplate = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrWhiteSpace(connStringTemplate))
                    throw new Exception("ConnectionString 'DefaultConnection' が設定されていません");
                string connStr = DeviceApi.Data.ConnectionStringHelper.SetDatabase(connStringTemplate, contractClient.ContractClientCd);

                // DEBUG: 出力して本当に接続している DB 名を確認（運用前に削除してください）
                Console.WriteLine($"[DEBUG] AuthController constructed tenant connStr: {connStr}");

                // ② factory からテナントDB用 DeviceDbContext を生成
                var contractClientDbDb = _contractClientDbFactory.Create(connStr);

                if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest("ユーザー名とパスワードを入力してください。");
                }
                
                var user = await contractClientDbDb.AdminUsers.FirstOrDefaultAsync(u => u.Username == request.Username);
                if (user == null)
                {
                    return Unauthorized("ユーザー名またはパスワードが違います。");
                }
                
                var hash = ComputeSha256Hash(request.Password);
                if (!string.Equals(user.PasswordHash, hash, StringComparison.OrdinalIgnoreCase))
                {
                    return Unauthorized("ユーザー名またはパスワードが違います。");
                }
                
                // contractClientCd を Cookie に保存
                Response.Cookies.Append("contractClientCd", contractClient.ContractClientCd, new Microsoft.AspNetCore.Http.CookieOptions
                {
                    HttpOnly = false,  // JavaScriptからアクセス可能
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                    Secure = false  // 開発環境なので HTTP 対応
                });
                
                return Ok(new
                {
                    contractClientCd = contractClient.ContractClientCd,
                    username = user.Username,
                    role = user.Role
                });
            }
        }

        private static string ComputeSha256Hash(string rawData)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(rawData);
            var hashBytes = sha256.ComputeHash(bytes);
            var builder = new StringBuilder();
            foreach (var b in hashBytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
