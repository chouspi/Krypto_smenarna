using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Windows.Media.Animation;
namespace KryptoSmenarna.Wpf.Data
{
    public class WalletsRepository
    {
        public bool TryWithdraw(decimal amount, string currencyCode, int userId) //pokusi se vybrat částku z dené peněženky, pokud uspěje, vrátí true
        {
            try
            {
                OracleConnection connection = new OracleConnectionFactory().CreateConnection();
                connection.Open();

                OracleCommand command = new OracleCommand("WithdrawFromWallet",connection);
                command.CommandType = CommandType.StoredProcedure;
                command.BindByName = true;

                command.Parameters.Add("p_user_id",OracleDbType.Int32).Value = userId;
                command.Parameters.Add("p_currency_code", OracleDbType.Varchar2).Value = currencyCode;
                command.Parameters.Add("p_amount",OracleDbType.Decimal).Value = amount;
                command.ExecuteNonQuery();


                return true;
            }
            catch (OracleException ex)
            {
                return false;
            }
        }
        public void Deposit(decimal amount, string currencyCode, int userId)
        {
            OracleConnection connection = new OracleConnectionFactory().CreateConnection();
            connection.Open();

            OracleCommand command = new OracleCommand("DepositToWallet", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.BindByName = true;

            command.Parameters.Add("p_user_id",OracleDbType.Int32).Value = userId;
            command.Parameters.Add("p_currency_code", OracleDbType.Varchar2).Value = currencyCode;
            command.Parameters.Add("p_amount", OracleDbType.Decimal).Value = amount;

            command.ExecuteNonQuery();
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

            if(walletBalance.Value == DBNull.Value)
                return 0;

            return Convert.ToDecimal(walletBalance.Value.ToString());
        }
        
    }
}
