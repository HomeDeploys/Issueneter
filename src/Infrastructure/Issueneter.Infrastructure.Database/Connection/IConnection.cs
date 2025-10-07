namespace Issueneter.Infrastructure.Database.Connection;

internal interface IConnection : IAsyncDisposable
{
    Task Execute(string query, object? param = null);
    Task<T?> QuerySingleOrDefault<T>(string query, object? param = null);
}