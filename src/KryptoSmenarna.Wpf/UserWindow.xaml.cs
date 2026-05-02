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
        }
    }
}
