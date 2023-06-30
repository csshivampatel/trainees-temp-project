using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Neuron.Esb;

namespace ShoppingCart
{
    public class Program
    {
        private static string _customerName;
        private static string _mailingAddress;

        private static Guid _orderId;
        public static Publisher _publisher;
        public static Subscriber _subscriber;
        private static Dictionary<string, string> items;

        static void Main(string[] args)
        {
            items = new Dictionary<string, string>();

            Console.Write("Please Enter Customer Name: ");
            _customerName = Console.ReadLine();
            Console.Write("Please Enter Customer Mailing Address: ");
            _mailingAddress = Console.ReadLine();

            OrderMenu();
        }

        private static void ReadItem()
        {
            Console.Clear();
            Console.Write("Please Enter Item Id: ");
            var item = Console.ReadLine();

            var resourceManager = Properties.Resources.ResourceManager;
            var resourceSet = resourceManager.GetResourceSet(new CultureInfo("en-US"), true, true);

            var resource = from entry in resourceSet.Cast<DictionaryEntry>() where entry.Key.ToString() == item select entry.Value;

            if (resource.FirstOrDefault() == null)
            {
                Console.WriteLine();
                Console.WriteLine("Invalid Item Id");
                Console.WriteLine("Press any key to continue");
                Console.ReadKey();
                OrderMenu();
            }
            else
            {
                Console.Write("Please enter quantity: ");
                var quantity = Console.ReadLine().ToString();

                items.Add(item, quantity);
                OrderMenu();
            }
        }
        private static void OrderMenu()
        {
            if (items.Count == 0)
            {
                ReadItem();
            }
            else
            {
                Console.Clear();
                Console.WriteLine("[1] Add Item");
                Console.WriteLine("[2] Complete Order");
                var input = Console.ReadKey().Key;

                if (input == ConsoleKey.D1)
                {
                    ReadItem();
                }
                else if (input == ConsoleKey.D2)
                {
                    ReviewOrder();
                    Console.ReadKey();
                }
                else
                {
                    OrderMenu();
                }
            }
        }
        private static void ReviewOrder()
        {
            Console.Clear();
            Console.WriteLine("Order Summary");
            Console.WriteLine();

            _orderId = Guid.NewGuid();

            Console.WriteLine("Order Id: " + _orderId);
            Console.WriteLine("Customer Name: " + _customerName);
            Console.WriteLine("Mailing Address: " + _mailingAddress);

            foreach (var item in items)
            {
                Console.WriteLine();

                var resourceManager = Properties.Resources.ResourceManager;
                var resourceSet = resourceManager.GetResourceSet(new CultureInfo("en-US"), true, true);
                var resource = from entry in resourceSet.Cast<DictionaryEntry>() where entry.Key.ToString() == item.Key select entry.Value;

                Console.WriteLine("Item Id: " + item.Key);
                Console.WriteLine("Item Name: " + resource.First());
                Console.WriteLine("Item Quantity: " + item.Value);
            }

            Console.WriteLine();
            Console.WriteLine("[1] Add Item");
            Console.WriteLine("[2] Submit Order");
            Console.WriteLine("[3] Cancel Order");
            var input = Console.ReadKey().Key;

            if (input == ConsoleKey.D1)
            {
                ReadItem();
            }
            else if (input == ConsoleKey.D2)
            {
                _publisher = new Publisher("OrdersPublisher");
                SendMessage();
            }
            else
            {
                Environment.Exit(0);
            }
        }
        private static void SendMessage()
        {
            using (_publisher)
            {
                var exceptions = _publisher.Connect(); //Connects the publisher to the Neuron ESB instance and retrieves a list of errors that occured when connecting, if there are any.

                if (exceptions.Count > 0) //Checks to see if any errors were returned from the Neuron ESB instance when attempting to connect. If errors exist, display the errors instead of sending a message.
                {
                    Console.WriteLine("An error(s) occured on connecting to Neuron");
                    Console.WriteLine();
                    foreach (var exception in exceptions.GetResults())
                    {
                        Console.WriteLine(exception.Exception.Message);
                    }
                }
                else
                {
                    var message = new StringBuilder();
                    message.Append("<Order>");
                    message.Append("<OrderId>" + _orderId + "</OrderId>");
                    message.Append("<CustomerName>" + _orderId + "</CustomerName>");
                    message.Append("<MailingAddress>" + _orderId + "</MailingAddress>");
                    message.Append("<Items>");

                    foreach (var item in items)
                    {
                        Console.WriteLine();

                        var resourceManager = Properties.Resources.ResourceManager;
                        var resourceSet = resourceManager.GetResourceSet(new CultureInfo("en-US"), true, true);
                        var resource = from entry in resourceSet.Cast<DictionaryEntry>() where entry.Key.ToString() == item.Key select entry.Value;

                        message.Append("<Item>");
                        message.Append("<Id>" + item.Key + "</Id>");
                        message.Append("<Name>" + resource.First() + "</Name>");
                        message.Append("<Quantity>" + item.Value + "</Quantity>");
                        message.Append("</Item>");
                    }

                    message.Append("</Items>");
                    message.Append("</Order>");

                    Console.Clear();
                    Console.WriteLine("Awaiting Reponses...");

                    var response = _publisher.SendXml("Orders.Request", message.ToString(), "", SendOptions.Request);

                    Console.WriteLine();
                    Console.WriteLine("Initial response received from Neuron ESB");
                    Console.WriteLine("Message Id:\t\t" + response.Header.MessageId);
                    Console.WriteLine("Topic:\t\t\t" + response.Header.Topic);
                    Console.WriteLine("Publisher:\t\t" + response.Header.SourceId);
                    Console.WriteLine("Message Body:\t\t" + response.Text);

                    _subscriber = new Subscriber("OrdersPublisher");

                    InitializeOnReceive();
                }
            }
        }

        public static void InitializeOnReceive()
        {            

            using (_subscriber)
            {
                var exceptions = _subscriber.Connect();

                if (exceptions.Count > 0)
                {
                    Console.WriteLine("An error(s) occured on connecting to Neuron");
                    Console.WriteLine();
                    foreach (var exception in exceptions.GetResults())
                    {
                        Console.WriteLine(exception.Exception.Message);
                    }
                }
                else
                {
                    _subscriber.OnReceive += OnReceive;
                    Console.ReadKey();
                }
            }
        }
        private static void OnReceive(object o, MessageEventArgs e)
        {
            ESBMessage message = e.Message;

            Console.WriteLine();
            Console.WriteLine("Order fulfillment received from Neuron ESB");
            Console.WriteLine("Message Id:\t\t" + message.Header.MessageId);
            Console.WriteLine("Topic:\t\t\t" + message.Header.Topic);
            Console.WriteLine("Publisher:\t\t" + message.Header.SourceId);
            Console.WriteLine("Message Body:\t\t" + message.Text);
        }
    }
}
