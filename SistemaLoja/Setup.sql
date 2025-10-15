--para criar o container do banco execute o comando abaixo
----docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=SqlServer2024!" -p 1433:1433 --name sqlserver2022 -d mcr.microsoft.com/mssql/server:2022-latest


-- ===============================================
-- SCRIPT DE SETUP - LAB 12
-- Conexão com SQL Server
-- ===============================================

-- Criar database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'LojaDB')
BEGIN
    CREATE DATABASE LojaDB;
END
GO

USE LojaDB;
GO

-- ===============================================
-- CRIAR TABELAS
-- ===============================================

-- Tabela de Clientes
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Clientes')
BEGIN
    CREATE TABLE Clientes (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Nome NVARCHAR(100) NOT NULL,
        Email NVARCHAR(100) NOT NULL UNIQUE,
        Telefone NVARCHAR(20),
        DataCadastro DATETIME DEFAULT GETDATE()
    );
END
GO

-- Tabela de Categorias
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categorias')
BEGIN
    CREATE TABLE Categorias (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Nome NVARCHAR(50) NOT NULL UNIQUE,
        Descricao NVARCHAR(200)
    );
END
GO

-- Tabela de Produtos
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Produtos')
BEGIN
    CREATE TABLE Produtos (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Nome NVARCHAR(100) NOT NULL,
        Preco DECIMAL(10,2) NOT NULL CHECK (Preco >= 0),
        Estoque INT NOT NULL DEFAULT 0 CHECK (Estoque >= 0),
        CategoriaId INT NOT NULL,
        DataCadastro DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (CategoriaId) REFERENCES Categorias(Id)
    );
END
GO

-- Tabela de Pedidos
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Pedidos')
BEGIN
    CREATE TABLE Pedidos (
        Id INT PRIMARY KEY IDENTITY(1,1),
        ClienteId INT NOT NULL,
        DataPedido DATETIME NOT NULL DEFAULT GETDATE(),
        ValorTotal DECIMAL(10,2) NOT NULL CHECK (ValorTotal >= 0),
        Status NVARCHAR(20) DEFAULT 'Pendente',
        FOREIGN KEY (ClienteId) REFERENCES Clientes(Id)
    );
END
GO

-- Tabela de Itens do Pedido
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PedidoItens')
BEGIN
    CREATE TABLE PedidoItens (
        Id INT PRIMARY KEY IDENTITY(1,1),
        PedidoId INT NOT NULL,
        ProdutoId INT NOT NULL,
        Quantidade INT NOT NULL CHECK (Quantidade > 0),
        PrecoUnitario DECIMAL(10,2) NOT NULL CHECK (PrecoUnitario >= 0),
        FOREIGN KEY (PedidoId) REFERENCES Pedidos(Id) ON DELETE CASCADE,
        FOREIGN KEY (ProdutoId) REFERENCES Produtos(Id)
    );
END
GO

-- ===============================================
-- INSERIR DADOS DE TESTE
-- ===============================================

-- Limpar dados existentes (cuidado em produção!)
DELETE FROM PedidoItens;
DELETE FROM Pedidos;
DELETE FROM Produtos;
DELETE FROM Categorias;
DELETE FROM Clientes;
GO

-- Reset dos Identity seeds
DBCC CHECKIDENT ('PedidoItens', RESEED, 0);
DBCC CHECKIDENT ('Pedidos', RESEED, 0);
DBCC CHECKIDENT ('Produtos', RESEED, 0);
DBCC CHECKIDENT ('Categorias', RESEED, 0);
DBCC CHECKIDENT ('Clientes', RESEED, 0);
GO

-- Inserir Categorias
INSERT INTO Categorias (Nome, Descricao) VALUES
('Eletrônicos', 'Produtos eletrônicos e gadgets'),
('Livros', 'Livros físicos e digitais'),
('Roupas', 'Vestuário em geral'),
('Alimentos', 'Produtos alimentícios'),
('Esportes', 'Artigos esportivos');
GO

-- Inserir Produtos
INSERT INTO Produtos (Nome, Preco, Estoque, CategoriaId) VALUES
-- Eletrônicos
('Notebook Dell Inspiron', 3499.90, 15, 1),
('Mouse Logitech MX', 249.90, 50, 1),
('Teclado Mecânico RGB', 459.90, 30, 1),
('Monitor LG 27"', 1299.90, 20, 1),
('Webcam Full HD', 199.90, 40, 1),

