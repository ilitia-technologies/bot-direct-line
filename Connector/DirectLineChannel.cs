using Model;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Connector
{
    /// <summary>
    /// Using Direct Line version 1.1 for a PoC with Xamarin
    /// </summary>
    public class DirectLineChannel
    {
        private const string DirectLineBaseUrl = "https://directline.botframework.com/api/conversations/";

        private const string botConnectorSecretKey = <botConnectorSecretKey>;

        private const string apologizeText = "Sorry, there is a problem ";
        private const string autethicationErrorText = "with authentication.";
        private const string conversationErrorText = "starting a conversation.";
        private const string sendMessageErrorText = "getting the message.";
        private const string getMessageErrorText = "sending the message.";
                
        private const double tokenExpirationMinutes = 30;
        private DateTime tokenDateTime;

        private ConversationResult conversationResult;
        private Conversation currentConversation;

        private HttpClient client;
        private HttpResponseMessage response;

        private int retryCount = 0;
        private string lastWatermark = string.Empty;

        public async Task<ConversationResult> Conversation(string message)
        {
            conversationResult = new ConversationResult();

            if (await IsValidToken())
            {
                return await HandleConversation(message);
            }

            return conversationResult;
        }

        public async Task<ConversationResult> GetWelcomeMessage()
        {
            conversationResult = new ConversationResult();

            if (await IsValidToken())
            {
                if (await TryGetCurrentConversation())
                {
                    return await GetBotResponse();
                }
            }

            return conversationResult;
        }

        private async Task<bool> IsValidToken()
        {
            if (tokenDateTime == DateTime.MinValue)
            {
                return await TryGetToken();
            }
            else
            {
                if (tokenDateTime.AddMinutes(tokenExpirationMinutes) > DateTime.UtcNow)
                {
                    return await TryGetToken();
                }
            }
            return false;
        }

        private async Task<bool> TryGetToken()
        {
            if (await GetToken())
            {
                tokenDateTime = DateTime.UtcNow;
                return true;
            }

            conversationResult.isReplyReceived = false;
            conversationResult.resultMessage = $"{apologizeText} {autethicationErrorText}.";
            return false;
        }

        private async Task<bool> GetToken()
        {
            try
            {
                client = new HttpClient();
                client.BaseAddress = new Uri(DirectLineBaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("BotConnector", botConnectorSecretKey);

                response = await client.GetAsync("/api/tokens/");

                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<ConversationResult> HandleConversation(string message)
        {
            if (await TryGetCurrentConversation())
            {
                return await Chat(message);
            }

            return new ConversationResult(false, null, $"{apologizeText} {conversationErrorText}");
        }

        private async Task<bool> TryGetCurrentConversation()
        {
            if (currentConversation == null)
            {
                var conversation = new Conversation();
                response = await client.PostAsync("/api/conversations/", SetJsonAsStringContent(conversation));

                if (response.IsSuccessStatusCode)
                {
                    currentConversation = JsonConvert.DeserializeObject<Conversation>(
                    response.Content.ReadAsStringAsync().Result);
                    return true;
                }

                return false;
            }

            return true;
        }

        private async Task<ConversationResult> Chat(string message)
        {
            string conversationUrl = currentConversation.conversationId + "/messages/";
            //From 
            Message msg = new Message() { text = message, from = "user" };

            response = await client.PostAsync(conversationUrl, SetJsonAsStringContent(msg));
            if (response.IsSuccessStatusCode)
            {
                return await GetBotResponse();
            }
            else
            {
                return new ConversationResult(false, null, $"{apologizeText} {sendMessageErrorText}");
            }
        }

        private async Task<ConversationResult> GetBotResponse()
        {
            string conversationUrl = currentConversation.conversationId + "/messages";

            if (!string.IsNullOrEmpty(lastWatermark))
            {
                conversationUrl += $"?watermark={lastWatermark}";
            }

            response = await client.GetAsync(conversationUrl);
            if (response.IsSuccessStatusCode)
            {
                MessageSet BotMessage = JsonConvert.DeserializeObject<MessageSet>(
                    response.Content.ReadAsStringAsync().Result);

                if (string.IsNullOrEmpty(BotMessage.watermark) && retryCount < 5)
                {
                    retryCount += 1;
                    return await Task.Delay(1000).ContinueWith(_ => GetBotResponse()).Result;
                }
                else
                {
                    lastWatermark = BotMessage.watermark;
                    return new ConversationResult(true, BotMessage);
                }
            }
            else
            {
                return new ConversationResult(false, null, $"{apologizeText} {getMessageErrorText}");
            }
        }

        private static StringContent SetJsonAsStringContent(object data)
        {
            return new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
        }
    }
}
