using Neuron.Esb.Administration;
using Peregrine.Application.Domain.Models;
using Peregrine.Application.Service.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using log4net;
using System.Xml;
using Neuron.Esb.Internal;
using System.Net.Http;
using System.Configuration;
using System.IO.Compression;

namespace Peregrine.Application.Service.Services
{

    #region ISampleSolutionsService interface
    /// <summary>
    /// Manage Sample Solutions List.
    /// </summary>
    public interface ISampleSolutionsService
    {
        /// <summary>
        /// Get List of Recent files.
        /// </summary>
        /// <returns></returns>
        IList<Sample> GetSampleSolutionsList();
        //List<string> GetSampleSolutionsList();
    }
    #endregion


    #region ISampleSolutionsService implemenation

    public class SampleSolutionsService : ISampleSolutionsService
    {
        private static List<Sample> Samples { get; set; }
        //private static List<string> Samples { get; set; }
        protected static readonly ILog _logger = LogManager.GetLogger("Peregrine.Application.Service");
        public SampleSolutionsService()
        {
        }

        /// <summary>
        /// Get Sample Solutions list items.
        /// </summary>
        public IList<Sample> GetSampleSolutionsList()
        {
            PrepareSamplesList();
            return Samples;
        }

        private async void PrepareSamplesList()
        {
            if (Samples == null)
            {
                string version = ConfigurationManager.AppSettings["Version"];
                string samplesRootFolder = Path.Combine(CustomDllUtility.AssemblyDirectory, "Samples");
                string sampleFolderVersion = Path.Combine(samplesRootFolder, version);
                var manifestFile = Path.Combine(sampleFolderVersion,  "NeuronSamples.xml");
                if(!Directory.Exists(sampleFolderVersion))
                {
                    await CloneVersion(samplesRootFolder, version);
                }

                // Ensure the sammples manifest file exists.
                if (!File.Exists(manifestFile))
                {
                    if (_logger != null)
                    {
                        _logger.Error($"Sample Manifest Not Found. The samples dialog cannot open because the samples manifest file is not present at {manifestFile}");
                    }
                    return;
                }

                // Open the samples manifest file and load its contents.
                var doc = new XmlDocument();
                doc.Load(manifestFile);
                var sampleNodes = doc.SelectNodes("//Sample");
                if (sampleNodes == null)
                {
                    if (_logger != null)
                    {
                        _logger.Error($"Sample Not Present. The samples dialog cannot open because there are no samples defined in the manifest file {manifestFile}");
                    }
                    return;
                }

                var samplesPath = Path.Combine(CustomDllUtility.AssemblyDirectory, "Samples");

                var samples = from XmlNode sampleNode in sampleNodes
                              select
                                  new Sample
                                  {
                                      TreeFolders = sampleNode.Attributes["tree"].InnerText,
                                      Name = sampleNode.Attributes["name"].InnerText,
                                      Config = String.IsNullOrWhiteSpace(sampleNode.Attributes["config"].InnerText) ? "" : Path.Combine(samplesPath, "Configurations", sampleNode.Attributes["config"].InnerText),
                                      Folder = sampleNode.Attributes["folder"].InnerText.StartsWith(@"\") ? samplesPath + sampleNode.Attributes["folder"].InnerText : Path.Combine(samplesPath, sampleNode.Attributes["folder"].InnerText),
                                      Solution = sampleNode.Attributes["solution"].InnerText,
                                      Description = sampleNode.Attributes["desc"].InnerText,
                                      SetupRequired = ESBHelper.BooleanValue(sampleNode.Attributes["setup"].InnerText),
                                      HelpPage = sampleNode.Attributes["help"].InnerText,
                                      Clients = sampleNode.Attributes["clients"].InnerText
                                  };

                if (samples != null && samples.Any())
                {
                    Samples = new List<Sample>();
                    foreach (var sample in samples)
                    {
                        if (Directory.Exists(sample.Folder))
                            sample.Folder = Path.GetFullPath(sample.Folder);

                        if (Directory.Exists(sample.Config))
                            sample.Config = Path.GetFullPath(sample.Config);

                        Samples.Add(sample);
                    }
                }
            }
        }

        /// <summary>
        /// Clone the samples from the public repository based on version specified in the app config file. 
        /// </summary>
        /// <returns></returns>
        private async Task<string> CloneVersion(string destDirectory,string version)
        {
            try
            {
                string repositoryUrl = ConfigurationManager.AppSettings["RepositoryUrl"]; 
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Other");
                    string zipUrl = $"{repositoryUrl}/archive/{version}.zip";
                    HttpResponseMessage response = await client.GetAsync(zipUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var contentDispositionHeader = response.Content.Headers.ContentDisposition;
                        var newContentDispositionHeader = new System.Net.Http.Headers.ContentDispositionHeaderValue(contentDispositionHeader.DispositionType);
                        response.Content.Headers.ContentDisposition.FileName = $"{version}.zip";

                        // Set the desired filename
                        newContentDispositionHeader.FileName = $"{version}.zip";

                        // Update the Content-Disposition header in the response headers
                        response.Content.Headers.ContentDisposition = newContentDispositionHeader;

                        using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                        {
                            string filename = response.Content.Headers.ContentDisposition.FileName;
                            string zipFileName = "Samples.zip";
                            string cloneToPath = Path.GetTempPath();
                            if (!Directory.Exists(cloneToPath))
                            {
                                Directory.CreateDirectory(cloneToPath);
                            }
                            string destinationPath = Path.Combine(cloneToPath, zipFileName);

                            using (FileStream fileStream = File.Create(destinationPath))
                            {
                                await contentStream.CopyToAsync(fileStream);
                            }
                            

                            // Extract the ZIP file
                            ZipFile.ExtractToDirectory(destinationPath, "D:\\shivam\\github\\clone-repos-version");

                            string sourceFilePath = Path.Combine(destDirectory, Directory.GetDirectories(destDirectory)[0]);
                            string destinationFilePath = Path.Combine(destDirectory, version);
                            Directory.Move(sourceFilePath, destinationFilePath);

                            //string extractPath = Path.Combine(destDirectory,version);
                            //ZipFile.ExtractToDirectory(destinationPath, extractPath);

                            //string firstFolder = Directory.GetDirectories(destDirectory).First();
                            //string sourceDirectory = firstFolder;
                            //string targetDirectory = destDirectory;

                            //// Move folders
                            //string[] directories = Directory.GetDirectories(sourceDirectory);
                            //foreach (string directory in directories)
                            //{
                            //    string folderName = Path.GetFileName(directory);
                            //    string destinationFolder = Path.Combine(targetDirectory, folderName);
                            //    Directory.Move(directory, destinationFolder);
                            //}

                            //// Move files
                            //string[] files = Directory.GetFiles(sourceDirectory);
                            //foreach (string file in files)
                            //{
                            //    string fileName = Path.GetFileName(file);
                            //    string destinationFile = Path.Combine(targetDirectory, fileName);
                            //    File.Move(file, destinationFile);
                            //}
                            // Delete the ZIP file
                            File.Delete(destinationPath);

                            return String.Format("Version {0} cloned successfully", version);

                        }
                    }
                    else
                    {
                        return ($"Failed to download version {version}. Version not found Status code: {response.StatusCode}");
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
                //return String.Format("Fail to clone Version {0}", version);
            }
        }
    }
    #endregion

}
