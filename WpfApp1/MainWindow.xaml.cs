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
                string query = "SELECT * FROM products ORDER BY id";
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

        private void DeleteByIdButton_Click(object sender, RoutedEventArgs e)
        {
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

            // Подтверждение удаления
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

                        // Очистка поля ID
                        RecordIdTextBox.Text = "";

                        // Обновление таблицы
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
            string idText = RecordIdTextBox.Text.Trim();
            string quantityText = ProductQuantityTextBox.Text.Trim();

            // Проверка введенных данных
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

                    // Очистка полей ввода
                    RecordIdTextBox.Text = "";
                    ProductQuantityTextBox.Text = "";

                    // Обновление таблицы
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
