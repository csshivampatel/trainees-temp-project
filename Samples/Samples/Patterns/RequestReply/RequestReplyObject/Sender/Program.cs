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

                    Console.WriteLine("Requesting contact");
                    ESBMessage replyMessage = publisher.Send("Contacts.Lookup", "Test", SendOptions.Request);
                    Console.WriteLine("Reply received");
                    if (replyMessage.Header.BodyType.Equals("Contact"))
                    {
                        Contact contact = replyMessage.GetBody<Contact>();
                        DisplayContact(contact);
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

        private static void DisplayContact(Contact contact)
        {
            Console.WriteLine("Contact type .............. " + contact.ContactType.ToString());
            Console.WriteLine("Last name ................. " + contact.LastName);
            Console.WriteLine("First name ................ " + contact.FirstName);
            Console.WriteLine("Street .................... " + contact.Street);
            Console.WriteLine("Street2 ................... " + contact.Street2);
            Console.WriteLine("City ...................... " + contact.City);
            Console.WriteLine("Region .................... " + contact.Region);
            Console.WriteLine("Postal code ............... " + contact.PostalCode);
            Console.WriteLine("Country ................... " + contact.Country);
            Console.WriteLine("Phone ..................... " + contact.Phone);
            Console.WriteLine("Email ..................... " + contact.Email);
            Console.WriteLine();
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
