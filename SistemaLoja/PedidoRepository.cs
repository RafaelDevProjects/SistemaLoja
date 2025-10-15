using Microsoft.Data.SqlClient;

namespace SistemaLoja.Lab12_ConexaoSQLServer;

public class PedidoRepository
{
    // EXERCÍCIO 7: Criar pedido com itens (transação)
    public void CriarPedido(Pedido pedido, List<PedidoItem> itens)
    {
        using (SqlConnection conn = DatabaseConnection.GetConnection())
        {
            conn.Open();

            SqlTransaction transaction = conn.BeginTransaction();

            try
            {
                // 1. Inserir pedido e obter ID
                string sqlPedido = "INSERT INTO Pedidos (ClienteId, DataPedido, ValorTotal) " +
                                   "OUTPUT INSERTED.Id " +
                                   "VALUES (@ClienteId, @DataPedido, @ValorTotal)";

                int pedidoId;
                using (SqlCommand cmd = new SqlCommand(sqlPedido, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@ClienteId", pedido.ClienteId);
                    cmd.Parameters.AddWithValue("@DataPedido", pedido.DataPedido);
                    cmd.Parameters.AddWithValue("@ValorTotal", pedido.ValorTotal);

                    pedidoId = (int)cmd.ExecuteScalar();
                }

                // 2. Inserir itens do pedido
                string sqlItem = "INSERT INTO PedidoItens (PedidoId, ProdutoId, Quantidade, PrecoUnitario) " +
                                "VALUES (@PedidoId, @ProdutoId, @Quantidade, @PrecoUnitario)";

                foreach (var item in itens)
                {
                    using (SqlCommand cmd = new SqlCommand(sqlItem, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@PedidoId", pedidoId);
                        cmd.Parameters.AddWithValue("@ProdutoId", item.ProdutoId);
                        cmd.Parameters.AddWithValue("@Quantidade", item.Quantidade);
                        cmd.Parameters.AddWithValue("@PrecoUnitario", item.PrecoUnitario);

                        cmd.ExecuteNonQuery();
                    }

                    // 3. Atualizar estoque
                    string sqlEstoque = "UPDATE Produtos SET Estoque = Estoque - @Quantidade WHERE Id = @ProdutoId";
                    using (SqlCommand cmd = new SqlCommand(sqlEstoque, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@Quantidade", item.Quantidade);
                        cmd.Parameters.AddWithValue("@ProdutoId", item.ProdutoId);
                        cmd.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
                Console.WriteLine($"✅ Pedido #{pedidoId} criado com sucesso!");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"❌ Erro ao criar pedido: {ex.Message}");
                throw;
            }
        }
    }

    // EXERCÍCIO 8: Listar pedidos de um cliente
    public void ListarPedidosCliente(int clienteId)
    {
        string sql = "SELECT * FROM Pedidos WHERE ClienteId = @ClienteId ORDER BY DataPedido DESC";

        using (SqlConnection conn = DatabaseConnection.GetConnection())
        {
            conn.Open();

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@ClienteId", clienteId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    Console.WriteLine($"\n=== PEDIDOS DO CLIENTE #{clienteId} ===");
                    Console.WriteLine("ID\tData\t\tValor Total");
                    Console.WriteLine("--------------------------------");

                    bool temPedidos = false;
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader["Id"]}\t{((DateTime)reader["DataPedido"]).ToShortDateString()}\t{reader["ValorTotal"]:C}");
                        temPedidos = true;
                    }

                    if (!temPedidos)
                        Console.WriteLine("Nenhum pedido encontrado para este cliente!");
                }
            }
        }
    }

    // EXERCÍCIO 9: Obter detalhes completos de um pedido
    public void ObterDetalhesPedido(int pedidoId)
    {
        string sql = @"SELECT 
                            pi.*, 
                            p.Nome as NomeProduto,
                            (pi.Quantidade * pi.PrecoUnitario) as Subtotal
                          FROM PedidoItens pi
                          INNER JOIN Produtos p ON pi.ProdutoId = p.Id
                          WHERE pi.PedidoId = @PedidoId";

        using (SqlConnection conn = DatabaseConnection.GetConnection())
        {
            conn.Open();

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@PedidoId", pedidoId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    Console.WriteLine($"\n=== DETALHES DO PEDIDO #{pedidoId} ===");
                    Console.WriteLine("Produto\t\tQtd\tPreço Unit.\tSubtotal");
                    Console.WriteLine("----------------------------------------------");

                    decimal total = 0;
                    bool temItens = false;

                    while (reader.Read())
                    {
                        string nomeProduto = reader["NomeProduto"].ToString();
                        int quantidade = Convert.ToInt32(reader["Quantidade"]);
                        decimal precoUnitario = Convert.ToDecimal(reader["PrecoUnitario"]);
                        decimal subtotal = Convert.ToDecimal(reader["Subtotal"]);

                        Console.WriteLine($"{nomeProduto}\t\t{quantidade}\t{precoUnitario:C}\t{subtotal:C}");
                        total += subtotal;
                        temItens = true;
                    }

                    if (temItens)
                        Console.WriteLine($"\n💵 TOTAL DO PEDIDO: {total:C}");
                    else
                        Console.WriteLine("Nenhum item encontrado para este pedido!");
                }
            }
        }
    }

    // DESAFIO 3: Calcular total de vendas por período
    public void TotalVendasPorPeriodo(DateTime dataInicio, DateTime dataFim)
    {
        string sql = "SELECT SUM(ValorTotal) FROM Pedidos WHERE DataPedido BETWEEN @DataInicio AND @DataFim";

        using (SqlConnection conn = DatabaseConnection.GetConnection())
        {
            conn.Open();

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@DataInicio", dataInicio);
                cmd.Parameters.AddWithValue("@DataFim", dataFim);

                object result = cmd.ExecuteScalar();
                decimal total = result != DBNull.Value ? Convert.ToDecimal(result) : 0;

                Console.WriteLine($"\n💰 TOTAL DE VENDAS ({dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}): {total:C}");
            }
        }
    }
}