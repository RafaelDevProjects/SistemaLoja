using Microsoft.Data.SqlClient;

namespace SistemaLoja.Lab12_ConexaoSQLServer;

public class DatabaseConnection
{
    // TODO: Complete a connection string com os dados corretos
    private static string connectionString = 
        "Server=localhost,1433;" +
        "Database=SistemaLojaDB;" +
        "User Id=SistemaLojaDB;" +
        "Password=SqlServer2024!;" +
        "TrustServerCertificate=True;";

    public static SqlConnection GetConnection()
    {
        return new SqlConnection(connectionString);
    }
}