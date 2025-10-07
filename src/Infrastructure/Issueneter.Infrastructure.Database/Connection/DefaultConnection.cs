using System.Data.Common;

namespace Issueneter.Infrastructure.Database.Connection;

internal class DefaultConnection : AbstractConnection
{
    public DefaultConnection(DbConnection connection)
    {
        Connection = connection;
    }

    protected override DbConnection Connection { get; }

    public override ValueTask DisposeAsync()
    {
        return Connection.DisposeAsync();
    }
}