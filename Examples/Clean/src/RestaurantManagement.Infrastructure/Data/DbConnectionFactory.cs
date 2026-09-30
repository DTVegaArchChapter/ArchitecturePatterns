using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace RestaurantManagement.Infrastructure.Data;

public interface IDbConnectionFactory
{
    DbConnection CreateConnection();
}

public sealed class SqliteConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public DbConnection CreateConnection() => new SqliteConnection(connectionString);
}
