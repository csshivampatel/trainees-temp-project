using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neuron.Esb;

namespace OrderProcessor
{
    public class Order
    {
        public string BatchID { get; set; }
        public string OrderID { get; set; }
        public ESBMessage Message { get; set; }
    }
}
