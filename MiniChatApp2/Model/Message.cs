namespace MiniChatApp2.Model
{
    public class Message
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public int receiverId { get; set; }
        public int senderId { get; set; }
        
        public User sender { get; set; }
        public User receiver { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
