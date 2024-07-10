using LibSql.Http.Client.Tests.Integration.Models;

namespace LibSql.Http.Client.Tests.Integration.Fixture;

public class ProductTestData(string tableName = "products")
{
    public static readonly byte[] ImageBytes =
        File.ReadAllBytes(
            Path.GetRelativePath(Directory.GetCurrentDirectory(), "Integration/Assets/product-image.png"));

    public static readonly List<ProductTestModel> Items =
    [
        new ProductTestModel("1", "Laptop", "High-performance gaming laptop", 1200.00m, 5, ImageBytes),
        new ProductTestModel("2", "Smartphone", "Latest model with advanced features", 999.99m, 10, null),
        new ProductTestModel("3", "Headphones", "Noise-cancelling headphones", 250.00m, 15, ImageBytes),
        new ProductTestModel("4", "Smartwatch", "Waterproof smartwatch with GPS", 199.99m, 20, null),
        new ProductTestModel("5", "Tablet", "Lightweight tablet with 12-inch screen", 450.00m, 8, ImageBytes),
        new ProductTestModel("6", "E-reader", "E-reader with adjustable light", 130.00m, 12, null),
        new ProductTestModel("7", "Camera", "DSLR camera with 24.1 MP", 600.00m, 7, ImageBytes),
        new ProductTestModel("8", "Portable Speaker", "Bluetooth portable speaker", 120.00m, 25, null),
        new ProductTestModel("9", "Video Game Console", "Next-gen video game console", 500.00m, 30, ImageBytes),
        new ProductTestModel("10", "Wireless Mouse", "Ergonomic wireless mouse", 50.00m, 40, null),
        new ProductTestModel("11", "Keyboard", "Mechanical keyboard with backlight", 70.00m, 35, ImageBytes),
        new ProductTestModel("12", "External Hard Drive", "2TB external hard drive", 80.00m, 22, null),
        new ProductTestModel("13", "USB Flash Drive", "128GB USB 3.0 flash drive", 25.00m, 50, ImageBytes),
        new ProductTestModel("14", "Router", "Wi-Fi 6 router", 150.00m, 18, null),
        new ProductTestModel("15", "Monitor", "27-inch 4K UHD monitor", 330.00m, 11, ImageBytes),
        new ProductTestModel("16", "Graphics Card", "High-end gaming graphics card", 700.00m, 6, ImageBytes),
        new ProductTestModel("17", "Processor", "8-core desktop processor", 320.00m, 9, null),
        new ProductTestModel("18", "SSD", "1TB NVMe SSD", 100.00m, 14, ImageBytes),
        new ProductTestModel("19", "RAM", "16GB DDR4 RAM", 60.00m, 28, null),
        new ProductTestModel("20", "Power Supply", "750W modular power supply", 90.00m, 16, ImageBytes),
        new ProductTestModel("21", "Gaming Chair", "Ergonomic gaming chair", 200.00m, 13, null),
        new ProductTestModel("22", "Desk Lamp", "LED desk lamp with wireless charging", 40.00m, 33, ImageBytes),
        new ProductTestModel("23", "Smart Home Hub", "Voice-controlled smart home hub", 130.00m, 19, null)
    ];

    public string InsertSqlWithPositionalArgs =>
        $"INSERT INTO {TableName} (id, name, description, price, stock, image) VALUES (?, ?, ?, ?, ?, ?)";

    public string SelectProductsWithoutImageSql =>
        $"SELECT id, name, description, price, stock, image FROM {TableName} WHERE image IS NULL";

    public string SelectProductsWithImageSql =>
        $"SELECT id, name, description, price, stock, image FROM {TableName} WHERE image IS NOT NULL";

    public string CountSql =>
        $"SELECT COUNT(id) FROM {TableName}";

    public string SelectAllSql =>
        $"SELECT id, name, description, price, stock, image FROM {TableName}";

    public string SelectLikeNameSql =>
        $"SELECT id, name, description, price, stock, image FROM {TableName} WHERE name like ? LIMIT 1";

    public string SelectByIdSql =>
        $"SELECT id, name, description, price, stock, image FROM {TableName} WHERE id = ? LIMIT 1";

    public string InsertSqlWithNamedArgs =>
        $"INSERT INTO {TableName} (id, name, description, price, stock, image) VALUES (@id, @name, @description, @price, @stock, @image)";

    public string CreateTableSql =>
        $"CREATE TABLE {TableName} (id TEXT PRIMARY KEY, name TEXT NOT NULL, description TEXT NOT NULL, price REAL NOT NULL, stock INTEGER NOT NULL, image BLOB) WITHOUT ROWID";

    public string TableName { get; } = tableName;
}
