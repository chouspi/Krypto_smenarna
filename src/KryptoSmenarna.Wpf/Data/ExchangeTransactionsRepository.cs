using KryptoSmenarna.Wpf.Models;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace KryptoSmenarna.Wpf.Data;

public class ExchangeTransactionsRepository
{
    public List<ExchangeTransaction> GetExchangeTransactionsForDays(int userId, int days)
    {
        if (days <= 0)
            throw new ArgumentException("Počet dní musí být větší než 0.");

        List<ExchangeTransaction> result = new List<ExchangeTransaction>();

        using OracleConnection connection = new OracleConnectionFactory().CreateConnection();
        connection.Open();

        using OracleCommand command = new OracleCommand(@"
            SELECT
                transaction_id,
                user_id,
                pair_id,
                from_currency_code,
                to_currency_code,
                from_amount,
                exchange_rate,
                to_amount,
                transaction_time,
                status
            FROM exchange_transactions
            WHERE user_id = :user_id
              AND transaction_time >= CAST(SYSTIMESTAMP AS TIMESTAMP) - NUMTODSINTERVAL(:days, 'DAY')
            ORDER BY transaction_time DESC
        ", connection);

        command.BindByName = true;
        command.Parameters.Add("user_id", OracleDbType.Int32).Value = userId;
        command.Parameters.Add("days", OracleDbType.Int32).Value = days;

        using OracleDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            ExchangeTransaction transaction = new ExchangeTransaction
            {
                TransactionId = ToInt(reader["transaction_id"]),
                UserId = ToInt(reader["user_id"]),
                PairId = ToInt(reader["pair_id"]),

                FromCurrencyCode = reader["from_currency_code"].ToString() ?? "",
                ToCurrencyCode = reader["to_currency_code"].ToString() ?? "",

                FromAmount = ToDecimal(reader["from_amount"]),
                ExchangeRate = ToDecimal(reader["exchange_rate"]),
                ToAmount = ToDecimal(reader["to_amount"]),

                TransactionTime = ToDateTime(reader["transaction_time"]),
                Status = reader["status"].ToString() ?? ""
            };

            result.Add(transaction);
        }

        return result;
    }

    public int InsertExchangeTransaction(ExchangeTransaction transaction)
    {
        using OracleConnection connection = new OracleConnectionFactory().CreateConnection();
        connection.Open();

        using OracleCommand command = new OracleCommand(@"
            INSERT INTO exchange_transactions (
                user_id,
                pair_id,
                from_currency_code,
                to_currency_code,
                from_amount,
                exchange_rate,
                to_amount,
                transaction_time,
                status
            )
            VALUES (
                :user_id,
                :pair_id,
                :from_currency_code,
                :to_currency_code,
                :from_amount,
                :exchange_rate,
                :to_amount,
                SYSTIMESTAMP,
                :status
            )
            RETURNING transaction_id INTO :transaction_id
        ", connection);

        command.BindByName = true;

        command.Parameters.Add("user_id", OracleDbType.Int32).Value = transaction.UserId;
        command.Parameters.Add("pair_id", OracleDbType.Int32).Value = transaction.PairId;
        command.Parameters.Add("from_currency_code", OracleDbType.Varchar2).Value = transaction.FromCurrencyCode;
        command.Parameters.Add("to_currency_code", OracleDbType.Varchar2).Value = transaction.ToCurrencyCode;
        command.Parameters.Add("from_amount", OracleDbType.Decimal).Value = transaction.FromAmount;
        command.Parameters.Add("exchange_rate", OracleDbType.Decimal).Value = transaction.ExchangeRate;
        command.Parameters.Add("to_amount", OracleDbType.Decimal).Value = transaction.ToAmount;
        command.Parameters.Add("status", OracleDbType.Varchar2).Value = transaction.Status;

        OracleParameter transactionIdParam = new OracleParameter("transaction_id", OracleDbType.Decimal)
        {
            Direction = System.Data.ParameterDirection.Output
        };

        command.Parameters.Add(transactionIdParam);

        command.ExecuteNonQuery();

        return ToInt(transactionIdParam.Value);
    }

    private static int ToInt(object value)
    {
        if (value is OracleDecimal oracleDecimal)
            return Convert.ToInt32(oracleDecimal.Value);

        return Convert.ToInt32(value);
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
