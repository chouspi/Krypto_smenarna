using KryptoSmenarna.Wpf.Models;
using System.Windows;

namespace KryptoSmenarna.Wpf
{
    /// <summary>
    /// Interakční logika pro UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        private readonly User currentUser;

        public UserWindow(User user)
        {
            currentUser = user;
            InitializeComponent();

            WalletOperations.Initialize(currentUser);
            TransactionHistory.Initialize(currentUser);
            Exchange.Initialize(currentUser);

            WalletOperations.WalletOperationCompleted += WalletOperations_WalletOperationCompleted;
            Exchange.ExchangeCompleted += Exchange_ExchangeCompleted;
        }

        private void WalletOperations_WalletOperationCompleted(object? sender, System.EventArgs e)
        {
            TransactionHistory.Refresh();
            Exchange.Refresh();
        }

        private void Exchange_ExchangeCompleted(object? sender, System.EventArgs e)
        {
            WalletOperations.Refresh();
            TransactionHistory.Refresh();
        }
    }
}
