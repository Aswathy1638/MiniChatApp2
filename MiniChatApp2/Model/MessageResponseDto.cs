namespace MiniChatApp2.Model
{
    public class MessageResponseDto
    {
        public class MessageResponse
        {
            public int MessageId { get; set; }
            public int SenderId { get; set; }
            public int ReceiverId { get; set; }
            public string Content { get; set; }
            public DateTime Timestemp { get; set; }

        }
    }
}
