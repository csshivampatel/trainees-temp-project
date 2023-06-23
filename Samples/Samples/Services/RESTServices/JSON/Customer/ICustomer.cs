using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Neuron.Samples.JSON
{
    [ServiceContract]
    public interface ICustomerService
    {
        [OperationContract]
        [WebGet(UriTemplate = "/{id}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        Stream GetCustomerByID(string id);
        [OperationContract]
        [WebGet(UriTemplate = "?Name={name}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        Stream GetCustomerByName(string name);
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream AddCustomer(Stream customer);
    }

    [CollectionDataContract(Name = "Customers", Namespace = "")]
    public class Customers : List<Customer>
    {
    }

   // [DataContract(Namespace = "", Name = "Customer")]
    public class Customer
    {
     //   [DataMember(Name = "ID")]
        public int ID;

     //   [DataMember(Name = "Name")]
        public string Name;

     //   [DataMember(Name = "Email")]
        public string Email;
    }
}
