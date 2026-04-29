using KryptoSmenarna.Wpf.Data;
using KryptoSmenarna.Wpf.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KryptoSmenarna.Wpf
{
    /// <summary>
    /// Interakční logika pro UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        private Wallet choosenCryptoWallet = new Wallet();
        private Wallet choosenFiatWallet = new Wallet();
        List<Wallet> userFiatWallets = new List<Wallet>();
        List<Wallet> userCryptoWallets = new List<Wallet>();
        public UserWindow(User user)
        {
            userFiatWallets = new WalletsRepository().GetAllWallets(user.user_id, false);
            userCryptoWallets = new WalletsRepository().GetAllWallets(user.user_id, true);
            choosenCryptoWallet = userCryptoWallets[0];
            choosenFiatWallet = userFiatWallets[0];
            InitializeComponent();
            ComboboxFiat.ItemsSource = userFiatWallets;
            ComboBoxCrypto.ItemsSource = userCryptoWallets;

            UpdateFiatBalanceInUI(choosenFiatWallet);
            UpdateCryptoBalanceInUI(choosenCryptoWallet);
        }
        private void UpdateFiatBalanceInUI(Wallet wallet)
        {
            if (choosenFiatWallet.balance != null)
                TextBlockFiatBalance.Text = choosenFiatWallet.balance.ToString() + " " + choosenFiatWallet.currencyCode;
            else
                TextBlockFiatBalance.Text = "0.00 " + choosenFiatWallet.currencyCode;
        }

        private void ComboboxFiat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            choosenFiatWallet = userFiatWallets[ComboboxFiat.SelectedIndex];
            UpdateFiatBalanceInUI(choosenFiatWallet);
            CalculateFiatValueFromCryptoValueInUI();
        }
        private void UpdateCryptoBalanceInUI(Wallet wallet)
        {
            if (choosenCryptoWallet.balance != null)
                TextBlockCryptoBalance.Text = choosenCryptoWallet.balance.ToString() + " " + choosenCryptoWallet.currencyCode;
            else
                TextBlockCryptoBalance.Text = "0.00 " + choosenCryptoWallet.currencyCode;
        }

        private void ComboBoxCrypto_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            choosenCryptoWallet = userCryptoWallets[ComboBoxCrypto.SelectedIndex];
            UpdateCryptoBalanceInUI(choosenCryptoWallet);
            CalculateFiatValueFromCryptoValueInUI();
        }
        private void CalculateFiatValueFromCryptoValueInUI()
        {
            if (choosenCryptoWallet == null || choosenFiatWallet == null)
                return;

            decimal cryptoAmount = choosenCryptoWallet.balance;

            TradingPairsRepository tp = new TradingPairsRepository();
            ExchangeRateRepository er = new ExchangeRateRepository();

            bool isReversed;

            int? tradingPairId = tp.FindTradingPairId(
                choosenCryptoWallet.currencyCode,
                choosenFiatWallet.currencyCode,
                out isReversed
            );

            if (tradingPairId == null)
            {
                TextBlockCryptoInFiat.Text = "≈ kurz nenalezen";
                return;
            }

            ExchangeRate? rate = er.GetLatestExchangeRate(tradingPairId.Value);

            if (rate == null)
            {
                TextBlockCryptoInFiat.Text = "≈ kurz neexistuje";
                return;
            }

            // Neplatný kurz se hned nahradí novým simulovaným kurzem.
            if (!rate.IsValid)
            {
                rate = er.MakeNewValid(rate.RateId);
            }

            decimal fiatValue;

            // Pokud byl pár nalezen opačně, používá se převrácený kurz.
            if (!isReversed)
            {
                fiatValue = cryptoAmount * rate.Rate;
            }
            else
            {
                fiatValue = cryptoAmount / rate.Rate;
            }

            TextBlockCryptoInFiat.Text =
                "≈ " + fiatValue.ToString("N2") + " " + choosenFiatWallet.currencyCode;
        }
    }
}
