using Microsoft.AspNetCore.Mvc;
using RoutingAndTracingWCF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoutingAndTracingREST.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuotesController : Controller
    {

        [HttpGet]
        public ActionResult<Order> Get()
        {
            var OrderService = new CityOrderService();
            var Order = OrderService.RequestQuote("Bike", 12345, "");
            return Ok(Order);
        }


        [HttpGet("{Product}/{Quantity:int}/{City?}")]
        public ActionResult<Quote> RequestQuote(string Product, int Quantity, string City)
        {         
            if (string.IsNullOrEmpty(City))
            {
                //Retrieve the Custom HTTP Header City, used to illustrate how to set HTTP Headers
                var CityHeader = Request.Headers["City"];
                var QuoteService = new CityOrderService();
                return QuoteService.RequestQuote(Product, Quantity, CityHeader);
            }
            else
            {
                var QuoteService = new OutofCityOrderService();
                return QuoteService.RequestQuote(Product, Quantity, City);
            }
        }

        [HttpPost]
        public ActionResult<Order> ConvertQuoteToOrder(Quote quote)
        {
            if (string.IsNullOrEmpty(quote.City))
            {
                var QuoteService = new CityOrderService();
                return QuoteService.ConvertQuoteToOrder(quote.QuoteNumber, quote.City);
            }
            else
            {
                var QuoteService = new OutofCityOrderService();
                return QuoteService.ConvertQuoteToOrder(quote.QuoteNumber, quote.City);
            }
        }
    }
}
