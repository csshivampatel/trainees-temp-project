using System.Web;
using System.Web.Mvc;

namespace NeuronOAuthConsumerSample
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
