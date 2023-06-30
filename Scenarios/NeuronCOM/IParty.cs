using System;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Neuron.ESB.Excel.Interop
{
    [Guid(Party.InterfaceId)]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IParty
    {
        [DispId(1)]
        [DescriptionAttribute("Connects the Party to the Neuron ESB server")]
        void Connect(string partyId, string zone, string server, string port);

        [DispId(2)]
        [DescriptionAttribute("Send one way request message to bus")]
        void Send(string topic, string message);

        [DispId(3)]
        [DescriptionAttribute("Send request/response message to bus and return response as string")]
        string SendRequest(string topic, string message);

        [DispId(4)]
        [DescriptionAttribute("Disconnects the Party from the Neuron ESB server")]
        void Disconnect();

        [DispId(5)]
        [DescriptionAttribute("Adds the OnReceive event for the subscribing Party")]
        void StartReceive();

        [DispId(6)]
        [DescriptionAttribute("Removes the OnReceive event for the subscribing Party")]
        void StopReceive();
    }
}
