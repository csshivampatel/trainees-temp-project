using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Neuron.Samples.CustomerREST
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
            HttpClient client = new HttpClient();

            try
            {
                Console.WriteLine("Press <ENTER> to send REST messages to server");
                Console.ReadKey();

                // Create a new HttpClientHandler
                var clientHandler = new HttpClientHandler()
                {
                    SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls
                };
                client = new HttpClient(clientHandler);
                Task<HttpResponseMessage> task = null;
                var cts = new CancellationTokenSource();

                // Use this URL for a routed service call
                string baseurl = "http://localhost:8001/Customer";

                // Add Customer
                Console.WriteLine("Adding Customer");

                HttpRequestMessage addCustomerRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(baseurl));
                addCustomerRequest.Content = new StringContent("{\"email\":\"admin@contoso.com\",\"id\":1234,\"name\":\"Contoso\"}", Encoding.UTF8, "application/json");
                task = Task.Run(() => client.SendAsync(addCustomerRequest, HttpCompletionOption.ResponseContentRead, cts.Token));
                task.GetAwaiter().GetResult();
                HttpResponseMessage addCustomerResponse = task.Result;
                JObject addResponse = JObject.Parse(addCustomerResponse.Content.ReadAsStringAsync().Result);

                if (addCustomerResponse.IsSuccessStatusCode)
                {
                    if ((bool)addResponse["success"])
                        Console.WriteLine("Customer Added");
                    else
                        Console.WriteLine("There was an Error Adding the Customer - {0}", (string)addResponse["errorMsg"]);
                }
                else
                {
                    Console.WriteLine("The customer was not added");
                    Console.WriteLine(String.Format("Response status code = {0}, Reason Phrase = {1}, Response = {2}", addCustomerResponse.StatusCode, addCustomerResponse.ReasonPhrase, addResponse));
                }

                Console.WriteLine();

                // Get customer list
                Console.WriteLine("Getting Customer List");

                task = Task.Run(() => client.GetAsync(new Uri(baseurl), HttpCompletionOption.ResponseContentRead, cts.Token));
                task.GetAwaiter().GetResult();
                HttpResponseMessage getAllCustomersResponse = task.Result;

                if (getAllCustomersResponse.IsSuccessStatusCode)
                {
                    JArray getAllResponse = JArray.Parse(getAllCustomersResponse.Content.ReadAsStringAsync().Result);
                    foreach (var cust in getAllResponse)
                        Console.WriteLine("Customer ID = {0}, Name = {1}, Email = {2}", cust["id"], cust["name"], cust["email"]);
                }
                else
                {
                    JObject errorResponse = JObject.Parse(getAllCustomersResponse.Content.ReadAsStringAsync().Result);
                    Console.WriteLine("There was as error retrieving all customers");
                    Console.WriteLine(String.Format("Response status code = {0}, Reason Phrase = {1}, Response = {2}", getAllCustomersResponse.StatusCode, getAllCustomersResponse.ReasonPhrase, errorResponse));
                }

                Console.WriteLine();

                // Get Customer by ID
                Console.WriteLine("Getting Customer by ID");
                task = Task.Run(() => client.GetAsync(new Uri(baseurl + "/1001"), HttpCompletionOption.ResponseContentRead, cts.Token));
                task.GetAwaiter().GetResult();
                HttpResponseMessage getCustomerByIdResponse = task.Result;

                if (getCustomerByIdResponse.IsSuccessStatusCode)
                {
                    JObject jsonResponse = JObject.Parse(getCustomerByIdResponse.Content.ReadAsStringAsync().Result);
                    Console.WriteLine("Customer ID = {0}, Name = {1}, Email = {2}", jsonResponse["id"], jsonResponse["name"], jsonResponse["email"]);
                }
                else
                {
                    JObject jsonResponse = JObject.Parse(getCustomerByIdResponse.Content.ReadAsStringAsync().Result);
                    Console.WriteLine("There was as error retrieving all customers");
                    Console.WriteLine(String.Format("Response status code = {0}, Reason Phrase = {1}, Response = {2}", getCustomerByIdResponse.StatusCode, getCustomerByIdResponse.ReasonPhrase, jsonResponse));
                }

                Console.WriteLine();

                // Get Customer by Name
                Console.WriteLine("Getting Customer by Name");
                task = Task.Run(() => client.GetAsync(new Uri(baseurl + "?Name=Contoso"), HttpCompletionOption.ResponseContentRead, cts.Token));
                task.GetAwaiter().GetResult();
                HttpResponseMessage getCustomerByNameResponse = task.Result;

                if (getCustomerByNameResponse.IsSuccessStatusCode)
                {
                    JObject jsonResponse = JObject.Parse(getCustomerByNameResponse.Content.ReadAsStringAsync().Result);
                    Console.WriteLine("Customer ID = {0}, Name = {1}, Email = {2}", jsonResponse["id"], jsonResponse["name"], jsonResponse["email"]);
                }
                else
                {
                    JObject jsonResponse = JObject.Parse(getCustomerByNameResponse.Content.ReadAsStringAsync().Result);
                    Console.WriteLine("There was as error retrieving all customers");
                    Console.WriteLine(String.Format("Response status code = {0}, Reason Phrase = {1}, Response = {2}", getCustomerByNameResponse.StatusCode, getCustomerByNameResponse.ReasonPhrase, jsonResponse));
                }

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
