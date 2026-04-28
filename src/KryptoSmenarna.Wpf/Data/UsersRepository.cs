using KryptoSmenarna.Wpf.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KryptoSmenarna.Wpf.Data
{
    public class UsersRepository
    {
        public List<User> GetAllUsers()
        {
            List<User> users = new List<User>();
            OracleConnection connection = new OracleConnectionFactory().CreateConnection();
            connection.Open();


            string querry = @"
                    SELECT *
                    FROM users
                    ORDER BY full_name";

            using OracleCommand command = new OracleCommand(querry, connection);
            using OracleDataReader reader = command.ExecuteReader();
            

            while(reader.Read())
            {
                int id = reader.GetInt32(reader.GetOrdinal("USER_ID"));
                string useremail = reader.GetString(reader.GetOrdinal("EMAIL"));
                string passwordHash = reader.GetString(reader.GetOrdinal("HASH_OF_PASSWORD"));
                string fullName = reader.GetString(reader.GetOrdinal("FULL_NAME"));
                users.Add(
                    new User()
                    {
                        email = useremail,
                        full_name = fullName,
                        hash_of_password = passwordHash,
                        user_id = id
                    }
                );
            }
            return users;
        }
    }
}
