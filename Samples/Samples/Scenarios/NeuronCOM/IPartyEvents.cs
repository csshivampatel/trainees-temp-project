using System;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Neuron.ESB.Excel.Interop
{
    [ComVisible(true)]
    [Guid(Party.EventsId)]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IPartyEvents
    {
        // event declarations
        [DispId(7)] 
        [DescriptionAttribute("Event fires when Party successfully connects to Neuron ESB")]
        void OnConnect(string partyId, string zone, string server, string port);

        [DispId(8)]
        [DescriptionAttribute("Event fires when message is received from Neuron ESB by connected Party")]
        void OnMessageReceive(string message);

        [DispId(9)]
        [DescriptionAttribute("Event fires when Party successfully disconnects from Neuron ESB")]
        void OnDisconnect(string partyId);
    }
}
