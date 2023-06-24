using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Neuron.Samples.CustomerSOAP
{
    [ServiceContract(Namespace = "http://Neuron.Samples")]
    public interface ICustomerService
    {
        [OperationContract]
        [FaultContract(typeof(Result))]
        Customers GetAllCustomers();

        [OperationContract]
        [FaultContract(typeof(Result))]
        Customer GetCustomerByID(string id);

        [OperationContract]
        [FaultContract(typeof(Result))]
        Customer GetCustomerByName(string name);

        [OperationContract]
        [FaultContract(typeof(Result))]
        Result AddCustomer(Customer customer);
    }

    [CollectionDataContract(Namespace = "http://Neuron.Samples", Name = "customers")]
    public class Customers : List<Customer>
    {
    }

    [DataContract(Namespace = "http://Neuron.Samples", Name = "customer")]
    public class Customer
    {
        [DataMember(Name = "ID")]
        public int ID;

        [DataMember(Name = "Name")]
        public string Name;

        [DataMember(Name = "Email")]
        public string Email;
    }

    [DataContract(Namespace = "http://Neuron.Samples", Name = "result")]
    public class Result
    {
        public Result(bool result)
        {
            Success = result;
            ErrorMsg = string.Empty;
        }

        public Result(bool result, string error)
        {
            Success = result;
            ErrorMsg = error;
        }

        [DataMember(Name = "success")]
        public bool Success;

        [DataMember(Name = "errorMsg")]
        public string ErrorMsg;
    }
}
