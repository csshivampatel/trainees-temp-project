using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Diagnostics;
using log4net;
using log4net.Config;


namespace UploadFilesToAzurePortal
{
    internal static class Program
    {
        private static readonly TraceSource traceSource = new TraceSource("MyTraceSource");

        static async Task Main(string[] args)
        {
            traceSource.Listeners.Add(new ConsoleTraceListener());

            // Azure Storage account connection string
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=confidosoft;AccountKey=/cNFqgRzVY/DdA2FQ/RdSTJrgyglfEtdF7ALmrwUMrqw1Bnp2WG8vA66X8Y377rp6NDh6Zx05l7Z+AStWp2vlw==;EndpointSuffix=core.windows.net";

            // Local directory to monitor
            string localDirectory = @"D:\cloud";

            // Azure Blob container name
            string containerName = "confidosoft-container";

            // rename extension of the file
            string newExtension = ".dummy";


            // Create Blob service client 
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            // Get Blob container client
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Function to upload file to Azure Blob storage
            async Task UploadFileToBlobAsync(string filePath)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                string fileExtension = Path.GetExtension(filePath);

                if (fileExtension.Equals(newExtension))
                {
                    return;
                }

                string blobName = Path.GetFileName(filePath);
                Console.WriteLine("filePath = watcher code := {0}", filePath);
                traceSource.TraceInformation("filePath = watcher code := {0}", filePath);
                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                using FileStream fileStream = File.OpenRead(filePath);
                await blobClient.UploadAsync(fileStream, true);

                string renamedFilePath = GetUniqueRenamedFilePath(filePath, fileNameWithoutExtension);

                RenameFileAsync(filePath, renamedFilePath);
            }

            async Task RenameFileAsync(string filePath, string renamedFilePath)
            {
                try
                {
                    if (!File.Exists(renamedFilePath))
                    {
                        await Task.Run(() => File.Move(filePath, renamedFilePath));
                    }
                    Console.WriteLine("file is successfully renamed to -> {0}", renamedFilePath);
                    traceSource.TraceInformation("file is successfully renamed to -> {0}", renamedFilePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occurred while renaming the file: {ex.Message}");
                    traceSource.TraceEvent(TraceEventType.Error,0,$"Error occurred while renaming the file: {ex.Message}");
                }
            }

            string GetUniqueRenamedFilePath(string filePath, string filename)
            {
                // Path.GetDirectoryName(filePath) may be null. So converting nullable type to non-nullable
                string directory = Path.GetDirectoryName(filePath) ?? localDirectory;
                string baseFilename = Path.Combine(directory, filename);
                string uniqueFilename = baseFilename + newExtension;

                int counter = 1;
                while (File.Exists(uniqueFilename))
                {
                    uniqueFilename = $"{baseFilename}({counter++}){newExtension}";
                }

                return uniqueFilename;
            }

            async Task HandleFileEventAsync(string filePath)
            {
                if (File.Exists(filePath))
                {
                    await UploadFileToBlobAsync(filePath);
                }
            }

            // Create a ManualResetEvent
            ManualResetEvent exitEvent = new ManualResetEvent(false);

            foreach (string filePath in Directory.EnumerateFiles(localDirectory, "*", SearchOption.AllDirectories))
            {
                await UploadFileToBlobAsync(filePath);
            }

            FileSystemWatcher watcher = new FileSystemWatcher(localDirectory);
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.EnableRaisingEvents = true;

            watcher.Created += async (sender, e) => await HandleFileEventAsync(e.FullPath);
            watcher.Deleted += async (sender, e) =>
            {
                    await HandleFileEventAsync(e.FullPath);
            };
            watcher.Changed += async (sender, e) => await HandleFileEventAsync(e.FullPath);

            watcher.Error += (sender, e) =>
            {
                Console.WriteLine($"Network error occurred while monitoring the directory: {e.GetException().Message}");
                traceSource.TraceEvent(TraceEventType.Error, 0, $"Network error occurred while monitoring the directory: {e.GetException().Message}");
            };

            Console.WriteLine("File monitoring started. Press any key to exit.");
            traceSource.TraceInformation("File monitoring started. Press any key to exit.");
            Console.ReadKey();

            exitEvent.Set();
        }
    }
}

