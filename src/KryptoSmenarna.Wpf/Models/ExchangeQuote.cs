namespace KryptoSmenarna.Wpf.Models;

public class ExchangeQuote
{
    public int RateId { get; set; }
    public decimal ExchangeRate { get; set; }
    public string FromCurrencyCode { get; set; } = "";
    public string ToCurrencyCode { get; set; } = "";
    public decimal FromAmount { get; set; }
    public decimal ToAmount { get; set; }
}
