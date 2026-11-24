using MySql.Data.MySqlClient;

namespace WinFormsApp1.DAL
{
    /// <summary>
    /// MySQL өгөгдлийн сангийн холболт үүсгэх туслах класс (DAL)
    /// </summary>
    public static class MySqlDbHelper
    {
        public static string ConnectionString { get; } =
            "Server=localhost;Database=haais_db;Uid=root;Pwd=;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }
    }
}