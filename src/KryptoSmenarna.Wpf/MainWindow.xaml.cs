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
        TextBlock_VybranyUzivatel.Text = "Vybraný Uživatel: " + selectedUser;
        TextBlock_userEmail.Text = "email: " + selectedUser.email;
        UserBorder.Visibility = Visibility.Visible;
    }
    private void LoginUser()
    {

    }

    private void ListBoxUsers_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        selectedUser = (User)ListBoxUsers.SelectedItem;
        ShowSelectedUserInfo();
    }

    private void Button_Login_Click(object sender, RoutedEventArgs e)
    {
        LoginUser();
    }
}