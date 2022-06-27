using AreYouGoingBot;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AreYouGoingBot.Cfg;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingCtx, configuration) =>
    {
        configuration.AddAppConfigurationProviders();
    })
    .ConfigureServices((hostBuilderCtx, services) =>
    {
        var connectionString = hostBuilderCtx.Configuration.GetAppConnectionString();
        services.AddDbContext<AreYouGoingBot.Storage.AttendersDb>(options => options.UseSqlite(connectionString));

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();