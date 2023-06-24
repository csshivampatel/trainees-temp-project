using System;
using System.ServiceModel.Web;

namespace Neuron.Samples.JSON
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing service");

            // Create a ServiceHost for the CustomerService type
            string baseUri = "http://localhost:8015/Customer";
            using (WebServiceHost sh = new WebServiceHost(typeof(CustomerService), new Uri(baseUri)))
            {
                sh.Open();

                // The service can now be accessed.
                Console.WriteLine("The service is ready.");
                Console.WriteLine("Press <ENTER> to terminate service");
                Console.WriteLine();
                Console.ReadLine();
            }
        }
    }
}
