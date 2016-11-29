using Connector;
using Model;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// ConsoleOutput demo purpose using Direct Line Connector
/// </summary>
namespace ConsoleOutput
{
    class Program
    {
        //BotEstadoGeneral = @from field of botId
        private static string BotId = "BotEstadoGeneral";
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("|| Direct Line Connector Channel Demo ||");
            Console.WriteLine("|| Give an empty answer to close this ||");
            var dl = new DirectLineChannel();
            StartConversation(dl, "");
      
            var runner = true;
            while (runner)
            {
                var userText = Console.ReadLine();
                if (string.IsNullOrEmpty(userText))
                {
                    runner = false;
                }
                StartConversation(dl, userText);
            }
        }

        static async void StartConversation(DirectLineChannel dl, string userText)
        {
            var endConversation = false;

            var sendingMessage = true;
            var timer = DateTime.Now;
            Console.ForegroundColor = ConsoleColor.Yellow;
            var loading = Task.Factory.StartNew(() =>
            {
                while (sendingMessage)
                {
                    if (DateTime.Now >= timer.AddSeconds(1))
                    {
                        Console.Write(".");
                        timer = DateTime.Now;
                    }
                }
            });

            var result = await dl.Conversation(userText);
            sendingMessage = false;

            if (result.resultMessage != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.resultMessage);
            }

            if (result.botMessage != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;

                //BotId @from field of botId
                foreach (var mssg in result.botMessage.messages.Where(m => m.from == BotId))
                {
                    //If you are using a simple bot backed this is not necessary, just use mssg.text
                    var text = JsonConvert.DeserializeObject<BotResponseModel>(mssg.text);
        
                    Console.WriteLine(text.Message);
                    if (text.ConversationResult != null)
                    {
                        foreach (var conversation in text.ConversationResult)
                        {
                            Console.WriteLine($"{conversation.Key}:{conversation.Value}");
                        }
                    }
                        endConversation = text.EndConversation;
                }

                if (endConversation)
                {
                    Console.WriteLine("--- Please, close this console or press enter ---");
                }
                else
                {
                    Console.Write(">");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }

    }
}
