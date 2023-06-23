using System;
using System.ServiceModel;

namespace Neuron.Samples.CustomerSOAP
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
            Console.WriteLine("Press <ENTER> to send SOAP messages to server");
            Console.ReadKey();

            BasicHttpBinding binding = new BasicHttpBinding();

            using (var chan = new ChannelFactory<ICustomerService>(binding, new EndpointAddress("http://localhost:8002/Customer")))
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

                    var result = proxy.AddCustomer(customer);

                    if (result.Success)
                        Console.WriteLine("Customer Added");
                    else
                        Console.WriteLine("There was an Error Adding the Customer - {0}", result.ErrorMsg);

                    Console.WriteLine();

                    // Get customer list
                    Console.WriteLine("Reading Customer List");

                    Customers customerList = proxy.GetAllCustomers();

                    foreach (Customer cust in customerList)
                        Console.WriteLine("Customer ID = {0}, Name = {1}, Email = {2}", cust.ID, cust.Name, cust.Email);

                    Console.WriteLine();

                    // Get Customer by ID
                    Console.WriteLine("Reading Customer by ID");

                    customer = proxy.GetCustomerByID("1001");

                    Console.WriteLine("Customer ID = {0}, Name = {1}, Email = {2}", customer.ID, customer.Name, customer.Email);
                    Console.WriteLine();

                    // Get Customer by Name
                    Console.WriteLine("Reading Customer by Name");

                    customer = proxy.GetCustomerByName("Contoso");

                    Console.WriteLine("Customer ID = {0}, Name = {1}, Email = {2}", customer.ID, customer.Name, customer.Email);
                    Console.WriteLine();
                    Console.WriteLine("Press <Enter> to close");
                }
                chan.Close();
            }
        }
    }
}
