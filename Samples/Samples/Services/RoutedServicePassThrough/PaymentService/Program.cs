using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Runtime.Serialization;
using System.Text;

namespace Neudesic
{
    public class PaymentService : IPayment
    {
        static void Main(string[] args)
        {
            using (ServiceHost host = new ServiceHost(typeof(PaymentService), new Uri("http://localhost:8200/PaymentService/")))
            {
                host.Open();
                Console.WriteLine("PaymentService is running - press <ENTER> to shut down");
                Console.ReadLine();
                host.Close();
            }
        }

        public string ApprovePayment(Payment payment)
        {
            string approvalCode = Guid.NewGuid().ToString();
            Console.WriteLine("Received payment {0}, sending approval code {1}", payment.Account, approvalCode);

            //System.Threading.Thread.Sleep(1000 * 3);
            return approvalCode;
        }

        public void ProcessMessage(Message message)
        {
            Console.WriteLine("Received a message");
        }
    }

    [ServiceContract]
    public interface IPayment
    {
        [OperationContract]               string ApprovePayment(Payment payment);
        [OperationContract(Action="*")]    void ProcessMessage(Message message);
    }

    [DataContract]
    public class Payment
    {
        [DataMember]    public string Account;
        [DataMember]    public string Method;
        [DataMember]    public string Name;
        [DataMember]    public string Address;
        [DataMember]    public string Address2;
        [DataMember]    public decimal Amount;
    }
}
