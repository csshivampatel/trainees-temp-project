using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Runtime.Serialization;
using System.Text;

namespace Neudesic
{
    public class OrderEntryService : IOrderEntry
    {
        static void Main(string[] args)
        {
            using (ServiceHost host = new ServiceHost(typeof(OrderEntryService), new Uri("http://localhost:8000/OrderEntryService/")))
            {
                host.Open();
                Console.WriteLine("OrderEntryService is running - press <ENTER> to shut down");
                Console.ReadLine();
                host.Close();
            }
        }

        public void SubmitOrder(Order order)
        {
            Console.WriteLine("Received order {0}", order.OrderId);
        }

        public void ProcessMessage(Message message)
        {
            Console.WriteLine("Received a message");
        }
    }

    [ServiceContract]
    public interface IOrderEntry
    {
        [OperationContract(IsOneWay = true)]                void SubmitOrder(Order order);
        [OperationContract(IsOneWay = true, Action="*")]    void ProcessMessage(Message message);
    }

    [DataContract]
    public class Order
    {
        [DataMember]    public int OrderId;
        [DataMember]    public string Name;
        [DataMember]    public string Address;
        [DataMember]    public string Address2;
        [DataMember]    public int Qty;
        [DataMember]    public string SKU;
        [DataMember]    public decimal UnitPrice;
        [DataMember]    public decimal SubTotal;
    }
}
