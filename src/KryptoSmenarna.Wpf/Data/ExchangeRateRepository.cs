using KryptoSmenarna.Wpf.Models;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KryptoSmenarna.Wpf.Data
{
    public class ExchangeRateRepository
    {
        public ExchangeRate? GetLatestExchangeRate(int pairId)
        {
            using OracleConnection connection = new OracleConnectionFactory().CreateConnection();
            connection.Open();

            using OracleCommand command = new OracleCommand("GetLatestExchangeRate", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.BindByName = true;

            command.Parameters.Add("p_pair_id", OracleDbType.Int32).Value = pairId;

            command.Parameters.Add("p_rate_id", OracleDbType.Int32).Direction = ParameterDirection.Output;
            command.Parameters.Add("p_rate", OracleDbType.Decimal).Direction = ParameterDirection.Output;
            command.Parameters.Add("p_valid_from", OracleDbType.TimeStamp).Direction = ParameterDirection.Output;
            command.Parameters.Add("p_valid_to", OracleDbType.TimeStamp).Direction = ParameterDirection.Output;
            command.Parameters.Add("p_is_valid", OracleDbType.Int32).Direction = ParameterDirection.Output;

            command.ExecuteNonQuery();

            object rateIdValue = command.Parameters["p_rate_id"].Value;

            if (IsNull(rateIdValue))
                return null;

            return new ExchangeRate
            {
                RateId = ToInt(rateIdValue),
                PairId = pairId,
                Rate = ToDecimal(command.Parameters["p_rate"].Value),
                ValidFrom = ToDateTime(command.Parameters["p_valid_from"].Value),
                ValidTo = ToDateTime(command.Parameters["p_valid_to"].Value),
                IsValid = ToInt(command.Parameters["p_is_valid"].Value) == 1
            };
        }

        public ExchangeRate MakeNewValid(int exchangeRateId)
        {
            using OracleConnection connection = new OracleConnectionFactory().CreateConnection();
            connection.Open();

            using OracleTransaction transaction = connection.BeginTransaction();

            try
            {
                int pairId;
                decimal oldRate;

                using (OracleCommand selectCommand = new OracleCommand(@"
            SELECT pair_id, rate
            FROM exchange_rates
            WHERE rate_id = :rate_id
            FOR UPDATE
        ", connection))
                {
                    selectCommand.Transaction = transaction;
                    selectCommand.BindByName = true;
                    selectCommand.Parameters.Add("rate_id", OracleDbType.Int32).Value = exchangeRateId;

                    using OracleDataReader reader = selectCommand.ExecuteReader();

                    if (!reader.Read())
                        throw new InvalidOperationException("Exchange rate nebyl nalezen.");

                    pairId = Convert.ToInt32(reader["pair_id"]);
                    oldRate = Convert.ToDecimal(reader["rate"]);
                }

                // Kurz se simuluje malou náhodnou změnou a nový interval platí dvě minuty.
                decimal randomChange = GetRandomDecimal(-0.05m, 0.05m);
                decimal newRate = Math.Round(oldRate * (1 + randomChange), 8);

                DateTime now = DateTime.Now;
                DateTime validTo = now.AddMinutes(2);

                using (OracleCommand updateCommand = new OracleCommand(@"
            UPDATE exchange_rates
            SET valid_to = :now_time
            WHERE pair_id = :pair_id
              AND valid_from <= :now_time
              AND (valid_to IS NULL OR valid_to > :now_time)
        ", connection))
                {
                    updateCommand.Transaction = transaction;
                    updateCommand.BindByName = true;
                    updateCommand.Parameters.Add("now_time", OracleDbType.TimeStamp).Value = now;
                    updateCommand.Parameters.Add("pair_id", OracleDbType.Int32).Value = pairId;
                    updateCommand.ExecuteNonQuery();
                }

                int newRateId;

                // RateId se nastavuje ručně, protože nový kurz se vkládá s explicitním ID.
                using (OracleCommand idCommand = new OracleCommand(@"
            SELECT NVL(MAX(rate_id), 0) + 1
            FROM exchange_rates
        ", connection))
                {
                    idCommand.Transaction = transaction;
                    newRateId = Convert.ToInt32(idCommand.ExecuteScalar());
                }

                using (OracleCommand insertCommand = new OracleCommand(@"
            INSERT INTO exchange_rates (
                rate_id,
                pair_id,
                rate,
                valid_from,
                valid_to
            )
            VALUES (
                :rate_id,
                :pair_id,
                :rate,
                :valid_from,
                :valid_to
            )
        ", connection))
                {
                    insertCommand.Transaction = transaction;
                    insertCommand.BindByName = true;

                    insertCommand.Parameters.Add("rate_id", OracleDbType.Int32).Value = newRateId;
                    insertCommand.Parameters.Add("pair_id", OracleDbType.Int32).Value = pairId;
                    insertCommand.Parameters.Add("rate", OracleDbType.Decimal).Value = newRate;
                    insertCommand.Parameters.Add("valid_from", OracleDbType.TimeStamp).Value = now;
                    insertCommand.Parameters.Add("valid_to", OracleDbType.TimeStamp).Value = validTo;

                    insertCommand.ExecuteNonQuery();
                }

                transaction.Commit();

                return new ExchangeRate
                {
                    RateId = newRateId,
                    PairId = pairId,
                    Rate = newRate,
                    ValidFrom = now,
                    ValidTo = validTo,
                    IsValid = true
                };
            }
            catch (OracleException ex)
            {
                transaction.Rollback();

                MessageBox.Show(
                    $"Oracle chyba:\nNumber: {ex.Number}\nMessage: {ex.Message}"
                );

                throw;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private static decimal GetRandomDecimal(decimal min, decimal max)
        {
            double randomDouble = Random.Shared.NextDouble();
            decimal randomDecimal = (decimal)randomDouble;

            return min + (randomDecimal * (max - min));
        }

        private static bool IsNull(object value)
        {
            return value == null || value == DBNull.Value || value.ToString() == "null";
        }

        private static int ToInt(object value)
        {
            if (value is OracleDecimal oracleDecimal)
                return oracleDecimal.ToInt32();

            return Convert.ToInt32(value);
        }

        private static decimal ToDecimal(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0;

            if (value is Oracle.ManagedDataAccess.Types.OracleDecimal oracleDecimal)
                return oracleDecimal.Value;

            return Convert.ToDecimal(value);
        }

        private static DateTime? ToDateTime(object value)
        {
            if (IsNull(value))
                return null;

            if (value is OracleTimeStamp oracleTimeStamp)
                return oracleTimeStamp.Value;

            if (value is DateTime dateTime)
                return dateTime;

            return Convert.ToDateTime(value.ToString(), CultureInfo.InvariantCulture);
        }
    }
}
