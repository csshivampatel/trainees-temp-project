using System;
using System.IO;
using Azure.Storage.Blobs;

/*
class Program
{
    static void Main(string[] args)
    {
        // Azure Storage account connection string
        string connectionString = "DefaultEndpointsProtocol=https;AccountName=confidosoft;AccountKey=/cNFqgRzVY/DdA2FQ/RdSTJrgyglfEtdF7ALmrwUMrqw1Bnp2WG8vA66X8Y377rp6NDh6Zx05l7Z+AStWp2vlw==;EndpointSuffix=core.windows.net";

        // Local directory to monitor
        string localDirectory = @"D:\cloud";

        // Azure Blob container name
        string containerName = "confidosoft-container";

        // Create Blob service client 
        BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

        // Get Blob container client
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

        // Function to upload file to Azure Blob storage
        void UploadFileToBlob(string filePath)
        {
            string blobName = Path.GetFileName(filePath);
            System.print("filePath = watcher code := " + filePath);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            using FileStream fileStream = File.OpenRead(filePath);
            blobClient.Upload(fileStream, true);
        }

        // Monitor the local directory for new files or changes
        while (true)
        {
            foreach (string filePath in Directory.EnumerateFiles(localDirectory, "*", SearchOption.AllDirectories))
            {
                UploadFileToBlob(filePath);
            }

            // Add a delay before scanning the directory again
            System.Threading.Thread.Sleep(10000);
        }
    }
}
*/


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

