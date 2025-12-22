using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace WpfApp1
{
    public class DatabaseHelper
    {
        private string connectionString;

        public DatabaseHelper(string server, string database, string username, string password)
        {
            connectionString = $"Server={server};Database={database};Uid={username};Pwd={password};";
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
            catch
            {
                return false;
            }
        }
    }
}