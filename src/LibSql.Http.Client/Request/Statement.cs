namespace LibSql.Http.Client.Request;

/// <summary>
/// Statement to execute
/// </summary>
/// <param name="sql">SQL query</param>
/// <param name="args">Positional args</param>
public class Statement(string sql, object?[]? args = null)
{
    /// <summary>
    /// SQL Query
    /// </summary>
    public string Sql { get; } = sql;
    /// <summary>
    /// Named Args
    /// </summary>
    public Dictionary<string, object?>? NamedArgs { get; }
    /// <summary>
    /// Positional Args
    /// </summary>
    public object?[]? Args { get; } = args;

    /// <inheritdoc />
    public Statement(string sql, Dictionary<string, object?> namedArgs) : this(sql)
    {
        NamedArgs = namedArgs;
    }

    /// <summary>
    /// Implicitly create a Statement from a SQL query
    /// </summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    public static implicit operator Statement(string sql) => new(sql);
    
    /// <summary>
    /// Implicitly create a statement from a (sql, positional args) tuple
    /// </summary>
    /// <param name="stmt"></param>
    /// <returns></returns>
    public static implicit operator Statement((string, object?[]) stmt) => new(stmt.Item1, stmt.Item2);
    
    /// <summary>
    /// Implicitly create a statement from a (sql, named args) tuple
    /// </summary>
    /// <param name="stmt"></param>
    /// <returns></returns>
    public static implicit operator Statement((string, Dictionary<string, object?>) stmt) => new(stmt.Item1, stmt.Item2);
}