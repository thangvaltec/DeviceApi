using System;
using System.Linq;
using System.Threading.Tasks;
using DeviceApi.Data;
using DeviceApi.Models;
using DeviceApi.Services; // 追加
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeviceApi.Controllers
{
    [Route("api/auth/logs")]
    [ApiController]
    public class AuthLogController : ControllerBase
    {
        private readonly ITenantResolver _resolver; // 変更

        public AuthLogController(ITenantResolver resolver) // 変更
        {
            _resolver = resolver;
        }


        public class CreateAuthLogRequest
        {
            public string SerialNo { get; set; } = string.Empty;
            public string UserId { get; set; } = string.Empty;
            public string? UserName { get; set; } // オプション
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
                using var db = _resolver.Resolve(); // 管理画面アクセス

                var query = db.AuthLogs.AsQueryable();

                if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out var targetDate))
                {
                    var startOfDay = targetDate.Date;
                    var endOfDay = startOfDay.AddDays(1);
                    
                    // DBの保存形式に応じてUTCまたはローカル。通常はUTCを想定。
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
            if (req == null) return BadRequest("リクエストボディが空です。");

            // デバイスの SerialNo に基づいてテナントを解決
            using var db = _resolver.Resolve(req.SerialNo);
            
            var log = new AuthLog
            {
                SerialNo = req.SerialNo,
                UserId = req.UserId,
                UserName = req.UserName,  // ユーザー名を保存
                AuthMode = req.AuthMode,
                IsSuccess = req.IsSuccess,
                ErrorMessage = req.ErrorMessage,
                CreatedAt = req.Timestamp ?? DateTime.UtcNow // 指定がない場合は現在のUTC日時
            };

            db.AuthLogs.Add(log);
            db.SaveChanges();

            return Ok(log);
        }
    }
}
