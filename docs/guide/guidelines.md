# Client Guidelines and best practices

This library uses the HttpClient and follows the same recommended usage patterns.
You can check the [official HTTP client guidelines](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines) for more information.

## Lifetime management

The HttpClient class is intended to be instantiated once and reused throughout the life of an application.
Same applies to the LibSqlHttpClient class, which is a wrapper around the HttpClient.
