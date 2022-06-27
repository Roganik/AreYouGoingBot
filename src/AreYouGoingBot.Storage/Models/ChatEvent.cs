namespace AreYouGoingBot.Storage.Models;

public class ChatEvent
{
    public int ChatEventId { get; set; }
    public long TelegramChatId { get; set; }
    
    public List<ChatEventParticipant> Participants = new List<ChatEventParticipant>();
}