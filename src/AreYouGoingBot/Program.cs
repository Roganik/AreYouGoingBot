using System.Reflection;
using AreYouGoingBot;
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
    .ConfigureServices(services => { services.AddHostedService<Worker>(); })
    .Build();

await host.RunAsync();