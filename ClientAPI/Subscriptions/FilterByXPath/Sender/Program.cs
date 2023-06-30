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

                    string xml;

                    for (int m=1; m<3; m++)
                    {
                        Console.WriteLine("Sending Contact message");
                        xml = CreateContactXml();
                        publisher.SendXml("Contacts.New", xml, "http://tempuri.org/schema-not-declared");

                        Console.WriteLine("Sending Customer message");
                        xml = CreateCustomerXml();
                        publisher.SendXml("Contacts.New", xml, "http://tempuri.org/schema-not-declared");
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

        private static string CreateContactXml()
        {
                return
                    "<?xml version=\"1.0\" encoding=\"ISO-8859-1\" ?>\r\n" +
                    "<con:contacts version=\"1.0\" xmlns:con=\"urn:xmlns:mycompany:contact\">\r\n" +
                    "  <con:contact>\r\n" +
                    "    <con:name>John A. Doe</con:name>\r\n" +
                    "    <con:lastBuildDate>Tue, 31 Jan 2006 07:01:01 PST</con:lastBuildDate>\r\n" +
                    "    <con:website type=\"personal\">http://johndoe.com</con:website> \r\n" +
                    "    <con:photo>http://johndoe.com/me.jpg</con:photo> \r\n" +
                    "    <con:description>This is John's contact information.</con:description> \r\n" +
                    "    <con:email type=\"primary\">jdoe@johndoe.com</con:email>\r\n" +
                    "    <con:messaging name=\"aim\" nick=\"jdoeAIM\"/>\r\n" +
                    "  </con:contact>\r\n" +
                    "</con:contacts>\r\n";
        }


        private static string CreateCustomerXml()
        {
            return
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                    "<cust:Customer xmlns:cust=\"urn:xmlns:othercompany:customer\">\r\n" +
                    "  <cust:Company></cust:Company>\r\n" +
                    "  <cust:LastName>Persimmon</cust:LastName>\r\n" +
                    "  <cust:FirstName>Patricia</cust:FirstName>\r\n" +
                    "  <cust:Street>988 Pierce Str</cust:Street>\r\n" +
                    "  <cust:Street2></cust:Street2>\r\n" +
                    "  <cust:City>Los Angeles</cust:City>\r\n" +
                    "  <cust:Region>CA</cust:Region>\r\n" +
                    "  <cust:PostalCode>99433</cust:PostalCode>\r\n" +
                    "  <cust:Country>USA</cust:Country>\r\n" +
                    "  <cust:Phone>555-123-1234</cust:Phone>\r\n" +
                    "  <cust:Email>patricia.persimmon@aol.com</cust:Email>\r\n" +
                    "  <cust:Salesperson>Jones</cust:Salesperson>\r\n" +
                    "  <cust:AccountRep>Jones</cust:AccountRep>\r\n" +
                    "</cust:Customer>\r\n";
        }

    }

}
