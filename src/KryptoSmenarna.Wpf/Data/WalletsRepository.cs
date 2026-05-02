using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Windows.Media.Animation;
using KryptoSmenarna.Wpf.Models;

namespace KryptoSmenarna.Wpf.Data
{
    public class WalletsRepository
    {
        private const decimal MaxWalletAmount = 9999999999.99999999m;

        public List<Wallet> GetAllWallets(int userId, bool isCrypto)
        {
            List<Wallet> result = new List<Wallet>();

            using OracleConnection connection = new OracleConnectionFactory().CreateConnection();
            connection.Open();

            string sql = @"
        SELECT
            w.wallet_id,
            w.user_id,
            w.currency_code,
            w.balance
        FROM wallets w
        JOIN currencies c ON c.currency_code = w.currency_code
        WHERE w.user_id = :p_user_id
          AND c.is_crypto = :p_is_crypto
        ORDER BY w.currency_code";

            using OracleCommand command = new OracleCommand(sql, connection);
            command.BindByName = true;

            command.Parameters.Add("p_user_id", OracleDbType.Int32).Value = userId;
            command.Parameters.Add("p_is_crypto", OracleDbType.Int32).Value = isCrypto ? 1 : 0;

            using OracleDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Wallet wallet = new Wallet()
                {
                    wallet_id = Convert.ToInt32(reader["wallet_id"]),
                    user_id = Convert.ToInt32(reader["user_id"]),
                    currencyCode = reader["currency_code"].ToString() ?? "",
                    balance = Convert.ToDecimal(reader["balance"])
                };

                result.Add(wallet);
            }

            return result;
        }

        // Databázová procedura řeší validaci výběru; OracleException znamená neúspěch.
        public bool TryWithdraw(decimal amount, string currencyCode, int userId)
        {
            if (amount <= 0 || decimal.Round(amount, 8) != amount || amount > MaxWalletAmount || string.IsNullOrWhiteSpace(currencyCode))
                return false;

            try
            {
                using OracleConnection connection = new OracleConnectionFactory().CreateConnection();
                connection.Open();

                using OracleCommand command = new OracleCommand("WithdrawFromWallet", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.BindByName = true;

                command.Parameters.Add("p_user_id", OracleDbType.Int32).Value = userId;
                command.Parameters.Add("p_currency_code", OracleDbType.Varchar2).Value = currencyCode;
                OracleParameter amountParameter = command.Parameters.Add("p_amount", OracleDbType.Decimal);
                amountParameter.Precision = 18;
                amountParameter.Scale = 8;
                amountParameter.Value = amount;
                command.ExecuteNonQuery();

                return true;
            }
            catch (OracleException)
            {
                return false;
            }
        }

        public void Deposit(decimal amount, string currencyCode, int userId)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Částka vkladu musí být větší než 0.");

            if (decimal.Round(amount, 8) != amount || amount > MaxWalletAmount)
                throw new ArgumentOutOfRangeException(nameof(amount), "Částka vkladu neodpovídá formátu NUMBER(18,8).");

            if (string.IsNullOrWhiteSpace(currencyCode))
                throw new ArgumentException("Kód měny nesmí být prázdný.", nameof(currencyCode));

            using OracleConnection connection = new OracleConnectionFactory().CreateConnection();
            connection.Open();

            using OracleCommand command = new OracleCommand("DepositToWallet", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.BindByName = true;

            command.Parameters.Add("p_user_id", OracleDbType.Int32).Value = userId;
            command.Parameters.Add("p_currency_code", OracleDbType.Varchar2).Value = currencyCode;
            OracleParameter amountParameter = command.Parameters.Add("p_amount", OracleDbType.Decimal);
            amountParameter.Precision = 18;
            amountParameter.Scale = 8;
            amountParameter.Value = amount;

            command.ExecuteNonQuery();
        }

        public Wallet? GetWallet(int userId, string currencyCode)
        {
            using OracleConnection connection = new OracleConnectionFactory().CreateConnection();
            connection.Open();

            using OracleCommand command = new OracleCommand(@"
        SELECT
            wallet_id,
            user_id,
            currency_code,
            balance
        FROM wallets
        WHERE user_id = :user_id
          AND currency_code = :currency_code
    ", connection);

            command.BindByName = true;
            command.Parameters.Add("user_id", OracleDbType.Int32).Value = userId;
            command.Parameters.Add("currency_code", OracleDbType.Varchar2).Value = currencyCode;

            using OracleDataReader reader = command.ExecuteReader();

            if (!reader.Read())
                return null;

            return new Wallet
            {
                wallet_id = Convert.ToInt32(reader["wallet_id"]),
                user_id = Convert.ToInt32(reader["user_id"]),
                currencyCode = reader["currency_code"].ToString() ?? "",
                balance = Convert.ToDecimal(reader["balance"])
            };
        }

        public decimal GetFiatBalance(int userId, string fiatCode)
        {
            using OracleConnection connection = new OracleConnectionFactory().CreateConnection();
            connection.Open();

            using OracleCommand command = new OracleCommand("GetFiatBalance", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.BindByName = true;

            command.Parameters.Add("p_user_id", OracleDbType.Int32).Value = userId;
            command.Parameters.Add("p_fiat_code", OracleDbType.Varchar2).Value = fiatCode;

            OracleParameter walletBalance = new OracleParameter("p_balance", OracleDbType.Decimal);
            walletBalance.Direction = ParameterDirection.Output;
            command.Parameters.Add(walletBalance);

            command.ExecuteNonQuery();

            if (walletBalance.Value == DBNull.Value)
                return 0;

            return Convert.ToDecimal(walletBalance.Value.ToString());
        }
    }
}
