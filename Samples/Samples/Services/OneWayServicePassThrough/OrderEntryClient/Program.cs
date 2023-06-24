using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Text;

namespace Neudesic
{
    public class OrderEntryClient
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press <ENTER> to start OrderEntryClient");
            Console.ReadLine();
            
            IOrderEntry orderEntry = new ChannelFactory<IOrderEntry>("OrderEntry").CreateChannel();

            for (int orderId = 1001; orderId <= 1010; orderId++)
            {
                Order order = new Order();
                order.OrderId = orderId;
                order.Name = "John Smith";
                order.Address = "100 Main St";
                order.Address = "Tempe, AZ 87654";
                order.Qty = 2;
                order.SKU = "100-001";
                order.UnitPrice = 50.00M;
                order.SubTotal = order.Qty * order.UnitPrice;

                Console.WriteLine("Submitting order {0}", order.OrderId);
                orderEntry.SubmitOrder(order);
            }

            Console.WriteLine("Press <ENTER> to shut down");
            Console.ReadLine();
        }

        public void SubmitOrder(Order order)
        {
            Console.WriteLine("Received order {0}", order.OrderId);
        }
    }

}
