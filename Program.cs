using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace AreYouGoingBot
{
    public class Program
    {
        private static TelegramBotClient _client;

        private static List<ChatUser> _attenders = new List<ChatUser>();

        private static readonly string[] AvailableCommands = { "+", "-", "?", "0" };

        public static void Main()
        {
            Console.WriteLine("Enter bot token:");
            var token = Console.ReadLine();
            try
            {
                _client = new TelegramBotClient(token);
                _client.OnMessage += BotOnMessageReceived;
                _client.OnMessageEdited += BotOnMessageReceived;
                _client.StartReceiving();
                Console.ReadLine();
                _client.StopReceiving();
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid token");
            }
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            if (message?.Type != MessageType.Text || !AvailableCommands.Contains(message.Text)) return;
            var username = $"{message.From.FirstName} {message.From.LastName}";
            if (string.IsNullOrWhiteSpace(username))
            {
                username = message.From.Username;
            }

            var chatUser = new ChatUser(message.Chat.Id, username);

            var task = message.Text switch
            {
                "+" => Add(chatUser),
                "-" => Remove(chatUser),
                "?" => ShowList(chatUser.ChatId),
                "0" => Task.Run(() => Clear(chatUser.ChatId)),
                _ => Task.Run(() => { })
            };
            await task;
        }

        private static void Clear(long chatId)
        {
            _attenders.RemoveAll(x => x.ChatId == chatId);
        }

        private static async Task ShowList(long chatId)
        {
            var text = GetList(chatId);
            if (!string.IsNullOrWhiteSpace(text))
            {
                await _client.SendTextMessageAsync(chatId, text);
            }
        }

        private static async Task Add(ChatUser chatUser)
        {
            if (_attenders.Count < 6)
            {
                _attenders.Add(chatUser);
                await ShowList(chatUser.ChatId);
            }
        }

        private static async Task Remove(ChatUser chatUser)
        {
            var attender = _attenders.Last(x => x == chatUser);
            _attenders.Remove(attender);

            await ShowList(chatUser.ChatId);
        }

        private static string GetList(long chatId)
        {
            var text =  string.Join(Environment.NewLine, _attenders
                .Where(x => x.ChatId == chatId)
                .Select((x, index) => $"{index+1}. {x.Username}" ));

            if (_attenders.Count == 6)
            {
                text += "\r\nНабор закрыт.";
            }

            return text;
        }

        private class ChatUser
        {
            private bool Equals(ChatUser other)
            {
                return ChatId == other.ChatId && Username == other.Username;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == GetType() && Equals((ChatUser) obj);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(ChatId, Username);
            }

            public static bool operator ==(ChatUser left, ChatUser right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(ChatUser left, ChatUser right)
            {
                return !Equals(left, right);
            }

            public ChatUser(long chatId, string username)
            {
                ChatId = chatId;
                Username = username;
            }

            public long ChatId { get; }

            public string Username { get; }
        }
    }
}