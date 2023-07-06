using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Peregrine.Application.Domain.Models;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using Neuron.Esb.Administration;
using System.Threading;
using System.ServiceProcess;
using System.Net.Http;
using Newtonsoft.Json;

namespace Peregrine.Application.Service.Services
{
    #region IConfigureServerService Interface
    public interface IConfigureServerService
    {
        /// <summary>
        /// Write Input Data from User to AppSettings.config File.
        /// </summary>
        /// <param name="configurationServer"></param>
        /// <returns>
        /// Object of ConfigurationServer Class which contains data of AppSettings.config File.
        /// </returns>
        ConfigurationServer UpdateConfigureServerSettings(ConfigurationServer configurationServer);

        /// <summary>
        /// Get Data from AppSettings.config File.
        /// </summary>
        /// <returns>
        /// Object of ConfigurationServer Class which contains data of AppSettings.config File.
        /// </returns>
        ConfigurationServer GetConfigureServerSettings();

        /// <summary>
        /// Get Data from AppSettings.config for all neuron instance File.
        /// </summary>
        /// <returns>
        /// Object of InstanceInfoModel Class which contains data of AppSettings.config File.
        /// </returns>
        List<InstanceInfoModel> GetInstancewiseConfigureServerSettings();

        /// <summary>
        /// returns model that contains zones and deployment Groups List
        /// </summary>
        /// <param name="serverPathModel"></param>
        /// <returns></returns>
        PathConfigurationResponseModel GetPathConfiguration(CommonPathModel serverPathModel);

        /// <summary>
        /// Write Input Data from User to Instance realted AppSettings.config File.
        /// </summary>
        /// <param name="updateConfigureServerInstance"></param>
        /// <returns>
        /// Object of ConfigurationServer Class which contains data of AppSettings.config File.
        /// </returns>
        List<UpdateConfigureServerInstanceWise> UpdateServerConfigurationbyKeyValuePairs(List<UpdateConfigureServerInstanceWise> updateConfigureServerInstance);
    }
    #endregion

    #region Implementaion of IConfigureServerService interface
    public class ConfigureServerService : IConfigureServerService
    {
        private const string _sources = "<sources>" +
            "   <source name=\"System.ServiceModel\" switchValue=\"Verbose, ActivityTracing\" propagateActivity=\"true\">" +
            "      <listeners>" +
            "         <add name = \"traceListener\" />" +
            "      </listeners>" +
            "   </source>" +
            "   <source name=\"System.ServiceModel.MessageLogging\">" +
            "       <listeners>" +
            "           <add name = \"traceListener\" />" +
            "       </listeners>" +
            "   </source>" +
            "</sources> ";

        private const string _sharedListeners = "<sharedListeners>" +
            "   <add name=\"traceListener\" type=\"System.Diagnostics.XmlWriterTraceListener\" initializeData=\"..\\logs\\WCF_Tracing_EndpointHosts.svclog\"/>" +
            "</sharedListeners>";

        /// <summary>
        /// This methos will return all instances for configure server.
        /// </summary>
        /// <returns>
        /// Return list of instances fetched from Web API
        /// </returns>
        private dynamic GetAllInstancesofConfigureServer()
        {
            // shivam..................................................
        //    public static string NEURON_DEFAULT_PATH = @"C:\Program Files\Neudesic\Neuron ESB v3\DEFAULT";
        //public static string NEURON_UPGRADED_DEFAULT_PATH = @"C:\Program Files\Peregrine\Neuron ESB v3\DEFAULT";

        string neuronConfigurationFileName = "NeuronEventProcessor.exe.config";
        string configFilePath = string.Format(@"{0}\..\..\Neuron Event Processor\{1}", CustomDllUtility.NEURON_UPGRADED_DEFAULT_PATH, neuronConfigurationFileName);

            if(!File.Exists(configFilePath))
            {
                configFilePath = string.Format(@"{0}\..\..\Neuron Event Processor\{1}", CustomDllUtility.NEURON_DEFAULT_PATH, neuronConfigurationFileName);
            }

            // Load the configuration file using System.Xml
            var configFileMap = new ExeConfigurationFileMap { ExeConfigFilename = configFilePath };
            var configuration = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

            string portNumber = configuration.AppSettings.Settings["NeuronOperationServicePort"].Value;

            int NeuronOperationServicePort;
            int.TryParse(portNumber, out NeuronOperationServicePort);

            string baseUrl = string.Format("http://localhost:{0}/neuronesb", NeuronOperationServicePort);
            string neuronAPIversion = "api/v1";
            string dataUrl = "machinedetails";

            string neuronAPIUrl = string.Format("{0}/{1}/{2}", baseUrl, neuronAPIversion, dataUrl);
            // shivam..................................................

            using (var httpClient = new HttpClient())
            {
                //using (var response = httpClient.GetAsync("http://localhost:51002/neuronesb/api/v1/machinedetails").GetAwaiter().GetResult())
                using (var response = httpClient.GetAsync(neuronAPIUrl).GetAwaiter().GetResult())
                {
                    string apiResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    dynamic res = JsonConvert.DeserializeObject(apiResponse);
                    return res.instances;
                }
            }
        }

        /// <summary>
        /// Method will tack particular instanceName and return all configuration setting related to that instance.
        /// </summary>
        /// <param name="instanceName"></param>
        /// <returns>
        /// A dynamic object which contain all configuration settings of instance.
        /// </returns>
        public dynamic GetConfigurationOfInstance(string instanceName)
		{
            using (var httpClient = new HttpClient())
            {
                using (var response = httpClient.GetAsync("http://localhost:51002/neuronesb/api/v1/runtime/" + instanceName).GetAwaiter().GetResult())
                {
                    string apiResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    dynamic res = JsonConvert.DeserializeObject(apiResponse);
                    return res;
                }
            }
        }

