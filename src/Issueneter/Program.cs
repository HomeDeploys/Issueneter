using Hangfire;
using Issueneter.Application;
using Issueneter.Application.Parser;
using Issueneter.Infrastructure.Background;
using Issueneter.Infrastructure.Database;
using Issueneter.Infrastructure.Github;
using Issueneter.Infrastructure.Telegram;
using Issueneter.Utils;
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

builder.Host.UseSerilog((_, lc) => lc.WriteTo.Console().MinimumLevel.Debug());

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseHangfireDashboard("/hangfire", new DashboardOptions()
{
    Authorization = [new UnsecureDashboardAuthorizationFilter()]
});

app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.RunMigrations();
}

app.Services.RunTelegramHandler();
app.Run();