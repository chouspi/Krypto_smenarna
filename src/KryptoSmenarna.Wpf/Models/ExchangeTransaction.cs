using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KryptoSmenarna.Wpf.Models;

public class ExchangeTransaction
{
    public int TransactionId { get; set; }
    public int UserId { get; set; }
    public int PairId { get; set; }

    public string FromCurrencyCode { get; set; } = "";
    public string ToCurrencyCode { get; set; } = "";

    public decimal FromAmount { get; set; }
    public decimal ExchangeRate { get; set; }
    public decimal ToAmount { get; set; }

    public DateTime TransactionTime { get; set; }
    public string Status { get; set; } = "";
}
