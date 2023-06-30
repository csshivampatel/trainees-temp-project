using System;
using System.Runtime.InteropServices;
using Neuron.Esb;
using System.Globalization;

namespace Neuron.ESB.Excel.Interop
{
    [ComVisible(true)]
    [Guid(Party.EnumId)]
    public enum SendOptions
    {
        /// <summary>None.</summary>
        None = 0,
        /// <summary>The message must be sent as multicast.</summary>
        Multicast = 1,
        /// <summary>The message must be sent as direct.</summary>
        Direct = 4,
        /// <summary>The message is a request.</summary>
        Request = 8,
        /// <summary>The message is a reply.</summary>
        Reply = 16,
    }

    [ComVisible(true)]
    [Guid(Party.ClassId)]
    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(IPartyEvents))]
    public class Party : IParty
    {

        internal const string ClassId = "41558DD7-87BD-433f-9B1B-C478213D6C67";
        internal const string EventsId = "12E3954F-5DC1-4ad9-7167-454F0E82BCA3";
        internal const string InterfaceId = "559A3835-4768-2ad5-82BE-7D708C2B86AB";
        internal const string EnumId = "259A6635-4768-2bd5-82BE-7D008C2B86AB";

        private Neuron.Esb.Party _party = null;
        private string _partyName;

        [ComVisible(false)]
        public delegate void OnReceive(string message);
        [ComVisible(false)]
        public delegate void OnPartyConnect(string partyId, string zone, string server, string port);
        [ComVisible(false)]
        public delegate void OnPartyDisconnect(string partyId);

        public event OnReceive OnMessageReceive;
        public event OnPartyConnect OnConnect;
        public event OnPartyDisconnect OnDisconnect;

        [ComVisible(true)]
        public void Connect(string partyId, string zone, string server, string port)
        {
            int portInt;
            string bootstrapAddress;

            // validate and build the boostrap address
            if (String.IsNullOrEmpty(partyId)) throw new ArgumentNullException("partyId", "A party id must be entered.");
            if (String.IsNullOrEmpty(zone)) zone = "Enterprise";
            if (String.IsNullOrEmpty(server)) server = "localhost";
            if (String.IsNullOrEmpty(port)) port = "50000";
            if (!int.TryParse(port, out portInt)) throw new ArgumentOutOfRangeException("port", "port must be a valid port number.");
            bootstrapAddress = string.Format(CultureInfo.CurrentCulture, "net.tcp://{0}:{1}/", server, port);

            _party = new Neuron.Esb.Party(new SubscriberConfiguration(partyId, zone, bootstrapAddress, null), SubscriberOptions.None);
            _partyName = partyId;

            // connect and raise event
            _party.Connect();

            if (OnConnect != null) OnConnect(partyId, zone, server, port);
        }

        [ComVisible(true)]
        public string SendRequest(string topic, string message)
        {
            if (String.IsNullOrEmpty(topic)) throw new ArgumentNullException("topic", "A Topic to publish the message to must be entered.");
            if (String.IsNullOrEmpty(message)) throw new ArgumentNullException("message", "The message to publish must be entered.");

            ESBMessage msg = _party.Send(topic, message, Neuron.Esb.SendOptions.Request);

            return msg.Text;
        }

        [ComVisible(true)]
        public void Send(string topic, string message)
        {
            if (String.IsNullOrEmpty(topic)) throw new ArgumentNullException("topic", "A Topic to publish the message to must be entered.");
            if (String.IsNullOrEmpty(message)) throw new ArgumentNullException("message", "The message to publish must be entered.");

            // defaults to multi cast
            _party.Send(topic, message, Neuron.Esb.SendOptions.Multicast);
        }

        [ComVisible(true)]
        public void Disconnect()
        {
            if (_party != null)
            {
                _party.OnReceive -= OnReceiveInternal;
                _party.Dispose();
                _party = null;
            }
            if (OnDisconnect != null) OnDisconnect(_partyName); 
        }

        [ComVisible(true)]
        public void StartReceive()
        {
            _party.OnReceive += OnReceiveInternal;
        }
        [ComVisible(true)]
        public void StopReceive()
        {
            _party.OnReceive -= OnReceiveInternal;
        }

        [ComVisible(true)]
        private void OnReceiveInternal(object sender, MessageEventArgs mea)
        {
            OnMessageReceive(mea.Message.ToString());
        }
    }
}
