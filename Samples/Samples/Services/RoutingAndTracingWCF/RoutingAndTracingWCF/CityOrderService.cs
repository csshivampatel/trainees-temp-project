using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoutingAndTracingWCF
{
    public class CityOrderService : IOrder, IQuote
    {
        public Order GetOrder(int OrderNumber, string City)
        {
            if (!string.IsNullOrEmpty(City))
            {
                return new RoutingAndTracingWCF.Order() { City = City, Amount=100.6, CustomerName="Edward Jones", OrderNumber = OrderNumber, Price = 50.3M, Quantity = 2, Product="New Bike" };      
            }
            else
            {
                return new RoutingAndTracingWCF.Order() { City = "Local", Amount = 80.6, CustomerName = "Joe Schmoe", OrderNumber = OrderNumber, Price = 40.3M, Quantity = 2, Product = "New Segway" };
            }
        }

        public int PlaceOrder(Order order, string City)
        {
            if (!string.IsNullOrEmpty(City))
            {
                return order.OrderNumber;
            }
            else
            {
                return order.OrderNumber;
            }
        }
        public Order ConvertQuoteToOrder(int QuoteNumber, string City)
        {
            if (!string.IsNullOrEmpty(City))
            {
                return new RoutingAndTracingWCF.Order() { City = City, Amount = 100.6, CustomerName = "Edward Jones", OrderNumber = QuoteNumber, Price = 50.3M, Quantity = 2, Product = "New Bike" };
            }
            else
            {
                return new RoutingAndTracingWCF.Order() { City = "Local", Amount = 80.6, CustomerName = "Joe Schmoe", OrderNumber = QuoteNumber, Price = 40.3M, Quantity = 2, Product = "New Segway" };
            }
        }

        public Quote RequestQuote(string Product, int Quantity, string City)
        {
            if (!string.IsNullOrEmpty(City))
            {
                return new RoutingAndTracingWCF.Quote() { City = City, Amount = 100.6,  QuoteNumber = 423483, Price = 50.3, Quantity = Quantity, Product = Product};
            }
            else
            {
                return new RoutingAndTracingWCF.Quote() { City = "Local", Amount = 80.6,  QuoteNumber = 324324, Price = 40.3, Quantity = Quantity, Product = Product };
            }
        }
    }
}