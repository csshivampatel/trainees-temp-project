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
                Console.WriteLine("Initializing receiver2");

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

            if (e.Message.Header.BodyType.Equals("Prospect"))
            {
                Prospect prospect = e.Message.GetBody<Prospect>();
                DisplayProspect(prospect);
            }
        }

        private static void DisplayProspect(Prospect prospect)
        {
            Console.WriteLine("Prospect");
            Console.WriteLine("  Last name ................ " + prospect.LastName);
            Console.WriteLine("  First name ............... " + prospect.FirstName);
            Console.WriteLine("  Street ................... " + prospect.Street);
            Console.WriteLine("  Street2 .................. " + prospect.Street2);
            Console.WriteLine("  City ..................... " + prospect.City);
            Console.WriteLine("  Region ................... " + prospect.Region);
            Console.WriteLine("  Postal code .............. " + prospect.PostalCode);
            Console.WriteLine("  Country .................. " + prospect.Country);
            Console.WriteLine("  Phone .................... " + prospect.Phone);
            Console.WriteLine("  Email .................... " + prospect.Email);
            Console.WriteLine("  Salesperson .............. " + prospect.Salesperson);
            Console.WriteLine("  Status ................... " + prospect.Status);
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
    public class Prospect : Contact
    {
        public string Salesperson;
        public string Status;
    }

}
