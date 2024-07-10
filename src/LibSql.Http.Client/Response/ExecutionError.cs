namespace LibSql.Http.Client.Response;

/// <summary>
/// Execution error
/// </summary>
/// <param name="Message"></param>
/// <param name="Code"></param>
public record ExecutionError(string Message, string? Code = null);