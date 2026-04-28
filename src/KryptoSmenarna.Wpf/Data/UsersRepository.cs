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
        }
    }
}
