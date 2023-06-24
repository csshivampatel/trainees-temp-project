using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace RoutingAndTracingWCF
{
    [ServiceContract(Namespace ="http://PeregrineConnnect.ServiceModel.Samples")]
    public interface IOrder
    {
        [OperationContract]
        Order GetOrder(int OrderNumber, string City);
        [OperationContract]
        int PlaceOrder(Order order, string City);
    }

    [DataContract(Namespace = "http://PeregrineConnnect.ServiceModel.Samples")]
    public class Order
    {
        [DataMember]
        public int OrderNumber { get; set; }

        [DataMember]
        public string CustomerName { get; set; }

        [DataMember]
        DateTime OrderDate { get; set; }

        [DataMember]
        public string Product { get; set; }

        [DataMember]
        public int Quantity { get; set; }

        [DataMember]
        public Decimal Price { get; set; }

        [DataMember]
        public Double Amount { get; set; }

        [DataMember]
        public Double Shipping { get; set; }

        [DataMember]
        public string City { get; set; }
    }

    [DataContract(Namespace = "http://PeregrineConnnect.ServiceModel.Samples")]
    public class OrderResponse
    {
        [DataMember]
        public int orderNumber { get; set; }
       }
       
       }
       
