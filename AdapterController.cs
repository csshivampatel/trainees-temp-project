using log4net;
using MongoDB.Bson;
using Neuron;
using Neuron.Esb.Adapters;
using Neuron.Esb.Adapters.MetadataRetrieval;
using Neuron.Esb.Adapters.MetadataRetrieval.Database;
using Neuron.Esb.Administration;
using Neuron.Pipelines;
using Peregrine.Application.Domain.Models;
using Peregrine.Application.Domain.Models.NeuronProperty;
using Peregrine.Application.Service.Services;
using Peregrine.Application.WebAPI.Helpers;
using Peregrine.Application.WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Peregrine.Application.WebAPI.Controllers
{
    /// <summary>
    /// Topic REST controller to manage topics.
    /// </summary>
    [RoutePrefix("adapter")]
    public class AdapterController : BaseController
    {
        private IAdapterService _AdapterService;

        #region ctor
        /// <summary>
        /// Default AdapterController ctor 
        /// </summary>
        public AdapterController()
        {
        }
        #endregion

        #region Adapter API

        /// <summary>
        /// Returns a List of all fields in adapter info for correlational and expressions property in Set Property process step.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("correlation-list")]
        [ResponseType(typeof(IList<String>))]
        public IHttpActionResult GetCorrelationProperties()
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetCorrelationProperties called.");
                }
                return Ok(this.AdapterService.GetCorrelationFieldProperties());
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        /// <summary>
        /// Returns a List of values for correlational and expressions property in Set Property process step.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("correlation-value-list")]
        [ResponseType(typeof(IList<String>))]
        public IHttpActionResult GetCorrelationValueProperties()
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetCorrelationValueProperties called.");
                }
                return Ok(this.AdapterService.GetCorrelationValueProperties());
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        /// <summary>
        /// Returns a List of Adapter Types.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("adaptertypes-list")]
        [ResponseType(typeof(IList<String>))]
        public IHttpActionResult GetAdapterTypes()
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetAdapterTypes called.");
                }
                return Ok(this.AdapterService.GetAdapterTypes());
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        /// <summary>
        /// Returns a List of Adapter Types.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("adaptermodes-list")]
        [ResponseType(typeof(IList<AdapterTypeMode>))]
        public IHttpActionResult GetAdapterModes()
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetAdapterModes called.");
                }
                return Ok(this.AdapterService.GetAdapterModes());
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        /// <summary>
        /// Returns a List of Adapter Properties for Connection Category only.
        /// </summary>
        /// <param name="adapterRequestModel">AdapterPropertiesRequestModel</param>
        /// <returns></returns>
        [HttpPost]
        [Route("connection-adapterprops-list")]
        [ResponseType(typeof(IList<NeuronProperty>))]
        public IHttpActionResult GetAdapterPropertiesForConnection(AdapterPropertiesRequestModel adapterRequestModel)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetAdapterPropertiesForConnection called for Adapter {adapterRequestModel.AdapterName}, AdapterMode {adapterRequestModel.AdapterMode}, EndpointID {adapterRequestModel.EndpointId}.");
                }
                return Ok(this.AdapterService.GetConnectionAdapterProperties(adapterRequestModel.AdapterName));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        /// <summary>
        /// Returns a List of Adapter Properties.
        /// </summary>
        /// <param name="adapterRequestModel">AdapterPropertiesRequestModel</param>
        /// <returns></returns>
        [HttpPost]
        [Route("adapterprops-list")]
        [ResponseType(typeof(IList<NeuronProperty>))]
        public IHttpActionResult GetAdapterPropertiesAll(AdapterPropertiesRequestModel adapterRequestModel)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetAdapterPropertiesAll called for Adapter {adapterRequestModel.AdapterName}, AdapterMode {adapterRequestModel.AdapterMode}, EndpointID {adapterRequestModel.EndpointId}.");
                }
                return Ok(this.AdapterService.GetAdapterProperties(adapterRequestModel.AdapterName,  adapterRequestModel.EndpointId, adapterRequestModel.AdapterMode));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        /// <summary>
        /// Returns a List of Adapter Properties after applying change.
        /// </summary>
        /// <param name="adapterRequestModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("adapterprops-list-change")]
        [ResponseType(typeof(IList<NeuronProperty>))]
        public IHttpActionResult GetAdapterPropertiesOnChangeEvent(AdapterPropertiesOnChangeEventRequestModel adapterRequestModel)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetAdapterPropertiesOnChangeEvent called for Adapter {adapterRequestModel.AdapterName}, AdapterMode {adapterRequestModel.AdapterMode}, EndpointID {adapterRequestModel.EndpointId}.");
                }
                return Ok(this.AdapterService.GetAdapterPropertiesOnChangeEvent(adapterRequestModel.AdapterName, adapterRequestModel.EndpointId, adapterRequestModel.AdapterMode, adapterRequestModel.Properties));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        /// <summary>
        /// Returns a List of OAuth Provider Properties.
        /// </summary>
        /// <param name="oAuthProviderRequestModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("providerprops-list")]
        [ResponseType(typeof(IList<NeuronProperty>))]
        public IHttpActionResult GetProviderPropertiesAll(ProviderPropertiesRequestModel oAuthProviderRequestModel)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetProviderPropertiesAll called for OAuthProviderId {oAuthProviderRequestModel.OAuthProviderId}, OAuthProviderReference {oAuthProviderRequestModel.OAuthProviderReference}.");
                }
                return Ok(this.AdapterService.GetOAuthProviderProperties(oAuthProviderRequestModel.OAuthProviderId, oAuthProviderRequestModel.OAuthProviderReference));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        /// <summary>
        /// Returns a Values of Adapter Property Combobox Item.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("prop-combo-values")]
        [ResponseType(typeof(AdapterComboPropertyResponseModel))]
        public IHttpActionResult GetPropertyComboValues(AdapterPropertyComboValuesRequestModel adapterPropertyComboValuesRequestModel)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetPropertyComboValues called called for Adapter {adapterPropertyComboValuesRequestModel.AdapterName}, Property {adapterPropertyComboValuesRequestModel.PropertyName}.");
                }
                return Ok(this.AdapterService.GetTypeConverterValues(adapterPropertyComboValuesRequestModel.AdapterName, adapterPropertyComboValuesRequestModel.PropertyName));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        /// <summary>
        /// Message routing table collection 
        /// Outbound message api call
        /// </summary>
        /// <param name="salesforceRequestModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("prop-combo-values-SFobjects")]
        [ResponseType(typeof(AdapterComboPropertyResponseModel))]
        public IHttpActionResult GetStandardValuesForSFObjects(SalesforceRequestModel salesforceRequestModel)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug("SalesForce adapter GetAvalilableObjects called");
                }
                return Ok(this.AdapterService.GetTypeConverterValuesSFObjects(salesforceRequestModel));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        /// <summary>
        /// Returns a Values of Oauth Provider Property Combobox Item.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("oauth-prop-combo-values")]
        [ResponseType(typeof(IList<String>))]
        public IHttpActionResult GetOauthPropertyComboValues(AdapterPropertyComboValuesRequestModel adapterPropertyComboValuesRequestModel)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetOauthPropertyComboValues called called for Adapter {adapterPropertyComboValuesRequestModel.AdapterName}, Property {adapterPropertyComboValuesRequestModel.PropertyName}.");
                }
                return Ok(this.AdapterService.GetOauthTypeConverterValues(adapterPropertyComboValuesRequestModel.AdapterName, adapterPropertyComboValuesRequestModel.PropertyName));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }


        /// <summary>
        /// Return list of Adapters for Database type node in Metadata generation UI
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("get-db-adapters")]
        [ResponseType(typeof(IList<AdapterMetadataCatagoryDto>))]
        public IHttpActionResult GetDatabaseMetadataGenerationAdapters()
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetDatabaseMetadataGenerationAdapters called.");
                }
                return Ok(this.AdapterService.GetDatabaseMetadataGenerationAdapters());
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }


        /// <summary>
        /// Return DbMetadataSupport support information for given database adapter.
        /// </summary>
        /// <param name="adapterName"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("get-db-metadata-support/{adapterName}")]
        [ResponseType(typeof(DbMetadataSupport))]
         public IHttpActionResult GetDbMetadataSupport(String adapterName)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetDbMetadataSupport called.");
                }
                return Ok(this.AdapterService.GetDbMetadataSupport(adapterName));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }


        /// <summary>
        /// returns a connection string
        /// </summary>
        /// <param name="connectionDto"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("build-connection")]
        [ResponseType(typeof(string))]
        public IHttpActionResult BuildConnection(AdapterConnectionDto connectionDto)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter BuildConnection called.");
                }
                return Ok(this.AdapterService.BuildConnection(connectionDto));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        /// <summary>
        /// get a list of key value pairs from connection string
        /// </summary>
        /// <param name="connectionDto"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("get-keyvalue-pairs")]
        [ResponseType(typeof(List<KeyValuePair<String, String>>))]
        public IHttpActionResult GetKeyValuePairs(AdapterConnectionDto connectionDto)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetKeyValuePairs called.");
                }
                return Ok(this.AdapterService.GetKeyValuePairs(connectionDto));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        /// <summary>
        /// return true if connection is successful
        /// </summary>
        /// <param name="connectionDto"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("test-connection")]
        [ResponseType(typeof(bool))]
        public IHttpActionResult TestConnection(AdapterConnectionDto connectionDto)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter TestConnection called.");
                }
                return Ok(this.AdapterService.TestConnection(connectionDto));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        /// <summary>
        /// get default keys in for new case
        /// </summary>
        /// <param name="connectionDto"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("get-default-keys")]
        [ResponseType(typeof(List<KeyValuePair<String, String>>))]
        public IHttpActionResult GetDefaultKeys(AdapterConnectionDto connectionDto)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetDefaultKeys called.");
                }
                return Ok(this.AdapterService.GetDefaultKeys(connectionDto));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        /// <summary>
        /// Returns a metadata of the specified adapter endpoint with required information.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("get-metadata")]
        [ResponseType(typeof(DbMetadataResponse))]
        public IHttpActionResult GetMetadata(MetadataRequest metadataRequest)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetMetadata called.");
                }
                return Ok(this.AdapterService.GetMetadata(metadataRequest.Endpoint, metadataRequest.DbMetadataRequest));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        /// <summary>
        /// Returns a entity list of the specified enity type for the adapter endpoint.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("get-db-entitylist")]
        [ResponseType(typeof(List<DbEntity>))]
        public IHttpActionResult GetDbEnityList(DbEnityListRequest dbEnityListRequest)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetDbEnityList called.");
                }
                return Ok(this.AdapterService.GetDbEnityList(dbEnityListRequest.Endpoint, dbEnityListRequest.DbEntityType));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        /// <summary>
        /// Get the parameter information for given store procedure database entity
        /// </summary>
        /// <param name="dbEnityRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("get-db-sp-schema")]
        [ResponseType(typeof(DbStoredProcedureSchema))]
        public IHttpActionResult GetDbStoredProcedureSchema(DbEntitySchemaRequest dbEnityRequest)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetDbStoredProcedureSchema called.");
                }
                return Ok(this.AdapterService.GetDbStoredProcedureSchema(dbEnityRequest.Endpoint, dbEnityRequest.DbEntity));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }


        /// <summary>
        /// Get the parameter information for given store procedure database entity
        /// </summary>
        /// <param name="dbEnityRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("get-db-tableview-schema")]
        [ResponseType(typeof(DbTableViewSchema))]
        public IHttpActionResult GetDbTableViewSchema(DbEntitySchemaRequest dbEnityRequest)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetDbTableViewSchema called.");
                }
                return Ok(this.AdapterService.GetDbTableViewSchema(dbEnityRequest.Endpoint, dbEnityRequest.DbEntity));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        /// <summary>
        /// Get the nosql record id list.
        /// </summary>
        /// <param name="metadataRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("get-nosql-record-list")]
        [ResponseType(typeof(List<NoSqlRecordInfo>))]
        public IHttpActionResult GetNoSqlRecordIDList(MetadataRequest metadataRequest)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetNoSqlRecordIDList called.");
                }
                return Ok(this.AdapterService.GetNoSqlRecordIDList(metadataRequest.Endpoint, metadataRequest.DbMetadataRequest));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }
        /// <summary>
        /// Method returns the Mongo Record for the selected ID passed through dbMetadataRequest.NoSqlSchemaRecordID
        /// </summary>
        /// <param name="metadataRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("get-nosql-record-schema")]
        [ResponseType(typeof(String))]
        public IHttpActionResult GetNoSqlSchemaRecord(MetadataRequest metadataRequest)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetNoSqlSchemaRecord called.");
                }
                return Ok(this.AdapterService.GetNoSqlSchemaRecord(metadataRequest.Endpoint, metadataRequest.DbMetadataRequest));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        /// <summary>
        /// Get the nosql collection schema in table/view format for selected record.
        /// </summary>
        /// <param name="metadataRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("get-nosql-collection-schema")]
        [ResponseType(typeof(DbTableViewSchema))]
        public IHttpActionResult GetNoSqlCollectionSchema(MetadataRequest metadataRequest)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetNoSqlCollectionSchema called.");
                }
                return Ok(this.AdapterService.GetNoSqlCollectionSchema(metadataRequest.Endpoint, metadataRequest.DbMetadataRequest));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }




        #region Application Metadata gen

        /// <summary>
        /// Return list of Adapters for Application type node in Metadata generation UI
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("get-application-adapters")]
        [ResponseType(typeof(IList<AdapterMetadataCatagoryDto>))]
        public IHttpActionResult GetApplicationMetadataGenerationAdapters()
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetApplicationMetadataGenerationAdapters called.");
                }
                return Ok(this.AdapterService.GetApplicationMetadataGenerationAdapters());
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        /// <summary>
        /// Return DbMetadataSupport support information for given database adapter.
        /// </summary>
        /// <param name="adapterName"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("get-application-metadata-support/{adapterName}")]
        [ResponseType(typeof(ApplicationMetadataSupport))]
        public IHttpActionResult GetApplicationMetadataSupport(String adapterName)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetApplicationMetadataSupport called.");
                }
                return Ok(this.AdapterService.GetApplicationMetadataSupport(adapterName));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        /// <summary>
        /// return true if application adapter able to make test connection successful.
        /// </summary>
        /// <param name="model">ApplicationMetadataRequestDto</param>
        /// <returns></returns>
        [HttpPost]
        [Route("application-test-connection")]
        [ResponseType(typeof(bool))]
        public IHttpActionResult ApplicationAdapterTestConnection(ApplicationMetadataRequestDto model)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter ApplicationAdapterTestConnection called.");
                }
                return Ok(this.AdapterService.ApplicationAdapterTestConnection(model.Endpoint, model.MetadataRequest));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        /// <summary>
        /// Get list of the Operation Categories supported by Application.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("get-operation-categories")]
        [ResponseType(typeof(List<MetadataRetrievalNode>))]
        public IHttpActionResult GetOperationCategories([FromBody]ApplicationMetadataRequestDto model)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetOperationCategories called.");
                }
                return Ok(this.AdapterService.GetCategories(model.Endpoint, model.MetadataRequest));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        /// <summary>
        /// Get list of the Operation detail for given category by Application.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("get-operations")]
        [ResponseType(typeof(List<MetadataRetrievalNode>))]
        public IHttpActionResult GetOperations([FromBody] ApplicationMetadataRequestDto model)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetOperations called.");
                }
                return Ok(this.AdapterService.GetOperations(model.Endpoint, model.MetadataRequest));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        /// <summary>
        /// Get list of the Operation detail for given category by Application.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("get-operation-schema")]
        [ResponseType(typeof(SchemaDocuments))]
        public IHttpActionResult GetOperationSchema([FromBody] ApplicationMetadataRequestDto model)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetOperationSchema called.");
                }
                var operationScheams = this.AdapterService.GetOperationSchema(model.Endpoint, model.MetadataRequest);
                return Ok(CommonHelpers.GetSchemaDocuments(operationScheams));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        #endregion


        /// <summary>
        /// returns list of available Properties
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("get-configure-suggestions")]
        [ResponseType(typeof(HttpConfigureGetSuggestionsModel))]
        public IHttpActionResult GetConfigureSuggestions()
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetConfigureSuggestions called");
                }
                return Ok(this.AdapterService.GetConfigureAutoCompletions());
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        /// <summary>
        /// returns directory Info using OAuth Providers
        /// </summary>
        /// <param name="directoryInfoDto"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("get-folders-for-drive-adapters")]
        [ResponseType(typeof(List<DriveFolderInfo>))]
        public IHttpActionResult GetFolders(AdapterDirectoryInfoDto directoryInfoDto)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetFolders called.");
                }

                return Ok(this.AdapterService.GetFoldersForDriveAdapters(directoryInfoDto));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        /// <summary>
        /// returns directory Info using OAuth Providers
        /// </summary>
        /// <param name="routableAdapterPropertiesModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("get-routable-adapter-properties")]
        [ResponseType(typeof(List<AdapterEndpointProperties>))]
        public IHttpActionResult GetRoutableProperties(GetRoutableAdapterPropertiesModel routableAdapterPropertiesModel)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetRoutableProperties called.");
                }
                
                return Ok(this.AdapterService.RoutableAdapterProperties(routableAdapterPropertiesModel));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }


        /// <summary>
        /// returns list of available Properties
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("get-service-routing-suggestions")]
        [ResponseType(typeof(ServiceRoutingGetSuggestionsModel))]
        public IHttpActionResult GetServiceRoutingSuggestions(string serviceName)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetServiceRoutingSuggestions called");
                }
                return Ok(this.AdapterService.GetServiceRoutingAutoCompletions(serviceName));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        #region MongoDb Adapter related Apis
        /// <summary>
        /// returns list of collections
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("get-mongodb-collections")]
        [ResponseType(typeof(List<string>))]
        public IHttpActionResult GetMongoDbCollections(MongoDbDataRequestModel requestModel)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetMongoDbCollections called");
                }
                return Ok(this.AdapterService.GetMongoDbCollectionsFromConnectionString(requestModel.ConnectionString));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        /// <summary>
        /// returns list of Record's Ids from Collection Name
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("get-mongodb-record-ids")]
        [ResponseType(typeof(List<BsonElement>))]
        public IHttpActionResult GetMongoDbRecordIds(MongoDbDataRequestModel requestModel)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetMongoDbRecordIds called");
                }
                return Ok(this.AdapterService.GetMongoDbRecordIdsFromCollectionName(requestModel));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        /// <summary>
        /// returns record xml from id
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("get-mongodb-record-xml")]
        [ResponseType(typeof(string))]
        public IHttpActionResult GetMongoDbRecordXml(MongoDbDataRequestModel requestModel)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetMongoDbRecordXml called");
                }
                return Ok(this.AdapterService.GetMongoDbRecordXmlFromRecordId(requestModel));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        /// <summary>
        /// returns Collection Fields
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("get-mongodb-collection-fields")]
        [ResponseType(typeof(List<MongoDBAdapterCollectionFields>))]
        public IHttpActionResult GetMongoDbCollectionFields(MongoDbDataRequestModel requestModel)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter GetMongoDbCollectionFields called");
                }
                return Ok(this.AdapterService.GetMongoDbCollectionFields(requestModel));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }

        /// <summary>
        /// returns mongo query from Query model
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("build-mongo-query")]
        [ResponseType(typeof(string))]
        public IHttpActionResult ValidateMongoValueType(BuildMongoQueryModel queryModel)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter ValidateMongoValueType called");
                }
                return Ok(this.AdapterService.BuildMongoQuery(queryModel));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }
        #endregion

        #region Validate Adapter Propeties Value

        /// <summary>
        /// validates prop value if property contains PropertyValueValidation Attribute
        /// </summary>
        /// <param name="validateAdapterPropValue"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("validate-prop-value")]
        [ResponseType(typeof(ValidateAdapterPropValueResponseModel))]
        public IHttpActionResult ValidatePropValue(ValidateAdapterPropValueRequestModel validateAdapterPropValue)
        {
            try
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.Debug($"Adapter ValidatePropValue called");
                }
                return Ok(this.AdapterService.ValidateAdapterPropValue(validateAdapterPropValue));
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(ex);
            }
        }
        #endregion
        #endregion

        #region private methods

        private IAdapterService AdapterService
        {
            get
            {
                if (this._AdapterService == null)
                {
                    this._AdapterService = new AdapterService(this.ApplicationContext);
                }
                return _AdapterService;
            }
        }

        #endregion
    }


}
