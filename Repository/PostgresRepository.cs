using System.Data;
using Npgsql;
using OpenTelemetry.Trace;
using Ecommerce.MonitoringApp.Repository.Models;

namespace Ecommerce.MonitoringApp.Repository;

public static class PostgresRepository
{
    private static NpgsqlDataSource? _dataSource;

    // =========================
    // INIT & CLOSE
    // =========================

    public static async Task InitAsync(CancellationToken ct = default)
    {
        var dsn = Environment.GetEnvironmentVariable("DATABASE_DSN")
                  ?? "postgres://user:pass@postgres:5432/shop?sslmode=disable";

        var builder = new NpgsqlDataSourceBuilder(dsn);
        _dataSource = builder.Build();

        // fail fast: ping
        await using var conn = await _dataSource.OpenConnectionAsync(ct);
        await SetupTablesAsync(ct);
    }

    public static async Task CloseAsync()
    {
        if (_dataSource is not null)
            await _dataSource.DisposeAsync();
    }

    // =========================
    // SCHEMA SETUP
    // =========================

    private static async Task SetupTablesAsync(CancellationToken ct)
    {
        var tracer = TracerProvider.Default.GetTracer("repository");
        using var span = tracer.StartActiveSpan("setup_tables");

        await using var conn = await _dataSource!.OpenConnectionAsync(ct);

        var queries = new[]
        {
            """
            CREATE TABLE IF NOT EXISTS products (
                id SERIAL PRIMARY KEY,
                name VARCHAR(255),
                price DECIMAL(10,2)
            )
            """,
            """
            CREATE TABLE IF NOT EXISTS orders (
                id SERIAL PRIMARY KEY,
                product_id INTEGER,
                quantity INTEGER,
                total DECIMAL(10,2),
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            )
            """
        };

        foreach (var q in queries)
        {
            await using var cmd = new NpgsqlCommand(q, conn);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        // seed data
        await using var countCmd = new NpgsqlCommand("SELECT COUNT(*) FROM products", conn);
        var count = (long)(await countCmd.ExecuteScalarAsync(ct) ?? 0);

        if (count == 0)
        {
            var seed = new[]
            {
                ("Gaming Laptop", 15000000m),
                ("Wireless Mouse", 300000m),
                ("Mechanical Keyboard", 800000m),
                ("4K Monitor", 3500000m),
            };

            foreach (var (name, price) in seed)
            {
                await using var insert = new NpgsqlCommand(
                    "INSERT INTO products (name, price) VALUES (@n, @p)", conn);
                insert.Parameters.AddWithValue("n", name);
                insert.Parameters.AddWithValue("p", price);
                await insert.ExecuteNonQueryAsync(ct);
            }
        }
    }

    // =========================
    // QUERIES
    // =========================

    public static async Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken ct)
    {
        var tracer = TracerProvider.Default.GetTracer("repository");
        using var span = tracer.StartActiveSpan("get_products");

        await using var conn = await _dataSource!.OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT id, name, price FROM products ORDER BY id", conn);

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        var list = new List<Product>();
        while (await reader.ReadAsync(ct))
        {
            list.Add(new Product
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Price = reader.GetDecimal(2)
            });
        }

        return list;
    }

    public static async Task<Product> GetProductAsync(int id, CancellationToken ct)
    {
        var tracer = TracerProvider.Default.GetTracer("repository");
        using var span = tracer.StartActiveSpan("get_product");

        await using var conn = await _dataSource!.OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT id, name, price FROM products WHERE id=@id", conn);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            throw new KeyNotFoundException("Product not found");

        return new Product
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Price = reader.GetDecimal(2)
        };
    }

    public static async Task<int> CreateOrderAsync(
        int productId, int quantity, decimal total, CancellationToken ct)
    {
        var tracer = TracerProvider.Default.GetTracer("repository");
        using var span = tracer.StartActiveSpan("create_order");

        await using var conn = await _dataSource!.OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO orders (product_id, quantity, total)
            VALUES (@pid, @q, @t)
            RETURNING id
            """, conn);

        cmd.Parameters.AddWithValue("pid", productId);
        cmd.Parameters.AddWithValue("q", quantity);
        cmd.Parameters.AddWithValue("t", total);

        var id = (int)(await cmd.ExecuteScalarAsync(ct))!;
        return id;
    }

    public static async Task<Order> GetOrderAsync(int id, CancellationToken ct)
    {
        var tracer = TracerProvider.Default.GetTracer("repository");
        using var span = tracer.StartActiveSpan("get_order");

        await using var conn = await _dataSource!.OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(
            """
            SELECT id, product_id, quantity, total, created_at
            FROM orders WHERE id=@id
            """, conn);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            throw new KeyNotFoundException("Order not found");

        return new Order
        {
            Id = reader.GetInt32(0),
            ProductId = reader.GetInt32(1),
            Quantity = reader.GetInt32(2),
            Total = reader.GetDecimal(3),
            CreatedAt = reader.GetDateTime(4)
        };
    }
}
