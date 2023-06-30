using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;

namespace RoutingAndTracingWCF
{
    public class Program
    {
        static void Main(string[] args)
        {
            ServiceHost hostA = null;
            ServiceHost hostB = null;

            try
            {
                hostA = new ServiceHost(typeof(RoutingAndTracingWCF.CityOrderService));
                hostB = new ServiceHost(typeof(RoutingAndTracingWCF.OutofCityOrderService));
                hostA.Open();

                hostB.Open();

                Console.WriteLine();
                Console.WriteLine("Press <ENTER> to terminate Host");
                Console.ReadLine();
            }
            finally
            {
                if (hostA.State == CommunicationState.Faulted)
                    hostA.Abort();
                else
                    hostA.Close();

                if (hostB.State == CommunicationState.Faulted)
                    hostB.Abort();
                else
                    hostB.Close();
            }
        }

    }
}