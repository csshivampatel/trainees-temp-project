using System;
using System.ServiceModel;
using Neuron.Samples.CustomerSOAP.CustomerServiceReference;

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

            try
            {
                CustomerServiceClient client = new CustomerServiceClient();

                // Add Customer
                Console.WriteLine("Adding Customer");

                customer customer = new customer
                {
                    ID = 1234,
                    Name = "Contoso",
                    Email = "admin@contoso.com"
                };

                var response = client.AddCustomer(customer);

                if (response.success)
                    Console.WriteLine("Customer Added");
                else
                    Console.WriteLine("There was an Error Adding the Customer - {0}", response.errorMsg);

                Console.WriteLine();

                // Get customer list
                Console.WriteLine("Reading Customer List");

                customers customerList = client.GetAllCustomers();

                    foreach (customer cust in customerList)
                    Console.WriteLine("Customer ID = {0}, Name = {1}, Email = {2}", cust.ID, cust.Name, cust.Email);

                Console.WriteLine();

                // Get Customer by ID
                Console.WriteLine("Reading Customer by ID");

                customer = client.GetCustomerByID("1001");

                Console.WriteLine("Customer ID {0}, {1}, {2}", customer.ID, customer.Name, customer.Email);
                Console.WriteLine();

                // Get Customer by Name
                Console.WriteLine("Reading Customer by Name");

                customer = client.GetCustomerByName("Contoso");

                Console.WriteLine("Customer ID {0}, {1}, {2}", customer.ID, customer.Name, customer.Email);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error has occurred - {0}", ex.Message);
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine("Press <Enter> to close");
                Console.ReadLine();
            }
        }
    }
}
