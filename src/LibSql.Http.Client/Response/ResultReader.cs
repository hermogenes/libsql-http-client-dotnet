using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using LibSql.Http.Client.Buffer;
using LibSql.Http.Client.Exceptions;
using LibSql.Http.Client.Interfaces;

namespace LibSql.Http.Client.Response;

/// <inheritdoc />
/// <param name="buffer"></param>
/// <param name="baton"></param>
/// <param name="errors"></param>
/// <param name="stats"></param>
/// <param name="rowsMarkers"></param>
internal class ResultReader(
    PooledByteBufferWriter buffer,
    string? baton,
    ExecutionError[] errors,
    ExecutionStats[] stats,
    List<List<long[]>> rowsMarkers) : IResultReader
{
    private int _position = -1;

    public ExecutionStats[] Stats { get; } = stats.ToArray();

    public string? Baton => baton;

    public ExecutionError[] Errors { get; } = errors.ToArray();

    public int Count => rowsMarkers.Count;

    public int AffectedRows => Stats.Sum(s => s.AffectedRows);

    public void Dispose()
    {
        buffer.Dispose();
        GC.SuppressFinalize(this);
    }

    public void ThrowIfError()
    {
        if (errors.Length > 0)
            throw new LibSqlClientExecutionException(errors);
    }

    public bool HasMoreResults()
    {
        _position++;
        return _position < Count;
    }

    public IEnumerable<T> Read<T>(JsonTypeInfo<T> typeInfo) => ReadAt(_position, typeInfo);

    public IEnumerable<T> ReadAt<T>(int index, JsonTypeInfo<T> typeInfo)
    {
        var uIndex = index < 0 ? Count + index : index;

        if (uIndex >= Count || uIndex < 0) throw new IndexOutOfRangeException();

        return rowsMarkers[uIndex].Select(
            row => JsonSerializer.Deserialize(buffer.AsSpan(row), typeInfo) ??
                   throw new JsonException($"Failed to deserialize row to type {typeof(T).Name}")).ToList();
    }

    public object? GetScalarValue()
    {
        if (Count < 1 || rowsMarkers[0].Count < 1) return null;

        var row = rowsMarkers[0][0];

        var reader = new Utf8JsonReader(buffer.AsSpan(row));

        while (reader.Read())
        {
            if (reader.TokenType is not JsonTokenType.PropertyName) continue;
            reader.Read();

            return reader.TokenType switch
            {
                JsonTokenType.String => reader.GetString(),
                JsonTokenType.Number => reader.TryGetInt64(out var i) ? i : reader.GetDouble(),
                JsonTokenType.True => true,
                JsonTokenType.False => false,
                JsonTokenType.Null => null,
                _ => throw new InvalidOperationException()
            };
        }

        return null;
    }


    public static async Task<ResultReader> ParseAsync(
        Stream stream,
        HashSet<int>? resultsToIgnore = null,
        CancellationToken cancellationToken = default)
    {
        var memoryOwner = ArrayPool<byte>.Shared.Rent((int)stream.Length);

        var ms = new PooledByteBufferWriter();
        await using var writer = new Utf8JsonWriter(ms);

        try
        {
            writer.WriteStartArray();

            await writer.FlushAsync(cancellationToken);

            var read = await stream.ReadAsync(memoryOwner, cancellationToken);

            var rowsMarks = new List<List<long[]>>();
            var errors = new List<ExecutionError>();
            var stats = new List<ExecutionStats>();

            var baton = Initialize(
                memoryOwner.AsSpan(0, read),
                writer,
                rowsMarks,
                errors,
                stats,
                resultsToIgnore ?? []);

            writer.WriteEndArray();

            await writer.FlushAsync(cancellationToken);

            return new ResultReader(ms, baton, errors.ToArray(), stats.ToArray(), rowsMarks);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(memoryOwner);
        }
    }

    public static async Task<ResultReader> ParseAsync(
        HttpContent content,
        HashSet<int>? resultsToIgnore = null,
        CancellationToken cancellationToken = default)
    {
        await content.LoadIntoBufferAsync();

        await using var stream = await content.ReadAsStreamAsync(cancellationToken);

        stream.Seek(0, SeekOrigin.Begin);

        return await ParseAsync(stream, resultsToIgnore, cancellationToken);
    }

    private static string? Initialize(
        ReadOnlySpan<byte> bytes,
        Utf8JsonWriter writer,
        List<List<long[]>> states,
        List<ExecutionError> errors,
        List<ExecutionStats> stats,
        HashSet<int> resultsToIgnore)
    {
        var reader = new Utf8JsonReader(bytes);

        string? baton = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName && reader.ValueTextEquals("baton"u8) && reader.Read() &&
                reader.TokenType is JsonTokenType.String)
            {
                baton = reader.GetString();
                continue;
            }

            if (reader.TokenType == JsonTokenType.PropertyName && reader.ValueTextEquals("results"u8))
            {
                reader.Read();
                HandleResultsArray(ref reader, writer, states, errors, stats, resultsToIgnore);
            }
        }

        return baton;
    }

    private static void HandleResultsArray(
        ref Utf8JsonReader reader,
        Utf8JsonWriter writer,
        List<List<long[]>> marks,
        List<ExecutionError> errors,
        List<ExecutionStats> stats,
        HashSet<int> resultsToIgnore)
    {
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            if (reader.TokenType == JsonTokenType.PropertyName && reader.ValueTextEquals("type"u8))
            {
                reader.Read();

                if (reader.ValueTextEquals("error"u8))
                    HandleErrorResult(ref reader, errors);
                else if (reader.ValueTextEquals("batch"u8))
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                    {
                        if (reader.TokenType != JsonTokenType.PropertyName) continue;

                        if (reader.ValueTextEquals("step_results"u8))
                        {
                            var counter = -1;

                            reader.Read();

                            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                            {
                                counter++;

                                if (resultsToIgnore.Contains(counter))
                                {
                                    reader.Skip();
                                    continue;
                                }

                                if (reader.TokenType is JsonTokenType.Null)
                                {
                                    marks.Add([]);
                                    continue;
                                }

                                ReadResults(ref reader, writer, marks, stats);
                            }

                            continue;
                        }

                        if (reader.ValueTextEquals("step_errors"u8) &&
                            reader.Read())
                            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                            {
                                if (reader.TokenType == JsonTokenType.Null) continue;

                                HandleErrorResult(ref reader, errors);
                            }
                    }
                else if (reader.ValueTextEquals("execute"u8))
                    while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject)
                        if (reader.TokenType is JsonTokenType.PropertyName && reader.ValueTextEquals("result"u8))
                            ReadResults(ref reader, writer, marks, stats);
            }
    }

    private static void HandleErrorResult(
        ref Utf8JsonReader reader,
        List<ExecutionError> errors)
    {
        if (reader.TokenType is JsonTokenType.Null) return;

        string? message = null;
        string? code = null;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            var isPropName = reader.TokenType == JsonTokenType.PropertyName;

            switch (isPropName)
            {
                case true when reader.ValueTextEquals("message"u8):
                    reader.Read();
                    message = reader.GetString();
                    break;
                case true when reader.ValueTextEquals("code"u8):
                    reader.Read();
                    code = reader.GetString();
                    break;
            }
        }

        if (message is not null) errors.Add(new ExecutionError(message, code));
    }

    private static void ReadResults(
        ref Utf8JsonReader reader,
        Utf8JsonWriter writer,
        List<List<long[]>> marks,
        List<ExecutionStats> stats)
    {
        var cols = new List<byte[]>();
        var rows = new List<long[]>();

        var rowsRead = 0;
        var affectedRows = 0;
        var rowsWritten = 0;
        double queryDurationInMilliseconds = 0;
        string? lastInsertedRowId = null;
        string? replicationIndex = null;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName) continue;

            if (reader.ValueTextEquals("rows_read"u8) && reader.Read())
            {
                rowsRead = reader.GetInt32();
                continue;
            }

            if (reader.ValueTextEquals("rows_written"u8) && reader.Read())
            {
                rowsWritten = reader.GetInt32();
                continue;
            }

            if (reader.ValueTextEquals("affected_row_count"u8) && reader.Read())
            {
                affectedRows = reader.GetInt32();
                continue;
            }

            if (reader.ValueTextEquals("query_duration_ms"u8) && reader.Read())
            {
                queryDurationInMilliseconds = reader.GetDouble();
                continue;
            }

            if (reader.ValueTextEquals("last_insert_rowid"u8) && reader.Read())
            {
                lastInsertedRowId = reader.GetString();
                continue;
            }

            if (reader.ValueTextEquals("replication_index"u8) && reader.Read())
            {
                replicationIndex = reader.GetString();
                continue;
            }

            if (reader.ValueTextEquals("cols"u8) && reader.Read())
            {
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                    if (reader.TokenType is JsonTokenType.PropertyName && reader.ValueTextEquals("name"u8) &&
                        reader.Read())
                        cols.Add(reader.ValueSpan.ToArray());

                continue;
            }

            if (reader.ValueTextEquals("rows"u8) && reader.Read())
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    writer.WriteStartObject();

                    var init = writer.BytesCommitted + writer.BytesPending - 1;

                    var colIndex = 0;

                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                    {
                        string? valueType = null;

                        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                        {
                            if (reader.TokenType is not JsonTokenType.PropertyName) continue;

                            if (valueType is null && reader.ValueTextEquals("type"u8) && reader.Read())
                                valueType = reader.GetString();
                            else if (reader.ValueTextEquals("base64"u8) && reader.Read())
                                writer.WriteBase64String(cols[colIndex], reader.GetBytesFromBase64());
                            else if (reader.ValueTextEquals("value"u8) && reader.Read())
                                switch (valueType)
                                {
                                    case not null when reader.TokenType == JsonTokenType.Null:
                                        writer.WriteNull(cols[colIndex]);
                                        break;
                                    case "null":
                                        writer.WriteNull(cols[colIndex]);
                                        break;
                                    case "float":
                                        var val = reader.GetDouble();
                                        writer.WriteNumber(cols[colIndex], val);
                                        break;
                                    case "integer":
                                        if (long.TryParse(reader.GetString(), out var longValue))
                                            writer.WriteNumber(cols[colIndex], longValue);
                                        else
                                            writer.WriteNull(cols[colIndex]);

                                        break;
                                    default:
                                        writer.WriteString(cols[colIndex], reader.ValueSpan);
                                        break;
                                }
                        }

                        colIndex++;
                    }

                    writer.WriteEndObject();

                    var length = writer.BytesCommitted + writer.BytesPending - init;

                    rows.Add([init, length]);
                }
        }

        stats.Add(
            new ExecutionStats(
                rowsRead,
                affectedRows,
                rowsWritten,
                queryDurationInMilliseconds,
                lastInsertedRowId,
                replicationIndex));

        marks.Add(rows);
    }
}
