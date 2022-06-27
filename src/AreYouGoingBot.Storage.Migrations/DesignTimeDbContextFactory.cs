using System.Reflection;
using AreYouGoingBot.Cfg;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AreYouGoingBot.Storage.Migrations;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AttendersDb>
{
    public AttendersDb CreateDbContext(string[] args)
    {
        var cfg = new ConfigurationBuilder()
            .AddAppConfigurationProviders()
            .Build();

        var connectionString = cfg.GetAppConnectionString();
        var dbContextOptions = new DbContextOptionsBuilder<AttendersDb>()
            .UseSqlite(connectionString, builder => builder.MigrationsAssembly("AreYouGoingBot.Storage.Migrations"))
            .Options;

        return new AttendersDb(dbContextOptions);
    }
}