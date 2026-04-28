using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KryptoSmenarna.Wpf.Models
{
    public class User
    {
        public int user_id { get; set; }
        public string email { get; set; }
        public string hash_of_password { get; set; }
        public string full_name { get; set; }
    }
}
