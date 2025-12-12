using Npgsql;

namespace DeviceApi.Data
{
    public static class ConnectionStringHelper
    {
        public static string SetDatabase(string connStr, string databaseName)
        {
            var builder = new NpgsqlConnectionStringBuilder(connStr)
            {
                Database = databaseName
            };
            return builder.ConnectionString;
        }
    }
}
