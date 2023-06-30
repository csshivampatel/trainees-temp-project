using System;
using System.ServiceModel;
using System.Xml;
using System.Xml.Serialization;

namespace Neuron.Esb.Samples.ScatterGather
{
    [ServiceContract(Namespace = "http://schema.neuron.sample/oldmart/broadcast")]
    public interface IQuote
    {
        [OperationContract]
        [XmlSerializerFormat]
        QuoteResult RequestQuote(QuoteRequest request);
    }

    [MessageContract(WrapperNamespace = "http://schema.neuron.sample/oldmart/broadcast/request")]
    public class QuoteRequest
    {
        [MessageBodyMember(Namespace = "http://schema.neuron.sample/oldmart/broadcast/request")]
        [XmlElement("Products")]
        public Products products; 
    }

    public class Products
    {
        [MessageBodyMember(Namespace = "http://schema.neuron.sample/oldmart/broadcast/request")]
        [XmlElement("Product")]
        public Product product;
    }

    public class Product
    {
        [XmlAttribute("SKU")]
        public string SKU;
        [XmlAttribute("quanity")]
        public int quanity;
        [XmlAttribute("warehouse")]
        public string warehouse;
    }

    [MessageContract(WrapperNamespace = "http://schema.neuron.sample/oldmart/broadcast/result")]
    public class QuoteResult
    {
        [MessageBodyMember(Namespace = "http://schema.neuron.sample/oldmart/broadcast/result")]
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

        [MessageBodyMember(Namespace = "http://schema.neuron.sample/oldmart/broadcast/result")]
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
    public class QuoteService : IQuote
    {
        public QuoteService()
        {
        }

        public QuoteResult RequestQuote(QuoteRequest request)
        {
            XmlSerializer serializer = new XmlSerializer(request.GetType());
            serializer.Serialize(Console.Out, request);


            QuoteResult response = new QuoteResult();
            response.vendor = new Vendor();
            response.vendor.product = new VendorProduct();

            response.vendor.name = "OLD MART";
            response.vendor.state = "CA";
            response.vendor.city = "Los Angeles";
            response.vendor.address = "476 some street";
            response.vendor.vendorId = "GID989087";
            response.vendor.zip = "98989";
            response.vendor.product.instock = true;
            response.vendor.product.price = 55.67;
            response.vendor.product.quantity = request.products.product.quanity;
            response.vendor.product.SKU = request.products.product.SKU;
            
            return response;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost shost = new ServiceHost(typeof(QuoteService));

            shost.Open();
            
            Console.WriteLine("Old Mart Host listening");
            Console.ReadLine();
        }
    }
}
