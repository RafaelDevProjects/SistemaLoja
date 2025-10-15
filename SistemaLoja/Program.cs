

using System;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using SistemaLoja.Lab12_ConexaoSQLServer;

namespace SistemaLoja
{
    // ===============================================
    // MODELOS DE DADOS
    // ===============================================

    // ===============================================
    // CLASSE DE CONEXÃO
    // ===============================================

    // ===============================================
    // REPOSITÓRIO DE PRODUTOS
    // ===============================================

    // ===============================================
    // REPOSITÓRIO DE PEDIDOS
    // ===============================================

    // ===============================================
    // CLASSE PRINCIPAL
    // ===============================================
    
    class Program
    {
        static void Main(string[] args)
        {
            // IMPORTANTE: Antes de executar, crie o banco de dados!
            // Execute o script SQL fornecido no arquivo setup.sql
            
            Console.WriteLine("=== LAB 12 - CONEXÃO SQL SERVER ===\n");
            
            var produtoRepo = new ProdutoRepository();
            var pedidoRepo = new PedidoRepository();
            
            bool continuar = true;
            
            while (continuar)
            {
                MostrarMenu();
                string opcao = Console.ReadLine();
                
                try
                {
                    switch (opcao)
                    {
                        case "1":
                            produtoRepo.ListarTodosProdutos();
                            break;
                            
                        case "2":
                            InserirNovoProduto(produtoRepo);
                            break;
                            
                        case "3":
                            AtualizarProdutoExistente(produtoRepo);
                            break;
                            
                        case "4":
                            DeletarProdutoExistente(produtoRepo);
                            break;
                            
                        case "5":
                            ListarPorCategoria(produtoRepo);
                            break;
                            
                        case "6":
                            CriarNovoPedido(pedidoRepo);
                            break;
                            
                        case "7":
                            ListarPedidosDeCliente(pedidoRepo);
                            break;
                            
                        case "8":
                            DetalhesDoPedido(pedidoRepo);
                            break;
                            
                        case "0":
                            continuar = false;
                            break;
                            
                        default:
                            Console.WriteLine("Opção inválida!");
                            break;
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"\n❌ Erro SQL: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n❌ Erro: {ex.Message}");
                }
                
                if (continuar)
                {
                    Console.WriteLine("\nPressione qualquer tecla para continuar...");
                    Console.ReadKey();
                    Console.Clear();
                }
            }
            
            Console.WriteLine("\nPrograma finalizado!");
        }

        static void MostrarMenu()
        {
            Console.WriteLine("\n╔════════════════════════════════════╗");
            Console.WriteLine("║       MENU PRINCIPAL               ║");
            Console.WriteLine("╠════════════════════════════════════╣");
            Console.WriteLine("║  PRODUTOS                          ║");
            Console.WriteLine("║  1 - Listar todos os produtos      ║");
            Console.WriteLine("║  2 - Inserir novo produto          ║");
            Console.WriteLine("║  3 - Atualizar produto             ║");
            Console.WriteLine("║  4 - Deletar produto               ║");
            Console.WriteLine("║  5 - Listar por categoria          ║");
            Console.WriteLine("║                                    ║");
            Console.WriteLine("║  PEDIDOS                           ║");
            Console.WriteLine("║  6 - Criar novo pedido             ║");
            Console.WriteLine("║  7 - Listar pedidos de cliente     ║");
            Console.WriteLine("║  8 - Detalhes de um pedido         ║");
            Console.WriteLine("║                                    ║");
            Console.WriteLine("║  0 - Sair                          ║");
            Console.WriteLine("╚════════════════════════════════════╝");
            Console.Write("\nEscolha uma opção: ");
        }

        static void InserirNovoProduto(ProdutoRepository repo)
        {
            Console.WriteLine("\n=== INSERIR NOVO PRODUTO ===");

            Console.Write("Nome: ");
            string nome = Console.ReadLine();

            Console.Write("Preço: ");
            decimal preco = decimal.Parse(Console.ReadLine());

            Console.Write("Estoque: ");
            int estoque = int.Parse(Console.ReadLine());

            Console.Write("Categoria ID: ");
            int categoriaId = int.Parse(Console.ReadLine());

            var produto = new Produto
            {
                Nome = nome,
                Preco = preco,
                Estoque = estoque,
                CategoriaId = categoriaId
            };

            repo.InserirProduto(produto);
        }

