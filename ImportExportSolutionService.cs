using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neuron;
using Neuron.Esb.Adapters;
using Neuron.Esb.Administration;
using Neuron.Esb.Channels;
using Neuron.Esb.Channels.Msmq;
using Neuron.Esb.Internal;
using Peregrine.Application.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Peregrine.Application.Service.Core;
using System.Globalization;
using Neuron.Esb.Administration;
using System.Text.RegularExpressions;

namespace Peregrine.Application.Service.Services
{
    #region IImportExportSolutionService interface
    /// <summary>
    /// Public interface for managing application impport export solution.
    /// </summary>
    public interface IImportExportSolutionService
    {
        /// <summary>
        /// Returns list of Duplicate entities while import solution
        /// </summary>
        /// <param name="configurationBaseRequest"></param>
        /// <returns></returns>
        List<GetListResponseModel> GetList(ConfigurationBaseRequest configurationBaseRequest);

        /// <summary>
        /// Returns list of Duplicate entities while import solution
        /// </summary>
        /// <param name="configurationBaseRequest"></param>
        /// <returns></returns>
        List<DuplicateEntitiesListModel> CheckDuplicateEntities(DuplicateEntitiesRequestModel duplicateEntitiesRequest);

        /// <summary>
        /// Import solution from path 
        /// </summary>
        /// <param name="configurationBaseRequest"></param>
        /// <returns></returns>
        ImportSolutionResponseModel ImportSolutionFromPath(ImportSolutionRequestModel importSolutionRequestModel);

        /// <summary>
        /// Export solution to path 
        /// </summary>
        /// <param name="configurationBaseRequest"></param>
        /// <returns></returns>
        bool ExportSolutionToPath(ExportSolutionRequestModel request);

        /// <summary>
        /// saves selected entities into Rsp file
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        bool SaveRspFille(ExportSolutionRequestModel requestModel);

        /// <summary>
        /// opens selected entities into Rsp file
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        List<EntityInfoModel> OpenRspFille(ConfigurationBaseRequest requestModel);
    }
    #endregion
    public class ImportExportSolutionService : IImportExportSolutionService
    {
        private readonly IApplicationContextService _applicationContextService;
        private readonly Administrator _administrator;

        public ImportExportSolutionService(IApplicationContextService applicationContextService, IApplicationContext applicationContext)
        {
            _applicationContextService = applicationContextService;
            _administrator = applicationContext.Administrator;
        }

        #region Import Solution

        /// <summary>
        /// Returns list of Duplicate entities while import solution
        /// </summary>
        /// <param name="configurationBaseRequest"></param>
        /// <returns></returns>
        public List<GetListResponseModel> GetList(ConfigurationBaseRequest configurationBaseRequest)
        {
            List<GetListResponseModel> responseModels = new List<GetListResponseModel>();
            ESBConfiguration configuration = null;
            if (string.IsNullOrEmpty(configurationBaseRequest.ConfigurationPath))
            {
                configuration = _administrator.Configuration;
            }
            else
            {
                configuration = GetConfiguration(configurationBaseRequest.ConfigurationPath);
            }
            if (configuration.IsEncrypted)
            {
                return null;
            }

            // Ds : Update the response.

            #region Messaging
            if(configuration.Topics.Count == 0)
			{
                responseModels.Add(new GetListResponseModel("Topic", "Topics", "", "Messaging", "",null,""));
			}
            foreach (var topic in configuration.Topics)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(topic.Value);
				responseModels.Add(new GetListResponseModel("Topic", "Topics", topic.Key, "Messaging", topic.Value.Group, dependedEntities,"Topic"));
            }

