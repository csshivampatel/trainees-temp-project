using System;
using System.ServiceModel;
using System.Xml;
using System.Xml.Serialization;

namespace Neuron.Esb.Samples.ScatterGather
{

    [ServiceContract(Namespace = "http://schema.neuron.sample/newmart/broadcast")]
    public interface IBid
    {
        [OperationContract]
        [XmlSerializerFormat]
        QuoteResult RequestBid(BidQuery request);
    }

    [MessageContract(WrapperNamespace = "http://schema.neuron.sample/newmart/broadcast/request")]
    public class BidQuery
    {
        [MessageBodyMember(Namespace = "http://schema.neuron.sample/newmart/broadcast/request")]
        [XmlElement("Catalog")]
        public Catalog catalog;
    }

    public class Catalog
    {
        [XmlAttribute("productId")]
        public string productId;
        [XmlAttribute("qty")]
        public int qty;
        [XmlAttribute("distributionCenter")]
        public string distributionCenter;
    }

    [MessageContract(WrapperNamespace = "http://schema.neuron.sample/newmart/broadcast/result")]
    public class QuoteResult
    {
        [MessageBodyMember(Namespace = "http://schema.neuron.sample/newmart/broadcast/result")]
        [XmlElement("Vendor")]
        public Vendor vendor;
    }

    public class Vendor
    {
        [XmlAttribute("name")]
        public string name;
        [XmlAttribute("vendorId")]
        public string vendorId;
        [XmlAttribute("address")]
        public string address;
        [XmlAttribute("city")]
        public string city;
        [XmlAttribute("state")]
        public string state;
        [XmlAttribute("zip")]
        public string zip;

        [MessageBodyMember(Namespace = "http://schema.neuron.sample/newmart/broadcast/result")]
        [XmlElement("Product")]
        public VendorProduct product;
    }

    public class VendorProduct
    {
        [XmlAttribute("SKU")]
        public string SKU;
        [XmlAttribute("instock")]
        public bool instock;
        [XmlAttribute("quantity")]
        public int quantity;
        [XmlAttribute("price")]
        public double price;
    }


    [ServiceBehavior(ValidateMustUnderstand = false, AddressFilterMode = AddressFilterMode.Any)]
    public class BidService : IBid
    {

        public BidService()
        {
        }

        public QuoteResult RequestBid(BidQuery request)
        {
            XmlSerializer serializer = new XmlSerializer(request.GetType());
            serializer.Serialize(Console.Out, request);


            QuoteResult response = new QuoteResult();
            response.vendor = new Vendor();
            response.vendor.product = new VendorProduct();

            response.vendor.name = "NEW MART";
            response.vendor.state = "MA";
            response.vendor.city = "Boston";
            response.vendor.address = "879 some street";
            response.vendor.vendorId = "GID000987";
            response.vendor.zip = "90008";
            response.vendor.product.instock = true;
            response.vendor.product.price = 125.67;
            response.vendor.product.quantity = request.catalog.qty;
            response.vendor.product.SKU = request.catalog.productId;

            return response;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost shost = new ServiceHost(typeof(BidService));

            shost.Open();

            Console.WriteLine("New Mart Host listening");
            Console.ReadLine();
        }
    }
}