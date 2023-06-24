using Azure.Storage.Blobs;
using System;
using System.Diagnostics;


namespace UploadFilesToAzurePortal
{
    internal static class UploadFilesToAzurePortal
    {
        // Azure Storage account connection string
        static string connectionString = "DefaultEndpointsProtocol=https;AccountName=confidosoft;AccountKey=/cNFqgRzVY/DdA2FQ/RdSTJrgyglfEtdF7ALmrwUMrqw1Bnp2WG8vA66X8Y377rp6NDh6Zx05l7Z+AStWp2vlw==;EndpointSuffix=core.windows.net";

        // Local directory to monitor
        static string localDirectory = @"C:\Users\LENOVO\Desktop\rethsZip\apiReths\RethsAPI\Resources";
        //static string localDirectory = @"D:\cloud";

        // Azure Blob container name
        static string containerName = "confidosoft-container";

        // rename extension of the file
        static string newExtension = ".dummy";

        // count the number of files uploaded 
        static int uploadedFileCount = 0;

        static BlobServiceClient blobServiceClient;

        static BlobContainerClient containerClient;

        static int noOfFilesUploaded = 0;

        static UploadFilesToAzurePortal()
        {
            try
            {
                // Create Blob service client 
                blobServiceClient = new BlobServiceClient(connectionString);
                Console.WriteLine("\nConnected to azure portal.\n");

                containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                Console.WriteLine("\nSuccessfully created blob container client.\n");

            }
            catch (System.FormatException formatException)
            {
                Debug.WriteLine(formatException);
                Console.WriteLine("Unable to connect azure portal.\nNetwork error or incorrect connection string\n");
            }
            catch (System.NullReferenceException nullReferenceException)
            {
                Debug.WriteLine(nullReferenceException.ToString());
                Console.WriteLine("Unable to create blob container client.....");
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.ToString());
                Console.WriteLine("Something went wrong...");
            }
        }

        static async Task Main(string[] args)
        {
            // Create a ManualResetEvent
            ManualResetEvent exitEvent = new ManualResetEvent(false);

            
            int noOfFilesfiles = Directory.GetFiles(localDirectory, "*", SearchOption.AllDirectories).Length;
            foreach (string filePath in Directory.EnumerateFiles(localDirectory, "*", SearchOption.AllDirectories))
            {
                Console.WriteLine($" Uploading file :- {filePath}");
                Console.WriteLine($"Uploading file {noOfFilesUploaded+1} ");
                await UploadFileToBlobAsync(filePath);
            }

            //FileSystemWatcher watcher = new FileSystemWatcher(localDirectory);
            //watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            //watcher.EnableRaisingEvents = true;

            ////watcher.Created += async (sender, e) => await HandleFileEventAsync(e.FullPath);
            //watcher.Created += (sender, e) =>
            //{
            //    //// Start the upload timer when a new file is created
            //    //// Delay the upload process to allow the file system to finish copying the file
            //    //Thread.Sleep(5000); // Adjust the delay time as needed
            //    //UploadFileToBlobAsync(e.FullPath).Wait();
            //    bool isFileReady = false;
            //    int retryCount = 0;
            //    const int maxRetries = 10;
            //    TimeSpan retryDelay = TimeSpan.FromSeconds(1);

            //    while (!isFileReady || retryCount < maxRetries)
            //    {
            //        try
            //        {
            //            using (FileStream fs = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.None))
            //            {
            //                // If the file can be opened without an exception, it is fully available
            //                isFileReady = true;
            //                Console.WriteLine("isFileReady :- " + isFileReady);
            //                Console.WriteLine("File is detected :- {0} of size {0}",e.FullPath, fs.Length);
            //            }
            //        }
            //        catch (IOException)
            //        {
            //            // File is still being copied, retry after a short delay
            //            Thread.Sleep(retryDelay);
            //            Console.WriteLine("Fetching file :- {0} ", e.FullPath);
            //            retryCount++;
            //        }
            //    }

            //    if (isFileReady)
            //    {
            //        // File is fully available, proceed with upload
            //        UploadFileToBlobAsync(e.FullPath).Wait();
            //    }
            //    else
            //    {
            //        // File is still not fully available after retries, handle accordingly
            //        Console.WriteLine("File '{0}' is not fully available.", e.FullPath);
            //    }
            //};
            //watcher.Deleted += async (sender, e) =>
            //{
            //    await HandleFileEventAsync(e.FullPath);
            //};
            //watcher.Changed += async (sender, e) => await HandleFileEventAsync(e.FullPath);

            //watcher.Error += (sender, e) =>
            //{
            //    Console.WriteLine($"Network error occurred while monitoring the directory: {e.GetException().Message}");
            //};

            //Console.WriteLine("File monitoring started. Press any key to exit.");
            //Console.ReadKey();

            //if (exitEvent.Set())
            //{
            //    Console.WriteLine("\n\n\n---------------------------------------------------------------------");
            //    Console.WriteLine("{0} files uploaded successfully", uploadedFileCount);
            //    Console.WriteLine("---------------------------------------------------------------------");

            //}

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

                try
                {
                    BlobClient blobClient = containerClient.GetBlobClient(blobName);
                    using FileStream fileStream = File.OpenRead(filePath);
                    await blobClient.UploadAsync(fileStream, true);
                    //Console.WriteLine($"Uploading {noOfFilesUploaded} / {noOfFilesfiles}");
                    noOfFilesUploaded++;
                    Console.WriteLine($"File uploaded :- {filePath}");
                    Console.WriteLine($"Uploaded {noOfFilesUploaded} / {noOfFilesfiles}");
                    //Console.WriteLine($"{noOfFilesUploaded} file uploaded.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    Console.WriteLine("Something went wrong :- {0}", filePath);
                }

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
                    uploadedFileCount++;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error occurred while renaming the file: {ex.Message}");
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
        }
    }
}