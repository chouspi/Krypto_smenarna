using KryptoSmenarna.Wpf.Models;
using KryptoSmenarna.Wpf.Data;
using KryptoSmenarna.Wpf.Models.TransactionHistory;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace KryptoSmenarna.Wpf.UserSubWindows
{
    /// <summary>
    /// Interakční logika pro TransactionHistorySubWindow.xaml
    /// </summary>
    public partial class TransactionHistorySubWindow : UserControl
    {
        private const int DefaultHistoryDays = 30;
        private User? currentUser;
        private int selectedHistoryDays = DefaultHistoryDays;
        private List<ITransactionHistoryItem> transactionHistoryItems = new List<ITransactionHistoryItem>();

        public IReadOnlyList<ITransactionHistoryItem> TransactionHistoryItems => transactionHistoryItems;

        public TransactionHistorySubWindow()
        {
            InitializeComponent();
        }

        public void Initialize(User user)
        {
            currentUser = user;
            LoadTransactionHistory(selectedHistoryDays);
            UpdateRangeButtons();
        }

        public void Refresh()
        {
            LoadTransactionHistory(selectedHistoryDays);
        }

        private void LoadTransactionHistory(int days)
        {
            if (currentUser == null)
                return;

            try
            {
                TransactionHistoryRepository repository = new TransactionHistoryRepository();
                transactionHistoryItems = repository.GetTransactionHistoryItemsForDays(currentUser.user_id, days);
            }
            catch (OracleException ex)
            {
                transactionHistoryItems = new List<ITransactionHistoryItem>();
                MessageBox.Show("Transakční historii se nepodařilo načíst.\n\n" + ex.Message);
            }

            ListBoxTransactionHistory.ItemsSource = transactionHistoryItems;

            if (transactionHistoryItems.Count == 0)
            {
                TextBlockEmptyHistory.Visibility = Visibility.Visible;
            }
            else
            {
                TextBlockEmptyHistory.Visibility = Visibility.Collapsed;
            }

            UpdateHistoryRangeText(days);
        }

        private void ButtonHistoryDay_Click(object sender, RoutedEventArgs e)
        {
            SetHistoryRange(1);
        }

        private void ButtonHistoryWeek_Click(object sender, RoutedEventArgs e)
        {
            SetHistoryRange(7);
        }

        private void ButtonHistoryMonth_Click(object sender, RoutedEventArgs e)
        {
            SetHistoryRange(30);
        }

        private void ButtonHistoryYear_Click(object sender, RoutedEventArgs e)
        {
            SetHistoryRange(365);
        }

        private void SetHistoryRange(int days)
        {
            selectedHistoryDays = days;
            LoadTransactionHistory(selectedHistoryDays);
            UpdateRangeButtons();
        }

        private void UpdateHistoryRangeText(int days)
        {
            string rangeText = days switch
            {
                1 => "poslední den",
                7 => "posledních 7 dní",
                30 => "poslední měsíc",
                365 => "poslední rok",
                _ => "posledních " + days + " dní"
            };

            TextBlockHistoryRange.Text = "Transakce za " + rangeText;
        }

        private void UpdateRangeButtons()
        {
            UpdateRangeButton(ButtonHistoryDay, selectedHistoryDays == 1);
            UpdateRangeButton(ButtonHistoryWeek, selectedHistoryDays == 7);
            UpdateRangeButton(ButtonHistoryMonth, selectedHistoryDays == 30);
            UpdateRangeButton(ButtonHistoryYear, selectedHistoryDays == 365);
        }

        private void UpdateRangeButton(Button button, bool isSelected)
        {
            if (isSelected)
            {
                button.Background = new SolidColorBrush(Color.FromRgb(79, 70, 229));
                button.Foreground = Brushes.White;
            }
            else
            {
                button.Background = new SolidColorBrush(Color.FromRgb(249, 250, 251));
                button.Foreground = new SolidColorBrush(Color.FromRgb(55, 65, 81));
            }

            button.BorderBrush = new SolidColorBrush(Color.FromRgb(209, 213, 219));
        }
    }
}
