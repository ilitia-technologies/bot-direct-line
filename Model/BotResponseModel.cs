using System.Collections.Generic;

namespace Model
{
    public class BotResponseModel
        {
            public string Intent { get; set; }
            public string Message { get; set; }
            public List<string> Entities { get; set; }
            public bool EndConversation { get; set; }
            public Dictionary<string, string> ConversationResult { get; set; }

            public BotResponseModel()
            {
                Entities = new List<string>();
                ConversationResult = new Dictionary<string, string>();
            }
        }
}
