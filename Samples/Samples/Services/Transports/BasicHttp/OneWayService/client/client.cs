
using System;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;

namespace Neudesic.EnterpriseServiceBus.Samples
{
    //The service contract is defined in generatedClient.cs, generated from the service by the svcutil tool.
    
    //Client implementation code.
    
    class Client
    {
        const int ITERATIONS = 1;

        static void Main()
        {
            Console.WriteLine("Initializing client");

            // Create a client with requestreply endpoint configuration
            CalculatorClient client = new CalculatorClient("oneway");

            try
            {
                Console.WriteLine("Press <ENTER> to begin messaging");
                Console.ReadLine();

                for (int m = 0; m < ITERATIONS; m++)
                {
                    // Call the Notify operation.

                    Console.WriteLine("Calling Notify");
                    client.Notify("The quick brown fox jumped over the lazy dogs.");
                }
                client.Close();
            }
            catch (TimeoutException e)
            {
                Console.WriteLine("Call timed out : {0}", e.Message);
                client.Abort();
            }
            catch (CommunicationException e)
            {
                Console.WriteLine("Call failed : {0}", e.Message);
                client.Abort();
            }
            catch (Exception e)
            {
                Console.WriteLine("Call failed : {0}", e.ToString());
                client.Abort();
            }

            Console.WriteLine();
            Console.WriteLine("Press <ENTER> to terminate client.");
            Console.ReadLine();
        }
    }
}
