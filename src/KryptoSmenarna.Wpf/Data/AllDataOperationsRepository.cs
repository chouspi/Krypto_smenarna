using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KryptoSmenarna.Wpf.Data
{
    internal class AllDataOperationsRepository
    {
        public void DeleteAllData()
        {
            // Aplikace běží z bin složky, proto se cesta skládá zpět ke kořeni repozitáře.
            string path = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..", "..",
                "db",
                "test",
                "deleteAllData.sql"
            ));

            string script = File.ReadAllText(path);

            using OracleConnection connection = new OracleConnectionFactory().CreateConnection();
            connection.Open();

            // Testovací skripty se spouští po jednotlivých příkazech oddělených středníkem.
            string[] commands = script.Split(
                ';',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            );

            foreach (string sql in commands)
            {
                if (string.IsNullOrWhiteSpace(sql))
                    continue;

                using OracleCommand command = new OracleCommand(sql, connection);
                command.ExecuteNonQuery();
            }
        }

        public void InsertTestData()
        {
            // Aplikace běží z bin složky, proto se cesta skládá zpět ke kořeni repozitáře.
            string path = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..", "..",
                "db",
                "test",
                "insertRestValues.sql"
            ));

            string script = File.ReadAllText(path);

            using OracleConnection connection = new OracleConnectionFactory().CreateConnection();
            connection.Open();

            // Testovací skripty se spouští po jednotlivých příkazech oddělených středníkem.
            string[] commands = script.Split(
                ';',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            );

            foreach (string sql in commands)
            {
                if (string.IsNullOrWhiteSpace(sql))
                    continue;

                using OracleCommand command = new OracleCommand(sql, connection);
                command.ExecuteNonQuery();
            }
        }
    }
}
