using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Text.Json;

public class ConexionDB
{
    private static string connectionString;

    static ConexionDB()
    {
        try {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            if (File.Exists(path)) {
                var json = File.ReadAllText(path);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("ConnectionStrings", out var cs) && cs.TryGetProperty("DefaultConnection", out var def)) {
                    connectionString = def.GetString();
                }
            }
        } catch { }

        if (string.IsNullOrEmpty(connectionString)) {
            connectionString = "Server=localhost;Database=escuela_pinto_salinas;User ID=root;Password=;";
        }
    }

    public MySqlConnection GetConnection()
    {
        return new MySqlConnection(connectionString);
    }
}