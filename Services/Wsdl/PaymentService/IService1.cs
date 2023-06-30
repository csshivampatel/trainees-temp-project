using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace PaymentService
{
    // NOTE: If you change the interface name "IService1" here, you must also update the reference to "IService1" in App.config.
    [ServiceContract(Namespace="http://neuron.esb.samples")]
    public interface IService1
    {

        [OperationContract]
        PaymentProcessResponse[] ProcessPayment(PaymentProcessRequest[] request);

    }


    [DataContract]
    public class PaymentProcessResponse
    {
        bool _authorized = false;


        [DataMember]
        public bool Authorized
        {
            get { return _authorized; }
            set { _authorized = value; }
        }

    }
    
    [DataContract]
    public class PaymentProcessRequest
    {
        int _amount = 0;
        Guid _orderId = Guid.NewGuid();

        [DataMember]
        public int Amount
        {
            get { return _amount; }
            set { _amount = value; }
        }
        [DataMember]
        public Guid OrderId
        {
            get { return _orderId; }
            set { _orderId = value; }
        }

    }
}
