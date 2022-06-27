namespace AreYouGoingBot.Storage.Models;

public class ChatEventParticipant
{
    public int ChatEventParticipantId { get; set; }
    public long TelegramUserId { get; set; }
    public string Name { get; set; }
    
    
    public int ChatEventId { get; set; } 
    public ChatEvent ChatEvent { get; set; }
}