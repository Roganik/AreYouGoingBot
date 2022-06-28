using AreYouGoingBot.Constants;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AreYouGoingBot.Bot;

public class Commands
{
    private readonly TelegramBotClient _bot;
    
    public Commands(TelegramBotClient bot)
    {
        _bot = bot;
    }

    public Task SendTyping(long chatId)
    {
        return _bot.SendChatActionAsync(chatId, ChatAction.Typing);
    }

    public Task ShowParticipants(long chatId, List<string> usernames)
    {
        if (!usernames.Any())
        {
            return Task.CompletedTask;
        }

        var text =  string.Join(Environment.NewLine, 
            usernames
            .Select((Username, index) => $"{index+1}. {Username}" ));
        
        InlineKeyboardMarkup inlineKeyboard = new(
            new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(InputMessages.AddUser, InputMessages.AddUser),
                    InlineKeyboardButton.WithCallbackData(InputMessages.RemoveUser, InputMessages.RemoveUser),
                },
            });

        return _bot.SendTextMessageAsync(chatId: chatId,
            text: text,
            replyMarkup: inlineKeyboard);
    }

    public Task DeleteMessage(long chatId, int messageId)
    {
        return _bot.DeleteMessageAsync(chatId, messageId);
    }
}