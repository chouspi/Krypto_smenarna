using Oracle.ManagedDataAccess.Client;
using System.CodeDom;
using System.IO;

namespace KryptoSmenarna.Wpf.Data;

public class OracleConnectionFactory
{
    private string connectionString;

    public OracleConnectionFactory()
    {
        Dictionary<string, string> envValues = LoadEnvFile();
        string userName = envValues["APP_USER"];
        string password = envValues["APP_USER_PASSWORD"];
        connectionString = "User Id=" + userName + ";Password=" + password + ";Data Source=localhost:1521/FREEPDB1;";
    }

    public OracleConnection CreateConnection()
    {
        return new OracleConnection(connectionString);
    }

    // Hledá .env od výstupní složky aplikace směrem ke kořeni projektu.
    private string FindEnvFile()
    {
        DirectoryInfo directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null)
        {
            string path = Path.Combine(directory.FullName, ".env");

            if (File.Exists(path))
                return path;

            directory = directory.Parent;
        }
        throw new FileNotFoundException("Soubor .env nebyl nalezen.");
    }

    // Načte neprázdné řádky .env ve formátu KEY=VALUE do slovníku.
    private Dictionary<string, string> LoadEnvFile()
    {
        Dictionary<string, string> result = new Dictionary<string, string>();

        using StreamReader sr = new StreamReader(FindEnvFile());
        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine().Trim();
            if (String.IsNullOrWhiteSpace(line))
                continue;
            else if (line.StartsWith("#"))
                continue;

            string[] parts = line.Split("=");
            if (parts.Length != 2)
            {
                continue;
            }

            string key = parts[0].Trim();
            string value = parts[1].Trim();

            result[key] = value;
        }
        return result;
    }

}
