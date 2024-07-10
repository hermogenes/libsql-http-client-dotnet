using LibSql.Http.Client.Response;

namespace LibSql.Http.Client.Exceptions;

/// <inheritdoc />
public class LibSqlClientException(string message, Exception? innerException = default)
    : Exception(message, innerException)
{
    private const string TabConstant = "\t";

    /// <summary>
    /// Create exception using errors from pipeline request
    /// </summary>
    /// <param name="executionErrors">Errors from pipeline response</param>
    public LibSqlClientException(IReadOnlyCollection<ExecutionError> executionErrors) : this(
        FormatErrorMessage(executionErrors))
    {
    }

    private static string FormatErrorMessage(IReadOnlyCollection<ExecutionError> executionErrors)
    {
        var joinedMessages = string.Join(
            Environment.NewLine,
            executionErrors.Select(
                (error, index) =>
                {
                    var codePart = error.Code is null ? string.Empty : $"({error.Code}) ";

                    return $"{TabConstant}[{index}]: {codePart}{error.Message}";
                }));

        return $"[LibSqlPipelineError] The request failed.{Environment.NewLine}{joinedMessages}";
    }
}