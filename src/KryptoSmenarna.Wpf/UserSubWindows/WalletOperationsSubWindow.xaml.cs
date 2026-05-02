using KryptoSmenarna.Wpf.Data;
using KryptoSmenarna.Wpf.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace KryptoSmenarna.Wpf.UserSubWindows
{
    /// <summary>
    /// Interakční logika pro WalletOperationsSubWindow.xaml
    /// </summary>
    public partial class WalletOperationsSubWindow : UserControl
    {
        private const decimal MaxWalletAmount = 9999999999.99999999m;
        private User? currentUser;
        private Wallet? choosenCryptoWallet;
        private Wallet? choosenFiatWallet;
        private List<Wallet> userFiatWallets = new List<Wallet>();
        private List<Wallet> userCryptoWallets = new List<Wallet>();

        public WalletOperationsSubWindow()
        {
            InitializeComponent();
        }

        public void Initialize(User user)
        {
            currentUser = user;

            WalletsRepository walletsRepository = new WalletsRepository();
            userFiatWallets = walletsRepository.GetAllWallets(user.user_id, false);
            userCryptoWallets = walletsRepository.GetAllWallets(user.user_id, true);

            ComboboxFiat.ItemsSource = userFiatWallets;
            ComboBoxCrypto.ItemsSource = userCryptoWallets;

            if (userFiatWallets.Count > 0)
            {
                choosenFiatWallet = userFiatWallets[0];
                ComboboxFiat.SelectedIndex = 0;
            }
            else
            {
                TextBlockFiatBalance.Text = "Žádná FIAT peněženka";
                TextBoxFiatOperation.IsEnabled = false;
            }

            if (userCryptoWallets.Count > 0)
            {
                choosenCryptoWallet = userCryptoWallets[0];
                ComboBoxCrypto.SelectedIndex = 0;
            }
            else
            {
                TextBlockCryptoBalance.Text = "Žádná krypto peněženka";
                TextBlockCryptoInFiat.Text = "";
                TextBoxCryptoOperation.IsEnabled = false;
            }

            UpdateFiatBalanceInUI(choosenFiatWallet);
            UpdateCryptoBalanceInUI(choosenCryptoWallet);
        }

        private void UpdateFiatBalanceInUI(Wallet? wallet)
        {
            if (wallet == null)
            {
                TextBlockFiatBalance.Text = "Žádná FIAT peněženka";
                return;
            }

            TextBlockFiatBalance.Text = wallet.balance.ToString("N2") + " " + wallet.currencyCode;
        }

        private void ComboboxFiat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboboxFiat.SelectedIndex < 0 || ComboboxFiat.SelectedIndex >= userFiatWallets.Count)
                return;

            choosenFiatWallet = userFiatWallets[ComboboxFiat.SelectedIndex];
            UpdateFiatBalanceInUI(choosenFiatWallet);
            CalculateFiatValueFromCryptoValueInUI();
        }

        private void UpdateCryptoBalanceInUI(Wallet? wallet)
        {
            if (wallet == null)
            {
                TextBlockCryptoBalance.Text = "Žádná krypto peněženka";
                return;
            }

            TextBlockCryptoBalance.Text = wallet.balance.ToString("N8") + " " + wallet.currencyCode;
        }

        private void ComboBoxCrypto_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxCrypto.SelectedIndex < 0 || ComboBoxCrypto.SelectedIndex >= userCryptoWallets.Count)
                return;

            choosenCryptoWallet = userCryptoWallets[ComboBoxCrypto.SelectedIndex];
            UpdateCryptoBalanceInUI(choosenCryptoWallet);
            CalculateFiatValueFromCryptoValueInUI();
        }

        private void CalculateFiatValueFromCryptoValueInUI()
        {
            if (choosenCryptoWallet == null || choosenFiatWallet == null)
            {
                TextBlockCryptoInFiat.Text = "";
                return;
            }

            decimal cryptoAmount = choosenCryptoWallet.balance;

            TradingPairsRepository tp = new TradingPairsRepository();
            ExchangeRateRepository er = new ExchangeRateRepository();

            int? tradingPairId = tp.FindTradingPairId(
                choosenCryptoWallet.currencyCode,
                choosenFiatWallet.currencyCode,
                out bool isReversed
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

            if (!rate.IsValid)
                rate = er.MakeNewValid(rate.RateId);

            if (rate.Rate <= 0)
            {
                TextBlockCryptoInFiat.Text = "≈ neplatný kurz";
                return;
            }

            decimal fiatValue = !isReversed
                ? cryptoAmount * rate.Rate
                : cryptoAmount / rate.Rate;

            TextBlockCryptoInFiat.Text =
                "≈ " + fiatValue.ToString("N2") + " " + choosenFiatWallet.currencyCode;
        }

        private void ButtonFiatDeposit_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null || choosenFiatWallet == null)
            {
                MessageBox.Show("Nejdříve vyberte FIAT peněženku.");
                return;
            }

            if (!TryReadWalletAmount(TextBoxFiatOperation, out decimal toDeposit))
                return;

            if (choosenFiatWallet.balance + toDeposit > MaxWalletAmount)
            {
                MessageBox.Show("Po vkladu by zůstatek překročil maximální povolenou hodnotu.");
                return;
            }

            WalletsRepository walletsRepository = new WalletsRepository();

            try
            {
                walletsRepository.Deposit(
                    toDeposit,
                    choosenFiatWallet.currencyCode,
                    currentUser.user_id
                );
            }
            catch (OracleException ex)
            {
                MessageBox.Show("Vklad se nepodařilo provést.\n\n" + ex.Message);
                return;
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            if (!ReloadFiatWallet(walletsRepository, choosenFiatWallet.currencyCode))
                return;

            TextBoxFiatOperation.Text = "";
        }

        private void ButtonFiatWithdraw_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null || choosenFiatWallet == null)
            {
                MessageBox.Show("Nejdříve vyberte FIAT peněženku.");
                return;
            }

            if (!TryReadWalletAmount(TextBoxFiatOperation, out decimal toWithdraw))
                return;

            if (toWithdraw > choosenFiatWallet.balance)
            {
                MessageBox.Show("Nedostatečný zůstatek na peněžence.");
                return;
            }

            WalletsRepository walletsRepository = new WalletsRepository();

            if (!walletsRepository.TryWithdraw(toWithdraw, choosenFiatWallet.currencyCode, currentUser.user_id))
            {
                MessageBox.Show("Výběr se nepodařilo provést.");
                return;
            }

            if (!ReloadFiatWallet(walletsRepository, choosenFiatWallet.currencyCode))
                return;

            TextBoxFiatOperation.Text = "";
        }

        private void ButtonCryptoDeposit_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null || choosenCryptoWallet == null)
            {
                MessageBox.Show("Nejdříve vyberte krypto peněženku.");
                return;
            }

            if (!TryReadWalletAmount(TextBoxCryptoOperation, out decimal toDeposit))
                return;

            if (choosenCryptoWallet.balance + toDeposit > MaxWalletAmount)
            {
                MessageBox.Show("Po vkladu by zůstatek překročil maximální povolenou hodnotu.");
                return;
            }

            WalletsRepository walletsRepository = new WalletsRepository();

            try
            {
                walletsRepository.Deposit(
                    toDeposit,
                    choosenCryptoWallet.currencyCode,
                    currentUser.user_id
                );
            }
            catch (OracleException ex)
            {
                MessageBox.Show("Vklad se nepodařilo provést.\n\n" + ex.Message);
                return;
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            if (!ReloadCryptoWallet(walletsRepository, choosenCryptoWallet.currencyCode))
                return;

            TextBoxCryptoOperation.Text = "";
        }

        private void ButtonCryptoWithdraw_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null || choosenCryptoWallet == null)
            {
                MessageBox.Show("Nejdříve vyberte krypto peněženku.");
                return;
            }

            if (!TryReadWalletAmount(TextBoxCryptoOperation, out decimal toWithdraw))
                return;

            if (toWithdraw > choosenCryptoWallet.balance)
            {
                MessageBox.Show("Nedostatečný zůstatek na peněžence.");
                return;
            }

            WalletsRepository walletsRepository = new WalletsRepository();

            if (!walletsRepository.TryWithdraw(toWithdraw, choosenCryptoWallet.currencyCode, currentUser.user_id))
            {
                MessageBox.Show("Výběr se nepodařilo provést.");
                return;
            }

            if (!ReloadCryptoWallet(walletsRepository, choosenCryptoWallet.currencyCode))
                return;

            TextBoxCryptoOperation.Text = "";
        }

        private bool TryReadWalletAmount(TextBox textBox, out decimal amount)
        {
            if (!decimal.TryParse(textBox.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out amount)
                && !decimal.TryParse(textBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out amount))
            {
                MessageBox.Show("Zadejte platnou částku.");
                return false;
            }

            if (amount <= 0)
            {
                MessageBox.Show("Částka musí být větší než 0.");
                return false;
            }

            if (decimal.Round(amount, 8) != amount)
            {
                MessageBox.Show("Částka může mít maximálně 8 desetinných míst.");
                return false;
            }

            if (amount > MaxWalletAmount)
            {
                MessageBox.Show("Částka je příliš vysoká.");
                return false;
            }

            return true;
        }

        private bool ReloadFiatWallet(WalletsRepository walletsRepository, string currencyCode)
        {
            if (currentUser == null)
                return false;

            Wallet? updatedWallet = walletsRepository.GetWallet(currentUser.user_id, currencyCode);

            if (updatedWallet == null)
            {
                MessageBox.Show("Peněženku se nepodařilo znovu načíst.");
                return false;
            }

            choosenFiatWallet = updatedWallet;
            ReplaceWalletInList(userFiatWallets, updatedWallet);
            UpdateFiatBalanceInUI(choosenFiatWallet);
            CalculateFiatValueFromCryptoValueInUI();
            return true;
        }

        private bool ReloadCryptoWallet(WalletsRepository walletsRepository, string currencyCode)
        {
            if (currentUser == null)
                return false;

            Wallet? updatedWallet = walletsRepository.GetWallet(currentUser.user_id, currencyCode);

            if (updatedWallet == null)
            {
                MessageBox.Show("Peněženku se nepodařilo znovu načíst.");
                return false;
            }

            choosenCryptoWallet = updatedWallet;
            ReplaceWalletInList(userCryptoWallets, updatedWallet);
            UpdateCryptoBalanceInUI(choosenCryptoWallet);
            CalculateFiatValueFromCryptoValueInUI();
            return true;
        }

        private void ReplaceWalletInList(List<Wallet> wallets, Wallet updatedWallet)
        {
            int index = wallets.FindIndex(wallet => wallet.currencyCode == updatedWallet.currencyCode);

            if (index >= 0)
                wallets[index] = updatedWallet;
        }
    }
}
