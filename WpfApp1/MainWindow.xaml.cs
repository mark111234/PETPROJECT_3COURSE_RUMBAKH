using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private DatabaseHelper dbHelper;

        public MainWindow()
        {
            InitializeComponent();

            // Укажите свои параметры подключения к БД
            string server = "localhost";
            string database = "mydatabase";
            string username = "root";
            string password = "yourpassword";

            dbHelper = new DatabaseHelper(server, database, username, password);
        }

        // Обработчик для кнопки "Тест подключения"
        private void TestConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var connection = new MySqlConnection(dbHelper.GetConnectionString()))
                {
                    connection.Open();
                    MessageBox.Show("Подключение к MySQL успешно!");
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}");
            }
        }

        // Обработчик для кнопки "Создать БД"
        private void CreateDB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dbHelper.InitializeDatabase();
                MessageBox.Show("База данных инициализирована");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании БД: {ex.Message}");
            }
        }

        // Обработчик для кнопки "Добавить пользователя"
        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtAge.Text))
            {
                MessageBox.Show("Заполните все поля");
                return;
            }

            if (!int.TryParse(txtAge.Text, out int age))
            {
                MessageBox.Show("Введите корректный возраст");
                return;
            }

            bool success = dbHelper.AddUser(txtName.Text, txtEmail.Text, age);

            if (success)
            {
                MessageBox.Show("Пользователь добавлен");
                txtName.Clear();
                txtEmail.Clear();
                txtAge.Clear();
            }
            else
            {
                MessageBox.Show("Ошибка при добавлении пользователя");
            }
        }

        // Обработчик для кнопки "Поиск"
        private void SearchUser_Click(object sender, RoutedEventArgs e)
        {
            string email = txtSearchEmail.Text;

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Введите email для поиска");
                return;
            }

            var user = dbHelper.FindUserByEmail(email);

            if (user != null)
            {
                MessageBox.Show($"Найден пользователь: {user.Name}, {user.Email}, {user.Age} лет");
            }
            else
            {
                MessageBox.Show("Пользователь не найден");
            }
        }

        // Обработчик для кнопки "Показать всех пользователей"
        private void ShowUsers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var users = dbHelper.GetAllUsers();
                usersListBox.ItemsSource = users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}");
            }
        }
    }
}
