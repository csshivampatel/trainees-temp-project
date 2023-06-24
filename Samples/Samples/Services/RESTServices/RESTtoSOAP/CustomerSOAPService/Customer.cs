using System;
using System.Linq;
using System.ServiceModel.Web;

namespace Neuron.Samples.CustomerSOAP
{
    public class CustomerService : ICustomerService
    {
        #region ICustomer Members

        Customers ICustomerService.GetAllCustomers()
        {
            try
            {
                Console.WriteLine("Get All Customers called");
                return _customers;
            }
            catch (Exception ex)
            {
                throw new WebFaultException(System.Net.HttpStatusCode.InternalServerError);
            }
        }

        Customer ICustomerService.GetCustomerByID(string id)
        {
            try
            {
                Console.WriteLine(String.Format("Get Customer by ID called. Id = {0}", id));    
                Customer c = null;
                c = (from cust in _customers
                     where cust.ID == System.Convert.ToInt32(id)
                     select cust).Single();
                return c; 
            }
            catch (Exception ex)
            {
                throw new WebFaultException(System.Net.HttpStatusCode.InternalServerError);
            }
        }

        Customer ICustomerService.GetCustomerByName(string name)
        {
            try
            {
                Console.WriteLine(String.Format("Get Customer by Name called. Name = {0}", name));
                Customer c = null;
                c = (from cust in _customers
                     where cust.Name == name
                     select cust).Single();
                return c;
            }
            catch (Exception ex)
            {
                throw new WebFaultException(System.Net.HttpStatusCode.InternalServerError);
            }
        }

        Result ICustomerService.AddCustomer(Customer customer)
        {
            try
            {
                Console.WriteLine(String.Format("Add Customer called. Customer Id = {0}", customer.ID));
                // Check to see if the customer exists, if it does, just return it
                Customer c = null;
                c = (from cust in _customers
                     where cust.ID == System.Convert.ToInt32(customer.ID)
                     select cust).Single();
            }
            catch (InvalidOperationException ioe)
            {
                // this code is reached if the customer doesn't exist, so add it
                Console.WriteLine(String.Format("   Customer with Id {0} not found. Adding now", customer.ID));
                _customers.Add(customer);
                return new Result(true);
            }
            catch (Exception ex)
            {
                var exResult = new Result(false, "An unknown error occurred");
                throw new WebFaultException<Result>(exResult, System.Net.HttpStatusCode.InternalServerError);
            }

            // The customer exists, so return an ErrorResult
            Console.WriteLine(String.Format("   Customer with Id {0} already exists", customer.ID));
            return new Result(false, String.Format("Customer with Id {0} already exists", customer.ID));
        }

        #endregion


        static Customers _customers = InitCustomers();
        private static Customers InitCustomers()
        {
            Customers ret = new Customers();
            ret.Add(new Customer
            {
                ID = 1001,
                Name = "Northwind Traders",
                Email = "orders@northwind.com"
            });


            ret.Add(new Customer
            {
                ID = 1002,
                Name = "LitWare Inc.",
                Email = "accounts@litware.com"
            });

            ret.Add(new Customer
            {
                ID = 1003,
                Name = "AdventureWorks Cycles",
                Email = "admin@adventureworks.com"
            });

            return ret;
        }
    }
}
