using System.Collections.Generic;
using System.Linq;

namespace AreYouGoingBot.Bot;

public class AttendersManager
{
    private Dictionary<long, List<ChatUser>> _attenders = new();

    public void Init(long chatId, List<ChatUser> users)
    {
        _attenders[chatId] = new List<ChatUser>();
        _attenders[chatId].AddRange(users);
    }
    
    public bool Contains(ChatUser chatUser)
    {
        return _attenders[chatUser.ChatId].Contains(chatUser);
    }

    public void Add(ChatUser chatUser)
    {
        _attenders[chatUser.ChatId].Add(chatUser);
    }

    public void Remove(ChatUser chatUser)
    {
        var attender = _attenders.Get(chatUser.ChatId).Last(cu => cu == chatUser);
        _attenders.Get(chatUser.ChatId).Remove(attender);
    }

    public List<string> GetUsernames(long chatId)
    {
        return _attenders[chatId].Select(x => x.Username).ToList();
    }

    public void RemoveAll(long chatId)
    {
        _attenders?.Get(chatId)?.Clear();
    }

    public int Count(long chatId)
    {
        return _attenders.Get(chatId)?.Count ?? 0;
    }
    
}