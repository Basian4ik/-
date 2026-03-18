using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace БаязитовЛангуге
{
    /// <summary>
    /// Логика взаимодействия для ClientPage.xaml
    /// </summary>
    public partial class ClientPage : Page
    {
        int CountRecords;
        int CountPage;
        int CurrentPage = 0;
        List<Client> CurrentPageList = new List<Client>();
        List<Client> TableList;
        public ClientPage()
        {
            InitializeComponent();
            var currenClients = БаязитовLanguageEntities.GetContext().Client.ToList();
            ClientListView.ItemsSource = currenClients;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage());
        }

        private void LeftDirButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RightDirButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
