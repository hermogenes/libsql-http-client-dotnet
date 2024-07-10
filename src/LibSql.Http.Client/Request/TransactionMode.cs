namespace LibSql.Http.Client.Request;

/// <summary>
/// Transaction mode for the pipeline request
/// </summary>
public enum TransactionMode
{
    /// <summary>
    /// No transaction required
    /// </summary>
    None = 0,
    /// <summary>
    /// BEGIN IMMEDIATE
    /// </summary>
    WriteImmediate = 1,
    /// <summary>
    /// BEGIN DEFERRED
    /// </summary>
    Deferred = 2,
    /// <summary>
    /// BEGIN TRANSACtION READONLY
    /// </summary>
    ReadOnly = 3
}