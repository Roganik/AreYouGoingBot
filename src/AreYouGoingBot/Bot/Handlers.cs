using AreYouGoingBot.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static AreYouGoingBot.Constants.InputMessages;

namespace AreYouGoingBot.Bot;

public class Handlers
{
    private readonly TelegramBotClient _bot;

    private readonly AttendersManager _attenders;
    private readonly Commands _commands;
    private readonly ILogger _logger;

    public Handlers(string telegramBotToken, CancellationToken cancellationToken, AttendersDb db, ILoggerFactory loggerFactory)
    {
        _bot = new TelegramBotClient(telegramBotToken);
        _bot.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: PollingErrorHandler,
            cancellationToken: cancellationToken
        );
        _attenders = new AttendersManager(db);
        _commands = new Commands(_bot);
        _logger = loggerFactory.CreateLogger("BotHandlers");
    }
    
    private Task PollingErrorHandler(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.Log(LogLevel.Error, "PollingErrorHandler: {0}", errorMessage);
        return Task.CompletedTask;
    }
    
    private Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        var handler = update.Type switch
        {
            // UpdateType.Unknown:
            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            // UpdateType.Poll:
            UpdateType.CallbackQuery      => BotOnCallbackQueryReceived(botClient, update.CallbackQuery!),
            UpdateType.Message => BotOnMessageReceived(botClient, update.Message!),
            UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage!),
            _ => Task.CompletedTask, 
        };

        return handler;
    }
    
    private async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        await botClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            text: $"Received {callbackQuery.Data}. Updating participants...");
        
        // todo: similar logic in BotOnMessageReceived, refactor.
        var msg = callbackQuery.Message;
        var username = $"{callbackQuery.From.FirstName} {callbackQuery.From.LastName}";
        if (string.IsNullOrWhiteSpace(username))
        {
            username = callbackQuery.From.Username;
        }
        var chatUser = new ChatUser(msg.Chat.Id, username, callbackQuery.From.Id);

        if (callbackQuery.Data == AddUser)
        {
            if (!_attenders.Contains(chatUser))
            {
                _attenders.Add(chatUser);
            }
        }
        else if (callbackQuery.Data == RemoveUser)
        {
            _attenders.Remove(chatUser);
        }
        else
        {
            throw new NotImplementedException($"callback not supported: {callbackQuery.Data}");
        }
        
        await ShowList(msg.Chat.Id);
    }

    private async Task BotOnMessageReceived(object sender, Message message)
    {
        try
        {
            if (message?.Type != MessageType.Text)
            {
                return;
            }
            var username = $"{message.From.FirstName} {message.From.LastName}";
            if (string.IsNullOrWhiteSpace(username))
            {
                username = message.From.Username;
            }

            var chatUser = new ChatUser(message.Chat.Id, username, message.From.Id);

            var task = message.Text switch
            {
                AddUser => AddChatUserAndShowList(chatUser),
                RemoveUser => Remove(chatUser),
                WhoIsGoing => ShowList(chatUser.ChatId),
                ClearList => Task.Run(() =>
                {
                    _attenders.RemoveAll(chatUser.ChatId);
                }),
                _ => Task.Run(() => { })
            };
            
            await task;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private async Task AddChatUserAndShowList(ChatUser chatUser)
    {
        if (!_attenders.Contains(chatUser))
        {
            _attenders.Add(chatUser);
            await ShowList(chatUser.ChatId);
        }
    }
    
    private async Task Remove(ChatUser chatUser)
    {
        _attenders.Remove(chatUser);
        await ShowList(chatUser.ChatId);
    }
    
    private async Task ShowList(long chatId)
    {
        var typingJob = _commands.SendTyping(chatId);

        var oldMsgId = await _attenders.GetEventMessageId(chatId);
        var usernames = _attenders.GetUsernames(chatId);
        
        await typingJob;
        var msg = await _commands.ShowParticipants(chatId, usernames);
        if (msg == null)
        {
            // no new message were posted
            return;
        }
        
        await _attenders.SaveEventMessageId(chatId, msg.MessageId);
        await _commands.DeleteMessage(chatId, oldMsgId);
    }
}