using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KryptoSmenarna.Wpf.Data
{
    internal class TradingPairsRepository
    {
        // Procedura hledá pár i v opačném pořadí měn a vrací příznak isReversed.
        public int? FindTradingPairId(string baseCurrencyCode, string quoteCurrencyCode, out bool isReversed)
        {
            using OracleConnection connection = new OracleConnectionFactory().CreateConnection();
            connection.Open();

            using OracleCommand command = new OracleCommand("FindTradingPair", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.BindByName = true;

            command.Parameters.Add("p_base_currency_code", OracleDbType.Varchar2).Value = baseCurrencyCode;
            command.Parameters.Add("p_quote_currency_code", OracleDbType.Varchar2).Value = quoteCurrencyCode;

            OracleParameter pairIdParam = new OracleParameter("p_pair_id", OracleDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };

            OracleParameter isReversedParam = new OracleParameter("p_is_reversed", OracleDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };

            OracleParameter messageParam = new OracleParameter("p_message", OracleDbType.Varchar2, 255)
            {
                Direction = ParameterDirection.Output
            };

            command.Parameters.Add(pairIdParam);
            command.Parameters.Add(isReversedParam);
            command.Parameters.Add(messageParam);

            command.ExecuteNonQuery();

            if (pairIdParam.Value == DBNull.Value || pairIdParam.Value == null)
            {
                isReversed = false;
                return null;
            }

            int pairId = Convert.ToInt32(pairIdParam.Value.ToString());

            isReversed = isReversedParam.Value != DBNull.Value
                         && Convert.ToInt32(isReversedParam.Value.ToString()) == 1;

            return pairId;
        }
    }
}
