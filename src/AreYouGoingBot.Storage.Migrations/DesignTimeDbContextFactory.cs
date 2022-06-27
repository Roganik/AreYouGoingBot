using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AreYouGoingBot.Storage.Migrations;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AttendersDb>
{
    public AttendersDb CreateDbContext(string[] args)
    {
        var cfg = new ConfigurationBuilder()
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .Build();

        var connectionString = cfg.GetConnectionString("AreYouGoingDb");
        
        Console.WriteLine("Connection String = " + connectionString);
        if (string.IsNullOrEmpty(connectionString))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Connection sting looks empty. Please configure it with set_secrets.sh");
            Console.ResetColor();
        }

        var dbContextOptions = new DbContextOptionsBuilder<AttendersDb>()
            .UseSqlite(connectionString, builder => builder.MigrationsAssembly("AreYouGoingBot.Storage.Migrations"))
            .Options;

        return new AttendersDb(dbContextOptions);
    }
}