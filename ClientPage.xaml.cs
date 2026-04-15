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
using System.Data.Entity;

namespace БаязитовЛангуге
{
    /// <summary>
    /// Логика взаимодействия для ClientPage.xaml
    /// </summary>
    public partial class ClientPage : Page
    {
        private List<Client> _filteredClients;
        private List<Client> _allClients;
        private int pageSize = 10;
        private int currentPage = 1;

        public ClientPage()
        {
            InitializeComponent();

            var currentClients = БаязитовLanguageEntities.GetContext().Client
                .Include(c => c.ClientService).ToList();
            ClientListView.ItemsSource = currentClients;

            _filteredClients = currentClients;

            PageListCB.SelectedIndex = 0;

            _allClients = currentClients;
            

            currentPage = 1;
            ChangePage();
        }

        private void ChangePage()
        {
            if (_filteredClients == null)
                return;

            if (_filteredClients.Count == 0)
            {
                ClientListView.ItemsSource = new List<Client>();
                PageListBox.Items.Clear();
                TBCount.Text = "0";
                TBALLRecords.Text = " из 0";
                return;
            }

            int totalPages = (_filteredClients.Count + pageSize - 1) / pageSize;

            PageListBox.Items.Clear();
            for (int i = 1; i <= totalPages; i++)
            {
                PageListBox.Items.Add(i);
            }
            PageListBox.SelectedItem = currentPage;

            var clientsPage = _filteredClients
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ClientListView.ItemsSource = clientsPage;

            TBCount.Text = clientsPage.Count.ToString();
            TBALLRecords.Text = " из " + _filteredClients.Count.ToString();
        }


        private void LeftDirButton_Click(object sender, RoutedEventArgs e)
        {
            if (_filteredClients == null)
                return;
            int totalPages = (_filteredClients.Count + pageSize - 1) / pageSize;
            if (currentPage > 1)
            {
                currentPage--;
                ChangePage();
            }
        }

        private void RightDirButton_Click(object sender, RoutedEventArgs e)
        {
            if (_filteredClients == null)
                return;
            int totalPages = (_filteredClients.Count + pageSize - 1) / pageSize;
            if (currentPage < totalPages)
            {
                currentPage++;
                ChangePage();
            }
        }

        private void PageListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_filteredClients == null)
                return;
            if (PageListBox.SelectedItem is int page && page != currentPage)
            {
                currentPage = page;
                ChangePage();
            }
        }

        private void PageListCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_filteredClients == null)
                return;
            if (PageListCB.SelectedItem is TextBlock textBlock)
            {
                string content = textBlock.Text;
                if (content == "Все")
                {
                    pageSize = _filteredClients.Count > 0 ? _filteredClients.Count : 1;
                }
                else
                {
                    pageSize = int.Parse(content);
                }
                currentPage = 1;
                ChangePage();
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            // получение выбранных клиентов
            var selected = ClientListView.SelectedItems.Cast<Client>().ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("Не выбран ни один клиент.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // проверка есть ли у кого-то посещения
            if (selected.Any(c => c.VisitsCount > 0))
            {
                MessageBox.Show("Удаление невозможно: у некоторых клиентов есть посещения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // подтверждение удаления
            if (MessageBox.Show($"Удалить {selected.Count} клиента(ов)?", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                var context = БаязитовLanguageEntities.GetContext();

                // прикрепляем и удаляем каждого клиента
                foreach (var client in selected)
                {
                    if (context.Entry(client).State == System.Data.Entity.EntityState.Detached)
                        context.Client.Attach(client);
                    context.Client.Remove(client);
                }

                context.SaveChanges();

                // ОБНОВЛЯЕМ оба списка
                _allClients = context.Client
                    .Include(c => c.ClientService)
                    .ToList();

                _filteredClients = _allClients.ToList(); // Важно: копируем, а не ссылку

                // Применяем текущие фильтры и сортировку
                ApplyFiltersAndSort();

                MessageBox.Show("Удаление выполнено.", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClientsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = ClientListView.SelectedItems.Count > 0;
            EditBtn.Visibility = hasSelection ? Visibility.Visible : Visibility.Hidden;
            DeleteBtn.Visibility = hasSelection ? Visibility.Visible : Visibility.Hidden;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFiltersAndSort();
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFiltersAndSort();
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFiltersAndSort();
        }
        private void ApplyFiltersAndSort()
        {
            if (_allClients == null) return;

            // Начинаем со всех клиентов
            var query = _allClients.AsEnumerable();

            // 1. ПОИСК
            string searchText = SearchTextBox.Text?.ToLower() ?? "";
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(c =>
                    (c.LastName != null && c.LastName.ToLower().Contains(searchText)) ||
                    (c.FirstName != null && c.FirstName.ToLower().Contains(searchText)) ||
                    (c.Patronymic != null && c.Patronymic.ToLower().Contains(searchText)) ||
                    (c.Email != null && c.Email.ToLower().Contains(searchText)) ||
                    (c.Phone != null && c.Phone.Contains(searchText)));
            }

            // 2. ФИЛЬТР ПО ПОЛУ (нет/муж/жен)
            switch (FilterComboBox.SelectedIndex)
            {
                case 1: // Мужской
                    query = query.Where(c => c.GenderCode == "м");
                    break;
                case 2: // Женский
                    query = query.Where(c => c.GenderCode == "ж");
                    break;
                    // case 0: "Все" - без фильтрации
            }

            // 3. СОРТИРОВКА
            switch (SortComboBox.SelectedIndex)
            {
                case 1: // Фамилия (А-Я)
                    query = query.OrderBy(c => c.LastName)
                                 .ThenBy(c => c.FirstName)
                                 .ThenBy(c => c.Patronymic);
                    break;

                case 2: // Кол-во посещений (по возрастанию)
                    query = query.OrderByDescending(c => c.VisitsCount);
                    break;

                case 3: // По дате последнего посещения
                    query = query.OrderByDescending(c =>
                    {
                        if (c.LastVisitDate == "нет")
                            return DateTime.MinValue;
                        else
                            return DateTime.Parse(c.LastVisitDate);
                    });
                    break;

                default: // case 0: "Нет" - сортировка по ID
                    query = query.OrderBy(c => c.ID);
                    break;
            }

            // Сохраняем результат
            _filteredClients = query.ToList();

            // Сбрасываем на первую страницу
            currentPage = 1;
            ChangePage();
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible)
            {
                RefreshData();
            }
        }

        // Добавьте новый метод для обновления данных
        private void RefreshData()
        {
            var context = БаязитовLanguageEntities.GetContext();
            _allClients = context.Client
                .Include(c => c.ClientService)
                .ToList();

            ApplyFiltersAndSort(); // Это обновит _filteredClients и перерисует список
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage(null));
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ClientListView.SelectedItem is Client selected)
            {
                Manager.MainFrame.Navigate(new AddEditPage(selected));
            }
        }
    }
}