using DeviceApi.Data;

namespace DeviceApi.Services
{
    public interface ITenantResolver
    {
        // 現在のテナントコードを取得 (Webセッションまたはデバイスシリアルから)
        string GetCurrentTenantCode(string? serialNo = null);

        // 指定された情報に基づいて DbContext を解決
        DeviceDbContext Resolve(string? serialNo = null);
    }
}
