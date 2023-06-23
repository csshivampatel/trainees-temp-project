using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Transactions;
using System.Globalization;
using Neuron.Esb.Adapters;
using Neuron.Esb.Administration;

namespace Neuron.Esb.Sample.Adapters
{
    /// <summary>
    /// This is part of the Custom adapter class. Generally the developer would only edit the Class name.
    /// everything else within this partial class are generic routines used to support the adapter
    /// base class.
    /// </summary>
    [TypeConverter(typeof(PropertiesTypeConverter))]
    public partial class LogAdapter : ESBAdapterBase
    {

        #region Adapter Mode Enumeration and Classes
        /// <summary>
        /// Contains all the string constants for the modes the adapter supports so we
        /// can always have known spellings. These will be displayed in the Mode dropdown control located
        /// on the General Tab of an Adapter endpoint.  these are referenced in the
        /// AdapterModeStringsEnum enumeration. Delete the ones that are not going to be supported
        /// within the custom adapter
        /// </summary>
        public static class AdapterModeStringConstants
        {
            public const string Publish = "Publish";
            public const string Subscribe = "Subscribe";
            public const string SolicitResponse = "Solicit Response";
            public const string RequestReply = "RequestReply";
        }
        /// <summary>
        /// Contains all of the modes that the adapter supports. These are referenced in the 
        /// Adapter constructor method
        /// </summary>
        public enum AdapterModeStringsEnum
        {
            [Description(AdapterModeStringConstants.Publish)]
            Publish = 0,
            [Description(AdapterModeStringConstants.Subscribe)]
            Subscriber = 1,
            [Description(AdapterModeStringConstants.SolicitResponse)]
            SolicitResponse= 2,
            [Description(AdapterModeStringConstants.RequestReply)]
            RequestReply = 3
        }
        #endregion

        #region private properties
        private ESBConfiguration Configuration { get; set; }
        private List<NameValuePair> MessageProperties = new List<NameValuePair>();    // reference to the Neuron ESB custom message properties populated by custom adapter
        public AdapterMode _adapterMode;                                        // Set in the Connect() method. The adapter mode selected at design time on the general tab of the adapter endpoint
        #endregion

        #region Base class overrides
        /// <summary>
        /// This routine is fired by the ESB framework when the adapter is started up
        /// </summary>
        /// <param name="adapterMode">Contains the adapter mode being requested</param>
        /// <remarks>
        /// ESB framework will have already set the properties.  The requested
        /// adapter mode is checked against supported modes for validity.
        public override void Connect(string adapterMode)
        {
            this._adapterMode = null;

            foreach (AdapterMode mode in AdapterModes)
            {
                // don't worry about capitalization
                if (adapterMode.Equals(mode.ModeName,StringComparison.InvariantCultureIgnoreCase))
                {
                    this._adapterMode = mode;
                    break;
                }
            }

            // IncludeMetadata is a option located in the General Tab of the Adapter endpoint. Its meant to control whether or not
            // custom message properties should be collected and persisted and/or retreived from the message
            if (IncludeMetadata) MessageProperties.Add(new NameValuePair("Mode", this._adapterMode.ModeName));

            // get a copy of the esb configuration.  Used primarily for custom error handling and for instantiating the Neuron Message 
            // Audit service used for publish mode exceptions.
            Configuration = this.Configuration;
            RaiseAdapterInfo(ErrorLevel.Verbose, Configuration == null ? "ESB Configuration Is NULL" : "ESB Configuration returned");

            // call user defined method
            ConnectAdapter();
        }

        public override void SendToEndpoint(ESBMessage message, CommittableTransaction tx)
        {
            // call user defined method subscribing to message received from the bus and sending out to some system
            SendToDataSource(message, tx);
        }
        /// <summary>
        /// This routine if fired by the ESB framework when the adapter is shutdown
        /// </summary>
        public override void Disconnect()
        {
            try
            {
                // call user defined method
                DisconnectResources();

              
            }
            catch(Exception ex)
            {
                string msg = string.Format("An error occurred releasing resources during the shutdown of the adapter,'{0}'. Exception: {1}", AdapterName,ex.ToString());
                RaiseAdapterInfo(ErrorLevel.Warning, msg);
            }

            try
            {
                if (Configuration != null) Configuration = null;
            }
            catch(ThreadAbortException) // we can ignore the thread abort
            {}
            catch (Exception ex)
            {
                string msg = string.Format("An error occurred while disconnecting from the data source during the shutdown of the adapter,'{0}'. Exception: {1}", AdapterName, ex.ToString());
                RaiseAdapterInfo(ErrorLevel.Warning, msg);
            }
        }
        #endregion
    }
}
