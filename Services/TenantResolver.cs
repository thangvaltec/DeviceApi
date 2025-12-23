using DeviceApi.Data;
using DeviceApi.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace DeviceApi.Services
{
    public class TenantResolver : ITenantResolver
    {
        private readonly ContractClientDbContext _masterDb;
        private readonly ContractClientDbContextFactory _factory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string DEFAULT_TENANT_CD = "MasterDb"; // デフォルトの振り分け先

        public TenantResolver(
            ContractClientDbContext masterDb,
            ContractClientDbContextFactory factory,
            IHttpContextAccessor httpContextAccessor)
        {
            _masterDb = masterDb;
            _factory = factory;
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCurrentTenantCode(string? serialNo = null)
        {
            var context = _httpContextAccessor.HttpContext;
            
            // 1. Webセッション (JWT/ヘッダー/Cookie) からの取得を試みる (管理画面からのアクセス)
            string? tenantCd = null;
            if (context != null)
            {
                tenantCd = context.User.FindFirst("contractClientCd")?.Value;

                if (string.IsNullOrWhiteSpace(tenantCd))
                    tenantCd = context.Request.Headers["X-Contract-Client-Code"].ToString();

                if (string.IsNullOrWhiteSpace(tenantCd))
                    context.Request.Cookies.TryGetValue("contractClientCd", out tenantCd);
                
                if (string.IsNullOrWhiteSpace(tenantCd))
                    tenantCd = context.Request.Query["contractClientCd"].ToString();
            }

            // 2. デバイスからの呼び出しの場合 (シリアル番号が提供されている場合)
            if (string.IsNullOrWhiteSpace(tenantCd) && !string.IsNullOrWhiteSpace(serialNo))
            {
                // マスタールーティングテーブルを確認
                var routing = _masterDb.DeviceRoutings.FirstOrDefault(r => r.SerialNo == serialNo);
                tenantCd = routing?.ContractClientCd;

                // レガシーな '9999' が残っている場合は 'MasterDb' に読み替える
                if (tenantCd == "9999")
                {
                    tenantCd = DEFAULT_TENANT_CD;
                }
            }

            // 3. 全て見つからない場合はデフォルト (MasterDb) へ
            return string.IsNullOrWhiteSpace(tenantCd) ? DEFAULT_TENANT_CD : tenantCd;
        }

        public DeviceDbContext Resolve(string? serialNo = null)
        {
            var tenantCd = GetCurrentTenantCode(serialNo);

            var contractClient = _masterDb.ContractClient.FirstOrDefault(t => t.ContractClientCd == tenantCd);
            if (contractClient == null)
            {
                // マッピングされたテナントがマスターに存在しない場合はデフォルトにフォールバック
                tenantCd = DEFAULT_TENANT_CD;
                contractClient = _masterDb.ContractClient.FirstOrDefault(t => t.ContractClientCd == tenantCd);
                
                if (contractClient == null)
                    throw new System.Exception($"MasterDB にテナント情報がありません (デフォルト: {DEFAULT_TENANT_CD} を含む)");
            }

            string connStr = $"Host=localhost;Port=5432;" +
                             $"Database={contractClient.ContractClientCd};" +
                             $"Username=postgres;Password=2234;SslMode=Disable;";

            return _factory.Create(connStr);
        }
    }
}
