using FluentMigrator;

namespace Issueneter.Infrastructure.Database.Migrations;

[Migration(002)]
public class ProviderSnapshotTable : Migration {
    
    public override void Up()
    {
        Execute.Sql("""
            CREATE TABLE IF NOT EXISTS provider_snapshot(
                worker_id bigint not null primary key,
                data jsonb
            );
        """);
    }

    public override void Down()
    {
        Execute.Sql("""
            DROP TABLE IF EXISTS provider_snapshot;            
        """);
    }
}