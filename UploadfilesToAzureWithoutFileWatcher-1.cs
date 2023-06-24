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

        static BlobServiceClient blobServiceClient;

        static BlobContainerClient containerClient;

        // count the number of files uploaded 
        static int noOfFilesUploaded = 0;

        static float uploadingCompleted = 0;

        static int noOfFilesInDirectory;

        static List<string> listOfFilesInDirectory;
        static UploadFilesToAzurePortal()
        {
            try
            {
                // Create Blob service client 
                Console.WriteLine("Connecting to azure portal.......\n");
                blobServiceClient = new BlobServiceClient(connectionString);
                Console.WriteLine("Connected to azure portal.\n");

                Console.WriteLine("Creating blob container.....\n");
                containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                Console.WriteLine("Successfully created blob container client.\n");

                listOfFilesInDirectory = Directory.EnumerateFiles(localDirectory, "*", SearchOption.AllDirectories)
                .Where(file => !file.EndsWith(newExtension)).ToList();

                noOfFilesInDirectory = listOfFilesInDirectory.Count;

                if (noOfFilesInDirectory == 0)
                {
                    Console.WriteLine("No files to upload.");
                    Environment.Exit(0);
                }
                else if (noOfFilesInDirectory == 1)
                {
                    Console.WriteLine($"{noOfFilesInDirectory} file to upload.");
                }
                else
                {
                    Console.WriteLine($"{noOfFilesInDirectory} files to upload.");
                }

                Console.WriteLine("Uploading started......\n");
            }
            catch (System.FormatException formatException)
            {
                Debug.WriteLine(formatException);
                Console.WriteLine("Unable to connect azure portal.\nNetwork error or incorrect connection string\n");
            }
            catch (System.NullReferenceException nullReferenceException)
            {
                Debug.WriteLine(nullReferenceException.ToString());
                Console.WriteLine("Unable to create blob container client.....\n");
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

            foreach (string filePath in listOfFilesInDirectory)
            {
                await UploadFileToBlobAsync(filePath);
            }

            // Function to upload file to Azure Blob storage
            async Task UploadFileToBlobAsync(string filePath)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);

                Console.WriteLine($"Uploading file {noOfFilesUploaded + 1} :- {filePath}");

                try
                {
                    string blobName = Path.GetFileName(filePath);
                    BlobClient blobClient = containerClient.GetBlobClient(blobName);
                    using FileStream fileStream = File.OpenRead(filePath);
                    await blobClient.UploadAsync(fileStream, true);
                    noOfFilesUploaded++;
                    uploadingCompleted = (noOfFilesUploaded * 100 / noOfFilesInDirectory);
                    uploadingFileMessages(filePath);
                    //noOfFilesUploaded++;
                    //uploadingCompleted = (noOfFilesUploaded * 100 / noOfFilesInDirectory);
                    //Console.WriteLine($"File {noOfFilesUploaded} uploaded :- {filePath}");
                    //Console.WriteLine($"Uploaded {noOfFilesUploaded} / {noOfFilesInDirectory}");
                    //Console.WriteLine("\n----------------------------------------------");
                    //Console.WriteLine($"{uploadingCompleted} % uploading completed.");
                    //Console.WriteLine("----------------------------------------------\n");

                }
                catch (Exception ex)
                {
                    uploadingFileMessages(filePath);
                    Debug.WriteLine(ex);
                    Console.WriteLine("\nSomething went wrong :- {0}......\nPlease check network connection.....\n", filePath);
                }

                string renamedFilePath = GetUniqueRenamedFilePath(filePath, fileNameWithoutExtension);

                //RenameFileAsync(filePath, renamedFilePath);
            }

            void uploadingFileMessages(string filePath)
            {
                Console.WriteLine($"File {noOfFilesUploaded} uploaded :- {filePath}");
                Console.WriteLine($"Uploaded {noOfFilesUploaded} / {noOfFilesInDirectory}");
                Console.WriteLine("\n----------------------------------------------");
                Console.WriteLine($"{uploadingCompleted} % uploading completed.");
                Console.WriteLine("----------------------------------------------\n");
            }
            async Task RenameFileAsync(string filePath, string renamedFilePath)
            {
                try
                {
                    if (!File.Exists(renamedFilePath))
                    {
                        await Task.Run(() => File.Move(filePath, renamedFilePath));
                        Console.WriteLine("file is successfully renamed to -> {0}", renamedFilePath);
                    }
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