using System;

namespace AreYouGoingBot
{
    public class ChatUser
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