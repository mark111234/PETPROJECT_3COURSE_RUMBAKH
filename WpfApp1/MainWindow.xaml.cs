using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace YourNamespace
{
    public partial class MainWindow : Window
    {
        private DatabaseHelper databaseHelper;

        public MainWindow()
        {
            InitializeComponent();
            databaseHelper = new DatabaseHelper();

            // Инициализация базы данных
            InitializeDatabase();

            // Загрузка данных при старте
            LoadProductsData();
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
                string query = "SELECT * FROM products ORDER BY name";
                DataTable dataTable = databaseHelper.ExecuteQuery(query);
                ProductsDataGrid.ItemsSource = dataTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            string name = ProductNameTextBox.Text.Trim();
            string quantityText = ProductQuantityTextBox.Text.Trim();

            // Проверка введенных данных
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

                    // Очистка полей ввода
                    ProductNameTextBox.Text = "";
                    ProductQuantityTextBox.Text = "";

                    // Обновление таблицы
                    LoadProductsData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления товара: {ex.Message}");
            }
        }

        private void ClearAllButton_Click(object sender, RoutedEventArgs e)
        {
            // Подтверждение удаления
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

                    // Обновление таблицы
                    LoadProductsData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления записей: {ex.Message}");
                }
            }
        }

        // Автоматическое обновление данных при переключении на вкладку товаров
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                LoadProductsData();
            }
        }
    }
}
