using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoutingAndTracingWCF
{
    public class OutofCityOrderService : IOrder, IQuote
    {
        public Order GetOrder(int OrderNumber, string City)
        {
            return new RoutingAndTracingWCF.Order() { City = City, Amount = 100.6, CustomerName = "Edward Jones", OrderNumber = OrderNumber, Price = 50.3M, Quantity = 2, Product = "New Bike" };
        }

        public int PlaceOrder(Order order, string City)
        {
            return order.OrderNumber;
        }

        public Order ConvertQuoteToOrder(int QuoteNumber, string City)
        {
            return new RoutingAndTracingWCF.Order() { City = City, Amount = 100.6, CustomerName = "Edward Jones", OrderNumber = QuoteNumber, Price = 50.3M, Quantity = 2, Product = "New Bike" };
        }

        public Quote RequestQuote(string Product, int Quantity, string City)
        {
            return new RoutingAndTracingWCF.Quote() { City = City, Amount = 100.6, QuoteNumber = 423483, Price = 50.3, Quantity = Quantity, Product = Product };
        }
    }
}