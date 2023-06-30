using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Messaging;

namespace MSMQMessageProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Create Queues

            MessageQueue receiveQueue = null;
            MessageQueue sendQueue = null;

            // Verify the receive queue exists
            if (!MessageQueue.Exists(@".\Private$\MsgsFromNeuron"))
            {
                // Create the Queue                
                MessageQueue.Create(@".\Private$\MsgsFromNeuron", false);
            }
            receiveQueue = new MessageQueue(@".\Private$\MsgsFromNeuron");
            receiveQueue.Label = "Receive Queue";
            receiveQueue.Formatter = new XmlMessageFormatter(new string[] { "System.String" });
            receiveQueue.MessageReadPropertyFilter.CorrelationId = true;

            // Verify the send queue exists
            if (!MessageQueue.Exists(@".\Private$\MsgsToNeuron"))
            {
                // Create the Queue                
                MessageQueue.Create(@".\Private$\MsgsToNeuron", false);
            }
            sendQueue = new MessageQueue(@".\Private$\MsgsToNeuron");
            sendQueue.Label = "Send Queue";

            Console.WriteLine("Ready to start processing messages");

            #endregion  

            #region Receiving Messages

            while (true)
            {
                Message recMsg = receiveQueue.Receive();

                if (recMsg != null)
                {
                    // The Neuron MSMQ step sets the BodyStream property of the MSMQ message, so that's what we look for
                    if (recMsg.BodyStream != null)
                    {
                        byte[] binaryBody = null;

                        using (System.IO.BinaryReader reader = new System.IO.BinaryReader(recMsg.BodyStream))
                        {
                            binaryBody = new byte[recMsg.BodyStream.Length];
                            reader.Read(binaryBody, 0, binaryBody.Length);
                        }

                        Encoding enc = Encoding.UTF8;
                        string readMessage = enc.GetString(binaryBody);
                        Console.WriteLine(readMessage);
                    }
                    else
                    {
                        string readMessage = recMsg.Body.ToString();
                        Console.WriteLine(readMessage);
                    }

                    // Send 5 correlated responses to the MsgsToNeuron queue
                    for (int x = 0; x < 5; x++)
                    {
                        Message respMsg = new Message("Correlated response " + x);
                        respMsg.CorrelationId = recMsg.CorrelationId;

                        sendQueue.Send(respMsg, "Message " + x);
                    }
                }
            }

            #endregion
        }
    }
}
