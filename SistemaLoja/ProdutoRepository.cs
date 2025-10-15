using Microsoft.Data.SqlClient;

namespace SistemaLoja.Lab12_ConexaoSQLServer;

public class ProdutoRepository
{
    // EXERCÍCIO 1: Listar todos os produtos
    public void ListarTodosProdutos()
    {
        string sql = "SELECT * FROM Produtos";

        using (SqlConnection conn = DatabaseConnection.GetConnection())
        {
            conn.Open();

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    Console.WriteLine("\n=== LISTA DE PRODUTOS ===");
                    Console.WriteLine("ID\tNome\t\tPreço\tEstoque");
                    Console.WriteLine("----------------------------------------");

                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader["Id"]}\t{reader["Nome"]}\t\t{reader["Preco"]:C}\t{reader["Estoque"]}");
                    }
                }
            }
        }
    }

    // EXERCÍCIO 2: Inserir novo produto
    public void InserirProduto(Produto produto)
    {
        string sql = "INSERT INTO Produtos (Nome, Preco, Estoque, CategoriaId) " +
                     "VALUES (@Nome, @Preco, @Estoque, @CategoriaId)";

        using (SqlConnection conn = DatabaseConnection.GetConnection())
        {
            conn.Open();

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Nome", produto.Nome);
                cmd.Parameters.AddWithValue("@Preco", produto.Preco);
                cmd.Parameters.AddWithValue("@Estoque", produto.Estoque);
                cmd.Parameters.AddWithValue("@CategoriaId", produto.CategoriaId);

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                    Console.WriteLine("Produto inserido com sucesso!");
                else
                    Console.WriteLine("Erro ao inserir produto!");
            }
        }
    }

    // EXERCÍCIO 3: Atualizar produto
    public void AtualizarProduto(Produto produto)
    {
        string sql = "UPDATE Produtos SET " +
                     "Nome = @Nome, " +
                     "Preco = @Preco, " +
                     "Estoque = @Estoque, " +
                     "CategoriaId = @CategoriaId " +
                     "WHERE Id = @Id";

        using (SqlConnection conn = DatabaseConnection.GetConnection())
        {
            conn.Open();

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Nome", produto.Nome);
                cmd.Parameters.AddWithValue("@Preco", produto.Preco);
                cmd.Parameters.AddWithValue("@Estoque", produto.Estoque);
                cmd.Parameters.AddWithValue("@CategoriaId", produto.CategoriaId);
                cmd.Parameters.AddWithValue("@Id", produto.Id);

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                    Console.WriteLine("Produto atualizado com sucesso!");
                else
                    Console.WriteLine("Produto não encontrado!");
            }
        }
    }

    // EXERCÍCIO 4: Deletar produto
    public void DeletarProduto(int id)
    {
        // Primeiro verifica se há pedidos vinculados
        string sqlVerifica = "SELECT COUNT(*) FROM PedidoItens WHERE ProdutoId = @Id";
        string sqlDelete = "DELETE FROM Produtos WHERE Id = @Id";

        using (SqlConnection conn = DatabaseConnection.GetConnection())
        {
            conn.Open();

            // Verifica se há pedidos vinculados
            using (SqlCommand cmdVerifica = new SqlCommand(sqlVerifica, conn))
            {
                cmdVerifica.Parameters.AddWithValue("@Id", id);
                int count = (int)cmdVerifica.ExecuteScalar();

                if (count > 0)
                {
                    Console.WriteLine("❌ Não é possível deletar o produto pois existem pedidos vinculados a ele!");
                    return;
                }
            }

            // Deleta o produto
            using (SqlCommand cmdDelete = new SqlCommand(sqlDelete, conn))
            {
                cmdDelete.Parameters.AddWithValue("@Id", id);

                int rowsAffected = cmdDelete.ExecuteNonQuery();

                if (rowsAffected > 0)
                    Console.WriteLine("Produto deletado com sucesso!");
                else
                    Console.WriteLine("Produto não encontrado!");
            }
        }
    }

    // EXERCÍCIO 5: Buscar produto por ID
    public Produto BuscarPorId(int id)
    {
        string sql = "SELECT * FROM Produtos WHERE Id = @Id";
        Produto produto = null;

        using (SqlConnection conn = DatabaseConnection.GetConnection())
        {
            conn.Open();

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        produto = new Produto
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Nome = reader["Nome"].ToString(),
                            Preco = Convert.ToDecimal(reader["Preco"]),
                            Estoque = Convert.ToInt32(reader["Estoque"]),
                            CategoriaId = Convert.ToInt32(reader["CategoriaId"])
                        };
                    }
                }
            }
        }

        return produto;
    }

    // EXERCÍCIO 6: Listar produtos por categoria
    public void ListarProdutosPorCategoria(int categoriaId)
    {
        string sql = @"SELECT p.*, c.Nome as NomeCategoria 
                          FROM Produtos p
                          INNER JOIN Categorias c ON p.CategoriaId = c.Id
                          WHERE p.CategoriaId = @CategoriaId";

        using (SqlConnection conn = DatabaseConnection.GetConnection())
        {
            conn.Open();

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@CategoriaId", categoriaId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    Console.WriteLine($"\n=== PRODUTOS DA CATEGORIA ===");
                    Console.WriteLine("ID\tNome\t\tPreço\tEstoque\tCategoria");
                    Console.WriteLine("------------------------------------------------");

                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader["Id"]}\t{reader["Nome"]}\t\t{reader["Preco"]:C}\t{reader["Estoque"]}\t{reader["NomeCategoria"]}");
                    }
                }
            }
        }
    }

    // DESAFIO 1: Buscar produtos com estoque baixo
    public void ListarProdutosEstoqueBaixo(int quantidadeMinima)
    {
        string sql = "SELECT * FROM Produtos WHERE Estoque < @QuantidadeMinima";

        using (SqlConnection conn = DatabaseConnection.GetConnection())
        {
            conn.Open();

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@QuantidadeMinima", quantidadeMinima);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    Console.WriteLine($"\n⚠️  PRODUTOS COM ESTOQUE BAIXO (menos de {quantidadeMinima} unidades)");
                    Console.WriteLine("ID\tNome\t\tPreço\tEstoque");
                    Console.WriteLine("----------------------------------------");

                    bool temProdutos = false;
                    while (reader.Read())
                    {
                        Console.WriteLine($"🚨 {reader["Id"]}\t{reader["Nome"]}\t\t{reader["Preco"]:C}\t{reader["Estoque"]}");
                        temProdutos = true;
                    }

                    if (!temProdutos)
                        Console.WriteLine("✅ Nenhum produto com estoque baixo!");
                }
            }
        }
    }

    // DESAFIO 2: Buscar produtos por nome (LIKE)
    public void BuscarProdutosPorNome(string termoBusca)
    {
        string sql = "SELECT * FROM Produtos WHERE Nome LIKE @TermoBusca";

        using (SqlConnection conn = DatabaseConnection.GetConnection())
        {
            conn.Open();

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@TermoBusca", $"%{termoBusca}%");

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    Console.WriteLine($"\n🔍 RESULTADOS DA BUSCA: '{termoBusca}'");
                    Console.WriteLine("ID\tNome\t\tPreço\tEstoque");
                    Console.WriteLine("----------------------------------------");

                    bool encontrou = false;
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader["Id"]}\t{reader["Nome"]}\t\t{reader["Preco"]:C}\t{reader["Estoque"]}");
                        encontrou = true;
                    }

                    if (!encontrou)
                        Console.WriteLine("Nenhum produto encontrado!");
                }
            }
        }
    }
}