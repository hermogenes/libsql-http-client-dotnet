# LibSql.Http.Client

An alternative [libSQL](https://github.com/tursodatabase/libsql) .NET client, supporting HTTP protocol, fully trimmable and AOT compatible.

> [!WARNING]
> This is not an official libSQL client

[![Continuous Integration](https://github.com/hermogenes/libsql-http-client-dotnet/actions/workflows/ci.yml/badge.svg?branch=main&event=push)](https://github.com/hermogenes/libsql-http-client-dotnet/actions/workflows/ci.yml) [![NuGet](https://img.shields.io/nuget/dt/LibSql.Http.Client.svg)](https://www.nuget.org/stats/packages/LibSql.Http.Client?groupby=Version) [![NuGet](https://img.shields.io/nuget/vpre/LibSql.Http.Client.svg)](https://www.nuget.org/packages/LibSql.Http.Client/)

## About

This client is a .NET implementation of HRANA protocol, intented to communicate with libSQL server.

This lib is inspired by [libsql-stateless-easy](https://github.com/DaBigBlob/libsql-stateless-easy).

## Requirements

- .NET 8 (6 and 7 are supported as well)

## Installation

Install [Nuget](https://www.nuget.org/packages/LibSql.Http.Client/) package:

```sh
dotnet add package LibSql.Http.Client
```

## Usage

The instance of the client expect an instance of HttpClient.

The most performant way is use a singleton instance of HttpClient.

Check the offical .NET [HTTP client guidelines](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines) for more information.

```csharp
var handler = new SocketsHttpHandler
{
    PooledConnectionLifetime = TimeSpan.FromMinutes(15) // Recreate every 15 minutes
};
var sharedClient = new HttpClient(handler);
var libSqlClient = new LibSqlHttpClient(sharedClient, new Uri("http://localhost:8080"));
```

## [Check the Docs for more](https://libsql-http-client-dotnet.pages.dev/)

## Features

- ✅ Single and batch commands
- ✅ Transactions (\*)
- ✅ Positional args
- ✅ Named args via Dictionary
- ✅ Micro ORM like queries and results with minimum overhead
- ❌ Interactive transactions not supported (transactions are done in a single request)

**\*** Transactions are possible per statement(s) only. So distributed transaction is not possible (yet)!

## Demo App

```sh
 dotnet run --project src/LibSql.Http.Client.DemoConsoleApp/LibSql.Http.Client.DemoConsoleApp.csproj
```

Check the [code](./src/LibSql.Http.Client.DemoConsoleApp/Program.cs).
