using System.Collections.Generic;
using System.Linq;
using AreYouGoingBot.Storage;
using AreYouGoingBot.Storage.Models;
using Microsoft.EntityFrameworkCore;

namespace AreYouGoingBot.Bot;

public class AttendersManager
{
    private readonly AttendersDb _db;

    public AttendersManager(AttendersDb dbDb)
    {
        _db = dbDb;
    }

    private ChatEvent GetEventWithAttendersSync(long chatId)
    {
        var @event = _db.ChatEvents
            .Include(e => e.Participants)
            .FirstOrDefault(e => e.TelegramChatId == chatId);
        
        if (@event == null)
        {
            @event = new ChatEvent() { TelegramChatId = chatId };
        }

        return @event;
    }

    public bool Contains(ChatUser chatUser)
    {
        var @event = GetEventWithAttendersSync(chatUser.ChatId);
        return @event.Participants.Any(a => a.TelegramUserId == chatUser.UserId);
    }

    public void Add(ChatUser chatUser)
    {
        var @event = GetEventWithAttendersSync(chatUser.ChatId);
        
        var attender = @event.Participants.FirstOrDefault(a => a.TelegramUserId == chatUser.UserId);
        if (attender == null)
        {
            attender = new Storage.Models.ChatEventParticipant() {Name = chatUser.Username, TelegramUserId = chatUser.UserId};
            @event.Participants.Add(attender);
        }

        if (@event.ChatEventId == default)
        {
            _db.Add(@event);
        }
        else
        {
            _db.Update(@event);
        }
        
        _db.SaveChanges();
    }

    public void Remove(ChatUser chatUser)
    {
        var @event = GetEventWithAttendersSync(chatUser.ChatId);
        var attender = @event.Participants.FirstOrDefault(a => a.TelegramUserId == chatUser.UserId);

        if (attender == null)
        {
            return;
        }

        @event.Participants.Remove(attender);
        _db.Update(@event);
        _db.SaveChanges();
    }

    public List<string> GetUsernames(long chatId)
    {
        var @event = GetEventWithAttendersSync(chatId);
        
        return @event.Participants.Select(x => x.Name).ToList();
    }

    public void RemoveAll(long chatId)
    {
        var @event = GetEventWithAttendersSync(chatId);
        _db.Remove(@event);
        _db.SaveChanges();
    }
}