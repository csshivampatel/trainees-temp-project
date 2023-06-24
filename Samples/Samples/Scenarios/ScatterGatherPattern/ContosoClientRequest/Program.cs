using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace Neuron.Esb.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press <ENTER> to submit the request to Neuron");
            Console.ReadLine();
            Utility.PipelineByWebService();
            Console.ReadLine();
        }
    }

    static class Utility
    {
        const string _msg = @"<PurchaseRequest>
	                            <Products>
		                            <Product name='BigBox' quanity='10' location='Denver'></Product>
	                            </Products>
                            </PurchaseRequest>";


        public static void PipelineByWebService()
        {
            XmlReader xmlReader = null;
            Message reqMsg = null;
            
            // Sending a request/response to a client connector
            using (var chan = new ChannelFactory<IRequestChannel>(new WSHttpBinding(),
                                                    new EndpointAddress("http://localhost:9001")))
            {
                var proxy = chan.CreateChannel();
                using (xmlReader = XmlReader.Create(new System.IO.MemoryStream(
                                                        System.Text.Encoding.UTF8.GetBytes(_msg))))
                {
                    using (reqMsg = Message.CreateMessage(chan.Endpoint.Binding.MessageVersion, "",
                                                                                            xmlReader))
                    {
                      
                        Console.WriteLine(proxy.Request(reqMsg).ToString());
                        reqMsg.Close();
                    }
                    xmlReader.Close();
                }
                proxy.Close();
                chan.Close();
            }
        }
    }
}
