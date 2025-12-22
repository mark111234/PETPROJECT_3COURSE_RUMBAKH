using System.Windows;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private DatabaseHelper dbHelper;

        public MainWindow()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
        }

        private void btnTestConnection_Click(object sender, RoutedEventArgs e)
        {
            if (dbHelper.TestConnection())
            {
                txtStatus.Text = "Подключение к MySQL серверу успешно!";
            }
            else
            {
                txtStatus.Text = "Не удалось подключиться к MySQL серверу";
            }
        }

        private void btnCreateDB_Click(object sender, RoutedEventArgs e)
        {
            dbHelper.InitializeDatabase();
            txtStatus.Text = "База данных и таблица созданы (если не существовали)";
        }

        private void btnAddNewUser_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtAge.Text))
            {
                txtStatus.Text = "Заполните все поля!";
                return;
            }

            if (!int.TryParse(txtAge.Text, out int age))
            {
                txtStatus.Text = "Введите корректный возраст!";
                return;
            }

            dbHelper.AddUser(txtName.Text, txtEmail.Text, age);

            // Очистка полей
            txtName.Text = "";
            txtEmail.Text = "";
            txtAge.Text = "";
        }

        private void btnShowUsers_Click(object sender, RoutedEventArgs e)
        {
            dbHelper.GetAllUsers();
            txtStatus.Text = "Данные пользователей выведены в консоль (View → Output)";
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearchEmail.Text))
            {
                txtStatus.Text = "Введите email для поиска!";
                return;
            }

            dbHelper.FindUserByEmail(txtSearchEmail.Text);
            txtSearchEmail.Text = "";
        }
    }
}