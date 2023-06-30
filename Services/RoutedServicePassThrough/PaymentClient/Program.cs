using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Text;

namespace Neudesic
{
    public class PaymentClient
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press <ENTER> to start PaymentClient");
            Console.ReadLine();
            
            IPayment Payment = new ChannelFactory<IPayment>("Payment").CreateChannel();

            for (int account = 1001; account <= 1010; account++)
            {
                Payment payment = new Payment();
                payment.Account = account.ToString();
                payment.Method = "VISA";
                payment.Name = "John Smith";
                payment.Address = "100 Main St";
                payment.Address = "Tempe, AZ 87654";
                payment.Amount = 100.00M;

                Console.WriteLine("Submitting payment for account {0}", payment.Account);
                string approvalCode = Payment.ApprovePayment(payment);
                Console.WriteLine("    Approval code: " + approvalCode);
            }

            Console.WriteLine("Press <ENTER> to shut down");
            Console.ReadLine();
        }

    }

}
