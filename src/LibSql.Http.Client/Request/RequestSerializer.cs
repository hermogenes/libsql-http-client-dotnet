using System.Buffers.Text;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Text.Json;
using LibSql.Http.Client.Buffer;

namespace LibSql.Http.Client.Request;

internal static class RequestSerializer
{
    private static readonly MediaTypeHeaderValue ContentTypeHeaderValue =
        MediaTypeHeaderValue.Parse("application/json");

    private static readonly JsonWriterOptions WriterOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    internal static HttpContent Serialize(
        Statement[] statements,
        TransactionMode transactionMode,
        string? baton = null,
        bool isInteractive = false)
    {
        using var stream = new PooledByteBufferWriter();

        using var writer = new Utf8JsonWriter(stream, WriterOptions);

        writer.WriteStartObject();

        if (baton is not null) writer.WriteString("baton"u8, baton);

        writer.WriteStartArray("requests"u8);

        if (statements.Length == 1 && transactionMode is TransactionMode.None)
            WriteStatementObject(writer, statements[0].Sql, statements[0].Args, statements[0].NamedArgs, "execute"u8);
        else WriteBatchRequest(writer, statements, transactionMode, isInteractive);

        if (!isInteractive)
            writer.WriteRawValue("""{"type":"close"}"""u8, true);

        writer.WriteEndArray();

        writer.WriteEndObject();

        writer.Flush();

        return new ReadOnlyMemoryContent(stream.WrittenMemory.ToArray())
        {
            Headers = { ContentType = ContentTypeHeaderValue }
        };
    }

    private static void WriteBatchRequest(
        Utf8JsonWriter writer,
        Statement[] statements,
        TransactionMode transactionMode,
        bool isInteractive)
    {
        writer.WriteStartObject();

        writer.WriteString("type"u8, "batch"u8);

        writer.WriteStartObject("batch"u8);

        writer.WriteStartArray("steps"u8);

        var lastStep = transactionMode is TransactionMode.None ? -1 : 0;

        switch (transactionMode)
        {
            case TransactionMode.WriteImmediate:
                writer.WriteRawValue("{\"stmt\":{\"sql\":\"BEGIN IMMEDIATE\"}}"u8, true);
                break;
            case TransactionMode.Deferred:
                writer.WriteRawValue("{\"stmt\":{\"sql\":\"BEGIN DEFERRED\"}}"u8, true);
                break;
            case TransactionMode.ReadOnly:
                writer.WriteRawValue("{\"stmt\":{\"sql\":\"BEGIN TRANSACTION READONLY\"}}"u8, true);
                break;
            case TransactionMode.None:
            default:
                break;
        }

        foreach (var stmt in statements)
        {
            WriteStatementObject(writer, stmt.Sql, stmt.Args, stmt.NamedArgs, default, lastStep);
            lastStep++;
        }

        if (transactionMode != TransactionMode.None && !isInteractive)
        {
            writer.WriteRawValue(
                $"{{\"stmt\":{{\"sql\":\"COMMIT\"}},\"condition\":{{\"type\":\"ok\",\"step\":{lastStep}}}}}",
                true);
            writer.WriteRawValue(
                $"{{\"stmt\":{{\"sql\":\"ROLLBACK\"}},\"condition\":{{\"type\":\"not\",\"cond\":{{\"type\":\"ok\",\"step\":{lastStep + 1}}}}}}}",
                true);
        }


        writer.WriteEndArray();

        writer.WriteEndObject();

        writer.WriteEndObject();
    }


