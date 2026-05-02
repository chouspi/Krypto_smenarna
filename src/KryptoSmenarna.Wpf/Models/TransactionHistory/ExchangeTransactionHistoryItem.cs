using System;
using KryptoSmenarna.Wpf.Models;

namespace KryptoSmenarna.Wpf.Models.TransactionHistory;

public class ExchangeTransactionHistoryItem : ITransactionHistoryItem
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
    public string TransactionStatus { get; set; } = "";

    public DateTime EventTime => TransactionTime;
    public string SourceType => "EXCHANGE";
    public string Status => TransactionStatus;
    public string MainText => CurrencyAmountFormatter.Format(FromAmount, FromCurrencyCode) + " " + FromCurrencyCode
        + " -> " + CurrencyAmountFormatter.Format(ToAmount, ToCurrencyCode) + " " + ToCurrencyCode;
    public string DetailText => $"Kurz: {ExchangeRate:N8}";
}
