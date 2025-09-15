using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.ApplicationInsights;
using Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        var pgd = PostgresData.FromEnv();

        var csb = new NpgsqlConnectionStringBuilder
        {
            Host = pgd.Host,
            Port = pgd.Port,
            Database = pgd.Database,
            Username = pgd.Username,
            Password = pgd.Password,
            SslMode = SslMode.Require
        };

        var dataSource = new NpgsqlDataSourceBuilder(csb.ConnectionString).Build();
        services.AddSingleton(dataSource);

        services.AddScoped<IVisitorRepository, VisitorRepository>();
        services.AddScoped<IVisitorService, VisitorService>();
    })
    .Build();

host.Run();