            if (configuration.Subscribers.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("Party", "Publishers/Subscribers", "", "Messaging", "",null,""));
            }
            foreach (var subscriber in configuration.Subscribers)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(subscriber.Value);
                string role = String.Empty;
                if(subscriber.Value.PublisherRole && subscriber.Value.SubscriberRole)
                {
                    role = "Publisher/Subscriber";
                }
                else if(subscriber.Value.PublisherRole)
                {
                    role = "Publisher";
                }
                else
                {
                    role = "Subscriber";
                }
                responseModels.Add(new GetListResponseModel("Party", "Publishers/Subscribers", subscriber.Key, "Messaging", subscriber.Value.Group, dependedEntities, role));
            }

            if (configuration.MessagePatterns.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("Condition", "Conditions", "", "Messaging", "",null, "Condition"));
            }
            foreach (var messagePattern in configuration.MessagePatterns)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(messagePattern.Value);
                responseModels.Add(new GetListResponseModel("Condition", "Conditions", messagePattern.Key, "Messaging", messagePattern.Value.Group, dependedEntities, "Condition"));
            }
            #endregion

            #region Data
            if (configuration.Schemas.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("XML Schema", "XML Schemas", "", "Data", "", null, ""));
            }
            foreach (var xmlSchema in configuration.Schemas)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(xmlSchema.Value);
                responseModels.Add(new GetListResponseModel("XML Schema", "XML Schemas", xmlSchema.Key, "Data", xmlSchema.Value.Group, dependedEntities, "XML Schema"));
            }

            if (configuration.Xslts.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("XSLT document", "XSLT Documents", "", "Data", "",null,""));
            }
            foreach (var xmlTransformation in configuration.Xslts)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(xmlTransformation.Value);
                responseModels.Add(new GetListResponseModel("XSLT document", "XSLT Documents", xmlTransformation.Key, "Data", xmlTransformation.Value.Group, dependedEntities, "XSLT document"));
            }

            if (configuration.XmlDocs.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("Xml document", "XML Documents", "", "Data", "",null,""));
            }
            foreach (var xmlDocument in configuration.XmlDocs)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(xmlDocument.Value);
                responseModels.Add(new GetListResponseModel("Xml document", "XML Documents", xmlDocument.Key, "Data", xmlDocument.Value.Group, dependedEntities,"Xml document"));
            }

            if (configuration.TextDocs.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("Text document", "Text Documents", "", "Data", "", null, ""));
            }
            foreach (var textDocument in configuration.TextDocs)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(textDocument.Value);
                responseModels.Add(new GetListResponseModel("Text document", "Text Documents", textDocument.Key, "Data", textDocument.Value.Group, dependedEntities, "Text document"));
            }

            if (configuration.JsonDocs.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("Json document", "Json Documents", "", "Data", "",null,""));
            }
            foreach (var jsonDocument in configuration.JsonDocs)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(jsonDocument.Value);
                responseModels.Add(new GetListResponseModel("Json document", "Json Documents", jsonDocument.Key, "Data", jsonDocument.Value.Group, dependedEntities, "Json document"));
            }

            if (configuration.WsdlDocuments.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("Xsdl Document", "Xsdl Documents", "", "Data", "", null, ""));
            }
            foreach (var wsdlDocument in configuration.WsdlDocuments)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(wsdlDocument.Value);
                responseModels.Add(new GetListResponseModel("Xsdl Document", "Xsdl Documents", wsdlDocument.Key, "Data", wsdlDocument.Value.Group, dependedEntities, "Xsdl Document"));
            }

            if (configuration.SwaggerDocs.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("Swagger document", "Swagger Documents", "", "Data", "", null, ""));
            }
            foreach (var swaggerDoc in configuration.SwaggerDocs)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(swaggerDoc.Value);
                responseModels.Add(new GetListResponseModel("Swagger document", "Swagger Documents", swaggerDoc.Key, "Data", swaggerDoc.Value.Group, dependedEntities, "Swagger document"));
            }

            if (configuration.DataMappers.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("DataMapper", "Data Mappers", "", "Data", "", null, ""));
            }
            foreach (var dataMap in configuration.DataMappers)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(dataMap.Value);
                responseModels.Add(new GetListResponseModel("DataMapper", "Data Mappers", dataMap.Key, "Data", dataMap.Value.Group, dependedEntities, "DataMapper"));
            }
            #endregion

            #region Connections
            if (configuration.Adapters.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("Adapter", "Adapters", "", "Connections", "", null, ""));
            }
            foreach (var adapter in configuration.Adapters)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(adapter.Value);
                responseModels.Add(new GetListResponseModel("Adapter", "Adapters", adapter.Key, "Connections", adapter.Value.Group, dependedEntities, "Adapter"));
            }

            if (configuration.Bindings.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("Binding", "Bindings", "", "Connections", "", null, ""));
            }
            foreach (var binding in configuration.Bindings)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(binding.Value);
                responseModels.Add(new GetListResponseModel("Binding", "Bindings", binding.Key, "Connections", binding.Value.Group, dependedEntities, "Binding"));
            }

            if (configuration.Behaviors.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("Behaviors", "Behaviors", "", "Connections", "", null, ""));
            }
            foreach (var behavior in configuration.Behaviors)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(behavior.Value);
                responseModels.Add(new GetListResponseModel("Behaviors", "Behaviors", behavior.Key, "Connections", behavior.Value.Group, dependedEntities, "Behaviors"));
            }

            if (configuration.Services.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("Service endpoint", "Service Endpoints", "", "Connections", "", null, ""));
            }
            foreach (var serviceEndpoint in configuration.Services)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(serviceEndpoint.Value);
                responseModels.Add(new GetListResponseModel("Service endpoint", "Service Endpoints", serviceEndpoint.Key, "Connections", serviceEndpoint.Value.Group, dependedEntities, "Service endpoint"));
            }

            if (configuration.Endpoints.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("Endpoint", "Adapter Endpoints", "", "Connections", "", null, ""));
            }
            foreach (var adapterEndpoint in configuration.Endpoints)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(adapterEndpoint.Value);
                responseModels.Add(new GetListResponseModel("Endpoint", "Adapter Endpoints", adapterEndpoint.Key, "Connections", adapterEndpoint.Value.Group, dependedEntities, "Endpoint"));
            }

            if (configuration.WorkflowHosts.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("WorkflowEndpoint", "Workflow Endpoints", "", "Connections", "", null, ""));
            }
            foreach (var workflowEndpoint in configuration.WorkflowHosts)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(workflowEndpoint.Value);
                responseModels.Add(new GetListResponseModel("WorkflowEndpoint", "Workflow Endpoints", workflowEndpoint.Key, "Connections",workflowEndpoint.Value.Group, dependedEntities, "WorkflowEndpoint"));
            }

            if (configuration.ServicePolicies.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("Service Policy", "Service Policies", "", "Connections", "", null, ""));
            }
            foreach (var servicePolicy in configuration.ServicePolicies)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(servicePolicy.Value);
                responseModels.Add(new GetListResponseModel("Service Policy", "Service Policies", servicePolicy.Key, "Connections", servicePolicy.Value.Group, dependedEntities, "Service Policy"));
            }

            if (configuration.AdapterPolicies.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("Adapter Policy", "Adapter Policies", "", "Connections", "", null, ""));
            }
            foreach (var adapterPolicy in configuration.AdapterPolicies)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(adapterPolicy.Value);
                responseModels.Add(new GetListResponseModel("Adapter Policy", "Adapter Policies", adapterPolicy.Key, "Connections", adapterPolicy.Value.Group, dependedEntities, "Adapter Policy"));
            }

            if (configuration.ServiceRoutingTables.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("ServiceRoutingTable", "Service Route Tables", "", "Connections", "", null, ""));
            }
            foreach (var serviceRoute in configuration.ServiceRoutingTables)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(serviceRoute.Value);
                responseModels.Add(new GetListResponseModel("ServiceRoutingTable", "Service Route Tables", serviceRoute.Key, "Connections", serviceRoute.Value.Group, dependedEntities, "ServiceRoutingTable"));
            }
            #endregion

            #region Security
            if (configuration.Credentials.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("Credentials", "Credentials", "", "Security", "",null, ""));
            }
            foreach (var credential in configuration.Credentials)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(credential.Value);
                responseModels.Add(new GetListResponseModel("Credentials", "Credentials", credential.Key, "Security", credential.Value.Group, dependedEntities, "Credentials"));
            }

            if (configuration.AccessControlLists.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("Access Control List", "Access Control Lists", "", "Security", "",null,""));
            }
            foreach (var accessControlList in configuration.AccessControlLists)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(accessControlList.Value);
                responseModels.Add(new GetListResponseModel("Access Control List", "Access Control Lists", accessControlList.Key, "Security", accessControlList.Value.Group, dependedEntities, "Access Control List"));
            }

            if (configuration.Administrators.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("Administrator", "Administrators", "", "Security", "",null,""));
            }
            foreach (var administrator in configuration.Administrators)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(administrator.Value);
                responseModels.Add(new GetListResponseModel("Administrator", "Administrators", administrator.Key, "Security", administrator.Value.Group, dependedEntities, "Administrator"));
            }

            if (configuration.Keys.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("Key", "Keys", "", "Security", "",null, ""));
            }
            foreach (var key in configuration.Keys)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(key.Value);
                responseModels.Add(new GetListResponseModel("Key", "Keys", key.Key, "Security", key.Value.Group, dependedEntities, "Key"));
            }

            if (configuration.OAuthProviders.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("OAuthProviders", "OAuth Providers", "", "Security", "",null, ""));
            }
            foreach (var oauthProvider in configuration.OAuthProviders)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(oauthProvider.Value);
                responseModels.Add(new GetListResponseModel("OAuthProviders", "OAuth Providers", oauthProvider.Key, "Security", oauthProvider.Value.Group, dependedEntities, "OAuthProviders"));
            }
            #endregion

            #region Processes
            if (configuration.ESBMessagePipelines.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("ProcessFlow", "All ProcessFlows", "", "Processes", "", null, ""));
                responseModels.Add(new GetListResponseModel("ProcessFragment", "All ProcessFragments", "", "Processes", "", null, ""));
            }
            foreach (var process in configuration.ESBMessagePipelines)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(process.Value);
                if (process.Value.IsProcessFlow)
                {
                    responseModels.Add(new GetListResponseModel("ProcessFlow", "All ProcessFlows", process.Key, "Processes", process.Value.Group, dependedEntities, "ProcessFlow"));
                }
                else
                {
                    responseModels.Add(new GetListResponseModel("ProcessFragment", "All ProcessFragments", process.Value.Name, "Processes", process.Value.Group, dependedEntities, "ProcessFragment"));
                }
            }

            if (configuration.WorkflowDefinitions.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("Workflow", "All Workflows", "", "Processes", "", null, ""));
            }
            foreach (var workflow in configuration.WorkflowDefinitions)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(workflow.Value);
                responseModels.Add(new GetListResponseModel("Workflow", "All Workflows", workflow.Key, "Processes",workflow.Value.Group, dependedEntities, "Workflow"));
            }
            #endregion

            #region Deployment
            if (configuration.Zones.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("Zone", "Zones", "", "Deployment", "", null, ""));
            }
            foreach (var zone in configuration.Zones)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(zone.Value);
                responseModels.Add(new GetListResponseModel("Zone", "Zones", zone.Key, "Deployment", zone.Value.Group, dependedEntities, "Zone"));
            }

            if (configuration.Deployments.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("DeploymentGroup", "Deployment Groups", "", "Deployment", "", null, ""));
            }
            foreach (var deploymentGroup in configuration.Deployments)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(deploymentGroup.Value);
                responseModels.Add(new GetListResponseModel("DeploymentGroup", "Deployment Groups", deploymentGroup.Key, "Deployment", deploymentGroup.Value.Group, dependedEntities, "DeploymentGroup"));
            }

            if (configuration.AvailabilityGroups.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("AvailabilityGroup", "Endpoint Hosts", "", "Deployment", "", null, ""));
            }
            foreach (var availabilityGroup in configuration.AvailabilityGroups)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(availabilityGroup.Value);
                responseModels.Add(new GetListResponseModel("AvailabilityGroup", "Endpoint Hosts", availabilityGroup.Key, "Deployment", availabilityGroup.Value.Group, dependedEntities, "AvailabilityGroup"));
            }

            if (configuration.NeuronEnvironmentVariables.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("EnvironmentVariables", "Environment Variables", "", "Deployment", "", null, ""));
            }
            foreach (var environmentVariable in configuration.NeuronEnvironmentVariables)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(environmentVariable.Value);
                responseModels.Add(new GetListResponseModel("EnvironmentVariables", "Environment Variables", environmentVariable.Key, "Deployment", environmentVariable.Value.Group, dependedEntities, "EnvironmentVariables"));
            }

            if (configuration.Databases.Count == 0)
            {
                responseModels.Add(new GetListResponseModel("Databases", "Databases", "", "Deployment", "", null, ""));
            }
            foreach (var database in configuration.Databases)
            {
                List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(database.Value);
                responseModels.Add(new GetListResponseModel("Databases", "Databases", database.Key, "Deployment", database.Value.Group, dependedEntities, "Databases"));
            }
            #endregion

   //         if (configuration.Bridges.Count == 0)
			//{
			//	responseModels.Add(new GetListResponseModel("Bridge", "", ""));
			//}
			//foreach (var bridge in configuration.Bridges)
			//{
   //             List<DependencyInfo> dependedEntities = _administrator.GetDependentEntitiesList(bridge.Value);
   //             responseModels.Add(new GetListResponseModel("Bridge", bridge.Key, bridge.Value.Group, dependedEntities));
			//}

			return responseModels;
        }

        /// <summary>
        /// Returns list of Duplicate entities while import solution
        /// </summary>
        /// <param name="configurationBaseRequest"></param>
        /// <returns></returns>
        public List<DuplicateEntitiesListModel> CheckDuplicateEntities(DuplicateEntitiesRequestModel duplicateEntitiesRequest)
        {
            List<DuplicateEntitiesListModel> duplicateEntitiesList = new List<DuplicateEntitiesListModel>();
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;

            foreach (var entity in duplicateEntitiesRequest.EntitiesList)
            {
                switch (entity.InfoModel.EntityTypeDisplayName)
                {
                    case "Zone":
                        if (contextAdministrator.Configuration.Zones.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "Bridge":
                        if (contextAdministrator.Configuration.Bridges.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "DeploymentGroup":
                        if (contextAdministrator.Configuration.Deployments.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "EnvironmentVariables":
                        if (contextAdministrator.Configuration.NeuronEnvironmentVariables.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "Databases":
                        if (contextAdministrator.Configuration.Databases.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "Topic":
                        if (contextAdministrator.Configuration.Topics.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "Party":
                        if (contextAdministrator.Configuration.Subscribers.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "Condition":
                        if (contextAdministrator.Configuration.MessagePatterns.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "ProcessFlow":
                    case "ProcessFragment":
                        var isProcessFlow = false;

                        if (entity.InfoModel.EntityTypeDisplayName == "ProcessFlow")
                        {
                            isProcessFlow = true;
                        }
                        if (contextAdministrator.Configuration.ESBMessagePipelines.Where(e => e.Value.Name == entity.InfoModel.EntityName && e.Value.IsProcessFlow == isProcessFlow).Any())
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "Workflow":
                        if (contextAdministrator.Configuration.WorkflowDefinitions.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "AvailabilityGroup":
                        if (contextAdministrator.Configuration.AvailabilityGroups.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "WorkflowEndpoint":
                        if (contextAdministrator.Configuration.WorkflowHosts.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "DataMapper":
                        if (contextAdministrator.Configuration.DataMappers.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "XML Schema":
                        if (contextAdministrator.Configuration.Schemas.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "XSLT document":
                        if (contextAdministrator.Configuration.Xslts.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "Xml document":
                        if (contextAdministrator.Configuration.XmlDocs.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "Text document":
                        if (contextAdministrator.Configuration.TextDocs.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "Json document":
                        if (contextAdministrator.Configuration.JsonDocs.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "Xsdl document":
                        if (contextAdministrator.Configuration.WsdlDocuments.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "Swagger document":
                        if (contextAdministrator.Configuration.SwaggerDocs.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "OAuthProviders":
                        if (contextAdministrator.Configuration.OAuthProviders.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "Adapter":
                        if (contextAdministrator.Configuration.Adapters.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "Binding":
                        if (contextAdministrator.Configuration.Bindings.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "Behaviors":
                        if (contextAdministrator.Configuration.Behaviors.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "Service endpoint":
                        if (contextAdministrator.Configuration.Services.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "ServiceRoutingTable":
                        if (contextAdministrator.Configuration.ServiceRoutingTables.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "Endpoint":
                        if (contextAdministrator.Configuration.Endpoints.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "Service Policy":
                        if (contextAdministrator.Configuration.ServicePolicies.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "Adapter Policy":
                        if (contextAdministrator.Configuration.AdapterPolicies.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "Credentials":
                        if (contextAdministrator.Configuration.Credentials.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "Access Control List":
                        if (contextAdministrator.Configuration.AccessControlLists.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "Administrator":
                        if (contextAdministrator.Configuration.Administrators.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;

                    case "Key":
                        if (contextAdministrator.Configuration.Keys.ContainsKey(entity.InfoModel.EntityName))
                        {
                            duplicateEntitiesList.Add(new DuplicateEntitiesListModel() { InfoModel = entity.InfoModel });
                        }
                        break;
                }
            }
            return duplicateEntitiesList;
        }

        /// <summary>
        /// Import solution from path 
        /// </summary>
        /// <param name="configurationBaseRequest"></param>
        /// <returns></returns>
        public ImportSolutionResponseModel ImportSolutionFromPath(ImportSolutionRequestModel importSolutionRequestModel)
        {
            ImportSolutionResponseModel responseModel = new ImportSolutionResponseModel();
            //returns EsbConfiguration based on Entities list which user wants to import
            var configuration = GetConfigurationFromEntitiesList(importSolutionRequestModel.ConfigurationPath, importSolutionRequestModel.EntitiesList);
            if (configuration.IsEncrypted)
            {
                return null;
            }
            responseModel.ValidationsErrors = new List<ValidationsErrorList>();
            var importContext = new ImportContext() { Configuration = configuration, EntitiesList = importSolutionRequestModel.EntitiesList, DuplicateEntitiesList = importSolutionRequestModel.DuplicateEntitiesList, ValidationsErrors = importSolutionRequestModel.ValidationsErrors };
            //returns list of validation errors if any while importing entities
            responseModel.ValidationsErrors = AddEntitiesinConfig(importContext.EntitiesList.Select(e => e.InfoModel).ToList(), importContext, importSolutionRequestModel.IsSaveWithError);
            var altered = false;
            responseModel.Issues = new List<ConfigurationError>();
            //returns list of issues if there is any upgrade
            ChannelMaintenance.UpgradeChannelAssemblyInformation(configuration, responseModel.Issues, ref altered);
            return responseModel;
        }

        /// <summary>          
        /// returns list of validation errors if any while importing entities
        /// </summary>
        /// <param name="infoModels"></param>
        /// <param name="importContext"></param>
        /// <param name="isErrorSave"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> AddEntitiesinConfig(List<EntityInfoModel> infoModels, ImportContext importContext, bool isErrorSave)
        {
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();
            ImportContext context = new ImportContext();
            context = importContext;
            if (importContext.ValidationsErrors != null)
            {
                if (infoModels.Where(e => e.EntityTypeDisplayName == "Zone").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Zone" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Zone" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportZones(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "Bridge").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Bridge" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Bridge" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportBridges(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "DeploymentGroup").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "DeploymentGroup" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "DeploymentGroup" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportDeploymentGroups(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "EnvironmentVariables").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "EnvironmentVariables" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "EnvironmentVariables" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportEnvironmentVariables(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "Databases").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Databases" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Databases" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportDatabases(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "Topic").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Topic" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Topic" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportTopics(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "Party").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Party" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Party" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportSubscribers(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "Condition").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Condition" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Condition" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportMessagePatterns(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "ProcessFlow" || e.EntityTypeDisplayName == "ProcessFragment").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => (e.InfoModel.EntityTypeDisplayName == "ProcessFlow" || e.InfoModel.EntityTypeDisplayName == "ProcessFragment") && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => (e.InfoModel.EntityTypeDisplayName == "ProcessFlow" || e.InfoModel.EntityTypeDisplayName == "ProcessFragment") && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportProcesses(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "Workflow").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Workflow" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Workflow" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportWorkflows(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "AvailabilityGroup").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "AvailabilityGroup" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "AvailabilityGroup" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportAvailabilityGroups(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "WorkflowEndpoint").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "WorkflowEndpoint" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "WorkflowEndpoint" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportWorkflowEndpoints(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "DataMapper").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "DataMapper" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "DataMapper" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportDataMaps(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "XML Schema").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "XML Schema" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "XML Schema" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportXmlSchemas(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "XSLT document").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "XSLT document" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "XSLT document" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportXmlTransforms(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "Xml document").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Xml document" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Xml document" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportXmlDocuments(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "Text document").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Text document" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Text document" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportTextDocuments(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "Json document").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Json document" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Json document" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportJsonDocuments(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "Xsdl document").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Xsdl document" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Xsdl document" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportWsdlDocuments(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "Swagger document").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Swagger document" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Swagger document" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportSwaggerDocuments(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "OAuthProviders").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "OAuthProviders" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "OAuthProviders" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportOauthProviders(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "Adapter").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Adapter" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Adapter" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportAdapters(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "Binding").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Binding" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Binding" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportBindings(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "Behaviors").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Behaviors" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Behaviors" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportBehaviors(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "Service endpoint").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Service endpoint" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Service endpoint" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportServiceEndpoints(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "ServiceRoutingTable").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "ServiceRoutingTable" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "ServiceRoutingTable" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportServiceRoutingTables(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "Endpoint").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Endpoint" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Endpoint" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportAdapterEndpoints(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "Service Policy").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Service Policy" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Service Policy" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportServicePolicies(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "Adapter Policy").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Adapter Policy" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Adapter Policy" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportAdapterPolicies(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "Credentials").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Credentials" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Credentials" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportCredentials(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "Access Control List").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Access Control List" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Access Control List" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportAccessControlLists(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "Administrator").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Administrator" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Administrator" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportAdministrators(context));
                    }
                }
                if (infoModels.Where(e => e.EntityTypeDisplayName == "Key").Any())
                {
                    if ((isErrorSave && importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Key" && e.AddinImport).Any()) || !isErrorSave)
                    {
                        context.ValidationsErrors = importContext.ValidationsErrors.Where(e => e.InfoModel.EntityTypeDisplayName == "Key" && e.AddinImport).ToList();
                        validationsErrors.AddRange(ImportKeys(context));
                    }
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// returns configuration based on path
        /// </summary>
        /// <param name="configurationPath"></param>
        /// <returns></returns>
        private ESBConfiguration GetConfiguration(string configurationPath)
        {
            var issues = new List<ConfigurationError>();
            var metabase = new ESBMetabase(configurationPath, issues, true);
            return metabase.Configuration;
        }

        /// <summary>
        /// returns EsbConfiguration based on Entities list which user wants to import
        /// </summary>
        /// <param name="importSolutionRequestModel"></param>
        /// <returns></returns>
        private ESBConfiguration GetConfigurationFromEntitiesList(string configurationPath, List<GetListResponseModel> entitiesList)
        {
            var contextConfiguration = GetConfiguration(configurationPath);
            ESBConfiguration configuration = new ESBConfiguration();
            foreach (var entity in entitiesList)
            {
                if (!string.IsNullOrEmpty(entity.InfoModel.EntityName))
                {
                    switch (entity.InfoModel.EntityTypeDisplayName)
                    {
                        case "Zone":
                            if (contextConfiguration.Zones.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.Zones.Add(entity.InfoModel.EntityName, contextConfiguration.Zones[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "Bridge":
                            if (contextConfiguration.Bridges.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.Bridges.Add(entity.InfoModel.EntityName, contextConfiguration.Bridges[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "DeploymentGroup":
                            if (contextConfiguration.Deployments.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.Deployments.Add(entity.InfoModel.EntityName, contextConfiguration.Deployments[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "EnvironmentVariables":
                            if (contextConfiguration.NeuronEnvironmentVariables.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.NeuronEnvironmentVariables.Add(entity.InfoModel.EntityName, contextConfiguration.NeuronEnvironmentVariables[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "Databases":
                            if (contextConfiguration.Databases.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.Databases.Add(entity.InfoModel.EntityName, contextConfiguration.Databases[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "Topic":
                            if (contextConfiguration.Topics.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.Topics.Add(entity.InfoModel.EntityName, contextConfiguration.Topics[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "Party":
                            if (contextConfiguration.Subscribers.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.Subscribers.Add(entity.InfoModel.EntityName, contextConfiguration.Subscribers[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "Condition":
                            if (contextConfiguration.MessagePatterns.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.MessagePatterns.Add(entity.InfoModel.EntityName, contextConfiguration.MessagePatterns[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "ProcessFlow":
                        case "ProcessFragment":
                            var process = contextConfiguration.ESBMessagePipelines.Where(e => e.Value.Name == entity.InfoModel.EntityName).Select(e => e.Value).FirstOrDefault();
                            if (process != null)
                            {
                                configuration.ESBMessagePipelines.Add(process.Id, process);
                            }
                            break;

                        case "Workflow":
                            if (contextConfiguration.WorkflowDefinitions.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.WorkflowDefinitions.Add(entity.InfoModel.EntityName, contextConfiguration.WorkflowDefinitions[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "AvailabilityGroup":
                            if (contextConfiguration.AvailabilityGroups.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.AvailabilityGroups.Add(entity.InfoModel.EntityName, contextConfiguration.AvailabilityGroups[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "WorkflowEndpoint":
                            if (contextConfiguration.WorkflowHosts.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.WorkflowHosts.Add(entity.InfoModel.EntityName, contextConfiguration.WorkflowHosts[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "DataMapper":
                            if (contextConfiguration.DataMappers.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.DataMappers.Add(entity.InfoModel.EntityName, contextConfiguration.DataMappers[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "XML Schema":
                            if (contextConfiguration.Schemas.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.Schemas.Add(entity.InfoModel.EntityName, contextConfiguration.Schemas[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "XSLT document":
                            if (contextConfiguration.Xslts.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.Xslts.Add(entity.InfoModel.EntityName, contextConfiguration.Xslts[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "Xml document":
                            if (contextConfiguration.XmlDocs.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.XmlDocs.Add(entity.InfoModel.EntityName, contextConfiguration.XmlDocs[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "Text document":
                            if (contextConfiguration.TextDocs.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.TextDocs.Add(entity.InfoModel.EntityName, contextConfiguration.TextDocs[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "Json document":
                            if (contextConfiguration.JsonDocs.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.JsonDocs.Add(entity.InfoModel.EntityName, contextConfiguration.JsonDocs[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "Xsdl document":
                            if (contextConfiguration.WsdlDocuments.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.WsdlDocuments.Add(entity.InfoModel.EntityName, contextConfiguration.WsdlDocuments[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "Swagger document":
                            if (contextConfiguration.SwaggerDocs.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.SwaggerDocs.Add(entity.InfoModel.EntityName, contextConfiguration.SwaggerDocs[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "OAuthProviders":
                            if (contextConfiguration.OAuthProviders.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.OAuthProviders.Add(entity.InfoModel.EntityName, contextConfiguration.OAuthProviders[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "Adapter":
                            if (contextConfiguration.Adapters.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.Adapters.Add(entity.InfoModel.EntityName, contextConfiguration.Adapters[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "Binding":
                            if (contextConfiguration.Bindings.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.Bindings.Add(entity.InfoModel.EntityName, contextConfiguration.Bindings[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "Behaviors":
                            if (contextConfiguration.Behaviors.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.Behaviors.Add(entity.InfoModel.EntityName, contextConfiguration.Behaviors[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "Service endpoint":
                            if (contextConfiguration.Services.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.Services.Add(entity.InfoModel.EntityName, contextConfiguration.Services[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "ServiceRoutingTable":
                            if (contextConfiguration.ServiceRoutingTables.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.ServiceRoutingTables.Add(entity.InfoModel.EntityName, contextConfiguration.ServiceRoutingTables[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "Endpoint":
                            if (contextConfiguration.Endpoints.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.Endpoints.Add(entity.InfoModel.EntityName, contextConfiguration.Endpoints[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "Service Policy":
                            if (contextConfiguration.ServicePolicies.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.ServicePolicies.Add(entity.InfoModel.EntityName, contextConfiguration.ServicePolicies[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "Adapter Policy":
                            if (contextConfiguration.AdapterPolicies.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.AdapterPolicies.Add(entity.InfoModel.EntityName, contextConfiguration.AdapterPolicies[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "Credentials":
                            if (contextConfiguration.Credentials.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.Credentials.Add(entity.InfoModel.EntityName, contextConfiguration.Credentials[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "Access Control List":
                            if (contextConfiguration.AccessControlLists.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.AccessControlLists.Add(entity.InfoModel.EntityName, contextConfiguration.AccessControlLists[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "Administrator":
                            if (contextConfiguration.Administrators.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.Administrators.Add(entity.InfoModel.EntityName, contextConfiguration.Administrators[entity.InfoModel.EntityName]);
                            }
                            break;

                        case "Key":
                            if (contextConfiguration.Keys.ContainsKey(entity.InfoModel.EntityName))
                            {
                                configuration.Keys.Add(entity.InfoModel.EntityName, contextConfiguration.Keys[entity.InfoModel.EntityName]);
                            }
                            break;
                    }
                }
            }
            return configuration;
        }

        /// <summary>
        /// imports Keys in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportKeys(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var key in configuration.Keys)
                {
                    if (contextAdministrator.Configuration.Keys.ContainsKey(key.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "Key" && e.InfoModel.EntityName == key.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.Keys[key.Key] = key.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision("Edit,*,key," + key.Value.Name);
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.AddKey(key.Value);
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("Key", key.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.Keys[error.InfoModel.EntityName] = configuration.Keys[error.InfoModel.EntityName];
                    contextAdministrator.Configuration.ChangeList.AddAdition("Add,*,key," + error.InfoModel.EntityName);
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports Administartors in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportAdministrators(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var administrator in configuration.Administrators)
                {
                    if (contextAdministrator.Configuration.Administrators.ContainsKey(administrator.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "Administrator" && e.InfoModel.EntityName == administrator.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.Administrators[administrator.Key] = administrator.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                string.Format(CultureInfo.InvariantCulture, "Edit,*,Administrator,{0}", administrator.Value.Name));
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.AddAdministrator(administrator.Value);
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("Administrator", administrator.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.Administrators[error.InfoModel.EntityName] = configuration.Administrators[error.InfoModel.EntityName];
                    contextAdministrator.Configuration.ChangeList.AddAdition("Add,*,Administrator," + error.InfoModel.EntityName);
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports Access Control Lists in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportAccessControlLists(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var accessControlList in configuration.AccessControlLists)
                {
                    if (contextAdministrator.Configuration.AccessControlLists.ContainsKey(accessControlList.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "Access Control List" && e.InfoModel.EntityName == accessControlList.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.AccessControlLists[accessControlList.Key] = accessControlList.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                "Edit,*,access control list," + accessControlList.Value.Name);
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.AddAccessControlList(accessControlList.Value);
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("Access Control List", accessControlList.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.AccessControlLists[error.InfoModel.EntityName] = configuration.AccessControlLists[error.InfoModel.EntityName];
                    contextAdministrator.Configuration.ChangeList.AddAdition("Add,*,access control list," + error.InfoModel.EntityName);
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports Bridges in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportBridges(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var bridge in configuration.Bridges)
                {
                    if (contextAdministrator.Configuration.Bridges.ContainsKey(bridge.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "Bridge" && e.InfoModel.EntityName == bridge.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.Bridges[bridge.Key] = bridge.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                string.Format(CultureInfo.InvariantCulture, "Edit,*,bridge,{0}", bridge.Value.Name));
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.AddBridge(bridge.Value);
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("Bridge", bridge.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.Bridges[error.InfoModel.EntityName] = configuration.Bridges[error.InfoModel.EntityName];
                    contextAdministrator.Configuration.ChangeList.AddAdition("Add,*,bridge," + error.InfoModel.EntityName);
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports Zones in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportZones(ImportContext context)
        {
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var zone in configuration.Zones)
                {
                    if (contextAdministrator.Configuration.Zones.ContainsKey(zone.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "Zone" && e.InfoModel.EntityName == zone.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.Zones[zone.Key] = zone.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                string.Format(CultureInfo.InvariantCulture, "Edit,*,zone,{0}", zone.Value.Name));
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.AddZone(zone.Value);
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("Zone", zone.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.Zones[error.InfoModel.EntityName] = configuration.Zones[error.InfoModel.EntityName];
                    contextAdministrator.Configuration.ChangeList.AddAdition("Add,*,zone," + error.InfoModel.EntityName);
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports Environment Variables in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportEnvironmentVariables(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var environmentVariable in configuration.NeuronEnvironmentVariables)
                {
                    if (contextAdministrator.Configuration.NeuronEnvironmentVariables.ContainsKey(environmentVariable.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "EnvironmentVariables" && e.InfoModel.EntityName == environmentVariable.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.NeuronEnvironmentVariables[environmentVariable.Key] =
                            environmentVariable.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                string.Format(
                                    CultureInfo.InvariantCulture, "Edit,*,variable,{0}", environmentVariable.Value.Name));
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.AddNeuronEnvironmentVariable(environmentVariable.Value);
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("EnvironmentVariables", environmentVariable.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.NeuronEnvironmentVariables[error.InfoModel.EntityName] = configuration.NeuronEnvironmentVariables[error.InfoModel.EntityName];
                    contextAdministrator.Configuration.ChangeList.AddAdition("Add,*,variable," + error.InfoModel.EntityName);
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports deployment Groups in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportDeploymentGroups(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var deploymentGroup in configuration.Deployments)
                {
                    if (contextAdministrator.Configuration.Deployments.ContainsKey(deploymentGroup.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "DeploymentGroup" && e.InfoModel.EntityName == deploymentGroup.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.Deployments[deploymentGroup.Key] = deploymentGroup.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    "Edit,{0},deployment group,{1}",
                                    deploymentGroup.Value.Zone,
                                    deploymentGroup.Value.Name));
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.AddDeployment(deploymentGroup.Value);
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("DeploymentGroup", deploymentGroup.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.Deployments[error.InfoModel.EntityName] = configuration.Deployments[error.InfoModel.EntityName];
                    contextAdministrator.Configuration.ChangeList.AddAdition(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Add,{0},deployment group,{1}",
                        configuration.Deployments[error.InfoModel.EntityName].Zone,
                        error.InfoModel.EntityName));
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports Topics in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportTopics(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var topic in configuration.Topics)
                {
                    if (contextAdministrator.Configuration.Topics.ContainsKey(topic.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "Topic" && e.InfoModel.EntityName == topic.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.Topics[topic.Key] = topic.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    "Edit,{0},topic,{1}",
                                    topic.Value.Zone,
                                    topic.Value.Name));
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.AddTopic(topic.Value);
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("Topic", topic.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.Topics[error.InfoModel.EntityName] = configuration.Topics[error.InfoModel.EntityName];
                    contextAdministrator.Configuration.ChangeList.AddAdition("Add," + configuration.Topics[error.InfoModel.EntityName].Zone + ",topic," + error.InfoModel.EntityName);
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports Databases in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportDatabases(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var database in configuration.Databases)
                {
                    if (contextAdministrator.Configuration.Databases.ContainsKey(database.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "Databases" && e.InfoModel.EntityName == database.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.Databases[database.Key] = database.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    "Edit,{0},database,{1}",
                                    database.Value.Zone,
                                    database.Value.GetDictionaryKey()));
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.AddDatabase(database.Value);
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("Databases", database.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.Databases[error.InfoModel.EntityName] = configuration.Databases[error.InfoModel.EntityName];
                    contextAdministrator.Configuration.ChangeList.AddAdition("Add," + configuration.Databases[error.InfoModel.EntityName].Zone + ",database," + error.InfoModel.EntityName);
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports Credentials in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportCredentials(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var credential in configuration.Credentials)
                {
                    if (contextAdministrator.Configuration.Credentials.ContainsKey(credential.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "Credentials" && e.InfoModel.EntityName == credential.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.Credentials[credential.Key] = credential.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                "Edit,*,credential," + credential.Value.Name);
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.AddCredential(credential.Value);
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("Credentials", credential.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.Credentials[error.InfoModel.EntityName] = configuration.Credentials[error.InfoModel.EntityName];
                    contextAdministrator.Configuration.ChangeList.AddAdition("Add,*,credential," + error.InfoModel.EntityName);
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports Adapter Policies in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportAdapterPolicies(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var adapterPolicy in configuration.AdapterPolicies)
                {
                    if (contextAdministrator.Configuration.AdapterPolicies.ContainsKey(adapterPolicy.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "Adapter Policy" && e.InfoModel.EntityName == adapterPolicy.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.AdapterPolicies[adapterPolicy.Key] = adapterPolicy.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                "Edit,*,adapter policy," + adapterPolicy.Value.Name);
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.AddAdapterPolicy(adapterPolicy.Value);
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("Adapter Policy", adapterPolicy.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.AdapterPolicies[error.InfoModel.EntityName] = configuration.AdapterPolicies[error.InfoModel.EntityName];
                    contextAdministrator.Configuration.ChangeList.AddAdition("Add,*,adapter policy," + error.InfoModel.EntityName);
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports Service Policies in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportServicePolicies(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var servicePolicy in configuration.ServicePolicies)
                {
                    if (contextAdministrator.Configuration.ServicePolicies.ContainsKey(servicePolicy.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "Service Policy" && e.InfoModel.EntityName == servicePolicy.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.ServicePolicies[servicePolicy.Key] = servicePolicy.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                "Edit,*,service policy," + servicePolicy.Value.Name);
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.AddServicePolicy(servicePolicy.Value);
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("Service Policy", servicePolicy.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.ServicePolicies[error.InfoModel.EntityName] = configuration.ServicePolicies[error.InfoModel.EntityName];
                    contextAdministrator.Configuration.ChangeList.AddAdition(
                        "Add,*,service policy," + error.InfoModel.EntityName);
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports Adapter Endpoints in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportAdapterEndpoints(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var adapterEndpoint in configuration.Endpoints)
                {
                    if (contextAdministrator.Configuration.Endpoints.ContainsKey(adapterEndpoint.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "Endpoint" && e.InfoModel.EntityName == adapterEndpoint.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.Endpoints[adapterEndpoint.Key] = adapterEndpoint.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    "Edit,{0},endpoint,{1}",
                                    adapterEndpoint.Value.Zone,
                                    adapterEndpoint.Value.Name));
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.AddEndpoint(adapterEndpoint.Value);
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("Endpoint", adapterEndpoint.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.Endpoints[error.InfoModel.EntityName] = configuration.Endpoints[error.InfoModel.EntityName];
                    contextAdministrator.Configuration.ChangeList.AddAdition(
                        "Add," + configuration.Endpoints[error.InfoModel.EntityName].Zone + ",endpoint," + error.InfoModel.EntityName);
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports Service Routing Tables in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportServiceRoutingTables(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var serviceRoutingTable in configuration.ServiceRoutingTables)
                {
                    if (contextAdministrator.Configuration.ServiceRoutingTables.ContainsKey(serviceRoutingTable.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "ServiceRoutingTable" && e.InfoModel.EntityName == serviceRoutingTable.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.ServiceRoutingTables[serviceRoutingTable.Key] = serviceRoutingTable.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    "Edit,{0},serviceRoutingTable,{1}",
                                    serviceRoutingTable.Value.Zone,
                                    serviceRoutingTable.Value.Name));
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.AddServiceRoutingTable(serviceRoutingTable.Value);
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("ServiceRoutingTable", serviceRoutingTable.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.ServiceRoutingTables[error.InfoModel.EntityName] = configuration.ServiceRoutingTables[error.InfoModel.EntityName];
                    contextAdministrator.Configuration.ChangeList.AddAdition("Add," + configuration.ServiceRoutingTables[error.InfoModel.EntityName].Zone + ",serviceRoutingTable," + error.InfoModel.EntityName);
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports Service Endpoints in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportServiceEndpoints(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var serviceEndpoint in configuration.Services)
                {
                    if (contextAdministrator.Configuration.Services.ContainsKey(serviceEndpoint.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "Service endpoint" && e.InfoModel.EntityName == serviceEndpoint.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.Services[serviceEndpoint.Key] = serviceEndpoint.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    "Edit,{0},service endpoint,{1}",
                                    serviceEndpoint.Value.Zone,
                                    serviceEndpoint.Value.Name));
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.AddService(serviceEndpoint.Value);
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("Service endpoint", serviceEndpoint.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.Services[error.InfoModel.EntityName] = configuration.Services[error.InfoModel.EntityName];
                    contextAdministrator.Configuration.ChangeList.AddAdition("Add," + configuration.Services[error.InfoModel.EntityName].Zone + ",service endpoint," + error.InfoModel.EntityName);
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports Behaviors in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportBehaviors(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var behavior in configuration.Behaviors)
                {
                    if (contextAdministrator.Configuration.Behaviors.ContainsKey(behavior.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "Behaviors" && e.InfoModel.EntityName == behavior.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.Behaviors[behavior.Key] = behavior.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                "Edit,*,behavior," + behavior.Value.Name);
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.AddBehavior(behavior.Value);
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("Behaviors", behavior.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.Behaviors[error.InfoModel.EntityName] = configuration.Behaviors[error.InfoModel.EntityName];
                    contextAdministrator.Configuration.ChangeList.AddAdition("Add,*,behavior," + error.InfoModel.EntityName);
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports Bindings in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportBindings(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var binding in configuration.Bindings)
                {
                    if (contextAdministrator.Configuration.Bindings.ContainsKey(binding.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "Binding" && e.InfoModel.EntityName == binding.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.Bindings[binding.Key] = binding.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision("Edit,*,binding," + binding.Value.Name);
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.AddBinding(binding.Value);
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("Binding", binding.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.Bindings[error.InfoModel.EntityName] = configuration.Bindings[error.InfoModel.EntityName];
                    contextAdministrator.Configuration.ChangeList.AddAdition("Add,*,binding," + error.InfoModel.EntityName);
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports Adapters in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportAdapters(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var adapter in configuration.Adapters)
                {
                    if (contextAdministrator.Configuration.Adapters.ContainsKey(adapter.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "Adapter" && e.InfoModel.EntityName == adapter.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.Adapters[adapter.Key] = adapter.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                string.Format(CultureInfo.InvariantCulture, "Edit,*,adapter,{0}", adapter.Value.Name));
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.AddAdapter(adapter.Value);
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("Adapter", adapter.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.Adapters[error.InfoModel.EntityName] = configuration.Adapters[error.InfoModel.EntityName];
                    contextAdministrator.Configuration.ChangeList.AddAdition("Add,*,adapter," + error.InfoModel.EntityName);
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports Xml Documents in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportXmlDocuments(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var xmlDocument in configuration.XmlDocs)
                {
                    if (contextAdministrator.Configuration.XmlDocs.ContainsKey(xmlDocument.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "Xml document" && e.InfoModel.EntityName == xmlDocument.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.XmlDocs[xmlDocument.Key] = xmlDocument.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                string.Format(CultureInfo.InvariantCulture, "Edit,*,xml document,{0}", xmlDocument.Value.Name));
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.AddXmlDoc(xmlDocument.Value);
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("Xml document", xmlDocument.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.XmlDocs[error.InfoModel.EntityName] = configuration.XmlDocs[error.InfoModel.EntityName];
                    contextAdministrator.Configuration.ChangeList.AddAdition("Add,*,xml document," + error.InfoModel.EntityName);
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports Text Documents in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportTextDocuments(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var textDocument in configuration.TextDocs)
                {
                    if (contextAdministrator.Configuration.TextDocs.ContainsKey(textDocument.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "Text document" && e.InfoModel.EntityName == textDocument.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.TextDocs[textDocument.Key] = textDocument.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                string.Format(CultureInfo.InvariantCulture, "Edit,*,text document,{0}", textDocument.Value.Name));
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.AddTextDoc(textDocument.Value);
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("Text document", textDocument.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.TextDocs[error.InfoModel.EntityName] = configuration.TextDocs[error.InfoModel.EntityName];
                    contextAdministrator.Configuration.ChangeList.AddAdition("Add,*,text document," + error.InfoModel.EntityName);
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports Json Documents in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportJsonDocuments(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var jsonDocument in configuration.JsonDocs)
                {
                    if (contextAdministrator.Configuration.JsonDocs.ContainsKey(jsonDocument.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "Json document" && e.InfoModel.EntityName == jsonDocument.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.JsonDocs[jsonDocument.Key] = jsonDocument.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                string.Format(CultureInfo.InvariantCulture, "Edit,*,json document,{0}", jsonDocument.Value.Name));
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.AddJsonDoc(jsonDocument.Value);
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("Json document", jsonDocument.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.JsonDocs[error.InfoModel.EntityName] = configuration.JsonDocs[error.InfoModel.EntityName];
                    contextAdministrator.Configuration.ChangeList.AddAdition("Add,*,json document," + error.InfoModel.EntityName);
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports Swagger Documents in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportSwaggerDocuments(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var swaggerDocument in configuration.SwaggerDocs)
                {
                    if (contextAdministrator.Configuration.SwaggerDocs.ContainsKey(swaggerDocument.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "Swagger document" && e.InfoModel.EntityName == swaggerDocument.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.SwaggerDocs[swaggerDocument.Key] = swaggerDocument.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                string.Format(CultureInfo.InvariantCulture, "Edit,*,swagger document,{0}", swaggerDocument.Value.Name));
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.AddSwaggerDoc(swaggerDocument.Value);
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("Swagger document", swaggerDocument.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.SwaggerDocs[error.InfoModel.EntityName] = configuration.SwaggerDocs[error.InfoModel.EntityName];
                    contextAdministrator.Configuration.ChangeList.AddAdition("Add,*,swagger document," + error.InfoModel.EntityName);
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports OAuth Providers in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportOauthProviders(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var oAuthProvider in configuration.OAuthProviders)
                {
                    if (contextAdministrator.Configuration.OAuthProviders.ContainsKey(oAuthProvider.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "OAuthProviders" && e.InfoModel.EntityName == oAuthProvider.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.OAuthProviders[oAuthProvider.Key] = oAuthProvider.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                string.Format(CultureInfo.InvariantCulture, "Edit,*,oauth provider,{0}", oAuthProvider.Value.Name));
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.AddOAuthProvider(oAuthProvider.Value);
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("OAuthProviders", oAuthProvider.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.OAuthProviders[error.InfoModel.EntityName] = configuration.OAuthProviders[error.InfoModel.EntityName];
                    contextAdministrator.Configuration.ChangeList.AddAdition("Add,*,oauth provider," + error.InfoModel.EntityName);
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports Wsdl Documents in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportWsdlDocuments(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var wsdlDocument in configuration.WsdlDocuments)
                {
                    if (contextAdministrator.Configuration.WsdlDocuments.ContainsKey(wsdlDocument.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "Xsdl document" && e.InfoModel.EntityName == wsdlDocument.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.WsdlDocuments[wsdlDocument.Key] = wsdlDocument.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                string.Format(CultureInfo.InvariantCulture, "Edit,*,wsdl document,{0}", wsdlDocument.Value.Name));
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.AddWsdlDoc(wsdlDocument.Value);
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("Xsdl document", wsdlDocument.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.WsdlDocuments[error.InfoModel.EntityName] = configuration.WsdlDocuments[error.InfoModel.EntityName];
                    contextAdministrator.Configuration.ChangeList.AddAdition("Add,*,wsdl document," + error.InfoModel.EntityName);
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports Xm Transforms in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportXmlTransforms(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var xmlTransformation in configuration.Xslts)
                {
                    if (contextAdministrator.Configuration.Xslts.ContainsKey(xmlTransformation.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "XSLT document" && e.InfoModel.EntityName == xmlTransformation.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.Xslts[xmlTransformation.Key] = xmlTransformation.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                string.Format(CultureInfo.InvariantCulture, "Edit,*,XSLT document,{0}", xmlTransformation.Value.Name));
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.AddXslt(xmlTransformation.Value);
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("XSLT document", xmlTransformation.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.Xslts[error.InfoModel.EntityName] = configuration.Xslts[error.InfoModel.EntityName];
                    contextAdministrator.Configuration.ChangeList.AddAdition("Add,*,XSLT document," + error.InfoModel.EntityName);
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports Data Mappers in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportDataMaps(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var dataMap in configuration.DataMappers)
                {
                    if (contextAdministrator.Configuration.DataMappers.ContainsKey(dataMap.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "DataMapper" && e.InfoModel.EntityName == dataMap.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.DataMappers[dataMap.Key] = dataMap.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                string.Format(CultureInfo.InvariantCulture, "Edit,*,Data Mapper document,{0}", dataMap.Value.Name));
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.AddDataMapper(dataMap.Value);
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("DataMapper", dataMap.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.DataMappers[error.InfoModel.EntityName] = configuration.DataMappers[error.InfoModel.EntityName];
                    contextAdministrator.Configuration.ChangeList.AddAdition("Add,*,Data Mapper document," + error.InfoModel.EntityName);
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports Xml Schemas in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportXmlSchemas(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var xmlSchema in configuration.Schemas)
                {
                    if (contextAdministrator.Configuration.Schemas.ContainsKey(xmlSchema.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "XML Schema" && e.InfoModel.EntityName == xmlSchema.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.Schemas[xmlSchema.Key] = xmlSchema.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                string.Format(CultureInfo.InvariantCulture, "Edit,*,XML Schema,{0}", xmlSchema.Value.Name));
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.AddSchema(xmlSchema.Value);
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("XML Schema", xmlSchema.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.Schemas[error.InfoModel.EntityName] = configuration.Schemas[error.InfoModel.EntityName];
                    contextAdministrator.Configuration.ChangeList.AddAdition("Add,*,XML Schema," + error.InfoModel.EntityName);
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports Processes in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportProcesses(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();
            var issues = new List<ConfigurationError>();

            //CS - update process to check it is processFlow or processFragment
            PipelineMaintenance.UpdateProcessPipeline(contextAdministrator.Configuration, issues);
            var processMap = contextAdministrator.Configuration.ESBMessagePipelines.ToDictionary(kvp => kvp.Value.Name,
                kvp => kvp.Key);

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var process in configuration.ESBMessagePipelines)
                {
                    if (contextAdministrator.Configuration.ESBMessagePipelines.Where(e => e.Value.Name == process.Value.Name && e.Value.IsProcessFlow == process.Value.IsProcessFlow).Any())
                    {
                        var response = context.DuplicateEntitiesList.Where(e => (e.InfoModel.EntityTypeDisplayName == "ProcessFragment" || e.InfoModel.EntityTypeDisplayName == "ProcessFlow") && e.InfoModel.EntityName == process.Value.Name).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.ESBMessagePipelines.Remove(processMap[process.Value.Name]);
                            contextAdministrator.Configuration.ESBMessagePipelines.Add(process.Key, process.Value);
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                string.Format(CultureInfo.InvariantCulture, "Edit,*,pipeline,{0}", process.Value.Name));
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.Configuration.ESBMessagePipelines.Add(process.Key, process.Value);
                            contextAdministrator.ConfigurationChangelist.AddAdition(
                                string.Format(CultureInfo.InvariantCulture, "Add,*,pipeline,{0}", process.Value.Name));
                        }
                        catch (Exception ex)
                        {
                            if (process.Value.IsProcessFlow)
                            {
                                validationsErrors.Add(new ValidationsErrorList("ProcessFlow", process.Value.Name));
                            }
                            else
                            {
                                validationsErrors.Add(new ValidationsErrorList("ProcessFragment", process.Value.Name));
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.ESBMessagePipelines.Add(error.InfoModel.EntityName, configuration.ESBMessagePipelines[processMap[error.InfoModel.EntityName]]);
                    contextAdministrator.ConfigurationChangelist.AddAdition(
                        string.Format(CultureInfo.InvariantCulture, "Add,*,pipeline,{0}", error.InfoModel.EntityName));
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports Workflows in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportWorkflows(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var workflow in configuration.WorkflowDefinitions)
                {
                    if (contextAdministrator.Configuration.WorkflowDefinitions.ContainsKey(workflow.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "Workflow" && e.InfoModel.EntityName == workflow.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.WorkflowDefinitions[workflow.Key] = workflow.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                string.Format(CultureInfo.InvariantCulture, "Edit,*,workflow definition,{0}", workflow.Value.Name));
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.Configuration.WorkflowDefinitions.Add(workflow.Key, workflow.Value);
                            contextAdministrator.ConfigurationChangelist.AddAdition(
                                string.Format(CultureInfo.InvariantCulture, "Add,*,workflow definition,{0}", workflow.Value.Name));
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("Workflow", workflow.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.WorkflowDefinitions.Add(error.InfoModel.EntityName, configuration.WorkflowDefinitions[error.InfoModel.EntityName]);
                    contextAdministrator.ConfigurationChangelist.AddAdition(
                        string.Format(CultureInfo.InvariantCulture, "Add,*,workflow definition,{0}", error.InfoModel.EntityName));
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports Availability Groups in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportAvailabilityGroups(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var availabilityGroup in configuration.AvailabilityGroups)
                {
                    if (contextAdministrator.Configuration.AvailabilityGroups.ContainsKey(availabilityGroup.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "AvailabilityGroup" && e.InfoModel.EntityName == availabilityGroup.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.AvailabilityGroups[availabilityGroup.Key] = availabilityGroup.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                string.Format(CultureInfo.InvariantCulture, "Edit,*,availability group,{0}", availabilityGroup.Value.Name));
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.Configuration.AvailabilityGroups.Add(availabilityGroup.Key, availabilityGroup.Value);
                            contextAdministrator.ConfigurationChangelist.AddAdition(
                                string.Format(CultureInfo.InvariantCulture, "Add,*,availability group,{0}", availabilityGroup.Value.Name));
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("AvailabilityGroup", availabilityGroup.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.AvailabilityGroups.Add(error.InfoModel.EntityName, configuration.AvailabilityGroups[error.InfoModel.EntityName]);
                    contextAdministrator.ConfigurationChangelist.AddAdition(
                        string.Format(CultureInfo.InvariantCulture, "Add,*,availability group,{0}", error.InfoModel.EntityName));
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports Workflow Endpoints in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportWorkflowEndpoints(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var workflowEndpoint in configuration.WorkflowHosts)
                {
                    if (contextAdministrator.Configuration.WorkflowHosts.ContainsKey(workflowEndpoint.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "WorkflowEndpoint" && e.InfoModel.EntityName == workflowEndpoint.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.WorkflowHosts[workflowEndpoint.Key] = workflowEndpoint.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                string.Format(CultureInfo.InvariantCulture, "Edit,*,workflow endpoint,{0}", workflowEndpoint.Value.Name));
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.Configuration.WorkflowHosts.Add(workflowEndpoint.Key, workflowEndpoint.Value);
                            contextAdministrator.ConfigurationChangelist.AddAdition(
                                string.Format(CultureInfo.InvariantCulture, "Add,*,workflow endpoint,{0}", workflowEndpoint.Value.Name));
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("WorkflowEndpoint", workflowEndpoint.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.WorkflowHosts.Add(error.InfoModel.EntityName, configuration.WorkflowHosts[error.InfoModel.EntityName]);
                    contextAdministrator.ConfigurationChangelist.AddAdition(
                        string.Format(CultureInfo.InvariantCulture, "Add,*,workflow endpoint,{0}", error.InfoModel.EntityName));
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports Message Patterns in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportMessagePatterns(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var messagePattern in configuration.MessagePatterns)
                {
                    if (contextAdministrator.Configuration.MessagePatterns.ContainsKey(messagePattern.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "Condition" && e.InfoModel.EntityName == messagePattern.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.MessagePatterns[messagePattern.Key] = messagePattern.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                "Edit,*,message pattern," + messagePattern.Value.Name);
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.AddMessagePattern(messagePattern.Value);
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("Condition", messagePattern.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.MessagePatterns[error.InfoModel.EntityName] = configuration.MessagePatterns[error.InfoModel.EntityName];
                    contextAdministrator.Configuration.ChangeList.AddAdition("Add,*,message pattern," + error.InfoModel.EntityName);
                }
            }
            return validationsErrors;
        }

        /// <summary>
        /// imports Subscribers in current configuration 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<ValidationsErrorList> ImportSubscribers(ImportContext context)
        {
            var configuration = context.Configuration;
            var contextAdministrator = this._applicationContextService.ApplicationContext.Administrator;
            List<ValidationsErrorList> validationsErrors = new List<ValidationsErrorList>();

            if (context.ValidationsErrors == null || context.ValidationsErrors.Count == 0)
            {
                foreach (var subscriber in configuration.Subscribers)
                {
                    if (contextAdministrator.Configuration.Subscribers.ContainsKey(subscriber.Key))
                    {
                        var response = context.DuplicateEntitiesList.Where(e => e.InfoModel.EntityTypeDisplayName == "Party" && e.InfoModel.EntityName == subscriber.Key).FirstOrDefault();
                        if (response != null && response.ReplaceValue)
                        {
                            contextAdministrator.Configuration.Subscribers[subscriber.Key] = subscriber.Value;
                            contextAdministrator.ConfigurationChangelist.AddRevision(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    "Edit,{0},{2},{1}",
                                    subscriber.Value.Zone,
                                    subscriber.Value.Name,
                                    subscriber.Value.PublisherRole ? "publisher" : "subscriber"
                                    ));
                        }
                    }
                    else
                    {
                        try
                        {
                            contextAdministrator.AddSubscriber(subscriber.Value);
                        }
                        catch (ValidationException ex)
                        {
                            validationsErrors.Add(new ValidationsErrorList("Party", subscriber.Key));
                        }
                    }
                }
            }
            else
            {
                foreach (var error in context.ValidationsErrors)
                {
                    contextAdministrator.Configuration.Subscribers[error.InfoModel.EntityName] = configuration.Subscribers[error.InfoModel.EntityName];
                    contextAdministrator.Configuration.ChangeList.AddAdition(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Add,{0},{2},{1}",
                        configuration.Subscribers[error.InfoModel.EntityName].Zone,
                        error.InfoModel.EntityName,
                        configuration.Subscribers[error.InfoModel.EntityName].PublisherRole ? "publisher" : "subscriber"
                        ));
                }
            }
            return validationsErrors;
        }
        #endregion

        #region Export Solution

        /// <summary>
        /// Export solution to path 
        /// </summary>
        /// <param name="configurationBaseRequest"></param>
        /// <returns></returns>
        public bool ExportSolutionToPath(ExportSolutionRequestModel request)
        {
            var isExportComplete = false;

            try
            {
                ESBConfiguration configuration;
                if (request.EntitiesList != null && request.EntitiesList.Count > 0)
                {
                    configuration = GetConfigurationFromEntitiesList(this._applicationContextService.ApplicationContext.Administrator.ConfigurationPath, request.EntitiesList);
                }
                else
                {
                    configuration = this._applicationContextService.ApplicationContext.Configuration;
                }
                var metabase = new ESBMetabase(configuration);
                metabase.SaveConfiguration(request.ConfigurationPath, true);
                isExportComplete = true;
            }
            catch (Exception e)
            {
                throw e;
            }
            return isExportComplete;
        }

        #endregion

        #region Save and open Rsp Files

        /// <summary>
        /// saves selected entities into Rsp file
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        public bool SaveRspFille(ExportSolutionRequestModel requestModel)
        {
            bool isFileSave = false;
            if (requestModel.EntitiesList != null && !string.IsNullOrEmpty(requestModel.ConfigurationPath))
            {
                using (var responseFileWriter = File.CreateText(requestModel.ConfigurationPath))
                {
                    foreach (var entity in requestModel.EntitiesList)
                    {
                        var line = string.Format(CultureInfo.InvariantCulture, "--{0}=\"{1}\"", entity.InfoModel.EntityTypeDisplayName, entity.InfoModel.EntityName);
                        responseFileWriter.WriteLine(line);
                    }
                }
                isFileSave = true;
            }
            return isFileSave;
        }


        /// <summary>
        /// opens selected entities into Rsp file
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        public List<EntityInfoModel> OpenRspFille(ConfigurationBaseRequest requestModel)
        {
            List<EntityInfoModel> entityList = new List<EntityInfoModel>();
            Regex longFormRegex = new Regex("^--(?<no>no-)?(?<option>[A-Za-z0-9][A-Za-z0-9\\-_ ]*)(?(no)|=(?<quoted>\")?(?<value>.*)(?(quoted)\"))?$");

            using (var responseFileReader = File.OpenText(requestModel.ConfigurationPath))
            {
                var lineNumber = 1;
                var line = responseFileReader.ReadLine();
                while (null != line)
                {
                    var match = longFormRegex.Match(line);
                    if (!match.Success)
                    {
                        var message = string.Format(
                            CultureInfo.CurrentCulture,
                            "There is a syntax error on line {0} of the response file.\n\n\"{1}\" is not a valid option.",
                            lineNumber,
                            line);
                        throw new Exception(message);
                    }

                    var option = match.Groups["option"].Value;

                    var valueGroup = match.Groups["value"];
                    if (!valueGroup.Success)
                    {
                        var message = string.Format(
                            CultureInfo.CurrentCulture,
                            "The option on line {0} of the response file does not specify the definition to import.",
                            lineNumber);
                        throw new Exception(message);
                    }

                    var key = valueGroup.Value;
                    entityList.Add(new EntityInfoModel() { EntityType = option, EntityName = key });
                    line = responseFileReader.ReadLine();
                }
            }
            return entityList;
        }
        #endregion
    }
}
