using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KryptoSmenarna.Wpf.Models
{
    public class ExchangeRate
    {
        public int RateId { get; set; }
        public int PairId { get; set; }
        public decimal Rate { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool IsValid { get; set; }
    }
}

