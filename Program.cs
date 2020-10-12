using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using static AreYouGoingBot.Commands;
using static AreYouGoingBot.RegexPatterns;

namespace AreYouGoingBot
{
    public class Program
    {
        private const int DefaultAttendersLimit = 6;

        private static Dictionary<long, int> _attendersLimitForChat = new Dictionary<long, int>();

        private static TelegramBotClient _client;

        private static List<ChatUser> _attenders = new List<ChatUser>();

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
                AddUser => Add(chatUser),
                RemoveUser => Remove(chatUser),
                WhoIsGoing => ShowList(chatUser.ChatId),
                ClearList => Task.Run(() => Clear(chatUser.ChatId)),
                WhatIsTheLimit => ShowLimit(chatUser.ChatId),
                var text when new Regex(SetLimitPattern).IsMatch(text) => SetLimit(GetLimit(text), chatUser.ChatId),
                var text when new Regex(AddRangeByNumberPattern).IsMatch(text) => AddRange(GetAttendersByNumber(text), chatUser),
                var text when new Regex(AddRangeByPlusesPattern).IsMatch(text) => AddRange(GetAttendersByPluses(text), chatUser),
                _ => Task.Run(() => { })
            };
            await task;
        }

        private static async Task Add(ChatUser chatUser)
        {
            if (_attenders.Count < GetAttendersLimitByChatOrDefault(chatUser.ChatId))
            {
                _attenders.Add(chatUser);
                await ShowList(chatUser.ChatId);
            }
        }

        private static async Task Remove(ChatUser chatUser)
        {
            var attender = _attenders.Last(cu => cu == chatUser);
            _attenders.Remove(attender);

            await ShowList(chatUser.ChatId);
        }

        private static async Task AddRange(int attendersNumber, ChatUser chatUser)
        {
            var remainingPlaces = GetAttendersLimitByChatOrDefault(chatUser.ChatId) - _attenders.Count;
            var users = Enumerable.Range(0, attendersNumber)
                .Select(x => chatUser)
                .Take(Math.Min(remainingPlaces, attendersNumber));

            _attenders.AddRange(users);

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
            var text =  string.Join(Environment.NewLine, _attenders
                .Where(chatUser => chatUser.ChatId == chatId)
                .Select((chatUser, index) => $"{index+1}. {chatUser.Username}" ));

            if (_attenders.Count == GetAttendersLimitByChatOrDefault(chatId))
            {
                text += "\r\nНабор закрыт.";
            }

            return text;
        }

        private static void Clear(long chatId)
        {
            _attenders.RemoveAll(chatUser => chatUser.ChatId == chatId);
        }

        private static async Task SetLimit(int limit, long chatId)
        {
            _attendersLimitForChat[chatId] = limit;
            await ShowLimit(chatId);

            if (_attenders.Count >= limit)
            {
                _attenders = _attenders.Take(limit).ToList();
                await ShowList(chatId);
            }
        }

        private static async Task ShowLimit(long chatId)
        {
            var limit = GetAttendersLimitByChatOrDefault(chatId);
            var text = $"Текущий лимит участников: {limit}.";

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
    }
}