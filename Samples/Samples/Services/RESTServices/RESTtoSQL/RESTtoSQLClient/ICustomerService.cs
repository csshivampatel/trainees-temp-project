using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Web;

namespace Neuron.Samples.CustomerREST
{
    [ServiceContract]
    public interface ICustomerService
    {
        [OperationContract]
        [WebGet(UriTemplate = "?ID={id}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Customer GetCustomerByID(string id);

        [OperationContract]
        [WebInvoke(UriTemplate = "", Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        void AddCustomer(Customer customer);

        [OperationContract]
        [WebInvoke(UriTemplate = "?ID={id}", Method = "PUT", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        void UpdateCustomer(string id, Customer customer);

        [OperationContract]
        [WebInvoke(UriTemplate = "?ID={id}", Method = "DELETE", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        void DeleteCustomer(string id);
    }

    [CollectionDataContract(Name = "Customers", Namespace = "")]
    public class Customers : List<Customer>
    {
    }

    [DataContract(Namespace = "", Name = "Customer")]
    public class Customer
    {
        [DataMember(Name = "id")]
        public int ID;

        [DataMember(Name = "name")]
        public string Name;

        [DataMember(Name = "email")]
        public string Email;
    }
}
