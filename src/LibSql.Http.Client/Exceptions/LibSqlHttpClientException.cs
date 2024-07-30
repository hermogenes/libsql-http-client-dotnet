namespace LibSql.Http.Client.Exceptions;

/// <inheritdoc />
public class LibSqlClientException(string message, Exception? innerException = default)
    : Exception(message, innerException)
{
}
