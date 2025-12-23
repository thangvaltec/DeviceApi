using System;
using System.Linq;
using DeviceApi.Data;
using DeviceApi.Models;
using DeviceApi.Services; // 追加
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeviceApi.Controllers
{
    public class UpdateDeviceRequest
    {
        public string SerialNo { get; set; } = string.Empty;
        public int AuthMode { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly ContractClientDbContext _masterDb;
        private readonly ITenantResolver _resolver; // 変更

        public DeviceController(
            ContractClientDbContext masterDb,
            ITenantResolver resolver) // 変更
        {
            _masterDb = masterDb;
            _resolver = resolver;
        }

        private const string DEFAULT_TENANT_CD = "MasterDb";

        // 1) BodyCamera から認証モードを取得（未登録なら自動作成）
        [HttpPost("getAuthMode")]
        public IActionResult GetAuthMode([FromBody] SerialRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.SerialNo))
            {
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ContentType = "text/plain; charset=utf-8",
                    Content = "serialNo は必須です。"
                };
            }

            // 1. テナントを解決 (シリアル番号に基づいてルーティングテーブルを確認)
            using var db = _resolver.Resolve(req.SerialNo);
            var currentTenant = _resolver.GetCurrentTenantCode(req.SerialNo);

            var device = db.Devices.FirstOrDefault(x => x.SerialNo == req.SerialNo && !x.DelFlg);

            if (device == null)
            {
                // 解決されたDB (未登録なら MasterDb/9999) に自動登録
                device = new Device
                {
                    SerialNo = req.SerialNo,
                    DeviceName = "Unknown",
                    AuthMode = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                db.Devices.Add(device);
                db.SaveChanges();

                db.DeviceLogs.Add(new DeviceLog
                {
                    SerialNo = req.SerialNo,
                    Action = $"デバイスを自動登録 (テナント: {currentTenant})",
                    CreatedAt = DateTime.UtcNow
                });

                db.SaveChanges();

                // 2. マスタールーティングテーブルにマッピングが存在することを確認
                if (!_masterDb.DeviceRoutings.Any(r => r.SerialNo == req.SerialNo))
                {
                    _masterDb.DeviceRoutings.Add(new DeviceRouting
                    {
                        SerialNo = req.SerialNo,
                        ContractClientCd = currentTenant
                    });
                    _masterDb.SaveChanges();
                }
            }

            if (!device.IsActive)
            {
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    ContentType = "text/plain; charset=utf-8",
                    Content = "このデバイスは無効化されています。"
                };
            }

            return Ok(new
            {
                authMode = device.AuthMode,
                deviceName = device.DeviceName,
                isActive = device.IsActive
            });
        }

        // 2) BodyCamera からの簡易更新（serialNo で上書き）
        [HttpPost("update")]
        public IActionResult UpdateDevice([FromBody] UpdateDeviceRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.SerialNo))
            {
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ContentType = "text/plain; charset=utf-8",
                    Content = "SerialNo は必須です。"
                };
            }

            using var db = _resolver.Resolve(req.SerialNo);

            var device = db.Devices.FirstOrDefault(d => d.SerialNo == req.SerialNo);

            if (device == null)
            {
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    ContentType = "text/plain; charset=utf-8",
                    Content = $"SerialNo '{req.SerialNo}' のデバイスが見つかりません。"
                };
            }

            device.AuthMode = req.AuthMode;
            device.DeviceName = req.DeviceName;
            device.IsActive = req.IsActive;
            device.UpdatedAt = DateTime.UtcNow;
            db.SaveChanges();

            return Ok(new
            {
                serialNo = device.SerialNo,
                authMode = device.AuthMode,
                deviceName = device.DeviceName,
                isActive = device.IsActive
            });
        }

        // 3) デバイス一覧を取得（削除済みを除外）
        [HttpGet]
        public IActionResult GetAllDevices()
        {
            using var db = _resolver.Resolve();
            var list = db.Devices
                .Where(d => !d.DelFlg)
                .OrderByDescending(d => d.Id)
                .ToList();

            return Ok(list);
        }

        // 4) 管理画面からデバイス新規登録 (スマートハンドオーバー対応)
        [HttpPost]
        public IActionResult CreateDevice([FromBody] Device model)
        {
            // 現在ログインしている管理用テナントを解決
            var targetTenant = _resolver.GetCurrentTenantCode();
            using var db = _resolver.Resolve();

            if (string.IsNullOrWhiteSpace(model.SerialNo))
            {
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ContentType = "text/plain; charset=utf-8",
                    Content = "SerialNo は必須です。"
                };
            }

            // A. マスタールーティングテーブルで既存の所有権を確認
            var existingRouting = _masterDb.DeviceRoutings.FirstOrDefault(r => r.SerialNo == model.SerialNo);

            if (existingRouting != null)
            {
                // ケース: MasterDb (ディスカバリープール) に存在する場合 -> 777などへ移行を許可
                // レガシーな '9999' もディスカバリープールとして扱い、移行を許可する
                bool isDiscovery = existingRouting.ContractClientCd == DEFAULT_TENANT_CD || existingRouting.ContractClientCd == "9999";
                if (isDiscovery && targetTenant != DEFAULT_TENANT_CD && targetTenant != "9999")
                {
                    // MasterDb からのサイレントクリーンアップ (必要に応じて)
                    CleanupFromDiscovery(model.SerialNo);
                }
                // ケース: 別の正式なテナントに既に存在する場合 (盗難・重複防止)
                else if (existingRouting.ContractClientCd != targetTenant)
                {
                    return new ContentResult
                    {
                        StatusCode = StatusCodes.Status409Conflict,
                        ContentType = "text/plain; charset=utf-8",
                        Content = "このデバイスは既に別のテナントに登録されています。管理者までご連絡ください。"
                    };
                }
                // ケース: 既にこのテナントに存在する場合
                else if (db.Devices.Any(x => x.SerialNo == model.SerialNo && !x.DelFlg))
                {
                    return new ContentResult
                    {
                        StatusCode = StatusCodes.Status409Conflict,
                        ContentType = "text/plain; charset=utf-8",
                        Content = "デバイスは既に存在します。"
                    };
                }
            }

            // B. 登録実行
            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = DateTime.UtcNow;

            db.Devices.Add(model);
            db.SaveChanges();

            // C. マスタールーティングテーブルを更新
            if (existingRouting == null)
            {
                _masterDb.DeviceRoutings.Add(new DeviceRouting
                {
                    SerialNo = model.SerialNo,
                    ContractClientCd = targetTenant
                });
            }
            else
            {
                existingRouting.ContractClientCd = targetTenant;
                existingRouting.UpdatedAt = DateTime.UtcNow;
            }
            _masterDb.SaveChanges();

            db.DeviceLogs.Add(new DeviceLog
            {
                SerialNo = model.SerialNo,
                Action = "デバイスを新規登録（手動）",
                CreatedAt = DateTime.UtcNow
            });
            db.SaveChanges();

            return Ok(model);
        }

        private void CleanupFromDiscovery(string serialNo)
        {
            try
            {
                // MasterDb (デフォルト) に接続して削除
                using var discoveryDb = _resolver.Resolve("FORCE_FALLBACK_BY_DUMMY_SERIAL"); // Resolver のロジック上、不明なシリアルは MasterDb になる
                var dev = discoveryDb.Devices.FirstOrDefault(x => x.SerialNo == serialNo);
                if (dev != null)
                {
                    discoveryDb.Devices.Remove(dev);
                    discoveryDb.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Cleanup] Warning: Failed to cleanup {serialNo} from discovery: {ex.Message}");
            }
        }

        // 5) 管理画面からデバイス更新（authMode / deviceName / isActive）
        [HttpPut("{serialNo}")]
        public IActionResult UpdateDevice(string serialNo, [FromBody] Device model)
        {
            using var db = _resolver.Resolve();
            var device = db.Devices.FirstOrDefault(x => x.SerialNo == serialNo && !x.DelFlg);
            if (device == null)
            {
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    ContentType = "text/plain; charset=utf-8",
                    Content = "デバイスが見つかりません。"
                };
            }

            device.DeviceName = model.DeviceName;
            device.AuthMode = model.AuthMode;
            device.IsActive = model.IsActive;
            device.UpdatedAt = DateTime.UtcNow;

            db.SaveChanges();

            db.DeviceLogs.Add(new DeviceLog
            {
                SerialNo = device.SerialNo,
                Action = $"デバイスを更新（認証モード={GetAuthModeLabel(model.AuthMode)}）",
                CreatedAt = DateTime.UtcNow
            });
            db.SaveChanges();

            return Ok(device);
        }

        // 6) ソフト削除（DelFlg = true）
        [HttpDelete("{serialNo}")]
        public IActionResult DeleteDevice(string serialNo)
        {
            var currentTenant = _resolver.GetCurrentTenantCode();
            using var db = _resolver.Resolve();
            var device = db.Devices.FirstOrDefault(x => x.SerialNo == serialNo && !x.DelFlg);
            if (device == null)
            {
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    ContentType = "text/plain; charset=utf-8",
                    Content = "デバイスが見つかりません。"
                };
            }

            device.DelFlg = true;
            device.IsActive = false;
            device.UpdatedAt = DateTime.UtcNow;
            db.SaveChanges();

            // この削除が「所有者」テナントからの「公式な」削除である場合のみ、マスタールーティングから削除 (Master Release)
            var routing = _masterDb.DeviceRoutings.FirstOrDefault(r => r.SerialNo == serialNo);
            if (routing != null && routing.ContractClientCd == currentTenant)
            {
                _masterDb.DeviceRoutings.Remove(routing);
                _masterDb.SaveChanges();
            }

            db.DeviceLogs.Add(new DeviceLog
            {
                SerialNo = device.SerialNo,
                Action = "デバイスを削除",
                CreatedAt = DateTime.UtcNow
            });
            db.SaveChanges();

            return new ContentResult
            {
                StatusCode = StatusCodes.Status200OK,
                ContentType = "text/plain; charset=utf-8",
                Content = "デバイスを削除しました。"
            };
        }

        // 7) デバイス変更履歴を取得
        [HttpGet("logs/{serialNo}")]
        public IActionResult GetLogs(string serialNo)
        {
            using var db = _resolver.Resolve(serialNo);
            var logs = db.DeviceLogs
                .Where(x => x.SerialNo == serialNo)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            return Ok(logs);
        }

        private static string GetAuthModeLabel(int authMode)
        {
            return authMode switch
            {
                0 => "顔認証",
                1 => "静脈認証",
                2 => "顔＋静脈認証",
                _ => $"不明 ({authMode})"
            };
        }
    }
}
