using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static AreYouGoingBot.Constants.InputMessages;

namespace AreYouGoingBot.Bot;

public static class Handlers
{
    private static TelegramBotClient _client;

    private static readonly AttendersManager _attenders = new(); 

    public static void Init(string telegramBotToken, CancellationToken cancellationToken)
    {
        _client = new TelegramBotClient(telegramBotToken);
        _client.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: PollingErrorHandler,
            cancellationToken: cancellationToken
        );
    }
    
        public static Task PollingErrorHandler(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.Write(errorMessage);
            return Task.CompletedTask;
        }
        
        static Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
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
                UpdateType.Message => BotOnMessageReceived(botClient, update.Message!),
                UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage!),
            };

            return handler;
        }

        private static async Task BotOnMessageReceived(object sender, Message message)
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

                var chatUser = new ChatUser(message.Chat.Id, username);

                var task = message.Text switch
                {
                    AddUser => AddChatUserAndShowList(chatUser),
                    RemoveUser => Remove(chatUser),
                    WhoIsGoing => ShowList(chatUser.ChatId),
                    ClearList => Task.Run(() => _attenders.RemoveAll(chatUser.ChatId)),
                    _ => Task.Run(() => { })
                };
                
                await task;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static async Task AddChatUserAndShowList(ChatUser chatUser)
        {
            if (!_attenders.Contains(chatUser))
            {
                _attenders.Add(chatUser);
                await ShowList(chatUser.ChatId);
            }
        }
        
        private static async Task Remove(ChatUser chatUser)
        {
            _attenders.Remove(chatUser);
            await ShowList(chatUser.ChatId);
        }
        
        private static async Task ShowList(long chatId)
        {
            var text = GetList(chatId);
            if (!string.IsNullOrWhiteSpace(text))
            {
                await _client.SendTextMessageAsync(chatId, text);
            }
        }

        private static string GetList(long chatId)
        {
            var text =  string.Join(Environment.NewLine, 
                _attenders.GetUsernames(chatId)
                .Select((Username, index) => $"{index+1}. {Username}" ));

            return text;
        }
}