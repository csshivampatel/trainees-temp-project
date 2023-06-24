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
                Console.WriteLine("Initializing receiver3");

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

            if (e.Message.Header.BodyType.Equals("Customer"))
            {
                Customer customer = e.Message.GetBody<Customer>();
                DisplayCustomer(customer);
            }
        }

        private static void DisplayCustomer(Customer customer)
        {
            Console.WriteLine("Customer");
            Console.WriteLine("  Last name ................ " + customer.LastName);
            Console.WriteLine("  First name ............... " + customer.FirstName);
            Console.WriteLine("  Street ................... " + customer.Street);
            Console.WriteLine("  Street2 .................. " + customer.Street2);
            Console.WriteLine("  City ..................... " + customer.City);
            Console.WriteLine("  Region ................... " + customer.Region);
            Console.WriteLine("  Postal code .............. " + customer.PostalCode);
            Console.WriteLine("  Country .................. " + customer.Country);
            Console.WriteLine("  Phone .................... " + customer.Phone);
            Console.WriteLine("  Email .................... " + customer.Email);
            Console.WriteLine("  Salesperson .............. " + customer.Salesperson);
            Console.WriteLine("  Account rep .............. " + customer.AccountRep);
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
    public class Customer : Contact
    {
        public string Salesperson;
        public string AccountRep;
    }
}
