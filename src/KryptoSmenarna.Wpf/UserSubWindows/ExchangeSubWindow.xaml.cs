using KryptoSmenarna.Wpf.Data;
using KryptoSmenarna.Wpf.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace KryptoSmenarna.Wpf.UserSubWindows
{
    /// <summary>
    /// Interakční logika pro ExchangeSubWindow.xaml
    /// </summary>
    public partial class ExchangeSubWindow : UserControl
    {
        private const decimal MaxWalletAmount = 9999999999.99999999m;

        private User? currentUser;
        private List<Wallet> fiatWallets = new List<Wallet>();
        private List<Wallet> cryptoWallets = new List<Wallet>();
        private List<Wallet> allWallets = new List<Wallet>();
        private ExchangeQuote? currentQuote;
        private bool isInitializing;
        private readonly DispatcherTimer quoteRefreshTimer;

        public event EventHandler? ExchangeCompleted;

        public ExchangeSubWindow()
        {
            isInitializing = true;
            quoteRefreshTimer = new DispatcherTimer();
            quoteRefreshTimer.Interval = TimeSpan.FromMilliseconds(300);
            quoteRefreshTimer.Tick += QuoteRefreshTimer_Tick;
            InitializeComponent();
            isInitializing = false;
        }

        public void Initialize(User user)
        {
            currentUser = user;
            LoadWallets(null, null);
        }

        public void Refresh()
        {
            Wallet? fromWallet = GetSelectedFromWallet();
            Wallet? toWallet = GetSelectedToWallet();

            string? fromCurrencyCode = null;
            string? toCurrencyCode = null;

            if (fromWallet != null)
                fromCurrencyCode = fromWallet.currencyCode;

            if (toWallet != null)
                toCurrencyCode = toWallet.currencyCode;

            LoadWallets(fromCurrencyCode, toCurrencyCode);
        }

        private void LoadWallets(string? preferredFromCurrencyCode, string? preferredToCurrencyCode)
        {
            if (currentUser == null)
                return;

            isInitializing = true;

            WalletsRepository walletsRepository = new WalletsRepository();
            fiatWallets = walletsRepository.GetAllWallets(currentUser.user_id, false);
            cryptoWallets = walletsRepository.GetAllWallets(currentUser.user_id, true);

            allWallets = new List<Wallet>();

            foreach (Wallet wallet in fiatWallets)
            {
                allWallets.Add(wallet);
            }

            foreach (Wallet wallet in cryptoWallets)
            {
                allWallets.Add(wallet);
            }

            ComboBoxFromCurrency.ItemsSource = allWallets;
            ComboBoxToCurrency.ItemsSource = allWallets;

            SelectWallet(ComboBoxFromCurrency, preferredFromCurrencyCode, fiatWallets);
            SelectWallet(ComboBoxToCurrency, preferredToCurrencyCode, cryptoWallets);

            isInitializing = false;

            UpdateSelectedBalanceText();
            ScheduleQuoteRefresh();
        }

        private void SelectWallet(ComboBox comboBox, string? preferredCurrencyCode, List<Wallet> fallbackWallets)
        {
            Wallet? selectedWallet = null;

            if (!string.IsNullOrWhiteSpace(preferredCurrencyCode))
            {
                foreach (Wallet wallet in allWallets)
                {
                    if (wallet.currencyCode == preferredCurrencyCode)
                    {
                        selectedWallet = wallet;
                        break;
                    }
                }
            }

            if (selectedWallet == null && fallbackWallets.Count > 0)
                selectedWallet = fallbackWallets[0];

            if (selectedWallet == null && allWallets.Count > 0)
                selectedWallet = allWallets[0];

            comboBox.SelectedItem = selectedWallet;
        }

        private void ComboBoxCurrency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInitializing)
                return;

            UpdateSelectedBalanceText();
            ScheduleQuoteRefresh();
        }

        private void TextBoxFromAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInitializing)
                return;

            ScheduleQuoteRefresh();
        }

        private void ButtonSwapCurrencies_Click(object sender, RoutedEventArgs e)
        {
            Wallet? fromWallet = GetSelectedFromWallet();
            Wallet? toWallet = GetSelectedToWallet();

            ComboBoxFromCurrency.SelectedItem = toWallet;
            ComboBoxToCurrency.SelectedItem = fromWallet;

            UpdateSelectedBalanceText();
            ScheduleQuoteRefresh();
        }

        private void QuoteRefreshTimer_Tick(object? sender, EventArgs e)
        {
            quoteRefreshTimer.Stop();
            UpdateQuoteFromCurrentInput();
        }

        private void ScheduleQuoteRefresh()
        {
            quoteRefreshTimer.Stop();
            InvalidateQuote("Počítám nabídku směny...");
            quoteRefreshTimer.Start();
        }

        private void UpdateQuoteFromCurrentInput()
        {
            if (currentUser == null)
            {
                InvalidateQuote("Směnu lze počítat až po přihlášení.");
                return;
            }

            Wallet? fromWallet = GetSelectedFromWallet();
            Wallet? toWallet = GetSelectedToWallet();

            if (fromWallet == null || toWallet == null)
            {
                InvalidateQuote("Vyberte obě měny pro směnu.");
                return;
            }

            if (fromWallet.currencyCode == toWallet.currencyCode)
            {
                InvalidateQuote("Nelze směnit měnu samu za sebe.");
                return;
            }

            if (!TryReadExchangeAmount(out decimal fromAmount, out string amountErrorMessage))
            {
                InvalidateQuote(amountErrorMessage);
                return;
            }

            if (fromAmount > fromWallet.balance)
            {
                InvalidateQuote("Nedostatečný zůstatek pro směnu.");
                return;
            }

            ExchangeRepository exchangeRepository = new ExchangeRepository();

            try
            {
                currentQuote = exchangeRepository.GetExchangeQuote(
                    currentUser.user_id,
                    fromWallet.currencyCode,
                    toWallet.currencyCode,
                    fromAmount
                );
            }
            catch (OracleException ex)
            {
                currentQuote = null;
                ButtonExecuteExchange.IsEnabled = false;
                TextBlockQuoteResult.Text = "Nabídku směny se nepodařilo spočítat.";
                TextBlockQuoteRate.Text = ex.Message;
                return;
            }
            catch (ArgumentException ex)
            {
                currentQuote = null;
                ButtonExecuteExchange.IsEnabled = false;
                TextBlockQuoteResult.Text = ex.Message;
                TextBlockQuoteRate.Text = "";
                return;
            }

            ShowQuote(currentQuote);
        }

        private void ButtonExecuteExchange_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null)
                return;

            Wallet? fromWallet = GetSelectedFromWallet();
            Wallet? toWallet = GetSelectedToWallet();

            if (fromWallet == null || toWallet == null)
            {
                MessageBox.Show("Vyberte obě měny pro směnu.");
                return;
            }

            if (!TryReadExchangeAmount(out decimal fromAmount))
                return;

            ExchangeRepository exchangeRepository = new ExchangeRepository();
            ExchangeExecutionResult result;

            try
            {
                currentQuote = exchangeRepository.GetExchangeQuote(
                    currentUser.user_id,
                    fromWallet.currencyCode,
                    toWallet.currencyCode,
                    fromAmount
                );

                ShowQuote(currentQuote);

                result = exchangeRepository.ExecuteExchange(
                    currentUser.user_id,
                    currentQuote.FromCurrencyCode,
                    currentQuote.ToCurrencyCode,
                    currentQuote.FromAmount,
                    currentQuote.RateId
                );
            }
            catch (OracleException ex)
            {
                MessageBox.Show("Směnu se nepodařilo provést.\n\n" + ex.Message);
                ScheduleQuoteRefresh();
                return;
            }


            TextBoxFromAmount.Text = "0";
            Refresh();
            OnExchangeCompleted();
        }

        private bool TryReadExchangeAmount(out decimal amount)
        {
            string errorMessage;

            if (TryReadExchangeAmount(out amount, out errorMessage))
                return true;

            MessageBox.Show(errorMessage);
            return false;
        }

        private bool TryReadExchangeAmount(out decimal amount, out string errorMessage)
        {
            errorMessage = "";

            if (!decimal.TryParse(TextBoxFromAmount.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out amount)
                && !decimal.TryParse(TextBoxFromAmount.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out amount))
            {
                errorMessage = "Zadejte platnou částku.";
                return false;
            }

            if (amount <= 0)
            {
                errorMessage = "Částka směny musí být větší než 0.";
                return false;
            }

            if (decimal.Round(amount, 8) != amount)
            {
                errorMessage = "Částka může mít maximálně 8 desetinných míst.";
                return false;
            }

            if (amount > MaxWalletAmount)
            {
                errorMessage = "Částka je příliš vysoká.";
                return false;
            }

            return true;
        }

        private Wallet? GetSelectedFromWallet()
        {
            return ComboBoxFromCurrency.SelectedItem as Wallet;
        }

        private Wallet? GetSelectedToWallet()
        {
            return ComboBoxToCurrency.SelectedItem as Wallet;
        }

        private void UpdateSelectedBalanceText()
        {
            Wallet? fromWallet = GetSelectedFromWallet();

            if (fromWallet == null)
            {
                TextBlockFromBalance.Text = "";
                return;
            }

            TextBlockFromBalance.Text = "Dostupný zůstatek: " + CurrencyAmountFormatter.Format(fromWallet.balance, fromWallet.currencyCode) + " " + fromWallet.currencyCode;
        }

        private void InvalidateQuote()
        {
            InvalidateQuote("Zadejte částku pro směnu.");
        }

        private void InvalidateQuote(string message)
        {
            currentQuote = null;
            ButtonExecuteExchange.IsEnabled = false;
            TextBlockQuoteResult.Text = message;
            TextBlockQuoteRate.Text = "";
        }

        private void ShowQuote(ExchangeQuote quote)
        {
            TextBlockQuoteResult.Text =
                CurrencyAmountFormatter.Format(quote.FromAmount, quote.FromCurrencyCode) + " " + quote.FromCurrencyCode
                + " -> "
                + CurrencyAmountFormatter.Format(quote.ToAmount, quote.ToCurrencyCode) + " " + quote.ToCurrencyCode;

            TextBlockQuoteRate.Text =
                "Kurz: " + quote.ExchangeRate.ToString("N8")
                + " | Rate ID: " + quote.RateId;

            ButtonExecuteExchange.IsEnabled = true;
        }

        private void OnExchangeCompleted()
        {
            EventHandler? handler = ExchangeCompleted;

            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}
