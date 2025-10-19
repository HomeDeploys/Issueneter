using Issueneter.Application;
using Issueneter.Application.Parser;
using Issueneter.Infrastructure.Background;
using Issueneter.Infrastructure.Database;
using Issueneter.Infrastructure.Github;
using Issueneter.Infrastructure.Telegram;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddYamlFile("appsettings.yaml", optional: false);

var configPath = Environment.GetEnvironmentVariable("CONFIG_PATH");
var configFilePath = $"appsettings.{builder.Environment.EnvironmentName}.yaml";

if (!string.IsNullOrEmpty(configPath))
{
    configFilePath = Path.Combine(configPath, configFilePath);
}
builder.Configuration.AddYamlFile(configFilePath, optional: true);

builder.Services.AddSerilog();
builder.Services.AddControllers();
builder.Services
    .AddBackground(builder.Configuration)
    .AddDatabase(builder.Configuration)
    .AddGithub(builder.Configuration)
    .AddTelegram(builder.Configuration)
    .AddApplicationServices()
    .AddFilters();

builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console().MinimumLevel.Debug());

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.Services.RunMigrations();

app.Run();