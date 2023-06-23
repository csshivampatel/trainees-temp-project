using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Runtime.Serialization;

//This sample service is intended to be run in conjunction with
//the Neuron Process sample: Retry Service that can be found in the 
//Neuron sample configuration file RetryWebSerivceCall.esb

namespace Neuron.Esb.Samples.Processes
{
    [ServiceContract(Namespace="http://neuron.esb.samples.processes/")]
    public interface IServiceProcessService
    {
        [OperationContract]
        Person GetPerson(string name);
    }

    [DataContract]
    public class Person
    {
        [DataMember]
        public string name;
        [DataMember]
        public int age;
    }

    public class ServiceProcessService : IServiceProcessService
    {
        static void Main(string[] args)
        {
            using (ServiceHost host = new ServiceHost(typeof(ServiceProcessService)))
            {
                host.Open();
                Console.WriteLine("The service is ready.");
                Console.WriteLine("Press <ENTER> to terminate service.");
                Console.ReadLine();
            }
        }


        public ServiceProcessService()
        {
            CreatePersonList();
        }

        static List<Person> people = new List<Person>();
        //create some default content
        private void CreatePersonList()
        {
            Person p = new Person();
            p.name = "Johny Neuron";
            p.age = 16;
            people.Add(p);

            p = new Person();
            p.name = "Bobby Neuron";
            p.age = 44;
            people.Add(p);

            p = new Person();
            p.name = "Betty Neuron";
            p.age = 36;
            people.Add(p);

        }

        public Person GetPerson(string name)
        {
            Person person = people.Find((p) => (p.name.Equals(name)));
            if (person == null)
            {
                Thread.Sleep(5000); 
            }

            return person;
        }
    }
}
