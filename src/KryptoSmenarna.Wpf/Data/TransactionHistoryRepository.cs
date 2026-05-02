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
                :p_result := GetWalletOperationsForDays(:p_user_id, :p_days);
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
            result.Add(new WalletOperationHistoryItem
            {
                OperationId = ToInt(reader["operation_id"]),
                WalletId = ToInt(reader["wallet_id"]),
                TransactionId = ToNullableInt(reader["transaction_id"]),
                OperationType = reader["operation_type"].ToString() ?? "",
                CurrencyCode = reader["currency_code"].ToString() ?? "",
                Amount = ToDecimal(reader["amount"]),
                OperationTime = ToDateTime(reader["operation_time"])
            });
        }

        return result;
    }

    private static int ToInt(object value)
    {
        if (value is OracleDecimal oracleDecimal)
            return Convert.ToInt32(oracleDecimal.Value);

        return Convert.ToInt32(value);
    }

    private static int? ToNullableInt(object value)
    {
        if (value == null || value == DBNull.Value)
            return null;

        return ToInt(value);
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
}
