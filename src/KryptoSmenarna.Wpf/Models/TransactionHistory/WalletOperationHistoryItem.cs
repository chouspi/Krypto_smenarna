using System;
using KryptoSmenarna.Wpf.Models;

namespace KryptoSmenarna.Wpf.Models.TransactionHistory;

public class WalletOperationHistoryItem : ITransactionHistoryItem
{
    public int OperationId { get; set; }
    public int WalletId { get; set; }
    public int? TransactionId { get; set; }
    public string OperationType { get; set; } = "";
    public string CurrencyCode { get; set; } = "";
    public decimal Amount { get; set; }
    public DateTime OperationTime { get; set; }

    public DateTime EventTime => OperationTime;
    public string SourceType => OperationType;
    public string Status => "DONE";
    public string MainText => GetReadableOperationType() + " " + CurrencyAmountFormatter.Format(Amount, CurrencyCode) + " " + CurrencyCode;
    public string DetailText => TransactionId == null
        ? "Wallet operation"
        : $"Wallet operation linked to transaction #{TransactionId}";

    private string GetReadableOperationType()
    {
        return OperationType switch
        {
            "FIAT_DEPOSIT" => "Fiat vklad",
            "FIAT_WITHDRAWAL" => "Fiat výběr",
            "CRYPTO_DEPOSIT" => "Crypto vklad",
            "CRYPTO_WITHDRAWAL" => "Crypto výběr",
            "BUY_FIAT_OUT" => "Nákup - fiat odesláno",
            "BUY_CRYPTO_IN" => "Nákup - crypto přijato",
            "SELL_CRYPTO_OUT" => "Prodej - crypto odesláno",
            "SELL_FIAT_IN" => "Prodej - fiat přijato",
            _ => OperationType
        };
    }
}
