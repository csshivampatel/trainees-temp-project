using System;
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
            CalculatorClient client = new CalculatorClient("requestreply");

            try
            {
                Console.WriteLine("Press <ENTER> to begin messaging");
                Console.ReadLine();

                int messageSize = 1024 * 1024 * 10;

                for (int m = 0; m < ITERATIONS; m++)
                {
                    // Call the Notify operation.

                    string text = CreateLargeMessage(messageSize);

                    Console.WriteLine("Sending message of " + text.Length.ToString() + " characters");

                    client.Notify(text);

                    messageSize = messageSize * 2;
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

        private static string CreateLargeMessage(int length)
        {
            return new String('X', length);
        }
    }
}
