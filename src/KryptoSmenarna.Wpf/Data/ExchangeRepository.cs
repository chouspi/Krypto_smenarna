using KryptoSmenarna.Wpf.Models;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Data;

namespace KryptoSmenarna.Wpf.Data;

public class ExchangeRepository
{
    public ExchangeQuote GetExchangeQuote(int userId, string fromCurrencyCode, string toCurrencyCode, decimal fromAmount)
    {
        using OracleConnection connection = new OracleConnectionFactory().CreateConnection();
        connection.Open();

        using OracleCommand command = new OracleCommand("GetExchangeQuote", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.BindByName = true;

        command.Parameters.Add("p_user_id", OracleDbType.Int32).Value = userId;
        command.Parameters.Add("p_from_currency_code", OracleDbType.Varchar2).Value = fromCurrencyCode;
        command.Parameters.Add("p_to_currency_code", OracleDbType.Varchar2).Value = toCurrencyCode;

        OracleParameter fromAmountParameter = command.Parameters.Add("p_from_amount", OracleDbType.Decimal);
        fromAmountParameter.Precision = 18;
        fromAmountParameter.Scale = 8;
        fromAmountParameter.Value = fromAmount;

        OracleParameter pairIdParameter = command.Parameters.Add("p_pair_id", OracleDbType.Int32);
        pairIdParameter.Direction = ParameterDirection.Output;

        OracleParameter rateIdParameter = command.Parameters.Add("p_rate_id", OracleDbType.Int32);
        rateIdParameter.Direction = ParameterDirection.Output;

        OracleParameter exchangeRateParameter = command.Parameters.Add("p_exchange_rate", OracleDbType.Decimal);
        exchangeRateParameter.Direction = ParameterDirection.Output;

        OracleParameter isReversedParameter = command.Parameters.Add("p_is_reversed", OracleDbType.Int32);
        isReversedParameter.Direction = ParameterDirection.Output;

        OracleParameter toAmountParameter = command.Parameters.Add("p_to_amount", OracleDbType.Decimal);
        toAmountParameter.Direction = ParameterDirection.Output;

        OracleParameter messageParameter = command.Parameters.Add("p_message", OracleDbType.Varchar2, 255);
        messageParameter.Direction = ParameterDirection.Output;

        command.ExecuteNonQuery();

        ExchangeQuote quote = new ExchangeQuote();
        quote.RateId = ToInt(rateIdParameter.Value);
        quote.ExchangeRate = ToDecimal(exchangeRateParameter.Value);
        quote.FromCurrencyCode = fromCurrencyCode;
        quote.ToCurrencyCode = toCurrencyCode;
        quote.FromAmount = fromAmount;
        quote.ToAmount = ToDecimal(toAmountParameter.Value);

        return quote;
    }

    public ExchangeExecutionResult ExecuteExchange(int userId, string fromCurrencyCode, string toCurrencyCode, decimal fromAmount, int expectedRateId)
    {
        using OracleConnection connection = new OracleConnectionFactory().CreateConnection();
        connection.Open();

        using OracleCommand command = new OracleCommand("ExecuteExchange", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.BindByName = true;

        command.Parameters.Add("p_user_id", OracleDbType.Int32).Value = userId;
        command.Parameters.Add("p_from_currency_code", OracleDbType.Varchar2).Value = fromCurrencyCode;
        command.Parameters.Add("p_to_currency_code", OracleDbType.Varchar2).Value = toCurrencyCode;

        OracleParameter fromAmountParameter = command.Parameters.Add("p_from_amount", OracleDbType.Decimal);
        fromAmountParameter.Precision = 18;
        fromAmountParameter.Scale = 8;
        fromAmountParameter.Value = fromAmount;

        command.Parameters.Add("p_expected_rate_id", OracleDbType.Int32).Value = expectedRateId;

        OracleParameter transactionIdParameter = command.Parameters.Add("p_transaction_id", OracleDbType.Int32);
        transactionIdParameter.Direction = ParameterDirection.Output;

        OracleParameter exchangeRateParameter = command.Parameters.Add("p_exchange_rate", OracleDbType.Decimal);
        exchangeRateParameter.Direction = ParameterDirection.Output;

        OracleParameter toAmountParameter = command.Parameters.Add("p_to_amount", OracleDbType.Decimal);
        toAmountParameter.Direction = ParameterDirection.Output;

        OracleParameter messageParameter = command.Parameters.Add("p_message", OracleDbType.Varchar2, 255);
        messageParameter.Direction = ParameterDirection.Output;

        command.ExecuteNonQuery();

        ExchangeExecutionResult result = new ExchangeExecutionResult();
        result.TransactionId = ToInt(transactionIdParameter.Value);

        return result;
    }

    private static int ToInt(object value)
    {
        if (value is OracleDecimal oracleDecimal)
            return Convert.ToInt32(oracleDecimal.Value);

        return Convert.ToInt32(value.ToString());
    }

    private static decimal ToDecimal(object value)
    {
        if (value is OracleDecimal oracleDecimal)
            return oracleDecimal.Value;

        return Convert.ToDecimal(value.ToString());
    }

}
