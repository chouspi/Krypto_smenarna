using KryptoSmenarna.Wpf.Data;
using KryptoSmenarna.Wpf.Models;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KryptoSmenarna.Wpf;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private User selectedUser;
    public MainWindow()
    {
        InitializeComponent();
        List<User> AllUsers = new UsersRepository().GetAllUsers();
        ListBoxUsers.ItemsSource = AllUsers;
    }

    private void ShowSelectedUserInfo()
    {
        Label_VybranyUzivatel.Visibility = Visibility.Visible;
        Label_VybranyUzivatel.Content = "Vybraný Uživatel: " + selectedUser;
        Label_userEmail.Visibility = Visibility.Visible;
        Label_userEmail.Content = "email: " + selectedUser.email;
    }

    private void ListBoxUsers_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        selectedUser = (User)ListBoxUsers.SelectedItem;
        ShowSelectedUserInfo();
    }
}