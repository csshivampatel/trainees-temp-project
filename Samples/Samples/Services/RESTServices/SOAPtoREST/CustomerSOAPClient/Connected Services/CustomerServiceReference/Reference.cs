﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Neuron.Samples.CustomerSOAP.CustomerServiceReference {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.CollectionDataContractAttribute(Name="customers", Namespace="http://Neuron.Samples", ItemName="customer")]
    [System.SerializableAttribute()]
    public class customers : System.Collections.Generic.List<Neuron.Samples.CustomerSOAP.CustomerServiceReference.customer> {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="customer", Namespace="http://Neuron.Samples")]
    [System.SerializableAttribute()]
    public partial class customer : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string EmailField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int IDField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string NameField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Email {
            get {
                return this.EmailField;
            }
            set {
                if ((object.ReferenceEquals(this.EmailField, value) != true)) {
                    this.EmailField = value;
                    this.RaisePropertyChanged("Email");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int ID {
            get {
                return this.IDField;
            }
            set {
                if ((this.IDField.Equals(value) != true)) {
                    this.IDField = value;
                    this.RaisePropertyChanged("ID");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Name {
            get {
                return this.NameField;
            }
            set {
                if ((object.ReferenceEquals(this.NameField, value) != true)) {
                    this.NameField = value;
                    this.RaisePropertyChanged("Name");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="result", Namespace="http://Neuron.Samples")]
    [System.SerializableAttribute()]
    public partial class result : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string errorMsgField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool successField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string errorMsg {
            get {
                return this.errorMsgField;
            }
            set {
                if ((object.ReferenceEquals(this.errorMsgField, value) != true)) {
                    this.errorMsgField = value;
                    this.RaisePropertyChanged("errorMsg");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool success {
            get {
                return this.successField;
            }
            set {
                if ((this.successField.Equals(value) != true)) {
                    this.successField = value;
                    this.RaisePropertyChanged("success");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://Neuron.Samples", ConfigurationName="CustomerServiceReference.ICustomerService")]
    public interface ICustomerService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://Neuron.Samples/ICustomerService/GetAllCustomers", ReplyAction="http://Neuron.Samples/ICustomerService/GetAllCustomersResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(Neuron.Samples.CustomerSOAP.CustomerServiceReference.result), Action="http://Neuron.Samples/ICustomerService/GetAllCustomersResultFault", Name="result")]
        Neuron.Samples.CustomerSOAP.CustomerServiceReference.customers GetAllCustomers();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://Neuron.Samples/ICustomerService/GetAllCustomers", ReplyAction="http://Neuron.Samples/ICustomerService/GetAllCustomersResponse")]
        System.Threading.Tasks.Task<Neuron.Samples.CustomerSOAP.CustomerServiceReference.customers> GetAllCustomersAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://Neuron.Samples/ICustomerService/GetCustomerByID", ReplyAction="http://Neuron.Samples/ICustomerService/GetCustomerByIDResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(Neuron.Samples.CustomerSOAP.CustomerServiceReference.result), Action="http://Neuron.Samples/ICustomerService/GetCustomerByIDResultFault", Name="result")]
        Neuron.Samples.CustomerSOAP.CustomerServiceReference.customer GetCustomerByID(string id);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://Neuron.Samples/ICustomerService/GetCustomerByID", ReplyAction="http://Neuron.Samples/ICustomerService/GetCustomerByIDResponse")]
        System.Threading.Tasks.Task<Neuron.Samples.CustomerSOAP.CustomerServiceReference.customer> GetCustomerByIDAsync(string id);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://Neuron.Samples/ICustomerService/GetCustomerByName", ReplyAction="http://Neuron.Samples/ICustomerService/GetCustomerByNameResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(Neuron.Samples.CustomerSOAP.CustomerServiceReference.result), Action="http://Neuron.Samples/ICustomerService/GetCustomerByNameResultFault", Name="result")]
        Neuron.Samples.CustomerSOAP.CustomerServiceReference.customer GetCustomerByName(string name);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://Neuron.Samples/ICustomerService/GetCustomerByName", ReplyAction="http://Neuron.Samples/ICustomerService/GetCustomerByNameResponse")]
        System.Threading.Tasks.Task<Neuron.Samples.CustomerSOAP.CustomerServiceReference.customer> GetCustomerByNameAsync(string name);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://Neuron.Samples/ICustomerService/AddCustomer", ReplyAction="http://Neuron.Samples/ICustomerService/AddCustomerResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(Neuron.Samples.CustomerSOAP.CustomerServiceReference.result), Action="http://Neuron.Samples/ICustomerService/AddCustomerResultFault", Name="result")]
        Neuron.Samples.CustomerSOAP.CustomerServiceReference.result AddCustomer(Neuron.Samples.CustomerSOAP.CustomerServiceReference.customer customer);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://Neuron.Samples/ICustomerService/AddCustomer", ReplyAction="http://Neuron.Samples/ICustomerService/AddCustomerResponse")]
        System.Threading.Tasks.Task<Neuron.Samples.CustomerSOAP.CustomerServiceReference.result> AddCustomerAsync(Neuron.Samples.CustomerSOAP.CustomerServiceReference.customer customer);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface ICustomerServiceChannel : Neuron.Samples.CustomerSOAP.CustomerServiceReference.ICustomerService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class CustomerServiceClient : System.ServiceModel.ClientBase<Neuron.Samples.CustomerSOAP.CustomerServiceReference.ICustomerService>, Neuron.Samples.CustomerSOAP.CustomerServiceReference.ICustomerService {
        
        public CustomerServiceClient() {
        }
        
        public CustomerServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public CustomerServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public CustomerServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public CustomerServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public Neuron.Samples.CustomerSOAP.CustomerServiceReference.customers GetAllCustomers() {
            return base.Channel.GetAllCustomers();
        }
        
        public System.Threading.Tasks.Task<Neuron.Samples.CustomerSOAP.CustomerServiceReference.customers> GetAllCustomersAsync() {
            return base.Channel.GetAllCustomersAsync();
        }
        
        public Neuron.Samples.CustomerSOAP.CustomerServiceReference.customer GetCustomerByID(string id) {
            return base.Channel.GetCustomerByID(id);
        }
        
        public System.Threading.Tasks.Task<Neuron.Samples.CustomerSOAP.CustomerServiceReference.customer> GetCustomerByIDAsync(string id) {
            return base.Channel.GetCustomerByIDAsync(id);
        }
        
        public Neuron.Samples.CustomerSOAP.CustomerServiceReference.customer GetCustomerByName(string name) {
            return base.Channel.GetCustomerByName(name);
        }
        
        public System.Threading.Tasks.Task<Neuron.Samples.CustomerSOAP.CustomerServiceReference.customer> GetCustomerByNameAsync(string name) {
            return base.Channel.GetCustomerByNameAsync(name);
        }
        
        public Neuron.Samples.CustomerSOAP.CustomerServiceReference.result AddCustomer(Neuron.Samples.CustomerSOAP.CustomerServiceReference.customer customer) {
            return base.Channel.AddCustomer(customer);
        }
        
        public System.Threading.Tasks.Task<Neuron.Samples.CustomerSOAP.CustomerServiceReference.result> AddCustomerAsync(Neuron.Samples.CustomerSOAP.CustomerServiceReference.customer customer) {
            return base.Channel.AddCustomerAsync(customer);
        }
    }
}
