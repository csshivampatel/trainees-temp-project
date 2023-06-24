using System;
using Neuron.Esb;

namespace Neudesic.EnterpriseServiceBus.Samples
{
    public class Receiver
    {
        static int messageCount = 0;

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Initializing receiver");

                using (Subscriber subscriber = new Subscriber())
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
            Console.WriteLine("Received messsage " + messageCount.ToString());

            if (e.Message.Header.BodyType.Equals("Lead"))
            {
                Lead lead = e.Message.GetBody<Lead>();
                DisplayLead(lead);
            }
        }

        private static void DisplayLead(Lead lead)
        {
            Console.WriteLine("Lead");
            Console.WriteLine("  Last name ................ " + lead.LastName);
            Console.WriteLine("  First name ............... " + lead.FirstName);
            Console.WriteLine("  Street ................... " + lead.Street);
            Console.WriteLine("  Street2 .................. " + lead.Street2);
            Console.WriteLine("  City ..................... " + lead.City);
            Console.WriteLine("  Region ................... " + lead.Region);
            Console.WriteLine("  Postal code .............. " + lead.PostalCode);
            Console.WriteLine("  Country .................. " + lead.Country);
            Console.WriteLine("  Phone .................... " + lead.Phone);
            Console.WriteLine("  Email .................... " + lead.Email);
            Console.WriteLine("  Salesperson .............. " + lead.Salesperson);
            Console.WriteLine();
        }

    }

    [Serializable]
    public class Contact
    {
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

    [Serializable]
    public class Lead : Contact
    {
        public string Salesperson;
    }

}