        /// <summary>
        /// Write Input Data from User to AppSettings.config File.
        /// </summary>
        /// <param name="configurationServer"></param>
        /// <returns>
        /// Object of ConfigurationServer Class which contains data of AppSettings.config File.
        /// </returns>
        public ConfigurationServer UpdateConfigureServerSettings(ConfigurationServer configurationServer)
        {
            try
            {
                var serverConfigFile = Path.Combine(CustomDllUtility.AssemblyDirectory, "ESBService.exe");
                // Get the application path.
                var exePath = serverConfigFile;
                // Get the configuration file.
                var config = ConfigurationManager.OpenExeConfiguration(exePath);

                #region For ESBService.exe.config File

                // update for esbservice.exe
                ///
                var configSection = config.Sections["system.diagnostics"];
                var rawXml = configSection.SectionInformation.GetRawXml();
                var xml = XElement.Parse(rawXml ?? "<system.diagnostics/>");
                var switches = xml.Element("switches");
                if (null == switches)
                {
                    switches = new XElement("switches");
                    xml.Add(switches);
                }

                var esbTraceSwitch =
                    switches.Elements("add").SingleOrDefault(x => x.Attributes("name").Any(a => "esbTraceSwitch" == a.Value));
                if (null == esbTraceSwitch)
                {
                    esbTraceSwitch = new XElement(
                        "add",
                        new XAttribute("name", "esbTraceSwitch"),
                        new XAttribute("value", (int)configurationServer.TraceLevelDescriptions));
                    switches.Add(esbTraceSwitch);
                }
                else
                {
                    esbTraceSwitch.Attribute("value").Value =
                        ((int)configurationServer.TraceLevelDescriptions).ToString(CultureInfo.InvariantCulture);
                }

                var newXml = xml.ToString();
                configSection.SectionInformation.SetRawXml(newXml);
                #endregion

                #region For NeuronEndpointHost.exe.config File
                // update for neuronendpointhost.exe
                ///
                var serverNeuronHostConfigFile = Path.Combine(CustomDllUtility.AssemblyDirectory, "NeuronEndpointHost.exe");
                // Get the application path.
                var exeNeuronHostPath = serverNeuronHostConfigFile;

                // Get the configuration file.
                var neuronHostconfig = ConfigurationManager.OpenExeConfiguration(exeNeuronHostPath);

                configSection = neuronHostconfig.Sections["system.diagnostics"];
                rawXml = configSection.SectionInformation.GetRawXml();
                xml = XElement.Parse(rawXml ?? "<system.diagnostics/>");
                switches = xml.Element("switches");
                if (null == switches)
                {
                    switches = new XElement("switches");
                    xml.Add(switches);
                }
                esbTraceSwitch =
                    switches.Elements("add").SingleOrDefault(x => x.Attributes("name").Any(a => "esbTraceSwitch" == a.Value));
                if (null == esbTraceSwitch)
                {
                    esbTraceSwitch = new XElement(
                        "add",
                        new XAttribute("name", "esbTraceSwitch"),
                        new XAttribute("value", configurationServer.TraceLevelDescriptions));
                    switches.Add(esbTraceSwitch);
                }
                else
                {
                    esbTraceSwitch.Attribute("value").Value =
                        (configurationServer.TraceLevelDescriptions).ToString(CultureInfo.InvariantCulture);
                }

                // If WCF Tracing is enabled, the NeuronEndpointHost.exe.config needs the sources and sharedListeners elements
                if (configurationServer.WCFTracingEnabled)
                {
                    // insert or uncomment sources and sharedListeners elements
                    XElement sources = null;
                    XElement sharedListeners = null;

                    // see if the sources element is commented-out
                    foreach (XComment comment in xml.Nodes().OfType<XComment>().Where(c => c.Value.StartsWith("<sources>")).ToList())
                    {
                        sources = XElement.Parse(comment.Value);
                        comment.ReplaceWith(sources);
                    }

                    // if the sources element doesn't exist, add it
                    sources = xml.Element("sources");
                    if (null == sources)
                    {
                        sources = XElement.Parse(_sources);
                        xml.Add(sources);
                    }

                    // see if the sharedListeners element is commented-out
                    foreach (XComment comment in xml.Nodes().OfType<XComment>().Where(c => c.Value.StartsWith("<sharedListeners>")).ToList())
                    {
                        sharedListeners = XElement.Parse(comment.Value);
                        comment.ReplaceWith(sharedListeners);
                    }

                    // if the sharedListeners element doesn't exist, add it
                    sharedListeners = xml.Element("sharedListeners");
                    if (null == sharedListeners)
                    {
                        sharedListeners = XElement.Parse(_sharedListeners);
                        xml.Add(sharedListeners);
                    }
                }
                else
                {
                    // comment-out sources and sharedListeners elements

                    // if the sources element exists, comment it out
                    var sources = xml.Element("sources");
                    if (null != sources)
                    {
                        XComment comment = new XComment(sources.ToString());
                        sources.ReplaceWith(comment);
                    }

                    // if the sources element exists, comment it out
                    var sharedListeners = xml.Element("sharedListeners");
                    if (null != sharedListeners)
                    {
                        XComment comment = new XComment(sharedListeners.ToString());
                        sharedListeners.ReplaceWith(comment);
                    }
                }

                newXml = xml.ToString();
                configSection.SectionInformation.SetRawXml(newXml);

                neuronHostconfig.Save();
                #endregion

                #region For appSettings.config File
                //Now add data to config file.
                //If data not exist then add data else change value of data.

                // Get the AppSettings section.
                var appSettings = config.AppSettings;
                if(appSettings == null)
                {
                }
                else
                {
                    #region Server Tab
                    //esbEnvironment
                    if (config.AppSettings.Settings["esbEnvironment"] == null)
                    {
                        config.AppSettings.Settings.Add("esbEnvironment", configurationServer.EsbEnvironment);
                    }
                    else
                    {
                        config.AppSettings.Settings["esbEnvironment"].Value = configurationServer.EsbEnvironment;
                    }
                    //esbZone
                    if (config.AppSettings.Settings["esbZone"] == null)
                    {
                        config.AppSettings.Settings.Add("esbZone", configurationServer.EsbZone);
                    }
                    else
                    {
                        config.AppSettings.Settings["esbZone"].Value = configurationServer.EsbZone;
                    }
                    //esbDeploymentGroup
                    if (config.AppSettings.Settings["esbDeploymentGroup"] == null)
                    {
                        config.AppSettings.Settings.Add("esbDeploymentGroup", configurationServer.EsbDeploymentGroup);
                    }
                    else
                    {
                        config.AppSettings.Settings["esbDeploymentGroup"].Value = configurationServer.EsbDeploymentGroup;
                    }
                    //SavedHistoryCleanupSchedule
                    if (config.AppSettings.Settings["SavedHistoryCleanupSchedule"] == null)
                    {
                        config.AppSettings.Settings.Add("SavedHistoryCleanupSchedule", configurationServer.SavedHistoryCleanupSchedule.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["SavedHistoryCleanupSchedule"].Value = configurationServer.SavedHistoryCleanupSchedule.ToString();
                    }
                    //neuronEventProcessor
                    if (config.AppSettings.Settings["NeuronHybridFeaturesEnabled"] == null)
                    {
                        config.AppSettings.Settings.Add("NeuronHybridFeaturesEnabled", configurationServer.NeuronHybridFeaturesEnabled.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["NeuronHybridFeaturesEnabled"].Value = configurationServer.NeuronHybridFeaturesEnabled.ToString();
                    }
                    #endregion

                    #region Logging Tab
                    //traceLevelDescriptions
                    //if (config.AppSettings.Settings["traceLevelDescriptions"] == null)
                    //{
                    //    config.AppSettings.Settings.Add("traceLevelDescriptions", configurationServer.traceLevelDescriptions.ToString());
                    //}
                    //else
                    //{
                    //    config.AppSettings.Settings["traceLevelDescriptions"].Value = configurationServer.traceLevelDescriptions.ToString();
                    //}

                    //MaximumLogFileSize
                    var setting = config.AppSettings.Settings["MaximumLogFileSize"];
                    var temp = Convert.ToInt32(configurationServer.MaximumLogFileSize) * 1048576;
                    var maximum = temp.ToString(CultureInfo.InvariantCulture);
                    if (null == setting)
                    {
                        config.AppSettings.Settings.Add("MaximumLogFileSize", maximum);
                    }
                    else
                    {
                        setting.Value = maximum;
                    }

                    //LogFolderCleanupSchedule
                    if (config.AppSettings.Settings["LogFolderCleanupSchedule"] == null)
                    {
                        config.AppSettings.Settings.Add("LogFolderCleanupSchedule", configurationServer.LogFolderCleanupSchedule.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["LogFolderCleanupSchedule"].Value = configurationServer.LogFolderCleanupSchedule.ToString();
                    }
                    //Neuron.Service.WCFTracingEnabled
                    if (config.AppSettings.Settings["Neuron.Service.WCFTracingEnabled"] == null)
                    {
                        config.AppSettings.Settings.Add("Neuron.Service.WCFTracingEnabled", configurationServer.WCFTracingEnabled.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["Neuron.Service.WCFTracingEnabled"].Value = configurationServer.WCFTracingEnabled.ToString();
                    }
                    #endregion

                    #region Performance Tab
                    //overrideDotNetThreadPoolSize
                    if (config.AppSettings.Settings["overrideDotNetThreadPoolSize"] == null)
                    {
                        config.AppSettings.Settings.Add("overrideDotNetThreadPoolSize", configurationServer.OverrideDotNetThreadPoolSize.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["overrideDotNetThreadPoolSize"].Value = configurationServer.OverrideDotNetThreadPoolSize.ToString();
                    }
                    //dotNetMaxWorkerThreads
                    if (config.AppSettings.Settings["dotNetMaxWorkerThreads"] == null)
                    {
                        config.AppSettings.Settings.Add("dotNetMaxWorkerThreads", configurationServer.DotNetMaxWorkerThreads.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["dotNetMaxWorkerThreads"].Value = configurationServer.DotNetMaxWorkerThreads.ToString();
                    }
                    //dotNetMinWorkerThreads
                    if (config.AppSettings.Settings["dotNetMinWorkerThreads"] == null)
                    {
                        config.AppSettings.Settings.Add("dotNetMinWorkerThreads", configurationServer.DotNetMinWorkerThreads.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["dotNetMinWorkerThreads"].Value = configurationServer.DotNetMinWorkerThreads.ToString();
                    }
                    //dotNetMaxIOThreads
                    if (config.AppSettings.Settings["dotNetMaxIOThreads"] == null)
                    {
                        config.AppSettings.Settings.Add("dotNetMaxIOThreads", configurationServer.DotNetMaxIOThreads.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["dotNetMaxIOThreads"].Value = configurationServer.DotNetMaxIOThreads.ToString();
                    }
                    //dotNetMinIOThreads
                    if (config.AppSettings.Settings["dotNetMinIOThreads"] == null)
                    {
                        config.AppSettings.Settings.Add("dotNetMinIOThreads", configurationServer.DotNetMinIOThreads.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["dotNetMinIOThreads"].Value = configurationServer.DotNetMinIOThreads.ToString();
                    }
                    #endregion

                    #region Others

                    //esbInstanceName
                    if (config.AppSettings.Settings["esbInstanceName"] == null)
                    {
                        config.AppSettings.Settings.Add("esbInstanceName", configurationServer.EsbInstanceName);
                    }
                    else
                    {
                        config.AppSettings.Settings["esbInstanceName"].Value = configurationServer.EsbInstanceName;
                    }

                    //ClientSettingsProvider.ServiceUri
                    if (config.AppSettings.Settings["ClientSettingsProvider.ServiceUri"] == null)
                    {
                        config.AppSettings.Settings.Add("ClientSettingsProvider.ServiceUri", configurationServer.ClientSettingsProviderServiceUri);
                    }
                    else
                    {
                        config.AppSettings.Settings["ClientSettingsProvider.ServiceUri"].Value = configurationServer.ClientSettingsProviderServiceUri;
                    }

                    //MinimumDiskSpaceThreshold
                    setting = config.AppSettings.Settings["MinimumDiskSpaceThreshold"];
                    temp = Convert.ToInt32(configurationServer.MinimumDiskSpaceThreshold) * 1048576;
                    var minimum = temp.ToString(CultureInfo.InvariantCulture);
                    if (null == setting)
                    {
                        config.AppSettings.Settings.Add("MinimumDiskSpaceThreshold", minimum);
                    }
                    else
                    {
                        setting.Value = minimum;
                    }

                    //LogFileSchedule
                    if (config.AppSettings.Settings["LogFileSchedule"] == null)
                    {
                        config.AppSettings.Settings.Add("LogFileSchedule", configurationServer.LogFileSchedule);
                    }
                    else
                    {
                        config.AppSettings.Settings["LogFileSchedule"].Value = configurationServer.LogFileSchedule;
                    }

                    //StartupTimeout
                    if (config.AppSettings.Settings["StartupTimeout"] == null)
                    {
                        config.AppSettings.Settings.Add("StartupTimeout", configurationServer.StartupTimeout.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["StartupTimeout"].Value = configurationServer.StartupTimeout.ToString();
                    }

                    //ConfigurationReloadDelay
                    if (config.AppSettings.Settings["ConfigurationReloadDelay"] == null)
                    {
                        config.AppSettings.Settings.Add("ConfigurationReloadDelay", configurationServer.ConfigurationReloadDelay.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["ConfigurationReloadDelay"].Value = configurationServer.ConfigurationReloadDelay.ToString();
                    }

                    //DiscoveryTCPPort
                    if (config.AppSettings.Settings["DiscoveryTCPPort"] == null)
                    {
                        config.AppSettings.Settings.Add("DiscoveryTCPPort", configurationServer.DiscoveryTCPPort.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["DiscoveryTCPPort"].Value = configurationServer.DiscoveryTCPPort.ToString();
                    }

                    //DiscoverySwaggerPort
                    if (config.AppSettings.Settings["DiscoverySwaggerPort"] == null)
                    {
                        config.AppSettings.Settings.Add("DiscoverySwaggerPort", configurationServer.DiscoverySwaggerPort.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["DiscoverySwaggerPort"].Value = configurationServer.DiscoverySwaggerPort.ToString();
                    }

                    //NeuronEndpointHostPort
                    if (config.AppSettings.Settings["NeuronEndpointHostPort"] == null)
                    {
                        config.AppSettings.Settings.Add("NeuronEndpointHostPort", configurationServer.NeuronEndpointHostPort.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["NeuronEndpointHostPort"].Value = configurationServer.NeuronEndpointHostPort.ToString();
                    }

                    //EnableUpdateNeuronEndpointHostPort
                    if (config.AppSettings.Settings["EnableUpdateNeuronEndpointHostPort"] == null)
                    {
                        config.AppSettings.Settings.Add("EnableUpdateNeuronEndpointHostPort", configurationServer.EnableUpdateNeuronEndpointHostPort.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["EnableUpdateNeuronEndpointHostPort"].Value = configurationServer.EnableUpdateNeuronEndpointHostPort.ToString();
                    }

                    //NeuronEventProcessorPort
                    if (config.AppSettings.Settings["NeuronEventProcessorPort"] == null)
                    {
                        config.AppSettings.Settings.Add("NeuronEventProcessorPort", configurationServer.NeuronEventProcessorPort.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["NeuronEventProcessorPort"].Value = configurationServer.NeuronEventProcessorPort.ToString();
                    }

                    //NeuronEventProcessorMachine
                    if (config.AppSettings.Settings["NeuronEventProcessorMachine"] == null)
                    {
                        config.AppSettings.Settings.Add("NeuronEventProcessorMachine", configurationServer.NeuronEventProcessorMachine);
                    }
                    else
                    {
                        config.AppSettings.Settings["NeuronEventProcessorMachine"].Value = configurationServer.NeuronEventProcessorMachine;
                    }

                    //DefaultHttpConnectionLimit
                    if (config.AppSettings.Settings["DefaultHttpConnectionLimit"] == null)
                    {
                        config.AppSettings.Settings.Add("DefaultHttpConnectionLimit", configurationServer.DefaultHttpConnectionLimit.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["DefaultHttpConnectionLimit"].Value = configurationServer.DefaultHttpConnectionLimit.ToString();
                    }

                    //ClusterNetworkName
                    if (config.AppSettings.Settings["ClusterNetworkName"] == null)
                    {
                        config.AppSettings.Settings.Add("ClusterNetworkName", configurationServer.ClusterNetworkName);
                    }
                    else
                    {
                        config.AppSettings.Settings["ClusterNetworkName"].Value = configurationServer.ClusterNetworkName;
                    }

                    //MaxHeartbeatFailures
                    if (config.AppSettings.Settings["MaxHeartbeatFailures"] == null)
                    {
                        config.AppSettings.Settings.Add("MaxHeartbeatFailures", configurationServer.MaxHeartbeatFailures.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["MaxHeartbeatFailures"].Value = configurationServer.MaxHeartbeatFailures.ToString();
                    }

                    //LoggingPollIntervalInSeconds
                    if (config.AppSettings.Settings["LoggingPollIntervalInSeconds"] == null)
                    {
                        config.AppSettings.Settings.Add("LoggingPollIntervalInSeconds", configurationServer.LoggingPollIntervalInSeconds.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["LoggingPollIntervalInSeconds"].Value = configurationServer.LoggingPollIntervalInSeconds.ToString();
                    }

                    //Microsoft.ServiceBus.ConnectionString
                    if (config.AppSettings.Settings["Microsoft.ServiceBus.ConnectionString"] == null)
                    {
                        config.AppSettings.Settings.Add("Microsoft.ServiceBus.ConnectionString", configurationServer.MicrosoftServiceBusConnectionString);
                    }
                    else
                    {
                        config.AppSettings.Settings["Microsoft.ServiceBus.ConnectionString"].Value = configurationServer.MicrosoftServiceBusConnectionString;
                    }

                    //esbUseMachineEnvironmentVars
                    if (config.AppSettings.Settings["esbUseMachineEnvironmentVars"] == null)
                    {
                        config.AppSettings.Settings.Add("esbUseMachineEnvironmentVars", configurationServer.EsbUseMachineEnvironmentVars.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["esbUseMachineEnvironmentVars"].Value = configurationServer.EsbUseMachineEnvironmentVars.ToString();
                    }

                    //XsltFilesLocation
                    if (config.AppSettings.Settings["XsltFilesLocation"] == null)
                    {
                        config.AppSettings.Settings.Add("XsltFilesLocation", configurationServer.XsltFilesLocation);
                    }
                    else
                    {
                        config.AppSettings.Settings["XsltFilesLocation"].Value = configurationServer.XsltFilesLocation;
                    }

                    //XsdFilesLocation
                    if (config.AppSettings.Settings["XsdFilesLocation"] == null)
                    {
                        config.AppSettings.Settings.Add("XsdFilesLocation", configurationServer.XsdFilesLocation);
                    }
                    else
                    {
                        config.AppSettings.Settings["XsdFilesLocation"].Value = configurationServer.XsdFilesLocation;
                    }

                    //LogPattern
                    if (config.AppSettings.Settings["LogPattern"] == null)
                    {
                        config.AppSettings.Settings.Add("LogPattern", configurationServer.LogPattern);
                    }
                    else
                    {
                        config.AppSettings.Settings["LogPattern"].Value = configurationServer.LogPattern;
                    }

                    //NeuronHybridManagementSuiteHost
                    if (config.AppSettings.Settings["NeuronHybridManagementSuiteHost"] == null)
                    {
                        config.AppSettings.Settings.Add("NeuronHybridManagementSuiteHost", configurationServer.NeuronHybridManagementSuiteHost);
                    }
                    else
                    {
                        config.AppSettings.Settings["NeuronHybridManagementSuiteHost"].Value = configurationServer.NeuronHybridManagementSuiteHost;
                    }

                    //TempDirectory
                    if (config.AppSettings.Settings["TempDirectory"] == null)
                    {
                        config.AppSettings.Settings.Add("TempDirectory", configurationServer.TempDirectory);
                    }
                    else
                    {
                        config.AppSettings.Settings["TempDirectory"].Value = configurationServer.TempDirectory;
                    }

                    //DisableGlobalExceptionHandler
                    if (config.AppSettings.Settings["DisableGlobalExceptionHandler"] == null)
                    {
                        config.AppSettings.Settings.Add("DisableGlobalExceptionHandler", configurationServer.DisableGlobalExceptionHandler.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["DisableGlobalExceptionHandler"].Value = configurationServer.DisableGlobalExceptionHandler.ToString();
                    }

                    //NeuronServiceRateFlushInterval
                    if (config.AppSettings.Settings["NeuronServiceRateFlushInterval"] == null)
                    {
                        config.AppSettings.Settings.Add("NeuronServiceRateFlushInterval", configurationServer.NeuronServiceRateFlushInterval.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["NeuronServiceRateFlushInterval"].Value = configurationServer.NeuronServiceRateFlushInterval.ToString();
                    }

                    //NeuronServiceRateCacheSize
                    if (config.AppSettings.Settings["NeuronServiceRateCacheSize"] == null)
                    {
                        config.AppSettings.Settings.Add("NeuronServiceRateCacheSize", configurationServer.NeuronServiceRateCacheSize.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["NeuronServiceRateCacheSize"].Value = configurationServer.NeuronServiceRateCacheSize.ToString();
                    }

                    //NeuronEventConsumerLevel
                    if (config.AppSettings.Settings["NeuronEventConsumerLevel"] == null)
                    {
                        config.AppSettings.Settings.Add("NeuronEventConsumerLevel", configurationServer.NeuronEventConsumerLevel.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["NeuronEventConsumerLevel"].Value = configurationServer.NeuronEventConsumerLevel.ToString();
                    }

                    //NeuronEventTimeToLive
                    if (config.AppSettings.Settings["NeuronEventTimeToLive"] == null)
                    {
                        config.AppSettings.Settings.Add("NeuronEventTimeToLive", configurationServer.NeuronEventTimeToLive.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["NeuronEventTimeToLive"].Value = configurationServer.NeuronEventTimeToLive.ToString();
                    }

                    //NeuronEventPoolSizeInit
                    if (config.AppSettings.Settings["NeuronEventPoolSizeInit"] == null)
                    {
                        config.AppSettings.Settings.Add("NeuronEventPoolSizeInit", configurationServer.NeuronEventPoolSizeInit.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["NeuronEventPoolSizeInit"].Value = configurationServer.NeuronEventPoolSizeInit.ToString();
                    }

                    //NeuronEventPoolSizeMax
                    if (config.AppSettings.Settings["NeuronEventPoolSizeMax"] == null)
                    {
                        config.AppSettings.Settings.Add("NeuronEventPoolSizeMax", configurationServer.NeuronEventPoolSizeMax.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["NeuronEventPoolSizeMax"].Value = configurationServer.NeuronEventPoolSizeMax.ToString();
                    }

                    //LicenseKey
                    if (config.AppSettings.Settings["LicenseKey"] == null)
                    {
                        config.AppSettings.Settings.Add("LicenseKey", configurationServer.LicenseKey);
                    }
                    else
                    {
                        config.AppSettings.Settings["LicenseKey"].Value = configurationServer.LicenseKey;
                    }

                    //SwaggerSelfHostingUrl
                    if (config.AppSettings.Settings["SwaggerSelfHostingUrl"] == null)
                    {
                        config.AppSettings.Settings.Add("SwaggerSelfHostingUrl", configurationServer.SwaggerSelfHostingUrl);
                    }
                    else
                    {
                        config.AppSettings.Settings["SwaggerSelfHostingUrl"].Value = configurationServer.SwaggerSelfHostingUrl;
                    }

                    //ESBServiceUsersTemp
                    if (config.AppSettings.Settings["ESBServiceUsersTemp"] == null)
                    {
                        config.AppSettings.Settings.Add("ESBServiceUsersTemp", configurationServer.ESBServiceUsersTemp);
                    }
                    else
                    {
                        config.AppSettings.Settings["ESBServiceUsersTemp"].Value = configurationServer.ESBServiceUsersTemp;
                    }

                    //SchedulerEnableLogging
                    if (config.AppSettings.Settings["SchedulerEnableLogging"] == null)
                    {
                        config.AppSettings.Settings.Add("SchedulerEnableLogging", configurationServer.SchedulerEnableLogging.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["SchedulerEnableLogging"].Value = configurationServer.SchedulerEnableLogging.ToString();
                    }

                    //DataMapperServicePort
                    if (config.AppSettings.Settings["DataMapperServicePort"] == null)
                    {
                        config.AppSettings.Settings.Add("DataMapperServicePort", configurationServer.DataMapperServicePort.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["DataMapperServicePort"].Value = configurationServer.DataMapperServicePort.ToString();
                    }

                    //WiretapCollectionFolder
                    if (config.AppSettings.Settings["WiretapCollectionFolder"] == null)
                    {
                        config.AppSettings.Settings.Add("WiretapCollectionFolder", configurationServer.WiretapCollectionFolder);
                    }
                    else
                    {
                        config.AppSettings.Settings["WiretapCollectionFolder"].Value = configurationServer.WiretapCollectionFolder;
                    }

                    //IgnoreSSLForRabbitMQ
                    if (config.AppSettings.Settings["IgnoreSSLForRabbitMQ"] == null)
                    {
                        config.AppSettings.Settings.Add("IgnoreSSLForRabbitMQ", configurationServer.IgnoreSSLForRabbitMQ.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["IgnoreSSLForRabbitMQ"].Value = configurationServer.IgnoreSSLForRabbitMQ.ToString();
                    }

                    //StartupAutoTuneThreadpools
                    if (config.AppSettings.Settings["StartupAutoTuneThreadpools"] == null)
                    {
                        config.AppSettings.Settings.Add("StartupAutoTuneThreadpools", configurationServer.StartupAutoTuneThreadpools.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["StartupAutoTuneThreadpools"].Value = configurationServer.StartupAutoTuneThreadpools.ToString();
                    }

                    //NumThreadsToAddPerTune
                    if (config.AppSettings.Settings["NumThreadsToAddPerTune"] == null)
                    {
                        config.AppSettings.Settings.Add("NumThreadsToAddPerTune", configurationServer.NumThreadsToAddPerTune.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["NumThreadsToAddPerTune"].Value = configurationServer.NumThreadsToAddPerTune.ToString();
                    }

                    //UseQuorumQueueForDeadLetters
                    if (config.AppSettings.Settings["UseQuorumQueueForDeadLetters"] == null)
                    {
                        config.AppSettings.Settings.Add("UseQuorumQueueForDeadLetters", configurationServer.UseQuorumQueueForDeadLetters.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["UseQuorumQueueForDeadLetters"].Value = configurationServer.UseQuorumQueueForDeadLetters.ToString();
                    }

                    //DeadLetterQuorumInitialSize
                    if (config.AppSettings.Settings["DeadLetterQuorumInitialSize"] == null)
                    {
                        config.AppSettings.Settings.Add("DeadLetterQuorumInitialSize", configurationServer.DeadLetterQuorumInitialSize.ToString());
                    }
                    else
                    {
                        config.AppSettings.Settings["DeadLetterQuorumInitialSize"].Value = configurationServer.DeadLetterQuorumInitialSize.ToString();
                    }
                    #endregion
                }

                config.Save();
                #endregion

                return configurationServer;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// returns folder path to update config files of that Instance
        /// </summary>
        /// <param name="instanceName"></param>
        /// <returns></returns>
        private string GetNeuronInstancePath(string instanceName)
        {
            string docspath = Path.Combine(CustomDllUtility.NEURON_UPGRADED_DEFAULT_PATH, "..\\", instanceName);
            if (!Directory.Exists(docspath))
            {
                docspath = Path.Combine(CustomDllUtility.NEURON_DEFAULT_PATH, "..\\", instanceName);
            }
            return docspath;
        }

        /// <summary>
        /// Write Input Data from User to Instance realted AppSettings.config File.
        /// </summary>
        /// <param name="configurationServer"></param>
        /// <returns>
        /// Object of ConfigurationServer Class which contains data of AppSettings.config File.
        /// </returns>
        public List<UpdateConfigureServerInstanceWise> UpdateServerConfigurationbyKeyValuePairs(List<UpdateConfigureServerInstanceWise> updateConfigureServerInstanceList)
        {
            foreach (var updateConfigureServerInstance in updateConfigureServerInstanceList)
            {
                //updated path so it will get and update settings from same config files
                var path = GetNeuronInstancePath(updateConfigureServerInstance.InstanceName);

                //will replace this when there are more than one neuron instances a
                var serverConfigFile = Path.Combine(path, "ESBService.exe");
                // Get the application path.
                var exePath = serverConfigFile;
                // Get the configuration file.
                var config = ConfigurationManager.OpenExeConfiguration(exePath);
                var TraceLevel = updateConfigureServerInstance.KeyValuePairs.Where(e => e.Name == "traceLevelDescriptions").FirstOrDefault();
                var WcfEnabled = updateConfigureServerInstance.KeyValuePairs.Where(e => e.Name == "Neuron.Service.WCFTracingEnabled").FirstOrDefault();


                #region For NeuronEndpointHost.exe.config File
                // update for neuronendpointhost.exe
                ///
                var serverNeuronHostConfigFile = Path.Combine(path, "NeuronEndpointHost.exe");
                // Get the application path.
                var exeNeuronHostPath = serverNeuronHostConfigFile;

                // Get the configuration file.
                var neuronHostconfig = ConfigurationManager.OpenExeConfiguration(exeNeuronHostPath);

                var configSection = neuronHostconfig.Sections["system.diagnostics"];
                var rawXml = configSection.SectionInformation.GetRawXml();
                var xml = XElement.Parse(rawXml ?? "<system.diagnostics/>");
                var switches = xml.Element("switches");
                if (null == switches)
                {
                    switches = new XElement("switches");
                    xml.Add(switches);
                }

                if (TraceLevel != null)
                {
                    var esbTraceSwitch =
                        switches.Elements("add").SingleOrDefault(x => x.Attributes("name").Any(a => "esbTraceSwitch" == a.Value));
                    if (null == esbTraceSwitch)
                    {
                        esbTraceSwitch = new XElement(
                            "add",
                            new XAttribute("name", "esbTraceSwitch"),
                            new XAttribute("value", TraceLevel.Value));
                        switches.Add(esbTraceSwitch);
                    }
                    else
                    {
                        esbTraceSwitch.Attribute("value").Value =
                            TraceLevel.Value.ToString(CultureInfo.InvariantCulture);
                    }
                }

                // If WCF Tracing is enabled, the NeuronEndpointHost.exe.config needs the sources and sharedListeners elements
                if ((WcfEnabled != null && !string.IsNullOrEmpty(WcfEnabled.Value) && Convert.ToBoolean(WcfEnabled.Value)) ||
                    (WcfEnabled == null && config.AppSettings.Settings["Neuron.Service.WCFTracingEnabled"] != null && !string.IsNullOrEmpty(config.AppSettings.Settings["Neuron.Service.WCFTracingEnabled"].Value) && Convert.ToBoolean(config.AppSettings.Settings["Neuron.Service.WCFTracingEnabled"].Value)))
                {
                    // insert or uncomment sources and sharedListeners elements
                    XElement sources = null;
                    XElement sharedListeners = null;

                    // see if the sources element is commented-out
                    foreach (XComment comment in xml.Nodes().OfType<XComment>().Where(c => c.Value.StartsWith("<sources>")).ToList())
                    {
                        sources = XElement.Parse(comment.Value);
                        comment.ReplaceWith(sources);
                    }

                    // if the sources element doesn't exist, add it
                    sources = xml.Element("sources");
                    if (null == sources)
                    {
                        sources = XElement.Parse(_sources);
                        xml.Add(sources);
                    }

                    // see if the sharedListeners element is commented-out
                    foreach (XComment comment in xml.Nodes().OfType<XComment>().Where(c => c.Value.StartsWith("<sharedListeners>")).ToList())
                    {
                        sharedListeners = XElement.Parse(comment.Value);
                        comment.ReplaceWith(sharedListeners);
                    }

                    // if the sharedListeners element doesn't exist, add it
                    sharedListeners = xml.Element("sharedListeners");
                    if (null == sharedListeners)
                    {
                        sharedListeners = XElement.Parse(_sharedListeners);
                        xml.Add(sharedListeners);
                    }
                }
                else
                {
                    // comment-out sources and sharedListeners elements

                    // if the sources element exists, comment it out
                    var sources = xml.Element("sources");
                    if (null != sources)
                    {
                        XComment comment = new XComment(sources.ToString());
                        sources.ReplaceWith(comment);
                    }

                    // if the sources element exists, comment it out
                    var sharedListeners = xml.Element("sharedListeners");
                    if (null != sharedListeners)
                    {
                        XComment comment = new XComment(sharedListeners.ToString());
                        sharedListeners.ReplaceWith(comment);
                    }
                }

                var newXml = xml.ToString();
                configSection.SectionInformation.SetRawXml(newXml);

                neuronHostconfig.Save();
                #endregion

                if (TraceLevel != null)
                {
                    #region For ESBService.exe.config File

                    // update for esbservice.exe
                    ///
                    configSection = config.Sections["system.diagnostics"];
                    rawXml = configSection.SectionInformation.GetRawXml();
                    xml = XElement.Parse(rawXml ?? "<system.diagnostics/>");
                    switches = xml.Element("switches");
                    if (null == switches)
                    {
                        switches = new XElement("switches");
                        xml.Add(switches);
                    }

                    var esbTraceSwitch =
                        switches.Elements("add").SingleOrDefault(x => x.Attributes("name").Any(a => "esbTraceSwitch" == a.Value));
                    if (null == esbTraceSwitch)
                    {
                        esbTraceSwitch = new XElement(
                            "add",
                            new XAttribute("name", "esbTraceSwitch"),
                            new XAttribute("value", TraceLevel.Value));
                        switches.Add(esbTraceSwitch);
                    }
                    else
                    {
                        esbTraceSwitch.Attribute("value").Value =
                            TraceLevel.Value.ToString(CultureInfo.InvariantCulture);
                    }

                    newXml = xml.ToString();
                    configSection.SectionInformation.SetRawXml(newXml);
                    #endregion
                }

                #region For appSettings.config File
                //Now add data to config file.
                //If data not exist then add data else change value of data.

                // Get the AppSettings section.
                var appSettings = config.AppSettings;
                if (appSettings == null)
                {
                }
                else
                {
                    foreach (var prop in updateConfigureServerInstance.KeyValuePairs)
                    {
                        if (prop.Name == "esbEnvironment")
                        {
                            #region Others

                            //esbInstanceName
                            if (config.AppSettings.Settings["esbInstanceName"] == null)
                            {
                                config.AppSettings.Settings.Add("esbInstanceName", updateConfigureServerInstance.InstanceName);
                            }

                            //ClientSettingsProvider.ServiceUri
                            if (config.AppSettings.Settings["ClientSettingsProvider.ServiceUri"] == null)
                            {
                                config.AppSettings.Settings.Add("ClientSettingsProvider.ServiceUri", "");
                            }

                            //MinimumDiskSpaceThreshold
                            var setting = config.AppSettings.Settings["MinimumDiskSpaceThreshold"];
                            var temp = 1048576;
                            var minimum = temp.ToString(CultureInfo.InvariantCulture);
                            if (null == setting)
                            {
                                config.AppSettings.Settings.Add("MinimumDiskSpaceThreshold", minimum);
                            }

                            //LogFileSchedule
                            if (config.AppSettings.Settings["LogFileSchedule"] == null)
                            {
                                config.AppSettings.Settings.Add("LogFileSchedule", "Daily");
                            }

                            //StartupTimeout
                            if (config.AppSettings.Settings["StartupTimeout"] == null)
                            {
                                config.AppSettings.Settings.Add("StartupTimeout", TimeSpan.Parse("00:10:00").ToString());
                            }

                            //ConfigurationReloadDelay
                            if (config.AppSettings.Settings["ConfigurationReloadDelay"] == null)
                            {
                                config.AppSettings.Settings.Add("ConfigurationReloadDelay", TimeSpan.Parse("00:01:00").ToString());
                            }

                            //DiscoveryTCPPort
                            if (config.AppSettings.Settings["DiscoveryTCPPort"] == null)
                            {
                                config.AppSettings.Settings.Add("DiscoveryTCPPort", "51001");
                            }

                            //DiscoverySwaggerPort
                            if (config.AppSettings.Settings["DiscoverySwaggerPort"] == null)
                            {
                                config.AppSettings.Settings.Add("DiscoverySwaggerPort", "51003");
                            }

                            //NeuronEndpointHostPort
                            if (config.AppSettings.Settings["NeuronEndpointHostPort"] == null)
                            {
                                config.AppSettings.Settings.Add("NeuronEndpointHostPort", "51004");
                            }

                            //EnableUpdateNeuronEndpointHostPort
                            if (config.AppSettings.Settings["EnableUpdateNeuronEndpointHostPort"] == null)
                            {
                                config.AppSettings.Settings.Add("EnableUpdateNeuronEndpointHostPort", "false");
                            }

                            //NeuronEventProcessorPort
                            if (config.AppSettings.Settings["NeuronEventProcessorPort"] == null)
                            {
                                config.AppSettings.Settings.Add("NeuronEventProcessorPort", "51005");
                            }

                            //NeuronEventProcessorMachine
                            if (config.AppSettings.Settings["NeuronEventProcessorMachine"] == null)
                            {
                                config.AppSettings.Settings.Add("NeuronEventProcessorMachine", Environment.MachineName);
                            }

                            //DefaultHttpConnectionLimit
                            if (config.AppSettings.Settings["DefaultHttpConnectionLimit"] == null)
                            {
                                config.AppSettings.Settings.Add("DefaultHttpConnectionLimit", "100");
                            }

                            //ClusterNetworkName
                            if (config.AppSettings.Settings["ClusterNetworkName"] == null)
                            {
                                config.AppSettings.Settings.Add("ClusterNetworkName", Environment.MachineName);
                            }

                            //MaxHeartbeatFailures
                            if (config.AppSettings.Settings["MaxHeartbeatFailures"] == null)
                            {
                                config.AppSettings.Settings.Add("MaxHeartbeatFailures", "2");
                            }

                            //LoggingPollIntervalInSeconds
                            if (config.AppSettings.Settings["LoggingPollIntervalInSeconds"] == null)
                            {
                                config.AppSettings.Settings.Add("LoggingPollIntervalInSeconds", "5");
                            }

                            //Microsoft.ServiceBus.ConnectionString
                            if (config.AppSettings.Settings["Microsoft.ServiceBus.ConnectionString"] == null)
                            {
                                config.AppSettings.Settings.Add("Microsoft.ServiceBus.ConnectionString", "Endpoint=sb://[your namespace].servicebus.windows.net;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=[your secret]");
                            }

                            //esbUseMachineEnvironmentVars
                            if (config.AppSettings.Settings["esbUseMachineEnvironmentVars"] == null)
                            {
                                config.AppSettings.Settings.Add("esbUseMachineEnvironmentVars", "false");
                            }

                            //XsltFilesLocation
                            if (config.AppSettings.Settings["XsltFilesLocation"] == null)
                            {
                                config.AppSettings.Settings.Add("XsltFilesLocation", "");
                            }

                            //XsdFilesLocation
                            if (config.AppSettings.Settings["XsdFilesLocation"] == null)
                            {
                                config.AppSettings.Settings.Add("XsdFilesLocation", "");
                            }

                            //LogPattern
                            if (config.AppSettings.Settings["LogPattern"] == null)
                            {
                                config.AppSettings.Settings.Add("LogPattern", "%date{yyyy-MM-dd HH:mm:ss.fffzzz} [%thread] %-5level - %message%newline%exception");
                            }

                            //NeuronHybridManagementSuiteHost
                            if (config.AppSettings.Settings["NeuronHybridManagementSuiteHost"] == null)
                            {
                                config.AppSettings.Settings.Add("NeuronHybridManagementSuiteHost", "localhost");
                            }

                            //TempDirectory
                            if (config.AppSettings.Settings["TempDirectory"] == null)
                            {
                                config.AppSettings.Settings.Add("TempDirectory", "");
                            }

                            //DisableGlobalExceptionHandler
                            if (config.AppSettings.Settings["DisableGlobalExceptionHandler"] == null)
                            {
                                config.AppSettings.Settings.Add("DisableGlobalExceptionHandler", "true");
                            }

                            //NeuronServiceRateFlushInterval
                            if (config.AppSettings.Settings["NeuronServiceRateFlushInterval"] == null)
                            {
                                config.AppSettings.Settings.Add("NeuronServiceRateFlushInterval", "5");
                            }

                            //NeuronServiceRateCacheSize
                            if (config.AppSettings.Settings["NeuronServiceRateCacheSize"] == null)
                            {
                                config.AppSettings.Settings.Add("NeuronServiceRateCacheSize", "10000");
                            }

                            //NeuronEventConsumerLevel
                            if (config.AppSettings.Settings["NeuronEventConsumerLevel"] == null)
                            {
                                config.AppSettings.Settings.Add("NeuronEventConsumerLevel", "1");
                            }

                            //NeuronEventTimeToLive
                            if (config.AppSettings.Settings["NeuronEventTimeToLive"] == null)
                            {
                                config.AppSettings.Settings.Add("NeuronEventTimeToLive", "1440");
                            }

                            //NeuronEventPoolSizeInit
                            if (config.AppSettings.Settings["NeuronEventPoolSizeInit"] == null)
                            {
                                config.AppSettings.Settings.Add("NeuronEventPoolSizeInit", "0");
                            }

                            //NeuronEventPoolSizeMax
                            if (config.AppSettings.Settings["NeuronEventPoolSizeMax"] == null)
                            {
                                config.AppSettings.Settings.Add("NeuronEventPoolSizeMax", "200");
                            }

                            //LicenseKey
                            if (config.AppSettings.Settings["LicenseKey"] == null)
                            {
                                config.AppSettings.Settings.Add("LicenseKey", "");
                            }

                            //SwaggerSelfHostingUrl
                            if (config.AppSettings.Settings["SwaggerSelfHostingUrl"] == null)
                            {
                                config.AppSettings.Settings.Add("SwaggerSelfHostingUrl", "");
                            }

                            //ESBServiceUsersTemp
                            if (config.AppSettings.Settings["ESBServiceUsersTemp"] == null)
                            {
                                config.AppSettings.Settings.Add("ESBServiceUsersTemp", @"C:\Windows\TEMP\");
                            }

                            //SchedulerEnableLogging
                            if (config.AppSettings.Settings["SchedulerEnableLogging"] == null)
                            {
                                config.AppSettings.Settings.Add("SchedulerEnableLogging", "false");
                            }

                            //DataMapperServicePort
                            if (config.AppSettings.Settings["DataMapperServicePort"] == null)
                            {
                                config.AppSettings.Settings.Add("DataMapperServicePort", "8585");
                            }

                            //WiretapCollectionFolder
                            if (config.AppSettings.Settings["WiretapCollectionFolder"] == null)
                            {
                                config.AppSettings.Settings.Add("WiretapCollectionFolder", @"C:\Program Files\Neudesic\Neuron ESB v3\DEFAULT\WiretapCollection");
                            }

                            //IgnoreSSLForRabbitMQ
                            if (config.AppSettings.Settings["IgnoreSSLForRabbitMQ"] == null)
                            {
                                config.AppSettings.Settings.Add("IgnoreSSLForRabbitMQ", "");
                            }

                            //StartupAutoTuneThreadpools
                            if (config.AppSettings.Settings["StartupAutoTuneThreadpools"] == null)
                            {
                                config.AppSettings.Settings.Add("StartupAutoTuneThreadpools", "false");
                            }

                            //NumThreadsToAddPerTune
                            if (config.AppSettings.Settings["NumThreadsToAddPerTune"] == null)
                            {
                                config.AppSettings.Settings.Add("NumThreadsToAddPerTune", "300");
                            }

                            //UseQuorumQueueForDeadLetters
                            if (config.AppSettings.Settings["UseQuorumQueueForDeadLetters"] == null)
                            {
                                config.AppSettings.Settings.Add("UseQuorumQueueForDeadLetters", "false");
                            }

                            //DeadLetterQuorumInitialSize
                            if (config.AppSettings.Settings["DeadLetterQuorumInitialSize"] == null)
                            {
                                config.AppSettings.Settings.Add("DeadLetterQuorumInitialSize", "3");
                            }
                            #endregion
                        }

                        if (prop.Name == "State")
                        {
                            continue;
                        }

                        if (prop.Name == "MaximumLogFileSize")
                        {
                            var setting = config.AppSettings.Settings["MaximumLogFileSize"];
                            var temp = Convert.ToInt32(prop.Value) * 1048576;
                            var maximum = temp.ToString(CultureInfo.InvariantCulture);
                            if (null == setting)
                            {
                                config.AppSettings.Settings.Add("MaximumLogFileSize", maximum);
                            }
                            else
                            {
                                setting.Value = maximum;
                            }
                            continue;
                        }

                        else
                        {
                            if (config.AppSettings.Settings[prop.Name] == null)
                            {
                                config.AppSettings.Settings.Add(prop.Name, prop.Value);
                            }
                            else
                            {
                                config.AppSettings.Settings[prop.Name].Value = prop.Value;
                            }
                        }
                    }
                }
                config.Save();

                var startServerThread = new Thread(
                    () =>
                    {
                        try
                        {
                            var stateProp = updateConfigureServerInstance.KeyValuePairs.Where(e => e.Name == "State").FirstOrDefault();
                            if (stateProp != null)
                            {
                                switch ((EServiceState)Enum.Parse(typeof(EServiceState), stateProp.Value))
                                {
                                    case EServiceState.Start:
                                        StartServer(updateConfigureServerInstance.InstanceName, path);
                                        break;
                                    case EServiceState.Stop:
                                        StopServer(updateConfigureServerInstance.InstanceName);
                                        break;
                                    case EServiceState.Restart:
                                        RestartServer(updateConfigureServerInstance.InstanceName);
                                        break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    });

                startServerThread.Start();

                #endregion
            }
            return updateConfigureServerInstanceList;
        }

        /// <summary>
        /// Get Data from AppSettings.config File.
        /// </summary>
        /// <returns>
        /// Object of ConfigurationServer Class which contains data of AppSettings.config File.
        /// </returns>
        public ConfigurationServer GetConfigureServerSettings()
        {
            ConfigurationServer configurationServerData = new ConfigurationServer();
            try
            {
                var serverConfigFile = Path.Combine(CustomDllUtility.AssemblyDirectory, "ESBService.exe");
                // Get the application path.
                var exePath = serverConfigFile;
                // Get the configuration file.
                var config = ConfigurationManager.OpenExeConfiguration(exePath);

                //Server Tab

                //esbEnvironment
                var setting = config.AppSettings.Settings["esbEnvironment"];
                if (setting != null)
                {
                    configurationServerData.EsbEnvironment = setting.Value;
                }

                //esbZone
                setting = config.AppSettings.Settings["esbZone"];
                if (setting != null)
                {
                    configurationServerData.EsbZone = setting.Value;
                }

                //esbDeploymentGroup
                if (config.AppSettings.Settings["esbDeploymentGroup"] != null)
                {
                    configurationServerData.EsbDeploymentGroup = config.AppSettings.Settings["esbDeploymentGroup"].Value;
                }

                //SavedHistoryCleanupSchedule
                setting = config.AppSettings.Settings["SavedHistoryCleanupSchedule"];
                int SavedHistoryCleanupSchedule;
                if (null == setting)
                {
                    SavedHistoryCleanupSchedule = 0;
                }
                else if (!int.TryParse(setting.Value, NumberStyles.None, CultureInfo.InvariantCulture, out SavedHistoryCleanupSchedule))
                {
                    SavedHistoryCleanupSchedule = 0;
                }
                configurationServerData.SavedHistoryCleanupSchedule = SavedHistoryCleanupSchedule;

                //neuronEventProcessor
                //NeuronHybridFeaturesEnabled
                setting = config.AppSettings.Settings["NeuronHybridFeaturesEnabled"];
                if (null != setting)
                {
                    configurationServerData.NeuronHybridFeaturesEnabled = Convert.ToBoolean(setting.Value);
                }

                //Logging Tab

                //traceLevelDescriptions which is stored in ESBService.exe.config file
                var configSection = config.Sections["system.diagnostics"];
                var rawXml = configSection.SectionInformation.GetRawXml();
                var xml = XElement.Parse(rawXml ?? "<system.diagnostics/>");
                var switches = xml.Element("switches");
                if (null == switches)
                {
                    switches = new XElement("switches");
                    xml.Add(switches);
                }
                var esbTraceSwitch = switches.Elements("add").SingleOrDefault(x => x.Attributes("name").Any(a => "esbTraceSwitch" == a.Value));
                int traceLevel = 4;
                if(esbTraceSwitch != null)
                {
                    if (!int.TryParse(esbTraceSwitch.LastAttribute.Value, NumberStyles.None, CultureInfo.InvariantCulture, out traceLevel))
                    {
                        traceLevel = 4;
                    }
                }
                configurationServerData.TraceLevelDescriptions = traceLevel;

                //MaximumLogFileSize
                setting = config.AppSettings.Settings["MaximumLogFileSize"];
                int maximumFileSize;
                if (null == setting)
                {
                    maximumFileSize = 104857600;
                }
                else if (!int.TryParse(setting.Value, NumberStyles.None, CultureInfo.InvariantCulture, out maximumFileSize))
                {
                    maximumFileSize = 104857600;
                }
                configurationServerData.MaximumLogFileSize = Convert.ToDecimal(maximumFileSize / 1048576);

                //LogFolderCleanupSchedule
                setting = config.AppSettings.Settings["LogFolderCleanupSchedule"];
                int LogFolderCleanupSchedule;
                if (null == setting)
                {
                    LogFolderCleanupSchedule = 10;
                }
                else if (!int.TryParse(setting.Value, NumberStyles.None, CultureInfo.InvariantCulture, out LogFolderCleanupSchedule))
                {
                    LogFolderCleanupSchedule = 10;
                }
                configurationServerData.LogFolderCleanupSchedule = LogFolderCleanupSchedule;

                //Neuron.Service.WCFTracingEnabled
                setting = config.AppSettings.Settings["Neuron.Service.WCFTracingEnabled"];
                if (null != setting)
                {
                    configurationServerData.WCFTracingEnabled = Convert.ToBoolean(setting.Value);
                }

                //Performance Tab

                //overrideDotNetThreadPoolSize
                setting = config.AppSettings.Settings["overrideDotNetThreadPoolSize"];
                if (null != setting)
                {
                    configurationServerData.OverrideDotNetThreadPoolSize = Convert.ToBoolean(setting.Value);
                }

                //if Override value is false then Max Min value for threads will be assigned
                if (!configurationServerData.OverrideDotNetThreadPoolSize)
                {
                    ThreadPool.GetMinThreads(out int minWorkerThreads, out int minIOThreads);
                    ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxIOThreads);

                    configurationServerData.MaxWorkerThreadsLimit = maxWorkerThreads;
                    configurationServerData.MaxIOThreadsLimit = maxIOThreads;
                    configurationServerData.MinIOThreadsLimit = minIOThreads;
                    configurationServerData.MinWorkerThreadsLimit = minWorkerThreads;
                }

                //dotNetMaxWorkerThreads
                if (config.AppSettings.Settings["dotNetMaxWorkerThreads"] != null)
                {
                    var threadPoolSize = 500;
                    try
                    {
                        threadPoolSize = int.Parse(config.AppSettings.Settings["dotNetMaxWorkerThreads"].Value);
                    }
                    catch (Exception)
                    {
                    }
                    configurationServerData.DotNetMaxWorkerThreads = threadPoolSize;
                }
                else
                {
                    configurationServerData.DotNetMaxWorkerThreads = 500;
                }

                //dotNetMinWorkerThreads
                if (config.AppSettings.Settings["dotNetMinWorkerThreads"] != null)
                {
                    var threadPoolSize = 50;
                    try
                    {
                        threadPoolSize = int.Parse(config.AppSettings.Settings["dotNetMinWorkerThreads"].Value);
                    }
                    catch (Exception)
                    {
                    }
                    configurationServerData.DotNetMinWorkerThreads = threadPoolSize;
                }
                else
                {
                    configurationServerData.DotNetMinWorkerThreads = 50;
                }

                //dotNetMaxIOThreads
                if (config.AppSettings.Settings["dotNetMaxIOThreads"] != null)
                {
                    var threadPoolSize = 1000;
                    try
                    {
                        threadPoolSize = int.Parse(config.AppSettings.Settings["dotNetMaxIOThreads"].Value);
                    }
                    catch (Exception)
                    {
                    }
                    configurationServerData.DotNetMaxIOThreads = threadPoolSize;
                }
                else
                {
                    configurationServerData.DotNetMaxIOThreads = 1000;
                }

                //dotNetMinIOThreads
                if (config.AppSettings.Settings["dotNetMinIOThreads"] != null)
                {
                    var threadPoolSize = 100;
                    try
                    {
                        threadPoolSize = int.Parse(config.AppSettings.Settings["dotNetMinIOThreads"].Value);
                    }
                    catch (Exception)
                    {
                    }
                    configurationServerData.DotNetMinIOThreads = threadPoolSize;
                }
                else
                {
                    configurationServerData.DotNetMinIOThreads = 100;
                }


                //Others---------------------------------------------------------------------------------

                //esbInstanceName
                var instanceNameSetting = config.AppSettings.Settings["esbInstanceName"];
                string instanceName;
                if (null != instanceNameSetting)
                {
                    instanceName = instanceNameSetting.Value;
                }
                else
                {
                    instanceName = "DEFAULT";
                }
                //instanceName = string.Format(CultureInfo.InvariantCulture,"Configure {0} Server",instanceName ?? "DEFAULT");
                configurationServerData.EsbInstanceName = instanceName;

                //ClientSettingsProvider.ServiceUri
                setting = config.AppSettings.Settings["ClientSettingsProvider.ServiceUri"];
                if (setting != null)
                {
                    configurationServerData.ClientSettingsProviderServiceUri = setting.Value;
                }
                else
                {
                    configurationServerData.ClientSettingsProviderServiceUri = "";
                }

                //MinimumDiskSpaceThreshold
                setting = config.AppSettings.Settings["MinimumDiskSpaceThreshold"];
                int minimumDiskSpace;
                if (null == setting)
                {
                    minimumDiskSpace = 10485760;
                }
                else if (!int.TryParse(setting.Value, NumberStyles.None, CultureInfo.InvariantCulture, out minimumDiskSpace))
                {
                    minimumDiskSpace = 10485760;
                }
                configurationServerData.MinimumDiskSpaceThreshold = Convert.ToDecimal(minimumDiskSpace / 1048576);

                //LogFileSchedule
                setting = config.AppSettings.Settings["LogFileSchedule"];
                configurationServerData.LogFileSchedule = null != setting ? setting.Value : "Daily";

                //StartupTimeout
                TimeSpan time;
                setting = config.AppSettings.Settings["StartupTimeout"];
                if (null == setting)
                {
                    time = TimeSpan.Parse("00:10:00");
                }
                else if (!TimeSpan.TryParse(setting.Value, out time))
                {
                    time = TimeSpan.Parse("00:10:00");
                }
                configurationServerData.StartupTimeout = time;

                //ConfigurationReloadDelay
                setting = config.AppSettings.Settings["ConfigurationReloadDelay"];
                if (null == setting)
                {
                    time = TimeSpan.Parse("00:01:00");
                }
                else if (!TimeSpan.TryParse(setting.Value, out time))
                {
                    time = TimeSpan.Parse("00:01:00");
                }
                configurationServerData.ConfigurationReloadDelay = time;

                //DiscoveryTCPPort
                setting = config.AppSettings.Settings["DiscoveryTCPPort"];
                int discoveryTCPPort;
                if (null == setting)
                {
                    discoveryTCPPort = 51001;
                }
                else if (!int.TryParse(setting.Value, NumberStyles.None, CultureInfo.InvariantCulture, out discoveryTCPPort))
                {
                    discoveryTCPPort = 51001;
                }
                configurationServerData.DiscoveryTCPPort = discoveryTCPPort;

                //DiscoverySwaggerPort
                setting = config.AppSettings.Settings["DiscoverySwaggerPort"];
                int discoverySwaggerPort;
                if (null == setting)
                {
                    discoverySwaggerPort = 51003;
                }
                else if (!int.TryParse(setting.Value, NumberStyles.None, CultureInfo.InvariantCulture, out discoverySwaggerPort))
                {
                    discoverySwaggerPort = 51003;
                }
                configurationServerData.DiscoverySwaggerPort = discoverySwaggerPort;

                //NeuronEndpointHostPort
                setting = config.AppSettings.Settings["NeuronEndpointHostPort"];
                int neuronEndpointHostPort;
                if (null == setting)
                {
                    neuronEndpointHostPort = 51004;
                }
                else if (!int.TryParse(setting.Value, NumberStyles.None, CultureInfo.InvariantCulture, out neuronEndpointHostPort))
                {
                    neuronEndpointHostPort = 51004;
                }
                configurationServerData.NeuronEndpointHostPort = neuronEndpointHostPort;

                //EnableUpdateNeuronEndpointHostPort
                setting = config.AppSettings.Settings["EnableUpdateNeuronEndpointHostPort"];
                if (null != setting)
                {
                    configurationServerData.EsbUseMachineEnvironmentVars = Convert.ToBoolean(setting.Value);
                }

                //NeuronEventProcessorPort
                setting = config.AppSettings.Settings["NeuronEventProcessorPort"];
                int neuronEventProcessorPort;
                if (null == setting)
                {
                    neuronEventProcessorPort = 51005;
                }
                else if (!int.TryParse(setting.Value, NumberStyles.None, CultureInfo.InvariantCulture, out neuronEventProcessorPort))
                {
                    neuronEventProcessorPort = 51005;
                }
                configurationServerData.NeuronEventProcessorPort = neuronEventProcessorPort;

                //NeuronEventProcessorMachine
                setting = config.AppSettings.Settings["NeuronEventProcessorMachine"];
                if (setting != null)
                {
                    configurationServerData.NeuronEventProcessorMachine = setting.Value;
                }
                else
                {
                    configurationServerData.NeuronEventProcessorMachine = Environment.MachineName;
                }

                //DefaultHttpConnectionLimit
                setting = config.AppSettings.Settings["DefaultHttpConnectionLimit"];
                int defaultHttpConnectionLimit;
                if (null == setting)
                {
                    defaultHttpConnectionLimit = 100;
                }
                else if (!int.TryParse(setting.Value, NumberStyles.None, CultureInfo.InvariantCulture, out defaultHttpConnectionLimit))
                {
                    defaultHttpConnectionLimit = 100;
                }
                configurationServerData.DefaultHttpConnectionLimit = defaultHttpConnectionLimit;

                //ClusterNetworkName
                setting = config.AppSettings.Settings["ClusterNetworkName"];
                if (setting != null)
                {
                    configurationServerData.ClusterNetworkName = setting.Value;
                }
                else
                {
                    configurationServerData.ClusterNetworkName = Environment.MachineName;
                }

                //MaxHeartbeatFailures
                setting = config.AppSettings.Settings["MaxHeartbeatFailures"];
                int maxHeartbeatFailures;
                if (null == setting)
                {
                    maxHeartbeatFailures = 2;
                }
                else if (!int.TryParse(setting.Value, NumberStyles.None, CultureInfo.InvariantCulture, out maxHeartbeatFailures))
                {
                    maxHeartbeatFailures = 2;
                }
                configurationServerData.MaxHeartbeatFailures = maxHeartbeatFailures;

                //LoggingPollIntervalInSeconds
                setting = config.AppSettings.Settings["LoggingPollIntervalInSeconds"];
                int loggingPollIntervalInSeconds;
                if (null == setting)
                {
                    loggingPollIntervalInSeconds = 5;
                }
                else if (!int.TryParse(setting.Value, NumberStyles.None, CultureInfo.InvariantCulture, out loggingPollIntervalInSeconds))
                {
                    loggingPollIntervalInSeconds = 5;
                }
                configurationServerData.LoggingPollIntervalInSeconds = loggingPollIntervalInSeconds;

                //MicrosoftServiceBusConnectionString
                setting = config.AppSettings.Settings["MicrosoftServiceBusConnectionString"];
                if (null != setting)
                {
                    configurationServerData.MicrosoftServiceBusConnectionString = setting.Value;
                }
                else
                {
                    string microsoftServiceBusConnectionString = "Endpoint=sb://[your namespace].servicebus.windows.net;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=[your secret]";
                    configurationServerData.MicrosoftServiceBusConnectionString = microsoftServiceBusConnectionString;
                }

                //esbUseMachineEnvironmentVars
                setting = config.AppSettings.Settings["esbUseMachineEnvironmentVars"];
                if (null != setting)
                {
                    configurationServerData.EsbUseMachineEnvironmentVars = Convert.ToBoolean(setting.Value);
                }

                //XsltFilesLocation
                setting = config.AppSettings.Settings["XsltFilesLocation"];
                if (null != setting)
                {
                    configurationServerData.XsltFilesLocation = setting.Value;
                }
                else
                {
                    configurationServerData.XsltFilesLocation = "";
                }

                //XsdFilesLocation
                setting = config.AppSettings.Settings["XsdFilesLocation"];
                if (null != setting)
                {
                    configurationServerData.XsdFilesLocation = setting.Value;
                }
                else
                {
                    configurationServerData.XsdFilesLocation = "";
                }

                //LogPattern
                setting = config.AppSettings.Settings["LogPattern"];
                if (null != setting)
                {
                    configurationServerData.LogPattern = setting.Value;
                }
                else
                {
                    string logPattern = "%date{yyyy-MM-dd HH:mm:ss.fffzzz} [%thread] %-5level - %message%newline%exception";
                    configurationServerData.LogPattern = logPattern;
                }

                //NeuronHybridManagementSuiteHost
                setting = config.AppSettings.Settings["NeuronHybridManagementSuiteHost"];
                if (null != setting)
                {
                    configurationServerData.NeuronHybridManagementSuiteHost = setting.Value;
                }
                else
                {
                    configurationServerData.NeuronHybridManagementSuiteHost = "localhost";
                }

                //TempDirectory
                setting = config.AppSettings.Settings["TempDirectory"];
                if (null != setting)
                {
                    configurationServerData.TempDirectory = setting.Value;
                }
                else
                {
                    configurationServerData.TempDirectory = "";
                }

                //DisableGlobalExceptionHandler
                setting = config.AppSettings.Settings["DisableGlobalExceptionHandler"];
                if (null != setting)
                {
                    configurationServerData.DisableGlobalExceptionHandler = Convert.ToBoolean(setting.Value);
                }
                else
                {
                    configurationServerData.DisableGlobalExceptionHandler = true;
                }

                //NeuronServiceRateFlushInterval
                setting = config.AppSettings.Settings["NeuronServiceRateFlushInterval"];
                int neuronServiceRateFlushInterval;
                if (null == setting)
                {
                    neuronServiceRateFlushInterval = 5;
                }
                else if (!int.TryParse(setting.Value, NumberStyles.None, CultureInfo.InvariantCulture, out neuronServiceRateFlushInterval))
                {
                    neuronServiceRateFlushInterval = 5;
                }
                configurationServerData.NeuronServiceRateFlushInterval = neuronServiceRateFlushInterval;

                //NeuronServiceRateCacheSize
                setting = config.AppSettings.Settings["NeuronServiceRateCacheSize"];
                int neuronServiceRateCacheSize;
                if (null == setting)
                {
                    neuronServiceRateCacheSize = 10000;
                }
                else if (!int.TryParse(setting.Value, NumberStyles.None, CultureInfo.InvariantCulture, out neuronServiceRateCacheSize))
                {
                    neuronServiceRateCacheSize = 10000;
                }
                configurationServerData.NeuronServiceRateCacheSize = neuronServiceRateCacheSize;

                //NeuronEventConsumerLevel
                setting = config.AppSettings.Settings["NeuronEventConsumerLevel"];
                int neuronEventConsumerLevel;
                if (null == setting)
                {
                    neuronEventConsumerLevel = 1;
                }
                else if (!int.TryParse(setting.Value, NumberStyles.None, CultureInfo.InvariantCulture, out neuronEventConsumerLevel))
                {
                    neuronEventConsumerLevel = 1;
                }
                configurationServerData.NeuronEventConsumerLevel = neuronEventConsumerLevel;

                //NeuronEventTimeToLive
                setting = config.AppSettings.Settings["NeuronEventTimeToLive"];
                int neuronEventTimeToLive;
                if (null == setting)
                {
                    neuronEventTimeToLive = 1440;
                }
                else if (!int.TryParse(setting.Value, NumberStyles.None, CultureInfo.InvariantCulture, out neuronEventTimeToLive))
                {
                    neuronEventTimeToLive = 1440;
                }
                configurationServerData.NeuronEventTimeToLive = neuronEventTimeToLive;

                //NeuronEventPoolSizeInit
                setting = config.AppSettings.Settings["NeuronEventPoolSizeInit"];
                int neuronEventPoolSizeInit;
                if (null == setting)
                {
                    neuronEventPoolSizeInit = 0;
                }
                else if (!int.TryParse(setting.Value, NumberStyles.None, CultureInfo.InvariantCulture, out neuronEventPoolSizeInit))
                {
                    neuronEventPoolSizeInit = 0;
                }
                configurationServerData.NeuronEventPoolSizeInit = neuronEventPoolSizeInit;

                //NeuronEventPoolSizeMax
                setting = config.AppSettings.Settings["NeuronEventPoolSizeMax"];
                int neuronEventPoolSizeMax;
                if (null == setting)
                {
                    neuronEventPoolSizeMax = 200;
                }
                else if (!int.TryParse(setting.Value, NumberStyles.None, CultureInfo.InvariantCulture, out neuronEventPoolSizeMax))
                {
                    neuronEventPoolSizeMax = 200;
                }
                configurationServerData.NeuronEventPoolSizeMax = neuronEventPoolSizeMax;

                //LicenseKey
                setting = config.AppSettings.Settings["LicenseKey"];
                if (null != setting)
                {
                    configurationServerData.LicenseKey = setting.Value;
                }
                else
                {
                    configurationServerData.LicenseKey = "";
                }

                //SwaggerSelfHostingUrl
                setting = config.AppSettings.Settings["SwaggerSelfHostingUrl"];
                if (null != setting)
                {
                    configurationServerData.SwaggerSelfHostingUrl = setting.Value;
                }
                else
                {
                    configurationServerData.SwaggerSelfHostingUrl = "";
                }

                //ESBServiceUsersTemp
                setting = config.AppSettings.Settings["ESBServiceUsersTemp"];
                if (null != setting)
                {
                    configurationServerData.ESBServiceUsersTemp = setting.Value;
                }
                else
                {
                    configurationServerData.ESBServiceUsersTemp = @"C:\Windows\TEMP\";
                }

                //SchedulerEnableLogging
                setting = config.AppSettings.Settings["SchedulerEnableLogging"];
                if (null != setting)
                {
                    configurationServerData.SchedulerEnableLogging = Convert.ToBoolean(setting.Value);
                }

                //DataMapperServicePort
                setting = config.AppSettings.Settings["DataMapperServicePort"];
                int dataMapperServicePort;
                if (null == setting)
                {
                    dataMapperServicePort = 8585;
                }
                else if (!int.TryParse(setting.Value, NumberStyles.None, CultureInfo.InvariantCulture, out dataMapperServicePort))
                {
                    dataMapperServicePort = 8585;
                }
                configurationServerData.DataMapperServicePort = dataMapperServicePort;

                //WiretapCollectionFolder
                setting = config.AppSettings.Settings["WiretapCollectionFolder"];
                if (null != setting)
                {
                    configurationServerData.WiretapCollectionFolder = setting.Value;
                }
                else
                {
                    configurationServerData.WiretapCollectionFolder = @"C:\Program Files\Neudesic\Neuron ESB v3\DEFAULT\WiretapCollection";
                }

                //IgnoreSSLForRabbitMQ
                setting = config.AppSettings.Settings["IgnoreSSLForRabbitMQ"];
                if (null != setting)
                {
                    configurationServerData.IgnoreSSLForRabbitMQ = setting.Value;
                }

                //StartupAutoTuneThreadpools
                setting = config.AppSettings.Settings["StartupAutoTuneThreadpools"];
                if (null != setting)
                {
                    configurationServerData.StartupAutoTuneThreadpools = Convert.ToBoolean(setting.Value);
                }

                //NumThreadsToAddPerTune
                setting = config.AppSettings.Settings["NumThreadsToAddPerTune"];
                int numThreadsToAddPerTune;
                if (null == setting)
                {
                    numThreadsToAddPerTune = 300;
                }
                else if (!int.TryParse(setting.Value, NumberStyles.None, CultureInfo.InvariantCulture, out numThreadsToAddPerTune))
                {
                    numThreadsToAddPerTune = 300;
                }
                configurationServerData.NumThreadsToAddPerTune = numThreadsToAddPerTune;

                //UseQuorumQueueForDeadLetters
                setting = config.AppSettings.Settings["UseQuorumQueueForDeadLetters"];
                if (null != setting)
                {
                    configurationServerData.UseQuorumQueueForDeadLetters = Convert.ToBoolean(setting.Value);
                }

                //DeadLetterQuorumInitialSize
                setting = config.AppSettings.Settings["DeadLetterQuorumInitialSize"];
                int deadLetterQuorumInitialSize;
                if (null == setting)
                {
                    deadLetterQuorumInitialSize = 3;
                }
                else if (!int.TryParse(setting.Value, NumberStyles.None, CultureInfo.InvariantCulture, out deadLetterQuorumInitialSize))
                {
                    deadLetterQuorumInitialSize = 3;
                }
                configurationServerData.DeadLetterQuorumInitialSize = deadLetterQuorumInitialSize;



            }
            catch (Exception ex)
            {
                throw ex;
            }

            return configurationServerData;
        }


        /// <summary>
        /// Get Data from AppSettings.config for all neuron instance File.
        /// </summary>
        /// <returns>
        /// Object of InstanceInfoModel Class which contains data of AppSettings.config File.
        /// </returns>
        public List<InstanceInfoModel> GetInstancewiseConfigureServerSettings()
        {
            List<InstanceInfoModel> infoModels = new List<InstanceInfoModel>();

            dynamic instances = GetAllInstancesofConfigureServer();

            foreach (var instance in instances)
            {
                InstanceInfoModel infoModel = new InstanceInfoModel();
                infoModel.InstanceName = instance.name;
                infoModel.SolutionPath = instance.configuration;

                // Commented this as this always calls openConfiguration for every neuron instance and
                // fails to return neuron instances list even if one configuration failed to load 
                
				//if (!string.IsNullOrEmpty(infoModel.SolutionPath))
				//{
				//	CommonPathModel pathModel = new CommonPathModel();
				//	pathModel.Path = infoModel.SolutionPath;
				//	try
				//	{
				//		var pathConfiguration = this.GetPathConfiguration(pathModel);
				//		if (pathConfiguration != null)
				//		{
				//			infoModel.DeploymentGroupList = pathConfiguration.DeploymentGroupList;
				//		}
				//	}
				//	catch (Exception e) 
    //                {
    //                    throw e;
    //                }
				//}

                infoModel.ActiveDeploymentGroup = instance.deploymentGroup;
                infoModel.ServiceStatus = GetServerStatus(infoModel.InstanceName);

                //Call an API for fetch Cofiguration Settings of perticular instance
                dynamic instanceConfigurationSetings = GetConfigurationOfInstance(infoModel.InstanceName);

                switch (instanceConfigurationSetings.traceLevelSetting.ToString())
				{
                    case "Off":
                        infoModel.TraceLevel = 0;
                        break;
                    case "Error":
                        infoModel.TraceLevel = 1;
                        break;
                    case "Warning":
                        infoModel.TraceLevel = 2;
                        break;
                    case "Info":
                        infoModel.TraceLevel = 3;
                        break;
                    case "Verbose":
                        infoModel.TraceLevel = 4;
                        break;
				}

                // DS : Show a MaximumLogFileSize in MB.
                infoModel.MaximumLogFileSize = Convert.ToDecimal(instanceConfigurationSetings.maximumLogFileSize / 1048576);
                infoModel.LogFolderCleanupSchedule = instanceConfigurationSetings.logFolderCleanupSchedule;
                infoModel.WCFTracingEnabled = instanceConfigurationSetings.wcfTracingEnabled;
                infoModel.OverrideDotNetThreadPoolSize = instanceConfigurationSetings.overrideDotNetThreadPoolSize;
                infoModel.DotNetMaxWorkerThreads = instanceConfigurationSetings.dotNetMaxWorkerThreads;
                infoModel.DotNetMinWorkerThreads = instanceConfigurationSetings.dotNetMinWorkerThreads;
                infoModel.DotNetMaxIOThreads = instanceConfigurationSetings.dotNetMaxIOThreads;
                infoModel.DotNetMinIOThreads = instanceConfigurationSetings.dotNetMinIOThreads;
                infoModel.NeuronHybridFeaturesEnabled = Convert.ToBoolean(instanceConfigurationSetings.neuronHybridFeaturesEnabled);

                //if Override value is false then Max Min value for threads will be assigned
                if (!infoModel.OverrideDotNetThreadPoolSize)
				{
					ThreadPool.GetMinThreads(out int minWorkerThreads, out int minIOThreads);
					ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxIOThreads);

					infoModel.MaxWorkerThreadsLimit = maxWorkerThreads;
                    infoModel.MinWorkerThreadsLimit = minWorkerThreads;
                    infoModel.MaxIOThreadsLimit = maxIOThreads;
					infoModel.MinIOThreadsLimit = minIOThreads;
				}

				infoModels.Add(infoModel);
            }

			return infoModels;
		}

        private ServiceControllerStatus GetServerStatus(string instanceName)
        {
            var SERVICE_NAME = string.Format(CultureInfo.InvariantCulture, "NeuronESBv3_{0}", instanceName);
            var sc = new ServiceController(Environment.MachineName) { MachineName = Environment.MachineName, ServiceName = SERVICE_NAME };
            sc.Refresh();
            return sc.Status;
        }

        //sended path so it will get and update settings from same config files
        private void StartServer(string instance, string path)
        {
            var exePath = Path.Combine(path, "ESBService.exe");
            var config = ConfigurationManager.OpenExeConfiguration(exePath);
            if (config == null)
            {
                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    "The application configuration (*.config) file for Neuron ESB runtime, '{0}', could not be found.",
                    exePath);
                throw new Exception(message);
            }
            var appSettings = config.AppSettings;
            if (appSettings == null)
            {
                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    "The application configuration (*.config) file for Neuron ESB runtime, '{0}', is invalid. The app settings section is missing.",
                    exePath);
                throw new Exception(message);
            }
            if (config.AppSettings.Settings["esbEnvironment"] == null || string.IsNullOrEmpty(config.AppSettings.Settings["esbEnvironment"].Value))
            {
                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    "There is no Solution currently configured for the Neuron ESB runtime service. Please select 'Configure Server' from the toolbar to assign a Solution for the Neuron EB runtime to load.",
                    exePath);
                throw new Exception(message);
            }
            ServiceController sc = new ServiceController(Environment.MachineName);
            sc.MachineName = Environment.MachineName;
            sc.ServiceName = string.Format(CultureInfo.InvariantCulture, "NeuronESBv3_{0}", instance);
            sc.Refresh();

            if (sc.Status != ServiceControllerStatus.Stopped) return;
            sc.Start();
            try
            {
                sc.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 3, 0));
            }
            catch (System.ServiceProcess.TimeoutException ex) { throw ex; }
        }

        private void StopServer(string instance)
        {
            var SERVICE_NAME = string.Format(CultureInfo.InvariantCulture, "NeuronESBv3_{0}", instance);
            using (var sc = new ServiceController(SERVICE_NAME, Environment.MachineName))
            {
                sc.Refresh();
                if (sc.Status == ServiceControllerStatus.Stopped || sc.Status == ServiceControllerStatus.StopPending) return;

                sc.Stop();
                try
                {
                    sc.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 3, 0));
                }
                catch (System.ServiceProcess.TimeoutException ex) { throw ex; }
            }
        }

        /// <summary>
        /// Restart the Neuron ESB Service on the specified machine
        /// Pass null, '.', or 'localhost' to check the local machine.
        /// </summary>
        /// <param name="machineName">Restart the Neuron ESB Service on this machine</param>
        private void RestartServer(string instanceName)
        {
            var serviceName = string.Format(CultureInfo.InvariantCulture, "NeuronESBv3_{0}", instanceName);

            using (var sc = new ServiceController(serviceName, Environment.MachineName))
            {
                sc.Refresh();

                if (sc.Status != ServiceControllerStatus.Stopped && sc.Status != ServiceControllerStatus.StopPending)
                {
                    sc.Stop();
                }

                try
                {
                    sc.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 3, 0));
                }
                catch (System.ServiceProcess.TimeoutException ex) { throw ex; }

                sc.Start();
                try
                {
                    sc.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 3, 0));
                }
                catch (System.ServiceProcess.TimeoutException ex) { throw ex; }
            }
        }

        /// <summary>
        /// returns model that contains zones and deployment Groups List
        /// </summary>
        /// <param name="serverPathModel"></param>
        /// <returns></returns>
        public PathConfigurationResponseModel GetPathConfiguration(CommonPathModel serverPathModel)
        {
            PathConfigurationResponseModel responseModel = new PathConfigurationResponseModel();
            FileAttributes fa = File.GetAttributes(serverPathModel.Path);

            if (Directory.Exists(serverPathModel.Path) || File.Exists(serverPathModel.Path))
            {
                using (var admin = new Administrator(serverPathModel.Path))
                {
                    if (fa.HasFlag(FileAttributes.Directory))
                    {
                        if (!Neuron.Esb.Internal.CurrentUserSecurity.CheckFileSystemRightsForFolder(serverPathModel.Path, System.Security.AccessControl.FileSystemRights.Read))
                            throw new System.Security.SecurityException(string.Format(CultureInfo.CurrentCulture, "Unable to Load the Solution from the '{0}' folder. The current user needs at least 'Read' Permissions to load a solution.", serverPathModel.Path));
                    }
                    admin.OpenConfiguration();
                    var listZones = admin.GetAllZones();
                    var listDeployments = admin.GetAllDeployments();

                    if (listZones.Count > 0)
                    {
                        responseModel.ZoneList = new List<string>();
                        foreach (var zone in listZones.Where(zone => zone.Enabled))
                        {
                            responseModel.ZoneList.Add(zone.Name);
                        }
                    }

                    if (listDeployments.Count > 0)
                    {
                        responseModel.DeploymentGroupList = new List<string>();
                        foreach (var t in listDeployments)
                        {
                            responseModel.DeploymentGroupList.Add(t.Name);
                        }
                    }
                    admin.CloseConfiguration();
                }
            }
            else
            {
                throw new Exception("Configuration settings were not loaded. The file " + serverPathModel.Path + " does not exist.");
            }
            return responseModel;
        }
    }
    #endregion
}
