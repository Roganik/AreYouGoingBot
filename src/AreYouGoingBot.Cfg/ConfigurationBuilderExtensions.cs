using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace AreYouGoingBot.Cfg;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddAppConfigurationProviders(this IConfigurationBuilder configuration)
    {
        configuration.AddUserSecrets(Assembly.GetExecutingAssembly());
        configuration.AddEnvironmentVariables(prefix: "AREYOUGOINGBOT_");
        
        return configuration;
    }

    public static string GetAppConnectionString(this IConfiguration cfg)
    {
        var connectionString = cfg.GetConnectionString("AreYouGoingDb");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("WARN: Connection sting looks empty. Please configure it with set_secrets.sh");
            Console.ResetColor();
        }

        return connectionString;
    }
}