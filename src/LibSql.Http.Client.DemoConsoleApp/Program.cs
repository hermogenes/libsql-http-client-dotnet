using System.Text.Json.Serialization;
using DotNet.Testcontainers.Builders;
using LibSql.Http.Client;
using LibSql.Http.Client.Request;

var libSqlServerContainer = new ContainerBuilder()
    .WithImage("ghcr.io/tursodatabase/libsql-server:latest")
    .WithPortBinding(8080, true)
    .WithWaitStrategy(
        Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPath("/health").ForPort(8080)))
    .Build();

await libSqlServerContainer.StartAsync();

var serverUri = new UriBuilder(
    Uri.UriSchemeHttp,
    libSqlServerContainer.Hostname,
    libSqlServerContainer.GetMappedPublicPort(8080)).Uri;

var handler = new SocketsHttpHandler
{
    PooledConnectionLifetime = TimeSpan.FromMinutes(15) // Recreate every 15 minutes
};
var sharedClient = new HttpClient(handler);
var libSqlClient = new LibSqlHttpClient(sharedClient, serverUri);

await libSqlClient.ExecuteAsync(
    "CREATE TABLE products (id TEXT PRIMARY KEY, name TEXT NOT NULL, description TEXT NOT NULL, price REAL NOT NULL, stock INTEGER NOT NULL) WITHOUT ROWID");

Console.WriteLine("\u2705 Table 'products' created!");

var insertedRows = await libSqlClient.ExecuteMultipleAsync(
    [
        "INSERT INTO products (id, name, description, price, stock) VALUES (\"product-123\", \"Laptop\", \"Nice Laptop\", 3200, 10)",
        ("INSERT INTO products (id, name, description, price, stock) VALUES (?, ?, ?, ?, ?)",
            ["product-456", "Camera", "Nice camera", 2000, 20]),
        ("INSERT INTO products (id, name, description, price, stock) VALUES (@id, @name, @description, @price, @stock)",
            new Dictionary<string, object>
            {
                { "id", "product-789" }, { "name", "TV" }, { "description", "Nice TV" }, { "price", 1000 },
                { "stock", 30 }
            })!
    ],
    TransactionMode.WriteImmediate);

Console.WriteLine($"{insertedRows} rows inserted!");

var count = await libSqlClient.ExecuteScalarAsync("SELECT count(id) FROM products");

Console.WriteLine($"There are {count} products registered");

var itemsResult = await libSqlClient.QueryAsync(
    "SELECT id, name, description, price, stock FROM products",
    AppSerializerContext.Default.Product);

var items = itemsResult.ToList();

Console.WriteLine($"Listed {items.Count} items:");
Console.WriteLine(string.Join(Environment.NewLine, items.Select((it, idx) => $"$\t[{idx}] => {it}")));

var byId = await libSqlClient.QuerySingleAsync(
    ("SELECT id, name, description, price, stock FROM products WHERE id = ? LIMIT 1", [items[0].Id]),
    AppSerializerContext.Default.Product);

Console.WriteLine($"Hey, we found the item with id '{items[0].Id}':");

Console.WriteLine(byId);

var deletedItemsCount = await libSqlClient.ExecuteAsync("DELETE FROM products");

Console.WriteLine($"{deletedItemsCount} items deleted!");

public record Product(string Id, string Name, string Description, decimal Price, int Stock);

[JsonSerializable(typeof(Product))]
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
public partial class AppSerializerContext : JsonSerializerContext
{
}