    private static void WriteStatementObject(
        Utf8JsonWriter writer,
        ReadOnlySpan<char> sql,
        object?[]? args,
        Dictionary<string, object?>? namedArgs,
        ReadOnlySpan<byte> type = default,
        int lastStep = -1)
    {
        writer.WriteStartObject();

        if (!type.IsEmpty) writer.WriteString("type"u8, type);

        writer.WriteStartObject("stmt"u8);
        writer.WriteString("sql"u8, sql);

        if (args is not null && args.Length > 0)
        {
            writer.WriteStartArray("args"u8);
            foreach (var arg in args) WriteArgObject(writer, arg);

            writer.WriteEndArray();
        }

        if (namedArgs is not null && namedArgs.Count > 0)
        {
            writer.WriteStartArray("named_args"u8);
            foreach (var namedArg in namedArgs)
            {
                writer.WriteStartObject();

                writer.WriteString("name"u8, namedArg.Key);

                writer.WritePropertyName("value"u8);

                WriteArgObject(writer, namedArg.Value);

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }

        writer.WriteEndObject();

        if (lastStep > -1)
        {
            writer.WriteStartObject("condition"u8);
            writer.WriteString("type"u8, "ok"u8);
            writer.WriteNumber("step"u8, lastStep);
            writer.WriteEndObject();
        }

        writer.WriteEndObject();

        writer.Flush();
    }

    internal static void WriteArgObject(Utf8JsonWriter writer, object? arg)
    {
        writer.WriteStartObject();

        switch (arg)
        {
            case null:
                writer.WriteString("type"u8, "null"u8);
                break;
            case byte[] blobArg:
                writer.WriteString("type"u8, "blob"u8);
                writer.WriteBase64String("base64"u8, blobArg);
                break;
            case int intArg:
                writer.WriteString("type"u8, "integer"u8);
                WriteNumberAsString(writer, "value"u8, intArg, 11);
                break;
            case uint intArg:
                writer.WriteString("type"u8, "integer"u8);
                WriteNumberAsString(writer, "value"u8, intArg, 11);
                break;
            case long intArg:
                writer.WriteString("type"u8, "integer"u8);
                WriteNumberAsString(writer, "value"u8, intArg, 20);
                break;
            case ulong intArg:
                writer.WriteString("type"u8, "integer"u8);
                WriteNumberAsString(writer, "value"u8, intArg, 20);
                break;
            case short intArg:
                writer.WriteString("type"u8, "integer"u8);
                WriteNumberAsString(writer, "value"u8, intArg, 6);
                break;
            case ushort intArg:
                writer.WriteString("type"u8, "integer"u8);
                WriteNumberAsString(writer, "value"u8, intArg, 6);
                break;
            case bool bArg:
                writer.WriteString("type"u8, "integer"u8);
                writer.WriteString("value"u8, bArg ? "1"u8 : "0"u8);
                break;
            case float nArg:
                writer.WriteString("type"u8, "float"u8);
                writer.WriteNumber("value"u8, nArg);
                break;
            case decimal nArg:
                writer.WriteString("type"u8, "float"u8);
                writer.WriteNumber("value"u8, nArg);
                break;
            case double nArg:
                writer.WriteString("type"u8, "float"u8);
                writer.WriteNumber("value"u8, nArg);
                break;
            case string sArg:
                writer.WriteString("type"u8, "text");
                writer.WriteString("value"u8, sArg);
                break;
            default:
                throw new ArgumentException($"Unsupported arg item of type: {arg.GetType()}");
        }

        writer.WriteEndObject();
    }

    private static void WriteNumberAsString(
        Utf8JsonWriter writer,
        ReadOnlySpan<byte> propName,
        ulong value,
        int byteSize)
    {
        Span<byte> destination = stackalloc byte[byteSize];
        Utf8Formatter.TryFormat(value, destination, out var bytesWritten);
        writer.WriteString(propName, destination[..bytesWritten]);
    }

    private static void WriteNumberAsString(
        Utf8JsonWriter writer,
        ReadOnlySpan<byte> propName,
        long value,
        int byteSize)
    {
        Span<byte> destination = stackalloc byte[byteSize];
        Utf8Formatter.TryFormat(value, destination, out var bytesWritten);
        writer.WriteString(propName, destination[..bytesWritten]);
    }
}
