using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoutingAndTracingWCF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoutingAndTracingREST.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OldOrdersController : Controller
    {
        [HttpGet]
        public ActionResult<Order> GetOrder([FromQuery] int id, [FromQuery]  string city)
        {
            if (string.IsNullOrEmpty(city))
            {
                var OrderService = new CityOrderService();
                return Ok(OrderService.GetOrder(id, city));
            }
            else
            {
                var OrderService = new OutofCityOrderService();
                return Ok(OrderService.GetOrder(id, city));
            }
        }
    }
}