-- Livros
('Clean Code - Robert Martin', 89.90, 100, 2),
('Design Patterns - GoF', 129.90, 80, 2),
('C# in Depth', 159.90, 60, 2),
('Domain-Driven Design', 139.90, 50, 2),
('Refactoring - Martin Fowler', 99.90, 70, 2),

-- Roupas
('Camiseta Básica Preta', 49.90, 200, 3),
('Calça Jeans Slim', 189.90, 150, 3),
('Jaqueta Corta-Vento', 259.90, 80, 3),
('Tênis Esportivo', 299.90, 100, 3),
('Boné Aba Reta', 79.90, 120, 3),

-- Alimentos
('Café Especial 250g', 29.90, 300, 4),
('Chocolate Amargo 70%', 15.90, 250, 4),
('Granola Premium', 24.90, 180, 4),
('Mel Orgânico', 34.90, 150, 4),
('Azeite Extra Virgem', 44.90, 200, 4),

-- Esportes
('Bola de Futebol', 89.90, 60, 5),
('Raquete de Tênis', 349.90, 40, 5),
('Luvas de Boxe', 159.90, 50, 5),
('Bicicleta Mountain Bike', 1899.90, 15, 5),
('Kit de Yoga', 129.90, 80, 5);
GO

-- Inserir Clientes
INSERT INTO Clientes (Nome, Email, Telefone) VALUES
('João Silva', 'joao.silva@email.com', '(11) 98765-4321'),
('Maria Santos', 'maria.santos@email.com', '(11) 97654-3210'),
('Pedro Oliveira', 'pedro.oliveira@email.com', '(11) 96543-2109'),
('Ana Costa', 'ana.costa@email.com', '(11) 95432-1098'),
('Carlos Souza', 'carlos.souza@email.com', '(11) 94321-0987');
GO

-- Inserir alguns pedidos de exemplo
-- Pedido 1 - João Silva
INSERT INTO Pedidos (ClienteId, DataPedido, ValorTotal, Status) 
VALUES (1, GETDATE()-5, 749.80, 'Entregue');

INSERT INTO PedidoItens (PedidoId, ProdutoId, Quantidade, PrecoUnitario) VALUES
(1, 2, 2, 249.90),  -- 2x Mouse Logitech
(1, 6, 1, 89.90),   -- 1x Clean Code
(1, 11, 2, 49.90);  -- 2x Camiseta
GO

-- Pedido 2 - Maria Santos
INSERT INTO Pedidos (ClienteId, DataPedido, ValorTotal, Status) 
VALUES (2, GETDATE()-3, 4059.70, 'Em Transporte');

INSERT INTO PedidoItens (PedidoId, ProdutoId, Quantidade, PrecoUnitario) VALUES
(2, 1, 1, 3499.90), -- 1x Notebook
(2, 3, 1, 459.90),  -- 1x Teclado
(2, 8, 1, 99.90);   -- 1x Refactoring
GO

-- Pedido 3 - Pedro Oliveira
INSERT INTO Pedidos (ClienteId, DataPedido, ValorTotal, Status) 
VALUES (3, GETDATE()-1, 2289.70, 'Processando');

INSERT INTO PedidoItens (PedidoId, ProdutoId, Quantidade, PrecoUnitario) VALUES
(3, 24, 1, 1899.90), -- 1x Mountain Bike
(3, 14, 1, 299.90),  -- 1x Tênis
(3, 21, 3, 29.90);   -- 3x Café
GO

-- ===============================================
-- CRIAR ÍNDICES PARA PERFORMANCE
-- ===============================================

-- Índice para busca por nome de produto
CREATE INDEX IX_Produtos_Nome ON Produtos(Nome);
GO

-- Índice para busca por categoria
CREATE INDEX IX_Produtos_CategoriaId ON Produtos(CategoriaId);
GO

-- Índice para busca de pedidos por cliente
CREATE INDEX IX_Pedidos_ClienteId ON Pedidos(ClienteId);
GO

-- Índice para busca de pedidos por data
CREATE INDEX IX_Pedidos_DataPedido ON Pedidos(DataPedido);
GO

-- ===============================================
-- VIEWS ÚTEIS
-- ===============================================

-- View: Produtos com nome da categoria
IF OBJECT_ID('vw_ProdutosCompleto', 'V') IS NOT NULL
    DROP VIEW vw_ProdutosCompleto;
GO

CREATE VIEW vw_ProdutosCompleto AS
SELECT 
    p.Id,
    p.Nome,
    p.Preco,
    p.Estoque,
    c.Nome AS Categoria,
    p.DataCadastro
FROM Produtos p
INNER JOIN Categorias c ON p.CategoriaId = c.Id;
GO

