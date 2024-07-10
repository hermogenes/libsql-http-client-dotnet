using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;
using LibSql.Http.Client.Exceptions;
using LibSql.Http.Client.Response;

namespace LibSql.Http.Client.Interfaces;

/// <summary>
///     Interface for reading result from a pipeline execution of SQL statements.
///     The reader is a forward-only reader, so it's not possible to go back to a previous result.
/// </summary>
/// <remarks>It's highly recommended dispose the object after process the data to free up memory</remarks>
public interface IResultReader : IDisposable
{
    /// <summary>
    /// Number of Result sets
    /// </summary>
    int Count { get; }

    /// <summary>
    ///     Number of rows affected by the execution of the SQL statements if any.
    /// </summary>
    int AffectedRows { get; }

    /// <summary>
    ///     Get the value of the first row of the first column of the result set.
    /// </summary>
    /// <returns></returns>
    object? GetScalarValue();

    /// <summary>
    ///     Throw an exception if there are errors in the execution of the SQL statements.
    /// </summary>
    /// <exception cref="LibSqlClientException"></exception>
    void ThrowIfError();

    /// <summary>
    /// Check if there are more results to process
    /// </summary>
    /// <returns></returns>
    bool HasMoreResults();

    /// <summary>
    ///     Read the result set as a sequence of data of <typeparamref name="T" />.
    /// </summary>
    /// <param name="typeInfo">Metadata information for JSON deserialization.</param>
    /// <typeparam name="T">The type of the result object.</typeparam>
    /// <returns></returns>
    IEnumerable<T> Read<T>(JsonTypeInfo<T> typeInfo);

    /// <summary>
    ///     Read the result set as a sequence of data of <typeparamref name="T" />.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="typeInfo">Metadata information for JSON deserialization.</param>
    /// <typeparam name="T">The type of the result object.</typeparam>
    /// <returns></returns>
    IEnumerable<T> ReadAt<T>(int index, JsonTypeInfo<T> typeInfo);
}
