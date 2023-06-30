// Chat - multi person chat program.
// Can be used with any channel type, but specifically coded for PeerChannel where the network can go offline/online at any time.

using System;
using System.Threading;
using Neuron.Esb;

namespace Neudesic.EnterpriseServiceBus.Samples
{
    public class Chat
    {
        private const string PublisherId = "Party";
        private const string Topic = "Chat";

        private static string Nickname;
        private static Publisher Publisher = null;
        private static ManualResetEvent OnlineEvent = new ManualResetEvent(false);
        private static bool Online = false;

        public static void Main(string[] args)
        {
            using (Publisher = new Publisher(PublisherId))
            {
                // Get nickname.

                Console.WriteLine("Welcome to Chat");
                Console.WriteLine();
                Console.Write("Please enter a nickname: ");
                Nickname = Console.ReadLine();
                Console.WriteLine();

                // Connect to ESB.

                Console.Write("Connecting to ESB... ");
                Publisher.OnOnline += OnOnline;
                Publisher.OnOffline += OnOffline;
                Publisher.OnReceive += OnReceive;
                Publisher.Connect();
                Console.WriteLine(" Done");

                WaitOnline();
                Console.WriteLine("Enter messages to send (q to exit)");
                
                // Messaging.

                bool shutdown = false;
                while (!shutdown)
                {
                    WaitOnline();
                    string text = Console.ReadLine();
                    if (text.Equals("q") || text.Equals("Q"))
                        shutdown = true;
                    else
                    {
                        ChatMessage message = new ChatMessage(Nickname, text);
                        WaitOnline();
                        Publisher.Send(Topic, message);
                    }
                }
            }
        }

        // Ensure we're in an online state. If we aren't, display a wait message and wait till we're online again.

        private static void WaitOnline()
        {
            if (!Online)
            {
                Console.WriteLine();
                Console.Write("Waiting for other parties to join... ");
                OnlineEvent.WaitOne();
                Console.WriteLine(" Done");
                Console.WriteLine();
            }
        }


        // Network has gone online. In PeerChannel, this means we are connected to at least one other party.

        private static void OnOnline(object sender, EventArgs e)
        {
            Online = true;
            OnlineEvent.Set();
        }

        // Network has gone offline. In PeerChannel, this can mean no other parties are online, or that the mesh is reorganizing itself.

        private static void OnOffline(object sender, EventArgs e)
        {
            Online = false;
            OnlineEvent.Reset();
        }

        // Message reecived. Deserialize to a ChatMessage object and display.

        private static void OnReceive(object sender, MessageEventArgs e)
        {
            ChatMessage message = e.Message.GetBody<ChatMessage>();
            Console.WriteLine(message.From + ": " + message.Text);
        }

    }

    [Serializable]
    public class ChatMessage
    {
        public string From;
        public string Text;

        public ChatMessage()
        {
        }

        public ChatMessage(string from, string text)
        {
            From = from;
            Text = text;
        }
    }
}
