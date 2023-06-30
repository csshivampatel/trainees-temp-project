using System;
using System.ServiceModel;

namespace Neudesic.EnterpriseServiceBus.Samples
{
    // Define a service contract.
    [ServiceContract(Namespace="http://Neudesic.EnterpriseServiceBus.Samples")]
    public interface ICalculator
    {
        [OperationContract(IsOneWay=true)]
        void Notify(string text);
    }

    // Service class which implements the service contract.

	[ServiceBehavior(IncludeExceptionDetailInFaults=true)]	
    public class CalculatorService : ICalculator
    {
        public void Notify(string text)
        {
            Console.WriteLine("Received message of size " + text.Length.ToString() + " characters");
        }

        // Host the service in this console application.

        public static void Main()
        {
            Console.WriteLine("Initializing service");
            
            // Create a ServiceHost for the CalculatorService type
            using (ServiceHost serviceHost = new ServiceHost(typeof(CalculatorService)))
            {
                serviceHost.Open();
				
                // The service can now be accessed.
                Console.WriteLine("The service is ready.");
                Console.WriteLine("Press <ENTER> to terminate service.");
                Console.WriteLine();
                Console.ReadLine();

            }
        }
    }

}
