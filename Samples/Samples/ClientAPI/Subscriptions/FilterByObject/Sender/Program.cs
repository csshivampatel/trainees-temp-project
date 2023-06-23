using System;
using Neuron.Esb;

namespace Neudesic.EnterpriseServiceBus.Samples
{
    public class Sender
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Initializing sender");

                using (Publisher publisher = new Publisher())
                {
                    publisher.Connect();

                    Console.WriteLine("Press <ENTER> to start sending");
                    Console.ReadLine();

                    for (int m=1; m<=3; m++)
                    {
                        Lead lead = CreateLead();
                        Console.WriteLine("Sending lead object");
                        publisher.Send("Contacts.New", lead);

                        Prospect prospect = CreateProspect();
                        Console.WriteLine("Sending prospect object");
                        publisher.Send("Contacts.New", prospect);

                        Customer customer = CreateCustomer();
                        Console.WriteLine("Sending customer object");
                        publisher.Send("Contacts.New", customer);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception: " + ex.ToString());
            }
            finally
            {
                Console.WriteLine("Press <ENTER> to shut down.");
                Console.ReadLine();
            }
        }

        private static Lead CreateLead()
        {
            Lead lead = new Lead();

            lead.LastName = "Smith";
            lead.FirstName = "John";
            lead.Street = "100 Main St";
            lead.Street2 = "";
            lead.City = "Los Angeles";
            lead.Region = "CA";
            lead.PostalCode = "99123";
            lead.Country = "USA";
            lead.Phone = "555-123-4567";
            lead.Email = "john.smith@acme.com";
            lead.Salesperson = "Jones";

            return lead;
        }

        private static Prospect CreateProspect()
        {
            Prospect prospect = new Prospect();

            prospect.LastName = "Smith";
            prospect.FirstName = "John";
            prospect.Street = "100 Main St";
            prospect.Street2 = "";
            prospect.City = "Los Angeles";
            prospect.Region = "CA";
            prospect.PostalCode = "99123";
            prospect.Country = "USA";
            prospect.Phone = "555-123-4567";
            prospect.Email = "john.smith@acme.com";
            prospect.Salesperson = "Jones";
            prospect.Status = "60% in pipeline";

            return prospect;
        }

        private static Customer CreateCustomer()
        {
            Customer customer = new Customer();

            customer.LastName = "Smith";
            customer.FirstName = "John";
            customer.Street = "100 Main St";
            customer.Street2 = "";
            customer.City = "Los Angeles";
            customer.Region = "CA";
            customer.PostalCode = "99123";
            customer.Country = "USA";
            customer.Phone = "555-123-4567";
            customer.Email = "john.smith@acme.com";
            customer.Salesperson = "Jones";
            customer.AccountRep = "Jones";

            return customer;
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

    [Serializable]
    public class Prospect : Contact
    {
        public string Salesperson;
        public string Status;
    }

    [Serializable]
    public class Customer : Contact
    {
        public string Salesperson;
        public string AccountRep;
    }
}
