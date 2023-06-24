using System;
using System.ServiceModel;

namespace Neuron.Samples.CustomerSOAP
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing service");

            // Create a ServiceHost for the CustomerService type
            using (ServiceHost sh = new ServiceHost(typeof(CustomerService)))
            {
                sh.Open();

                // The service can now be accessed.
                Console.WriteLine("The service is ready.");
                Console.WriteLine("Press <ENTER> to terminate service.");
                Console.WriteLine();
                Console.ReadLine();
            }
        }
    }
}
