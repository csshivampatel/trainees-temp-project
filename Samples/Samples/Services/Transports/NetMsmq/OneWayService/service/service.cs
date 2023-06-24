
using System;
using System.Messaging;
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
            Console.WriteLine("Received Notify({0})", text);
        }

        // Host the service in this console application.

        public static void Main()
        {
            Console.WriteLine("Initializing service");
            bool haveQueue = true;
            bool createdQueue = false;
            //Create the sample message queue if it does not exist
            if (!MessageQueue.Exists(@".\Private$\oneway"))
            {
                try
                {
                    MessageQueue mq = MessageQueue.Create(@".\Private$\oneway", true);

                    // Set security to ensure Neuron service account can access it
                    mq.SetPermissions("Everyone", MessageQueueAccessRights.FullControl);

                    createdQueue = true;
                }
                catch (MessageQueueException msqe)
                {
                    Console.WriteLine("Failed to create a message queue: " + msqe.Message);
                    haveQueue = false;
                }
            }

            if (!haveQueue)
            {
                Console.WriteLine("Unable to run the service without a message queue");
                Console.ReadLine();
            }
            else
            {
                // Create a ServiceHost for the CalculatorService type
                using (ServiceHost serviceHost = new ServiceHost(typeof(CalculatorService)))
                {
                    serviceHost.Open();

                    // The service can now be accessed.
                    Console.WriteLine("The service is ready.");
                    Console.WriteLine("Press <ENTER> to terminate service.");
                    Console.WriteLine();
                    Console.ReadLine();

                    //delete the message queue if we created it
                    if (createdQueue)
                    {
                        try
                        {
                            MessageQueue.Delete(@".\Private$\oneway");
                        }
                        catch (MessageQueueException msqe)
                        {
                            Console.WriteLine("Failed to delete the message queue created by this sample: " + msqe.Message);
                            Console.WriteLine("You will need to manualy delete the queue");
                            Console.ReadLine();
                        }
                    }

                }

            }
        }
    }

}
