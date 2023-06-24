using System;
using System.IO;
using System.Net;

namespace Neuron.Samples.ImageREST
{
    class Program
    {
        static void Main(string[] args)
        {
            CallRestService();
            Console.ReadLine();
        }

        public static void CallRestService()
        {
            try
            {
                Console.WriteLine("Press <ENTER> to send messages to server");
                Console.ReadKey();

                string baseurl = "http://" + Environment.MachineName + ":8020/Image";

                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(baseurl + "/UploadFile/Neuron.jpg");
                req.Method = "POST";
                req.ContentType = "image/jpeg";

                using (var reqStream = req.GetRequestStream())
                {
                    Console.WriteLine("Sending image");

                    FileStream fs = File.OpenRead(@"..\..\..\Neuron.jpg");

                    byte[] fileToSend = new byte[fs.Length];
                    fs.Read(fileToSend, 0, (int) fs.Length);
                    fs.Close();
                    reqStream.Write(fileToSend, 0, fileToSend.Length);
                }

                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

                Console.WriteLine("Image sent");
                Console.WriteLine("Client: Receive Response HTTP/{0} {1} {2}", resp.ProtocolVersion, (int)resp.StatusCode, resp.StatusDescription);
                    
                Console.WriteLine();
                Console.WriteLine("Press <Enter> to close");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown - " + ex.Message);
            }
        }
    }
}
