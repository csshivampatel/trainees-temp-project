using System;
using Neuron.Esb;

namespace Neudesic.EnterpriseServiceBus.Samples
{
    public class Sender
    {
        static bool dataSwitch = false;

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
                        Console.WriteLine("Sending Customer contact on topic Contacts.Customer");
                        xml = CreateCustomerContactXml();
                        publisher.SendXml("Contacts.Customer", xml, "http://tempuri.org/Contact");

                        Console.WriteLine("Sending Vendor contact on topic Contacts.Vendor");
                        xml = CreateVendorContactXml();
                        publisher.SendXml("Contacts.Vendor", xml, "http://tempuri.org/Contact");

                        Console.WriteLine("Sending Personal contact on topic Contacts.Personal");
                        xml = CreatePersonalContactXml();
                        publisher.SendXml("Contacts.Personal", xml, "http://tempuri.org/Contact");
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

        private static string CreateCustomerContactXml()
        {
            dataSwitch = !dataSwitch;
            if (!dataSwitch)
                return
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                    "<Contact type=\"Customer\">\r\n" +
                    "  <Company></Company>\r\n" + 
                    "  <LastName>Customer</LastName>\r\n" +
                    "  <FirstName>John</FirstName>\r\n" +
                    "  <Street>100 Main St</Street>\r\n" +
                    "  <Street2></Street2>\r\n" +
                    "  <City>Los Angeles</City>\r\n" +
                    "  <Region>CA</Region>\r\n" +
                    "  <PostalCode>99433</PostalCode>\r\n" +
                    "  <Country>USA</Country>\r\n" +
                    "  <Phone>555-123-1234</Phone>\r\n" +
                    "  <Email>john.smith@acme.com</Email>\r\n" +
                    "</Contact>\r\n";
            else
                return
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                    "<Contact type=\"Customer\">\r\n" +
                    "  <Company></Company>\r\n" + 
                    "  <LastName>Customer</LastName>\r\n" +
                    "  <FirstName>Kimberly</FirstName>\r\n" +
                    "  <Street>187 Oak St</Street>\r\n" +
                    "  <Street2></Street2>\r\n" +
                    "  <City>Los Angeles</City>\r\n" +
                    "  <Region>CA</Region>\r\n" +
                    "  <PostalCode>99433</PostalCode>\r\n" +
                    "  <Country>USA</Country>\r\n" +
                    "  <Phone>555-123-1234</Phone>\r\n" +
                    "  <Email>john.smith@acme.com</Email>\r\n" +
                    "</Contact>\r\n";
        }


        private static string CreateVendorContactXml()
        {
            dataSwitch = !dataSwitch;
            if (!dataSwitch)
                return
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                    "<Contact type=\"Vendor\">\r\n" +
                    "  <Company>Fielding Supplies</Company>\r\n" + 
                    "  <LastName>Fielding</LastName>\r\n" +
                    "  <FirstName>Howard</FirstName>\r\n" +
                    "  <Street>92545 Flower</Street>\r\n" +
                    "  <Street2></Street2>\r\n" +
                    "  <City>Los Angeles</City>\r\n" +
                    "  <Region>CA</Region>\r\n" +
                    "  <PostalCode>99433</PostalCode>\r\n" +
                    "  <Country>USA</Country>\r\n" +
                    "  <Phone>555-123-1234</Phone>\r\n" +
                    "  <Email>howard.fielding@fielding-supplies.com</Email>\r\n" +
                    "</Contact>\r\n";
            else
                return
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                    "<Contact type=\"Vendor\">\r\n" +
                    "  <Company>Marketing Advantage</Company>\r\n" + 
                    "  <LastName>Showcase</LastName>\r\n" +
                    "  <FirstName>Susan</FirstName>\r\n" +
                    "  <Street>8643 Red Hilll</Street>\r\n" +
                    "  <Street2></Street2>\r\n" +
                    "  <City>Los Angeles</City>\r\n" +
                    "  <Region>CA</Region>\r\n" +
                    "  <PostalCode>99433</PostalCode>\r\n" +
                    "  <Country>USA</Country>\r\n" +
                    "  <Phone>555-123-1234</Phone>\r\n" +
                    "  <Email>susan.showcase@mktg-adv.com</Email>\r\n" +
                    "</Contact>\r\n";
        }


        private static string CreatePersonalContactXml()
        {
            dataSwitch = !dataSwitch;
            if (!dataSwitch)
                return
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                    "<Contact type=\"Personal\">\r\n" +
                    "  <Company></Company>\r\n" +
                    "  <LastName>Persimmon</LastName>\r\n" +
                    "  <FirstName>Patricia</FirstName>\r\n" +
                    "  <Street>988 Pierce Str</Street>\r\n" +
                    "  <Street2></Street2>\r\n" +
                    "  <City>Los Angeles</City>\r\n" +
                    "  <Region>CA</Region>\r\n" +
                    "  <PostalCode>99433</PostalCode>\r\n" +
                    "  <Country>USA</Country>\r\n" +
                    "  <Phone>555-123-1234</Phone>\r\n" +
                    "  <Email>patricia.persimmon@aol.com</Email>\r\n" +
                    "</Contact>\r\n";
            else
                return
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                    "<Contact type=\"Personal\">\r\n" +
                    "  <Company></Company>\r\n" +
                    "  <LastName>Argyle</LastName>\r\n" +
                    "  <FirstName>Aaron</FirstName>\r\n" +
                    "  <Street>98454 Petunia</Street>\r\n" +
                    "  <Street2></Street2>\r\n" +
                    "  <City>Los Angeles</City>\r\n" +
                    "  <Region>CA</Region>\r\n" +
                    "  <PostalCode>99433</PostalCode>\r\n" +
                    "  <Country>USA</Country>\r\n" +
                    "  <Phone>555-123-1234</Phone>\r\n" +
                    "  <Email>aargyle@hotmail.com</Email>\r\n" +
                    "</Contact>\r\n";
        }
    }

}
