using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Neuron.Samples.JSON
{
    class Program
    {
        static void Main(string[] args)
        {
            CallRestService();
            Console.ReadLine();
        }

        public static void CallRestService()
        {
            try
            {
                Console.WriteLine("Press <ENTER> to send messages to server");
                Console.ReadKey();

                WebHttpBinding binding = new WebHttpBinding();

                // Use this URL for a direct service call
                //string baseurl = "http://localhost:8015/Customer";

                // Use this URL for a routed service call
                string baseurl = "http://localhost:8005/Customer";

                using (var chan = new WebChannelFactory<ICustomerService>(binding, new Uri(baseurl)))
                {
                    var proxy = chan.CreateChannel();
                    using ((IDisposable)proxy)
                    {
                        // Add Customer
                        Console.WriteLine("Adding Customer");

                        Customer customer = new Customer
                        {
                            ID = 1234,
                            Name = "Contoso",
                            Email = "admin@contoso.com"
                        };

                        StreamReader sr1 = new StreamReader(proxy.AddCustomer(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(customer))))); 

                        Console.WriteLine("Customer Added");
                        Console.WriteLine();

                        // Get Customer by ID
                        Console.WriteLine("Reading Customer by ID");

                        StreamReader sr2 = new StreamReader(proxy.GetCustomerByID("1001"));
                        customer = Newtonsoft.Json.JsonConvert.DeserializeObject<Customer>(sr2.ReadToEnd());

                        Console.WriteLine("Customer ID {0}, {1}, {2}", customer.ID, customer.Name, customer.Email);
                        Console.WriteLine();

                        // Get Customer by Name
                        Console.WriteLine("Reading Customer by Name");

                        StreamReader sr3 = new StreamReader(proxy.GetCustomerByName("Contoso"));
                        customer = Newtonsoft.Json.JsonConvert.DeserializeObject<Customer>(sr3.ReadToEnd());

                        Console.WriteLine("Customer ID {0}, {1}, {2}", customer.ID, customer.Name, customer.Email);
                        Console.WriteLine();
                        Console.WriteLine("Press <Enter> to close");
                    }
                    chan.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown - " + ex.Message);
            }
        }
    }
}
