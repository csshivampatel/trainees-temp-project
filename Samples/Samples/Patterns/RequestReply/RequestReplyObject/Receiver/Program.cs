using System;
using Neuron.Esb;

namespace Neudesic.EnterpriseServiceBus.Samples
{
    public class Receiver
    {
        static int messageCount = 0;
        static Subscriber subscriber;

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Initializing receiver");

                using (subscriber = new Subscriber())
                {
                    subscriber.OnReceive += OnReceive;
                    subscriber.Connect();

                    Console.WriteLine("Ready to receive");
                    Console.ReadLine();
                    Console.WriteLine("Press <ENTER> to shut down.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.ToString());
                Console.WriteLine("Press <ENTER> to shut down.");
                Console.ReadLine();
            }
        }

        public static void OnReceive(object sender, MessageEventArgs e)
        {
            messageCount++;
            Console.WriteLine("Received request messsage " + messageCount.ToString() + ", returning contact");

            Contact contact = CreateContact();
            subscriber.Reply(e.Message, contact);
        }


        private static Contact CreateContact()
        {
            Contact contact = new Contact();

            contact.ContactType = ContactType.Customer;
            contact.LastName = "Smith";
            contact.FirstName = "John";
            contact.Street = "100 Main St";
            contact.Street2 = "";
            contact.City = "Los Angeles";
            contact.Region = "CA";
            contact.PostalCode = "99123";
            contact.Country = "USA";
            contact.Phone = "555-123-4567";
            contact.Email = "john.smith@acme.com";

            return contact;
        }
    }

    [Serializable]
    public enum ContactType { Other, Customer, Vendor, Personal }

    [Serializable]
    public class Contact
    {
        public ContactType ContactType;
        public string LastName;
        public string FirstName;
        public string Street;
        public string Street2;
        public string City;
        public string Region;
        public string PostalCode;
        public string Country;
        public string Phone;
        public string Email;

    }
}
