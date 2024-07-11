# JSON Serializer Context

One of the goals of this library is to be fully trimmable and Native AOT compatible. This means that we don't use reflection or emit code at runtime. This is a big challenge when working with JSON serialization, because most libraries use reflection to discover the properties of a type.

In order to achieve this goal, all the methods which require JSON deserialization requires a JsonTypeInfo object as parameter.

This object can be easily generated using System.Text.Json source generation.

If you are not familiar with this concept, you can read more about it in the [official documentation](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation?pivots=dotnet-8-0).