-- View: Pedidos com informações do cliente
IF OBJECT_ID('vw_PedidosCompleto', 'V') IS NOT NULL
    DROP VIEW vw_PedidosCompleto;
GO

CREATE VIEW vw_PedidosCompleto AS
SELECT 
    p.Id AS PedidoId,
    p.DataPedido,
    p.ValorTotal,
    p.Status,
    c.Id AS ClienteId,
    c.Nome AS ClienteNome,
    c.Email AS ClienteEmail
FROM Pedidos p
INNER JOIN Clientes c ON p.ClienteId = c.Id;
GO

-- ===============================================
-- STORED PROCEDURES ÚTEIS
-- ===============================================

-- SP: Buscar produtos com estoque baixo
IF OBJECT_ID('sp_ProdutosEstoqueBaixo', 'P') IS NOT NULL
    DROP PROCEDURE sp_ProdutosEstoqueBaixo;
GO

CREATE PROCEDURE sp_ProdutosEstoqueBaixo
    @QuantidadeMinima INT = 30
AS
BEGIN
    SELECT 
        p.Id,
        p.Nome,
        p.Estoque,
        c.Nome AS Categoria
    FROM Produtos p
    INNER JOIN Categorias c ON p.CategoriaId = c.Id
    WHERE p.Estoque < @QuantidadeMinima
    ORDER BY p.Estoque ASC;
END
GO

-- SP: Obter total de vendas por período
IF OBJECT_ID('sp_TotalVendasPeriodo', 'P') IS NOT NULL
    DROP PROCEDURE sp_TotalVendasPeriodo;
GO

CREATE PROCEDURE sp_TotalVendasPeriodo
    @DataInicio DATETIME,
    @DataFim DATETIME
AS
BEGIN
    SELECT 
        COUNT(*) AS TotalPedidos,
        SUM(ValorTotal) AS ValorTotal,
        AVG(ValorTotal) AS TicketMedio
    FROM Pedidos
    WHERE DataPedido BETWEEN @DataInicio AND @DataFim;
END
GO

-- SP: Produtos mais vendidos
IF OBJECT_ID('sp_ProdutosMaisVendidos', 'P') IS NOT NULL
    DROP PROCEDURE sp_ProdutosMaisVendidos;
GO

CREATE PROCEDURE sp_ProdutosMaisVendidos
    @Top INT = 10
AS
BEGIN
    SELECT TOP (@Top)
        p.Id,
        p.Nome,
        SUM(pi.Quantidade) AS TotalVendido,
        SUM(pi.Quantidade * pi.PrecoUnitario) AS ReceitaTotal
    FROM Produtos p
    INNER JOIN PedidoItens pi ON p.Id = pi.ProdutoId
    GROUP BY p.Id, p.Nome
    ORDER BY TotalVendido DESC;
END
GO

-- ===============================================
-- QUERIES DE VERIFICAÇÃO
-- ===============================================

-- Verificar dados inseridos
PRINT '=== VERIFICAÇÃO DOS DADOS ===';
PRINT '';

PRINT 'Total de Categorias: ' + CAST((SELECT COUNT(*) FROM Categorias) AS VARCHAR);
PRINT 'Total de Produtos: ' + CAST((SELECT COUNT(*) FROM Produtos) AS VARCHAR);
PRINT 'Total de Clientes: ' + CAST((SELECT COUNT(*) FROM Clientes) AS VARCHAR);
PRINT 'Total de Pedidos: ' + CAST((SELECT COUNT(*) FROM Pedidos) AS VARCHAR);
PRINT 'Total de Itens: ' + CAST((SELECT COUNT(*) FROM PedidoItens) AS VARCHAR);
PRINT '';

-- Mostrar categorias com quantidade de produtos
PRINT '=== PRODUTOS POR CATEGORIA ===';
SELECT 
    c.Nome AS Categoria,
    COUNT(p.Id) AS QtdProdutos
FROM Categorias c
LEFT JOIN Produtos p ON c.Id = p.CategoriaId
GROUP BY c.Nome
ORDER BY QtdProdutos DESC;

-- Mostrar pedidos recentes
PRINT '';
PRINT '=== PEDIDOS RECENTES ===';
SELECT TOP 5
    p.Id,
    c.Nome AS Cliente,
    p.DataPedido,
    p.ValorTotal,
    p.Status
FROM Pedidos p
INNER JOIN Clientes c ON p.ClienteId = c.Id
ORDER BY p.DataPedido DESC;

PRINT '';
PRINT '✅ Setup concluído com sucesso!';
PRINT '🚀 Você pode executar a aplicação C# agora.';
GO