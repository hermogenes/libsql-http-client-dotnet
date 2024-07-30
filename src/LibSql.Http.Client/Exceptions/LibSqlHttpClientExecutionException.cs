using LibSql.Http.Client.Response;

namespace LibSql.Http.Client.Exceptions;

/// <inheritdoc />
public class LibSqlClientExecutionException : LibSqlClientException
{
    private const string TabConstant = "\t";

    /// <summary>
    ///     Create exception using errors from pipeline request
    /// </summary>
    /// <param name="executionErrors">Errors from pipeline response</param>
    public LibSqlClientExecutionException(IReadOnlyCollection<ExecutionError> executionErrors) : base(
        FormatErrorMessage(executionErrors))
    {
        ExecutionErrors = executionErrors;
    }

    /// <summary>
    ///     Errors from pipeline response
    /// </summary>
    public IReadOnlyCollection<ExecutionError> ExecutionErrors { get; }

    /// <summary>
    ///     First error from pipeline response
    /// </summary>
    public ExecutionError Error => ExecutionErrors.First();

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
