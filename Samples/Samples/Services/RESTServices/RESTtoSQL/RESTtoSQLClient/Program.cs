using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Neuron.Samples.CustomerREST
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
                Console.WriteLine("Press <ENTER> to send REST messages to server");
                Console.ReadKey();

                WebHttpBinding binding = new WebHttpBinding();

                using (var chan = new WebChannelFactory<ICustomerService>(binding, new Uri("http://localhost:8000/Customer")))
                {
                    var proxy = chan.CreateChannel();
                    using ((IDisposable)proxy)
                    {
                        // Create Customer
                        Console.WriteLine("Creating Customer");

                        Customer customer = new Customer { ID = 1234, Name = "Contoso", Email = "admin@contoso.com" };
                        proxy.AddCustomer(customer);

                        Console.WriteLine("Customer Created");
                        Console.WriteLine();

                        // Update Customer (expected to return 400 bad request)
                        Console.WriteLine("Updating Customer");

                        try
                        {
                            proxy.UpdateCustomer("1234", customer);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                        Console.WriteLine("Customer not updated - Method not supported");
                        Console.WriteLine();

                        // Get Customer by ID
                        Console.WriteLine("Reading Customer by ID");

                        customer = proxy.GetCustomerByID("1001");

                        Console.WriteLine("Customer {0}, {1}, {2}", customer.ID, customer.Name, customer.Email);
                        Console.WriteLine();

                        // Delete Customer
                        Console.WriteLine("Deleting Customer");

                        proxy.DeleteCustomer("1234");

                        Console.WriteLine("Customer Deleted");
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
