using Neuron.Esb.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peregrine.Application.Domain.Models
{

    /// <summary>
    /// Base request model for an application configuration open/save.
    /// </summary>
    public class ConfigurationBaseRequest
    {
        /// <summary>
        /// Configuration path
        /// </summary>
        public String ConfigurationPath { get; set; }

        /// <summary>
        /// Application Name
        /// </summary>
        public String ApplicationName { get; set; }

        /// <summary>
        /// Comments Added by User while save solution
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Is SaveAs or Save Clicked
        /// </summary>
        public bool IsSaveAsClicked { get; set; }
    }

    /// <summary>
    /// Request model to get dll files
    /// </summary>
    public class GetFilesReqeustModel
    {
        /// <summary>
        /// Configuration path
        /// </summary>
        public String ConfigurationPath { get; set; }

        /// <summary>
        /// flag to get dll files fullname or not
        /// </summary>
        public bool RequireFullName { get; set; }
    }

    /// <summary>
    /// Common path model to post the file or path information.
    /// </summary>
    public class ConfigurationPathModel
    {
        /// <summary>
        /// Selected path or file information.
        /// </summary>
        public String ConfigurationPath { get; set; }
    }

    public class ImportContext
    {
        public Neuron.Esb.Administration.ESBConfiguration Configuration { get; set; }

        /// <summary>
        /// List of Duplicate Entities List
        /// </summary>
        public List<GetListResponseModel> EntitiesList { get; set; }

        /// <summary>
        /// List of Duplicate Entities List
        /// </summary>
        public List<DuplicateEntitiesListModel> DuplicateEntitiesList { get; set; }

        /// <summary>
        /// Need to add entities with error or not while import solution
        /// </summary>
        public List<ValidationsErrorList> ValidationsErrors { get; set; }
    }

    /// <summary>
    /// list of redundent entities in import and current solution based on same name
    /// </summary>
    public class DuplicateEntitiesListModel
    {
        /// <summary>
        /// Entity Info
        /// </summary>
        public EntityInfoModel InfoModel { get; set; }

        /// <summary>
        /// need to replace value of redundent entity or not
        /// </summary>
        public bool ReplaceValue { get; set; }
    }

    public class ImportSolutionRequestModel
    {
        /// <summary>
        /// Configuration path
        /// </summary>
        public String ConfigurationPath { get; set; }

        /// <summary>
        /// List of Duplicate Entities List
        /// </summary>
        public List<GetListResponseModel> EntitiesList { get; set; }

        /// <summary>
        /// List of Duplicate Entities List
        /// </summary>
        public List<DuplicateEntitiesListModel> DuplicateEntitiesList { get; set; }

        /// <summary>
        /// Need to add entities with error or not while import solution
        /// </summary>
        public List<ValidationsErrorList> ValidationsErrors { get; set; }

        /// <summary>
        /// Save Entities With error or not
        /// </summary>
        public bool IsSaveWithError { get; set; }
    }

    public class DuplicateEntitiesRequestModel
    {
        /// <summary>
        /// List of Duplicate Entities List
        /// </summary>
        public List<GetListResponseModel> EntitiesList { get; set; }
    }

    public class EntityInfoModel
    {
        public string EntityType { get; set; }

        /// <summary>
        /// Display name of entity
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// Type of entity i.e. processflow, topic
        /// </summary>
        public string EntityTypeDisplayName { get; set; }
    }

    public class ValidationsErrorList
    {
        public ValidationsErrorList(string entityType, string entityName)
        {
            InfoModel = new EntityInfoModel();
            InfoModel.EntityType = entityType;
            InfoModel.EntityName = entityName;
        }

        /// <summary>
        /// Entity Info
        /// </summary>
        public EntityInfoModel InfoModel { get; set; }

        /// <summary>
        /// need to add error entity or not
        /// </summary>
        public bool AddinImport { get; set; }
    }

    public class GetListResponseModel
    {
        public GetListResponseModel(string type, string entityType, string entityName, string category, string entityCategory, List<DependencyInfo> dependedEntities = null,string role)
        {
            InfoModel = new EntityInfoModel();
            InfoModel.EntityTypeDisplayName = type;
            InfoModel.EntityType = entityType;
            InfoModel.EntityName = entityName;
            Category = category;
            EntityCategory = entityCategory;
            DependedEntities = dependedEntities;
            Role = role;
        }
        /// <summary>
        /// Entity Info
        /// </summary>
        public EntityInfoModel InfoModel { get; set; }

        /// <summary>
        /// Category in which perticular entity belongs to.
        /// </summary>
        public string EntityCategory { get; set; }

        /// <summary>
        /// List of dependency enetities for perticular topic.
        /// </summary>
        public List<DependencyInfo> DependedEntities { get; set; }

        /// <summary>
        /// Category
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Role - Publisher or Subscriber or Both
        /// </summary>
        public string Role { get; set; }
    }

    public class ImportSolutionResponseModel
    {
        /// <summary>
        /// Validation Errors
        /// </summary>
        public List<ValidationsErrorList> ValidationsErrors { get; set; }

        /// <summary>
        /// Issues List
        /// </summary>
        public List<Neuron.Esb.Administration.ConfigurationError> Issues { get; set; }
    }

    public class ExportSolutionRequestModel
    {
        /// <summary>
        /// Configuration path
        /// </summary>
        public String ConfigurationPath { get; set; }

        /// <summary>
        /// List of Duplicate Entities List
        /// </summary>
        public List<GetListResponseModel> EntitiesList { get; set; }
    }

    /// <summary>
    /// Renames the application name in config 
    /// </summary>
    public class ApplicationRenameRequestModel
    {
        /// <summary>
        /// Selected path or file information.
        /// </summary>
        public string ConfigurationPath { get; set; }

        /// <summary>
        /// Application new Name
        /// </summary>
        public string AppName { get; set; }
    }
}
