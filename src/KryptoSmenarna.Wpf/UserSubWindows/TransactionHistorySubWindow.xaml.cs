using KryptoSmenarna.Wpf.Models;
using System.Windows.Controls;

namespace KryptoSmenarna.Wpf.UserSubWindows
{
    /// <summary>
    /// Interakční logika pro TransactionHistorySubWindow.xaml
    /// </summary>
    public partial class TransactionHistorySubWindow : UserControl
    {
        private User? currentUser;

        public TransactionHistorySubWindow()
        {
            InitializeComponent();
        }

        public void Initialize(User user)
        {
            currentUser = user;
        }
    }
}
