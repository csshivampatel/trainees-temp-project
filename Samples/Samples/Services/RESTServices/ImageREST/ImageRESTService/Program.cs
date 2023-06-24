using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;

namespace Neuron.Samples.ImageREST
{
    [ServiceContract]
    public interface IReceiveData
    {
        [WebInvoke(UriTemplate = "UploadFile/{fileName}")]
        void UploadFile(string fileName, Stream fileContents);
    }

    public class RawDataService : IReceiveData
    {
        public void UploadFile(string fileName, Stream fileContents)
        {
            using (FileStream fs = File.OpenWrite(@"..\..\..\Out\" + fileName))
            {
                byte[] buffer = new byte[2048];

                while (true)
                {
                    var read = fileContents.Read(buffer, 0, buffer.Length);

                    if (read == 0)
                        break;

                    fs.Write(buffer, 0, read);
                }
                Console.WriteLine("Service: Received file {0} with {1} bytes", fileName, fs.Length);
                fs.Close();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            WebHttpBinding binding = new WebHttpBinding();
            binding.MaxReceivedMessageSize = Int32.MaxValue;

            string baseAddress = "http://localhost:8021/Image";
            using (ServiceHost host = new ServiceHost(typeof(RawDataService), new Uri(baseAddress)))
            {
                host.AddServiceEndpoint(typeof(IReceiveData), binding, "").Behaviors.Add(new WebHttpBehavior());
                host.Open();
                Console.WriteLine("Host opened");
                Console.WriteLine("Press <ENTER> to close.");

                Console.ReadLine();
            }
        }
    }
}
