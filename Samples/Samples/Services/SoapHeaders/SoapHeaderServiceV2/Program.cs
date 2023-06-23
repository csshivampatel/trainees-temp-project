using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using Neuron.Samples.Soap.Headers;

namespace EchoAnything
{
    [ServiceContract]
    public interface IEchoAnything
    {
        [OperationContract(Action = "*", ReplyAction = "*")]
        Message Echo(Message input);

    }

    [ServiceBehavior(ValidateMustUnderstand = false, AddressFilterMode = AddressFilterMode.Any)]
    public class EchoAnythingService : IEchoAnything
    {
        class Program
        {
            static void Main(string[] args)
            {
                ServiceHost shost = new ServiceHost(typeof(EchoAnythingService));
                shost.Open();

                Console.WriteLine("V2 Service listening for service requests - PORT 8742");
                Console.ReadLine();
            }
        }

        public EchoAnythingService()
        {
        }

        public Message Echo(Message input)
        {
            Message retVal = null;

            retVal = input.CreateBufferedCopy(int.MaxValue).CreateMessage();

            Console.WriteLine("Request message received:");
            Console.WriteLine(input.ToString());
            Console.WriteLine();
            return retVal;
        }
    }
}
