using MySql.Data.MySqlClient;
using System;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace YourNamespace
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private DatabaseHelper databaseHelper;
        private bool isAdminAuthenticated = false;

        public bool IsAdminAuthenticated
        {
            get { return isAdminAuthenticated; }
            set
            {
                isAdminAuthenticated = value;
                OnPropertyChanged(nameof(IsAdminAuthenticated));
                OnPropertyChanged(nameof(AdminPanelLockVisibility));
            }
        }

        public Visibility AdminPanelLockVisibility
        {
            get { return IsAdminAuthenticated ? Visibility.Collapsed : Visibility.Visible; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            databaseHelper = new DatabaseHelper();

            this.DataContext = this;

            // Инициализация базы данных
            InitializeDatabase();

            // Загрузка данных при старте
            LoadProductsData();

            // Блокировка админ панели по умолчанию
            IsAdminAuthenticated = false;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void InitializeDatabase()
        {
            // Создаем таблицу товаров если ее нет
            string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS products (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    name VARCHAR(255) NOT NULL,
                    quantity INT NOT NULL DEFAULT 0,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                )";

            databaseHelper.ExecuteNonQuery(createTableQuery);
        }

        private void LoadProductsData()
        {
            try
            {
                string query = "SELECT * FROM products ORDER BY id";
                DataTable dataTable = databaseHelper.ExecuteQuery(query);
                ProductsDataGrid.ItemsSource = dataTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string password = PasswordBox.Password;

            if (password == "1234")
            {
                IsAdminAuthenticated = true;
                AuthStatusText.Text = "Авторизация успешна!";
                AuthStatusText.Foreground = System.Windows.Media.Brushes.Green;

                // Переход на админ панель после успешной авторизации
                MainTabControl.SelectedItem = AdminTabItem;
            }
            else
            {
                IsAdminAuthenticated = false;
                AuthStatusText.Text = "Неверный пароль!";
                AuthStatusText.Foreground = System.Windows.Media.Brushes.Red;
                PasswordBox.Password = "";
            }
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsAdminAuthenticated)
            {
                MessageBox.Show("Требуется авторизация!");
                MainTabControl.SelectedItem = AdminTabItem;
                return;
            }

            string name = ProductNameTextBox.Text.Trim();
            string quantityText = ProductQuantityTextBox.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Введите название товара!");
                return;
            }

            if (!int.TryParse(quantityText, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Введите корректное количество товара!");
                return;
            }

            try
            {
                // Вставка новой записи
                string insertQuery = $@"
                    INSERT INTO products (name, quantity) 
                    VALUES ('{name}', {quantity})";

                int rowsAffected = databaseHelper.ExecuteNonQuery(insertQuery);

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Товар успешно добавлен!");

                    ProductNameTextBox.Text = "";
                    ProductQuantityTextBox.Text = "";

                    LoadProductsData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления товара: {ex.Message}");
            }
        }

        private void DeleteByIdButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsAdminAuthenticated)
            {
                MessageBox.Show("Требуется авторизация!");
                MainTabControl.SelectedItem = AdminTabItem;
                return;
            }

            string idText = RecordIdTextBox.Text.Trim();

            if (!int.TryParse(idText, out int id) || id <= 0)
            {
                MessageBox.Show("Введите корректный ID записи!");
                return;
            }

            // Проверка существования записи
            string checkQuery = $"SELECT COUNT(*) FROM products WHERE id = {id}";
            int recordCount = Convert.ToInt32(databaseHelper.ExecuteScalar(checkQuery));

            if (recordCount == 0)
            {
                MessageBox.Show($"Запись с ID {id} не найдена!");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"Вы уверены, что хотите удалить запись с ID {id}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    string deleteQuery = $"DELETE FROM products WHERE id = {id}";
                    int rowsAffected = databaseHelper.ExecuteNonQuery(deleteQuery);

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show($"Запись с ID {id} успешно удалена!");

                        RecordIdTextBox.Text = "";

                        LoadProductsData();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления записи: {ex.Message}");
                }
            }
        }

        private void UpdateQuantityButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsAdminAuthenticated)
            {
                MessageBox.Show("Требуется авторизация!");
                MainTabControl.SelectedItem = AdminTabItem;
                return;
            }

            string idText = RecordIdTextBox.Text.Trim();
            string quantityText = ProductQuantityTextBox.Text.Trim();

            if (!int.TryParse(idText, out int id) || id <= 0)
            {
                MessageBox.Show("Введите корректный ID записи!");
                return;
            }

            if (!int.TryParse(quantityText, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Введите корректное количество товара!");
                return;
            }

            // Проверка существования записи
            string checkQuery = $"SELECT COUNT(*) FROM products WHERE id = {id}";
            int recordCount = Convert.ToInt32(databaseHelper.ExecuteScalar(checkQuery));

            if (recordCount == 0)
            {
                MessageBox.Show($"Запись с ID {id} не найдена!");
                return;
            }

            try
            {
                string updateQuery = $@"
                    UPDATE products 
                    SET quantity = {quantity} 
                    WHERE id = {id}";

                int rowsAffected = databaseHelper.ExecuteNonQuery(updateQuery);

                if (rowsAffected > 0)
                {
                    MessageBox.Show($"Количество товара для записи ID {id} успешно обновлено!");

                    RecordIdTextBox.Text = "";
                    ProductQuantityTextBox.Text = "";

                    LoadProductsData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления количества: {ex.Message}");
            }
        }

        private void ClearAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsAdminAuthenticated)
            {
                MessageBox.Show("Требуется авторизация!");
                MainTabControl.SelectedItem = AdminTabItem;
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Вы уверены, что хотите удалить ВСЕ записи? Это действие нельзя отменить!",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    string deleteQuery = "DELETE FROM products";
                    int rowsAffected = databaseHelper.ExecuteNonQuery(deleteQuery);

                    MessageBox.Show($"Удалено записей: {rowsAffected}");

                    LoadProductsData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления записей: {ex.Message}");
                }
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                LoadProductsData();
            }
        }

        private void BulkInsertButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsAdminAuthenticated)
            {
                MessageBox.Show("Требуется авторизация!");
                MainTabControl.SelectedItem = AdminTabItem;
                return;
            }

            // Собираем данные из всех полей
            var productsToAdd = new List<(string name, int quantity)>();

            // Получаем ссылки на все текстовые поля
            TextBox[] nameBoxes = {
                BulkProductName1, BulkProductName2, BulkProductName3, BulkProductName4, BulkProductName5,
                BulkProductName6, BulkProductName7, BulkProductName8, BulkProductName9, BulkProductName10
            };

            TextBox[] quantityBoxes = {
                BulkProductQuantity1, BulkProductQuantity2, BulkProductQuantity3, BulkProductQuantity4, BulkProductQuantity5,
                BulkProductQuantity6, BulkProductQuantity7, BulkProductQuantity8, BulkProductQuantity9, BulkProductQuantity10
            };

            // Проверяем и собираем данные
            int validRecords = 0;
            int totalRecords = 0;

            for (int i = 0; i < nameBoxes.Length; i++)
            {
                string name = nameBoxes[i].Text.Trim();
                string quantityText = quantityBoxes[i].Text.Trim();

                // Пропускаем пустые названия
                if (string.IsNullOrEmpty(name))
                    continue;

                totalRecords++;

                if (!int.TryParse(quantityText, out int quantity) || quantity < 0)
                {
                    MessageBox.Show($"Некорректное количество в строке {i + 1}! Используется значение 0.");
                    quantity = 0;
                }

                productsToAdd.Add((name, quantity));
                validRecords++;
            }

            if (validRecords == 0)
            {
                MessageBox.Show("Нет данных для добавления! Заполните хотя бы одно поле с названием товара.");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"Вы уверены, что хотите добавить {validRecords} записей из {totalRecords} заполненных?",
                "Подтверждение массового добавления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    int successfullyAdded = 0;

                    // Используем транзакцию для более быстрого добавления
                    using (var connection = new MySqlConnection(databaseHelper.GetConnectionString()))
                    {
                        connection.Open();
                        using (var transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                foreach (var product in productsToAdd)
                                {
                                    string insertQuery = $@"
                                        INSERT INTO products (name, quantity) 
                                        VALUES ('{product.name.Replace("'", "''")}', {product.quantity})";

                                    using (var command = new MySqlCommand(insertQuery, connection, transaction))
                                    {
                                        if (command.ExecuteNonQuery() > 0)
                                        {
                                            successfullyAdded++;
                                        }
                                    }
                                }

                                transaction.Commit();
                            }
                            catch
                            {
                                transaction.Rollback();
                                throw;
                            }
                        }
                    }

                    // Очищаем поля после успешного добавления
                    if (successfullyAdded > 0)
                    {
                        foreach (var box in nameBoxes)
                        {
                            box.Text = "";
                        }
                        foreach (var box in quantityBoxes)
                        {
                            box.Text = "0";
                        }

                        BulkInsertStatus.Text = $"Успешно добавлено записей: {successfullyAdded}";
                        BulkInsertStatus.Foreground = System.Windows.Media.Brushes.Green;

                        // Обновляем данные в таблице
                        LoadProductsData();
                    }
                    else
                    {
                        BulkInsertStatus.Text = "Не удалось добавить ни одной записи";
                        BulkInsertStatus.Foreground = System.Windows.Media.Brushes.Red;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка массового добавления: {ex.Message}");
                    BulkInsertStatus.Text = $"Ошибка: {ex.Message}";
                    BulkInsertStatus.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
        }

    }
}
