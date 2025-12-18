using System;
using System.Linq;
using System.Threading.Tasks;
using DeviceApi.Data;
using DeviceApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeviceApi.Controllers
{
    [Route("api/auth/logs")]
    [ApiController]
    public class AuthLogController : ControllerBase
    {
        private readonly ContractClientDbContext _masterDb;
        private readonly ContractClientDbContextFactory _factory;

        public AuthLogController(
            ContractClientDbContext masterDb,
            ContractClientDbContextFactory factory)
        {
            _masterDb = masterDb;
            _factory = factory;
        }

        private DeviceDbContext GetContractClientDb()
        {
            var contractClientCd = User.FindFirst("contractClientCd")?.Value;
            
            if (string.IsNullOrWhiteSpace(contractClientCd))
            {
                contractClientCd = Request.Headers["X-Contract-Client-Code"].ToString();
            }

            if (string.IsNullOrWhiteSpace(contractClientCd))
            {
                contractClientCd = Request.Query["contractClientCd"].ToString();
            }

            if (string.IsNullOrWhiteSpace(contractClientCd))
            {
                Request.Cookies.TryGetValue("contractClientCd", out contractClientCd);
            }
            
            if (string.IsNullOrWhiteSpace(contractClientCd))
            {
                contractClientCd = "9999"; 
            }

            var contractClient = _masterDb.ContractClient.FirstOrDefault(t => t.ContractClientCd == contractClientCd);
            if (contractClient == null)
                throw new Exception($"MasterDB にテナント情報がありません (contractClientCd: '{contractClientCd}')");

            string connStr = $"Host=localhost;Port=5432;" +
                             $"Database={contractClient.ContractClientCd};" +
                             $"Username=postgres;Password=2234;SslMode=Disable;";

            return _factory.Create(connStr);
        }

        public class CreateAuthLogRequest
        {
            public string SerialNo { get; set; } = string.Empty;
            public string UserId { get; set; } = string.Empty;
            public string? UserName { get; set; } // Optional
            public int AuthMode { get; set; }
            public bool IsSuccess { get; set; }
            public string? ErrorMessage { get; set; }
            public DateTime? Timestamp { get; set; }
        }

        [HttpGet]
        public IActionResult GetAuthLogs([FromQuery] string? date)
        {
            try
            {
                using var db = GetContractClientDb();

                var query = db.AuthLogs.AsQueryable();

                if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out var targetDate))
                {
                    // Parse date as local time (JST), then convert to UTC for PostgreSQL
                    var startOfDayLocal = DateTime.SpecifyKind(targetDate.Date, DateTimeKind.Local);
                    var endOfDayLocal = startOfDayLocal.AddDays(1);
                    
                    var startOfDay = startOfDayLocal.ToUniversalTime();
                    var endOfDay = endOfDayLocal.ToUniversalTime();
                    
                    // Query using UTC timestamps
                    query = query.Where(x => x.CreatedAt >= startOfDay && x.CreatedAt < endOfDay);
                }
                else
                {
                    query = query.OrderByDescending(x => x.CreatedAt).Take(500);
                }

                var logs = query
                    .OrderByDescending(x => x.CreatedAt)
                    .ToList();

                return Ok(logs);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, stack = ex.StackTrace, inner = ex.InnerException?.Message });
            }
        }

        [HttpPost]
        public IActionResult CreateAuthLog([FromBody] CreateAuthLogRequest req)
        {
            using var db = GetContractClientDb();

            if (req == null) return BadRequest("Request body is empty.");
            
            var log = new AuthLog
            {
                SerialNo = req.SerialNo,
                UserId = req.UserId,
                UserName = req.UserName,  // Save the name
                AuthMode = req.AuthMode,
                IsSuccess = req.IsSuccess,
                ErrorMessage = req.ErrorMessage,
                CreatedAt = req.Timestamp ?? DateTime.UtcNow // Auto-timestamp if missing
            };

            db.AuthLogs.Add(log);
            db.SaveChanges();

            return Ok(log);
        }
    }
}
