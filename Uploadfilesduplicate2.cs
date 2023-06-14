using System;
using System.IO;
using Azure.Storage.Blobs;

namespace FileUploader
{
    class Program
    {
        static void Main(string[] args)
        {
            string localDirectory = @"D:\cloud";
            string containerSasUrl = "https://confidosoft.blob.core.windows.net/confidosoft-container?sp=racwdli&st=2023-06-10T17:17:56Z&se=2023-06-11T01:17:56Z&spr=https&sv=2022-11-02&sr=c&sig=GZB9jsVwlxSzkjJufqaQuSSEtFPQXxfWiP22ry6Jce0%3D";

            // Create a new instance of FileSystemWatcher
            FileSystemWatcher watcher = new FileSystemWatcher();

            // Set the path to the local directory to monitor
            watcher.Path = localDirectory;

            // Monitor only for new files
            watcher.NotifyFilter = NotifyFilters.FileName;

            // Subscribe to the Created event
            watcher.Created += (sender, e) =>
            {
                // Get the full path of the created file
                string filePath = e.FullPath;

                // Upload the file to Azure Blob container
                try
                {
                    UploadFileToBlob(filePath, containerSasUrl);
                }
                catch (Exception ex)
                {
                    // Handle the error and log or display the message
                    Console.WriteLine($"Error occurred while uploading file {filePath}: {ex.Message}");
                }
            };

            // Handle network errors
            watcher.Error += (sender, e) =>
            {
                // Handle the network error and log or display the message
                Console.WriteLine($"Network error occurred while monitoring the directory: {e.GetException().Message}");
            };

            // Start monitoring
            watcher.EnableRaisingEvents = true;

            Console.WriteLine("File monitoring started. Press any key to exit.");
            Console.ReadKey();
        }

        static void UploadFileToBlob(string filePath, string containerSasUrl)
        {
            // Open the file stream
            using (FileStream fileStream = File.OpenRead(filePath))
            {
                // Create a BlobClient using the container SAS URL
                BlobClient blobClient = new BlobClient(new Uri(containerSasUrl));

                // Upload the file to Azure Blob container
                blobClient.Upload(fileStream);
            }

            // Delete the file after successful upload
            File.Delete(filePath);
        }
    }
}
