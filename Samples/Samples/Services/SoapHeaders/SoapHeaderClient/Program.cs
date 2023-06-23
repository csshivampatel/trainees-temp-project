using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using System.Xml.Linq;
using System.Net;
using Neuron.Samples.Soap.Headers;

namespace Neuron.Esb.Samples
{
    class Program
    {
        #region constants
        const string _routeByActionEndpoint = "http://localhost:9000/northstar/hr";
        const string _routeByHeaderEndpoint = "http://localhost:9001/northstar/hr";
        const string _propName = "SecurityHeader";
        const string _ns = "http://www.neuronesb.com/samples/soapheaders"; 
        const string _msg = 
@"<PurchaseRequest>
    <Products>
        <Product name='BigBox' quanity='10' location='Denver'></Product>
    </Products>
</PurchaseRequest>";
        #endregion

        static void Main(string[] args)
        {
            Console.WriteLine("Press ENTER to call the Neuron Soap Header Services");
            Console.ReadLine();

            CallV1Service("V1");
            CallV1Service("V2");
            CallUsingSimpleSoapHeader();
            CallUsingCustomSoapHeader();

            Console.ReadLine();
        }

        /// <summary>
        /// Use the RouteByAction Neuron process that routes by action
        /// </summary>
        public static void CallV1Service(string action)
        {
            Console.WriteLine("Calling the RouteByAction Service with Action = " + action);

            XmlReader xmlReader = null;
            Message reqMsg = null;

 
            // Sending a request/response to a client connector
            using (var chan = new ChannelFactory<IRequestChannel>(new WSHttpBinding(SecurityMode.None), new EndpointAddress(_routeByActionEndpoint)))
            {
                var proxy = chan.CreateChannel();

                using (xmlReader = CreateMessage(_msg))
                {
                    using (reqMsg = Message.CreateMessage(chan.Endpoint.Binding.MessageVersion, action, xmlReader))
                    {
                        Message respMsg = proxy.Request(reqMsg);
                        Console.WriteLine("Response message received:");
                        Console.WriteLine(respMsg.ToString());
                        Console.WriteLine();
                        reqMsg.Close();
                    }
                    xmlReader.Close();
                }
                proxy.Close();
                chan.Close();
            }
        }

        /// <summary>
        /// Use the RouteByAction Neuron process that routes by action
        /// </summary>
        static void CallUsingSimpleSoapHeader()
        {
            Console.WriteLine("Calling the RouteByHeader Service with a simple Soap Header");

            XmlReader xmlReader = null;
            Message reqMsg = null;

            // Sending a request/response to a client connector
            using (var chan = new ChannelFactory<IRequestChannel>(new WSHttpBinding(SecurityMode.None), new EndpointAddress(_routeByHeaderEndpoint)))
            {
                var proxy = chan.CreateChannel();

                using (xmlReader = CreateMessage(_msg))
                {
                    using (reqMsg = Message.CreateMessage(chan.Endpoint.Binding.MessageVersion, "Echo", xmlReader))
                    {
                        // add simple header to message
                        MessageHeader header = MessageHeader.CreateHeader("Route", _ns, "NorthStar.HR.V1");
                        reqMsg.Headers.Add(header);

                        Message respMsg = proxy.Request(reqMsg);
                        Console.WriteLine("Response message received:");
                        Console.WriteLine(respMsg.ToString());
                        Console.WriteLine();
                        reqMsg.Close();
                    }
                    xmlReader.Close();
                }
                proxy.Close();
                chan.Close();
            }
        }

        /// <summary>
        /// Use the RouteBySecuritySoapHeader Neuron process. This will use the contents of the soap header to route.
        /// Also, the RouteByAction Neuron pipeline can be used since we're also setting the action
        /// </summary>
        public static void CallUsingCustomSoapHeader()
        {
            Console.WriteLine("Calling the RouteByHeader Service with a complex Soap Header");

            XmlReader xmlReader = null;
            Message reqMsg = null;

            // create custom soap header
            SecurityHeader securityHeader = new SecurityHeader { Token = Guid.NewGuid().ToString(), Service = "NorthStar.HR.V2" };

            using (var chan = new ChannelFactory<IRequestChannel>(new WSHttpBinding(SecurityMode.None), new EndpointAddress(_routeByHeaderEndpoint)))
            {
                var proxy = chan.CreateChannel();

                using (xmlReader = CreateMessage(_msg))
                {
                    using (reqMsg = Message.CreateMessage(chan.Endpoint.Binding.MessageVersion, "Echo", xmlReader))
                    {
                        // add custom header to message
                        MessageHeader<SecurityHeader> header = new MessageHeader<SecurityHeader>(securityHeader);
                        var untyped = header.GetUntypedHeader(_propName, _ns);
                        reqMsg.Headers.Add(untyped);

                        Message respMsg = proxy.Request(reqMsg);
                        Console.WriteLine("Response message received:");
                        Console.WriteLine(respMsg.ToString());
                        Console.WriteLine();
                        reqMsg.Close();
                    }
                    xmlReader.Close();
                }
                proxy.Close();
                chan.Close();
            }
        }

        #region helper functions

        /// <summary>
        /// creates wcf message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        static XmlReader CreateMessage(string message)
        {
            // create message
            string msg = message;

            
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(msg));
            return XmlReader.Create(memoryStream);
        }

        #endregion

    }
}
