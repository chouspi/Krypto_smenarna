using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KryptoSmenarna.Wpf.Models
{
    public class Wallet
    {
        public int wallet_id { get; set; }
        public int user_id { get; set; }
        public string currencyCode {  get; set; }
        public decimal balance { get; set; }
    }
}
