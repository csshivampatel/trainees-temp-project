using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Xml.Serialization;

namespace PaymentService
{
    [ServiceBehavior(Namespace="http://neuron.esb.samples")]
    public class Service1 : IService1
    {

        public PaymentProcessResponse[] ProcessPayment(PaymentProcessRequest[] requests)
        {
            List<PaymentProcessRequest> originalRequests = new List<PaymentProcessRequest>();
            originalRequests.AddRange(requests);

            List<PaymentProcessResponse> responses = new List<PaymentProcessResponse>();
            PaymentProcessResponse response;
            
            foreach (PaymentProcessRequest req in requests)
            {
                response = new PaymentProcessResponse();
                response.Authorized = true;
                responses.Add(response);
            }

            System.IO.StringWriter stringWriter = new System.IO.StringWriter();
            System.Xml.XmlTextWriter xmlWriter = new System.Xml.XmlTextWriter(stringWriter);
            XmlSerializer serializer = new XmlSerializer(typeof(List<PaymentProcessRequest>));
            serializer.Serialize(xmlWriter, originalRequests); 


            Console.WriteLine("    Batch of Process Payments = " + stringWriter.ToString());
            Console.WriteLine("");
            Console.WriteLine("");
            return responses.ToArray();
        }
    }
}
