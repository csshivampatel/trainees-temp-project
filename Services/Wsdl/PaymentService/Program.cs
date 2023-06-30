using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace PaymentService
{
    class Program
    {

        static void Main(string[] args)
        {
            ServiceHost shost = new ServiceHost(typeof(Service1));
            shost.Open();

            Console.WriteLine("Host listening for Payment service requests: http://localhost:8733/PaymentService/Service1");
            Console.ReadLine();
        }

    }
}