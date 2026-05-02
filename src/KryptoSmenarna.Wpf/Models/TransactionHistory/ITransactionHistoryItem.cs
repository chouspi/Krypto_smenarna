using System;

namespace KryptoSmenarna.Wpf.Models.TransactionHistory;

public interface ITransactionHistoryItem
{
    DateTime EventTime { get; }
    string SourceType { get; }
    string Status { get; }
    string MainText { get; }
    string DetailText { get; }
}
