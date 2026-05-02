using KryptoSmenarna.Wpf.Models;
using System.Windows.Controls;

namespace KryptoSmenarna.Wpf.UserSubWindows
{
    /// <summary>
    /// Interakční logika pro ExchangeSubWindow.xaml
    /// </summary>
    public partial class ExchangeSubWindow : UserControl
    {
        private User? currentUser;

        public ExchangeSubWindow()
        {
            InitializeComponent();
        }

        public void Initialize(User user)
        {
            currentUser = user;
        }
    }
}
