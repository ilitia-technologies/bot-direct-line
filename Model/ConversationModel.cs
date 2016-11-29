namespace Model
{
    public class Conversation
    {
        public string conversationId { get; set; }
        public string token { get; set; }
        public string eTag { get; set; }
    }

    public class ConversationResult
    {
        public bool isReplyReceived { get; set; }
        public string resultMessage { get; set; }
        public MessageSet botMessage { get; set; }
        public ConversationResult() { }
        public ConversationResult(bool isReplyReceived, MessageSet botMessage, string resultMessage = "")
        {
            this.isReplyReceived = isReplyReceived;
            this.botMessage = botMessage;
            this.resultMessage = resultMessage;
        }

    }
}
