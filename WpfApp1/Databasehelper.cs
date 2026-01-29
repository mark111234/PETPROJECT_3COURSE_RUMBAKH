using MySql.Data.MySqlClient;
using System;
using System.Data;

public class DatabaseHelper
{
    private string connectionString;

    public DatabaseHelper()
    {
        string server = "localhost";
        string database = "markdb";
        string username = "root";
        string password = "1234";

        connectionString = $"Server={server};Database={database};Uid={username};Pwd={password};" +
                         "AllowUserVariables=True;SslMode=Preferred;";
    }
    public string GetConnectionString()
    {
        return connectionString;
    }

    public bool TestConnection()
    {
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка подключения: {ex.Message}");
            return false;
        }
    }

    public DataTable ExecuteQuery(string query)
    {
        DataTable dataTable = new DataTable();

        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new MySqlCommand(query, connection))
                {
                    using (var adapter = new MySqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка выполнения запроса: {ex.Message}");
        }

        return dataTable;
    }

    public int ExecuteNonQuery(string query)
    {
        int rowsAffected = 0;

        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new MySqlCommand(query, connection))
                {
                    rowsAffected = command.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка выполнения команды: {ex.Message}");
        }

        return rowsAffected;
    }

    public object ExecuteScalar(string query)
    {
        object result = null;

        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new MySqlCommand(query, connection))
                {
                    result = command.ExecuteScalar();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка выполнения скалярного запроса: {ex.Message}");
        }

        return result;
    }
}
