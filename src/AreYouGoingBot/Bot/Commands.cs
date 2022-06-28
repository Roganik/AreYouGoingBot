using Telegram.Bot;

namespace AreYouGoingBot.Bot;

public class Commands
{
    private readonly TelegramBotClient _bot;
    
    public Commands(TelegramBotClient bot)
    {
        _bot = bot;
    }
    
    public async Task ShowParticipants(long chatId, List<string> usernames)
    {
        if (!usernames.Any())
        {
            return;
        }

        var text =  string.Join(Environment.NewLine, 
            usernames
            .Select((Username, index) => $"{index+1}. {Username}" ));
        
        await _bot.SendTextMessageAsync(chatId, text);
    }
}