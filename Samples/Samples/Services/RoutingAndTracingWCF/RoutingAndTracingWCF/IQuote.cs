using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace RoutingAndTracingWCF
{
    [ServiceContract(Namespace = "http://PeregrineConnnect.ServiceModel.Samples")]
    public interface IQuote
    {
        [OperationContract]
        Quote RequestQuote(string Product, int Quantity, string City);
        [OperationContract]
        Order ConvertQuoteToOrder(int QuoteNumber, string City);
    }

    [DataContract(Namespace = "http://PeregrineConnnect.ServiceModel.Samples")]
    public class Quote 
    {
        [DataMember]
        public int QuoteNumber { get; set; }
        [DataMember]
        public DateTime QuoteDate { get; set; }
        [DataMember]
        public string Product { get; set; }
        [DataMember]
        public int Quantity { get; set; }
        [DataMember]
        public string City { get; set; }
        [DataMember]
        public double Price { get; set; }
        [DataMember]
        public double Shipping { get; set; }
        [DataMember]
        public double Amount { get; set; }
    }
}
