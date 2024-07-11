# Getting started

## Installation

### Prerequisites

- .NET 6.0 or higher

### Install the package

```sh
dotnet add package LibSql.Http.Client
```

## Create a client

```csharp
var handler = new SocketsHttpHandler
{
    PooledConnectionLifetime = TimeSpan.FromMinutes(15) // Recreate every 15 minutes
};

var sharedClient = new HttpClient(handler)
{
    BaseAddress = new Uri("https://db.host.com/"),
    DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Bearer", "YOUR_AUTH_TOKEN" )}
};

var libSqlClient = new LibSqlHttpClient(sharedClient);
```

## Executing statements

```csharp
libSqlClient.ExecuteAsync(("INSERT INTO table (column) VALUES (?)", ["Hello, World!"]));
```

## What's next?

- Check the [guidelines](./guidelines.md) for the client instantiation
- Learn how to [execute statements](./query/index.md)
- Configure your project to query results and [type serialization](./concepts/json-serializer-context.md)
- Learn about [transactions](./concepts/transactions.md) using the lib
