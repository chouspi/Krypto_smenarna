using KryptoSmenarna.Wpf.Models;
using KryptoSmenarna.Wpf.Data;
using KryptoSmenarna.Wpf.Models.TransactionHistory;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace KryptoSmenarna.Wpf.UserSubWindows
{
    /// <summary>
    /// Interakční logika pro TransactionHistorySubWindow.xaml
    /// </summary>
    public partial class TransactionHistorySubWindow : UserControl
    {
        private const int DefaultHistoryDays = 30;
        private User? currentUser;
        private List<ITransactionHistoryItem> transactionHistoryItems = new List<ITransactionHistoryItem>();

        public IReadOnlyList<ITransactionHistoryItem> TransactionHistoryItems => transactionHistoryItems;

        public TransactionHistorySubWindow()
        {
            InitializeComponent();
        }

        public void Initialize(User user)
        {
            currentUser = user;
            LoadTransactionHistory(DefaultHistoryDays);
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
            TextBlockEmptyHistory.Visibility = transactionHistoryItems.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }
}
