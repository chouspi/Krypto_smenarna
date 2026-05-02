using KryptoSmenarna.Wpf.Models.TransactionHistory;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;

namespace KryptoSmenarna.Wpf.Data;

public class TransactionHistoryRepository
{
    public List<ITransactionHistoryItem> GetTransactionHistoryItemsForDays(int userId, int days)
    {
        if (userId <= 0)
            throw new ArgumentException("ID uživatele musí být větší než 0.", nameof(userId));

        if (days <= 0)
            throw new ArgumentException("Počet dní musí být větší než 0.", nameof(days));

        List<ITransactionHistoryItem> result = new List<ITransactionHistoryItem>();

        using OracleConnection connection = new OracleConnectionFactory().CreateConnection();
        connection.Open();

        using OracleCommand command = new OracleCommand(@"
            BEGIN
                :p_result := GetTransactionsForXDays(:p_user_id, :p_days);
            END;
        ", connection);

        command.BindByName = true;

        OracleParameter resultParameter = new OracleParameter("p_result", OracleDbType.RefCursor)
        {
            Direction = ParameterDirection.Output
        };

        command.Parameters.Add(resultParameter);
        command.Parameters.Add("p_user_id", OracleDbType.Int32).Value = userId;
        command.Parameters.Add("p_days", OracleDbType.Int32).Value = days;

        command.ExecuteNonQuery();

        using OracleDataReader reader = ((OracleRefCursor)resultParameter.Value).GetDataReader();

        while (reader.Read())
        {
            string sourceType = ToText(reader["source_type"]);

            if (sourceType == "EXCHANGE")
            {
                result.Add(new ExchangeTransactionHistoryItem
                {
                    FromCurrencyCode = ToText(reader["from_currency_code"]),
                    ToCurrencyCode = ToText(reader["to_currency_code"]),
                    FromAmount = ToDecimal(reader["from_amount"]),
                    ToAmount = ToDecimal(reader["to_amount"]),
                    ExchangeRate = ToDecimal(reader["exchange_rate"]),
                    TransactionTime = ToDateTime(reader["event_time"]),
                    TransactionStatus = ToText(reader["status"])
                });

                continue;
            }

            result.Add(new WalletOperationHistoryItem
            {
                OperationType = sourceType,
                CurrencyCode = ToText(reader["from_currency_code"]),
                Amount = ToDecimal(reader["from_amount"]),
                OperationTime = ToDateTime(reader["event_time"])
            });
        }

        return result;
    }

    private static decimal ToDecimal(object value)
    {
        if (value is OracleDecimal oracleDecimal)
            return oracleDecimal.Value;

        return Convert.ToDecimal(value);
    }

    private static DateTime ToDateTime(object value)
    {
        if (value is OracleTimeStamp oracleTimeStamp)
            return oracleTimeStamp.Value;

        return Convert.ToDateTime(value);
    }

    private static string ToText(object value)
    {
        if (value == null)
            return "";

        if (value == DBNull.Value)
            return "";

        if (value is OracleString oracleString)
        {
            if (oracleString.IsNull)
                return "";

            return oracleString.Value;
        }

        string? text = value.ToString();
        if (text == null)
            return "";

        return text;
    }
}
