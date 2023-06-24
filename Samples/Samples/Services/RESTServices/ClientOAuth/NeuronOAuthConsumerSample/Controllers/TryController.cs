using Microsoft.Ajax.Utilities;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Security.Claims;
using Microsoft.Owin;

namespace NeuronOAuthConsumerSample.Controllers
{
    public class TryController : Controller
    {
        // GET: Try
        public ActionResult Index()
        {
            string ConnString1 = ConfigurationManager.ConnectionStrings[1].ToString();
            string ConnString2 = ConfigurationManager.ConnectionStrings[2].ToString();
            ViewBag.Message = "PeregrineMS-" + ConnString1; // + "<BR> Azure-" + ConnString2;
            ViewBag.Message2 = "Azure-" + ConnString2;
            return View();
        }

        public ActionResult Token()
        {
            ViewBag.Message = GetToken(); ;
            return View();
        }

        public ActionResult Use()
        {
            var NeuronURL = ConfigurationManager.AppSettings["NeuronURL"];
            dynamic result = new JavaScriptSerializer().DeserializeObject(GetToken());

            var bearertoken = result["token_type"] + " " + result["access_token"];
            ViewBag.Message = bearertoken;


            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", bearertoken);
                var response = client.GetAsync(NeuronURL).GetAwaiter().GetResult();
                ViewBag.Message = bearertoken + Environment.NewLine + "Response Status code From Neuron " + response.StatusCode + Environment.NewLine + "Response Message from Neuron " + response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }

            return View();
        }

        private string GetToken()
        {
            string ConnString = ConfigurationManager.ConnectionStrings[1].ToString();
            var ClientId = ConnString.Split(';')[0].Split('=')[1];
            var ClientSecret = ConnString.Split(';')[1].Split('=')[1];
            var GrantType = "client_credentials";  //Grant type matches the grant type specified in Peregrine Management setup for your application's Client Id

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", ClientId),
                new KeyValuePair<string, string>("client_secret", ClientSecret ),
                 new KeyValuePair<string, string>("grant_type", GrantType )
             });

            using (HttpClient client = new HttpClient())
            {
                var response = client.PostAsync(ConfigurationManager.AppSettings["TokenURL"], formContent).GetAwaiter().GetResult();

                return JValue.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult()).ToString(Formatting.Indented);
            }
        }
    }
}