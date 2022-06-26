using System.Collections.Generic;
using System.Linq;

namespace AreYouGoingBot.Bot;

public class AttendersManager
{
    private readonly Dictionary<long, List<ChatUser>> _attenders = new();
    
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
}