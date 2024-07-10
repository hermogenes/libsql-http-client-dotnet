namespace LibSql.Http.Client.Response;

/// <summary>
/// Record to hold the stats of pipeline executions
/// </summary>
/// <param name="RowsRead">Number of rows read</param>
/// <param name="AffectedRows">Number of affected rows</param>
/// <param name="RowsWritten">Number of rows written</param>
/// <param name="QueryDurationInMilliseconds">Duration</param>
/// <param name="LastInsertedRowId"></param>
/// <param name="ReplicationIndex"></param>
public record ExecutionStats(
    int RowsRead = 0,
    int AffectedRows = 0,
    int RowsWritten = 0,
    double QueryDurationInMilliseconds = 0,
    string? LastInsertedRowId = null,
    string? ReplicationIndex = null);
