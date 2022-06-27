using AreYouGoingBot.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AreYouGoingBot;

public class Worker : BackgroundService
{
    private readonly CancellationTokenSource _cts;

    public Worker(ILoggerFactory logger, IConfiguration cfg, AttendersDb db)
    {
        var token = cfg["TELEGRAM_BOT_TOKEN"];
        _cts = new CancellationTokenSource();
        var cancellationToken = _cts.Token;
        var handlers = new Bot.Handlers(token, cancellationToken, db);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000*60, stoppingToken);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel(); // stop receiving updates
        return base.StopAsync(cancellationToken);
    }
}