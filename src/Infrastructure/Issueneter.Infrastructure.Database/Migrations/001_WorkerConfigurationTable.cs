using FluentMigrator;

namespace Issueneter.Infrastructure.Database.Migrations;

[Migration(001)]
public class WorkerConfigurationTable : Migration {
    public override void Up()
    {
        Execute.Sql("""
            CREATE TABLE IF NOT EXISTS worker_configuration(
                worker_id bigserial primary key,
                providerType smallint not null,
                providerTarget text not null,
                schedule text not null,
                filter text not null,
                clientType smallint not null,
                clientTarget text not null,
                template text not null,
            )
        """);
    }

    public override void Down()
    {
        Execute.Sql("""
            DROP TABLE IF EXISTS schedule;
        """);
    }
}