        static void AtualizarProdutoExistente(ProdutoRepository repo)
        {
            Console.WriteLine("\n=== ATUALIZAR PRODUTO ===");

            Console.Write("ID do produto: ");
            int id = int.Parse(Console.ReadLine());

            Produto produto = repo.BuscarPorId(id);
            if (produto == null)
            {
                Console.WriteLine("Produto não encontrado!");
                return;
            }

            Console.WriteLine($"Produto atual: {produto.Nome} - {produto.Preco:C} - Estoque: {produto.Estoque}");

            Console.Write("Novo nome (enter para manter): ");
            string novoNome = Console.ReadLine();
            if (!string.IsNullOrEmpty(novoNome))
                produto.Nome = novoNome;

            Console.Write("Novo preço (enter para manter): ");
            string novoPrecoStr = Console.ReadLine();
            if (!string.IsNullOrEmpty(novoPrecoStr))
                produto.Preco = decimal.Parse(novoPrecoStr);

            Console.Write("Novo estoque (enter para manter): ");
            string novoEstoqueStr = Console.ReadLine();
            if (!string.IsNullOrEmpty(novoEstoqueStr))
                produto.Estoque = int.Parse(novoEstoqueStr);

            repo.AtualizarProduto(produto);
        }

        static void DeletarProdutoExistente(ProdutoRepository repo)
        {
            Console.WriteLine("\n=== DELETAR PRODUTO ===");

            Console.Write("ID do produto: ");
            int id = int.Parse(Console.ReadLine());

            Console.Write("Tem certeza que deseja deletar este produto? (s/n): ");
            string confirmacao = Console.ReadLine();

            if (confirmacao.ToLower() == "s")
                repo.DeletarProduto(id);
            else
                Console.WriteLine("Operação cancelada!");
        }

        static void ListarPorCategoria(ProdutoRepository repo)
        {
            Console.WriteLine("\n=== PRODUTOS POR CATEGORIA ===");

            Console.Write("ID da categoria: ");
            int categoriaId = int.Parse(Console.ReadLine());

            repo.ListarProdutosPorCategoria(categoriaId);
        }

        static void CriarNovoPedido(PedidoRepository repo)
        {
            Console.WriteLine("\n=== CRIAR NOVO PEDIDO ===");

            Console.Write("ID do cliente: ");
            int clienteId = int.Parse(Console.ReadLine());

            var pedido = new Pedido
            {
                ClienteId = clienteId,
                DataPedido = DateTime.Now,
                ValorTotal = 0
            };

            var itens = new List<PedidoItem>();
            bool adicionarMais = true;

            while (adicionarMais)
            {
                Console.Write("ID do produto: ");
                int produtoId = int.Parse(Console.ReadLine());

                Console.Write("Quantidade: ");
                int quantidade = int.Parse(Console.ReadLine());

                Console.Write("Preço unitário: ");
                decimal precoUnitario = decimal.Parse(Console.ReadLine());

                itens.Add(new PedidoItem
                {
                    ProdutoId = produtoId,
                    Quantidade = quantidade,
                    PrecoUnitario = precoUnitario
                });

                pedido.ValorTotal += quantidade * precoUnitario;

                Console.Write("Adicionar outro item? (s/n): ");
                adicionarMais = Console.ReadLine().ToLower() == "s";
            }

            repo.CriarPedido(pedido, itens);
        }

        static void ListarPedidosDeCliente(PedidoRepository repo)
        {
            Console.WriteLine("\n=== PEDIDOS DO CLIENTE ===");

            Console.Write("ID do cliente: ");
            int clienteId = int.Parse(Console.ReadLine());

            repo.ListarPedidosCliente(clienteId);
        }

        static void DetalhesDoPedido(PedidoRepository repo)
        {
            Console.WriteLine("\n=== DETALHES DO PEDIDO ===");

            Console.Write("ID do pedido: ");
            int pedidoId = int.Parse(Console.ReadLine());

            repo.ObterDetalhesPedido(pedidoId);
        }
    }
}