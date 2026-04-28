using Oracle.ManagedDataAccess.Client;

namespace KryptoSmenarna.Wpf.Data;

public class OracleConnectionFactory
{
    private readonly string _connectionString;

    public OracleConnectionFactory()
    {
        _connectionString =
            "User Id=APP;Password=TVE_HESLO;Data Source=localhost:1521/FREEPDB1;";
    }

    public OracleConnection CreateConnection()
    {
        return new OracleConnection(_connectionString);
    }
}