using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using static AreYouGoingBot.Constants.InputMessages;
using static AreYouGoingBot.RegexPatterns;

namespace AreYouGoingBot
{
    public class Program
    {
        private const int DefaultAttendersLimit = 15;

        private static Dictionary<long, int> _attendersLimitForChat = new();

        private static TelegramBotClient _client;

        private static Dictionary<long, List<ChatUser>> _attenders = new();

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
                Console.WriteLine("Invalid token.");
            }
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            try
            {
                var message = messageEventArgs.Message;
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
                    ClearList => Task.Run(() => Clear(chatUser.ChatId)),
                    WhatIsTheLimit => ShowLimit(chatUser.ChatId),
                    var text when new Regex(SetLimitPattern).IsMatch(text) => SetLimit(GetLimit(text), chatUser.ChatId),
                    // var text when new Regex(AddRangeByNumberPattern).IsMatch(text) => AddRange(GetAttendersByNumber(text), chatUser),
                    // var text when new Regex(AddRangeByPlusesPattern).IsMatch(text) => AddRange(GetAttendersByPluses(text), chatUser),
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
            var file = CheckFileAndFillAttendersList(chatUser);

            if (GetAttendersCount(chatUser.ChatId) < GetAttendersLimitByChatOrDefault(chatUser.ChatId) &&
                !_attenders[chatUser.ChatId].Contains(chatUser))
            {
                _attenders[chatUser.ChatId].Add(chatUser);

                await File.AppendAllTextAsync(file, chatUser.Username + Environment.NewLine);

                await ShowList(chatUser.ChatId);
            }
        }

        private static string CheckFileAndFillAttendersList(ChatUser chatUser)
        {
            var file = $"{chatUser.ChatId}.txt";

            if (!File.Exists(file))
            {
                File.Create(file);
            }

            if (GetAttendersCount(chatUser.ChatId) == 0)
            {
                _attenders[chatUser.ChatId] = new List<ChatUser>();
                _attenders[chatUser.ChatId].AddRange(File.ReadLines(file)
                    .Select(username => new ChatUser(chatUser.ChatId, username))
                    .ToList());
            }

            return file;
        }

        private static async Task Remove(ChatUser chatUser)
        {
            CheckFileAndFillAttendersList(chatUser);
            
            var attender = _attenders.Get(chatUser.ChatId).Last(cu => cu == chatUser);
            _attenders.Get(chatUser.ChatId).Remove(attender);
            var usernames = _attenders[chatUser.ChatId].Select(x => x.Username).ToList();
            await File.WriteAllTextAsync($"{chatUser.ChatId}.txt",  string.Join(Environment.NewLine, usernames));

            await ShowList(chatUser.ChatId);
        }

        private static async Task AddRange(int attendersNumber, ChatUser chatUser)
        {
            var remainingPlaces = GetAttendersLimitByChatOrDefault(chatUser.ChatId) - GetAttendersCount(chatUser.ChatId);
            var users = Enumerable.Range(0, attendersNumber)
                .Select(x => chatUser)
                .Take(Math.Min(remainingPlaces, attendersNumber))
                .ToList();

            _attenders.Get(chatUser.ChatId).AddRange(users);
            var file = $"{chatUser.ChatId}.txt";
            var usernames = _attenders.Get(chatUser.ChatId).Select(x => x.Username).ToList();
            await File.WriteAllTextAsync(file, string.Join(Environment.NewLine, usernames));

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
            var file = $"{chatId}.txt";

            if (!File.Exists(file))
            {
                File.Create(file);
            }

            if (GetAttendersCount(chatId) == 0)
            {
                _attenders[chatId] = new List<ChatUser>();
                _attenders[chatId].AddRange(File.ReadLines(file)
                    .Select(username => new ChatUser(chatId, username))
                    .ToList());
            }

            var text =  string.Join(Environment.NewLine, _attenders
                .Get(chatId)
                .Select((chatUser, index) => $"{index+1}. {chatUser.Username}" ));

            if (GetAttendersCount(chatId) == GetAttendersLimitByChatOrDefault(chatId))
            {
                text += "\r\nRecruitment Closed.";
            }

            return text;
        }

        private static void Clear(long chatId)
        {
            _attenders?.Get(chatId)?.Clear();
            var file = $"{chatId}.txt";

            if (File.Exists(file))
            {
                File.WriteAllText($"{chatId}.txt", string.Empty);
            }
        }

        private static async Task SetLimit(int limit, long chatId)
        {
            _attendersLimitForChat[chatId] = limit;
            await ShowLimit(chatId);

            if (GetAttendersCount(chatId) >= limit)
            {
                _attenders[chatId] = _attenders.Get(chatId).Take(limit).ToList();
                var usernames = _attenders[chatId].Select(x => x.Username).ToList();
                await File.WriteAllTextAsync($"{chatId}.txt",  string.Join(Environment.NewLine, usernames));
                await ShowList(chatId);
            }
        }

        private static async Task ShowLimit(long chatId)
        {
            var limit = GetAttendersLimitByChatOrDefault(chatId);
            var text = $"Limit: {limit}.";

            await _client.SendTextMessageAsync(chatId, text);
        }

        private static int GetAttendersLimitByChatOrDefault(long chatId)
        {
            if (!_attendersLimitForChat.TryGetValue(chatId, out var limit))
            {
                limit = DefaultAttendersLimit;
            }

            return limit;
        }

        private static int GetLimit(string text)
        {
            return !int.TryParse(GetDigitsFromString(text), out var limit)
                ? DefaultAttendersLimit
                : limit;
        }

        private static int GetAttendersByPluses(string text)
        {
            return text.Count(x => x == '+');
        }

        private static int GetAttendersByNumber(string text)
        {
            int.TryParse(GetDigitsFromString(text), out var newAttenders);

            return newAttenders;
        }

        private static string GetDigitsFromString(string text)
        {
            return Regex.Replace(text, @"\D", "");
        }

        private static int GetAttendersCount(long chatId)
        {
            return _attenders.Get(chatId)?.Count ?? 0;
        }
    }
}