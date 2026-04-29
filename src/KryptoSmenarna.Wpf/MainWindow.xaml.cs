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
    List<User> AllUsers = new List<User>();
    private User selectedUser;
    public MainWindow()
    {
        InitializeComponent();
        AllUsers = new UsersRepository().GetAllUsers();
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
        new UserWindow(selectedUser).Show(); 
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

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        new AllDataOperationsRepository().DeleteAllData();
        AllUsers = new UsersRepository().GetAllUsers();

        ListBoxUsers.ItemsSource = null;
        ListBoxUsers.ItemsSource = AllUsers;

        selectedUser = null;

        TextBlock_VybranyUzivatel.Text = "";
        TextBlock_userEmail.Text = "";
        UserBorder.Visibility = Visibility.Hidden;
    }

    private void Button_Click_1(object sender, RoutedEventArgs e)
    {
        new AllDataOperationsRepository().InsertTestData();
        AllUsers = new UsersRepository().GetAllUsers();

        ListBoxUsers.ItemsSource = null;
        ListBoxUsers.ItemsSource = AllUsers;
    }
}