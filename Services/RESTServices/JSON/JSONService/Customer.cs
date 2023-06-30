using System;
using System.IO;
using System.Linq;

namespace Neuron.Samples.JSON
{
    public class CustomerService : ICustomerService
    {
        #region ICustomer Members

        Stream ICustomerService.GetCustomerByID(string id)
        {
            Customer c = null;
            c = (from cust in _customers
                 where cust.ID == System.Convert.ToInt32(id)
                 select cust).Single();

            Console.WriteLine(String.Format("Get Customer By ID called with parameter id = {0}, customer name \"{1}\" returned.", id, c.Name));
            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(c))); 
        }

        Stream ICustomerService.GetCustomerByName(string name)
        {
            try
            {
                Customer c = null;
                c = (from cust in _customers
                     where cust.Name == name
                     select cust).Single();

                Console.WriteLine(String.Format("Get Customer By Name called with parameter name = {0}, customer id \"{1}\" returned.", name, c.ID));
                return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(c))); 
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        Stream ICustomerService.AddCustomer(Stream sCustomer)
        {
            Customer customer = null;

            try
            {
                StreamReader sr = new StreamReader(sCustomer);
                customer = Newtonsoft.Json.JsonConvert.DeserializeObject<Customer>(sr.ReadToEnd());
                Console.WriteLine(String.Format("Add Customer called with id = {0}", customer.ID));

                // Check to see if the customer exists, if it does, just return it
                Customer c = null;
                c = (from cust in _customers
                     where cust.ID == System.Convert.ToInt32(customer.ID)
                     select cust).Single();

                Console.WriteLine(String.Format("Add Customer - customer with id {0} previously added", customer.ID));
                return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(c)));
            }
            // invalid operation exception occurs when customer doesn't exist in list
            catch (InvalidOperationException invalidOpEx)
            {
                // this code is reached if the customer doesn't exist, so add it
                _customers.Add(customer);
                Console.WriteLine(String.Format("Add Customer - customer successfully added"));
                return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(customer)));
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Add Customer failed with exception {0}", ex.Message));
                return null;
            }
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
