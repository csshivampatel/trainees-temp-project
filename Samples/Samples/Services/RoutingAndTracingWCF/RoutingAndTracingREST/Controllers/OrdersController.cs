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
    public class OrdersController : Controller
    {
       [HttpGet]
       public ActionResult<Order> Get()
       {
            var OrderService = new CityOrderService();
            var Order = OrderService.GetOrder(12345, "");            
            return Ok(Order);
        }

      [HttpGet("{id:int}/{city}")]
      public ActionResult<Order> GetOrder(int id,string city)
      {
            if (string.IsNullOrEmpty(city))
            {
                var OrderService = new CityOrderService();
                return Ok(OrderService.GetOrder(id, city));
            } else
            {
                var OrderService = new OutofCityOrderService();
                return Ok(OrderService.GetOrder(id,city));
            }
      }

      [HttpPost]
      public ActionResult<OrderResponse> PlaceOrder(Order order)
      {
            if (string.IsNullOrEmpty(order.City))
            {
                var OrderService = new CityOrderService();
                var orderNo = OrderService.PlaceOrder(order, order.City);
               return Accepted(new OrderResponse() { orderNumber = orderNo});
          }
          else
          {
                var OrderService = new OutofCityOrderService();
                var orderNo = OrderService.PlaceOrder(order, order.City);
                return Accepted(new OrderResponse() { orderNumber = orderNo });
            }
      }
    }
}
