using System.Reflection;
using AreYouGoingBot;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingCtx, configuration) =>
    {
        #if DEBUG
        configuration.AddUserSecrets(Assembly.GetExecutingAssembly());
        #endif
        
        configuration.AddEnvironmentVariables(prefix: "AREYOUGOINGBOT_");
    })
    .ConfigureServices((hostBuilderCtx, services) =>
    {
        var connectionString = hostBuilderCtx.Configuration.GetConnectionString("AreYouGoingDb");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("Connections string is not configured. " +
                                        "Please use env variables or set_secrets.sh");
        }
        services.AddDbContext<AreYouGoingBot.Storage.AttendersDb>(options => options.UseSqlite(connectionString));

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();