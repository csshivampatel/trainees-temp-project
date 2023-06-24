using Neuron.Esb.Administration;
using Neuron.Esb.Internal;
using Peregrine.Scheduler.Interfaces.Core;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Neuron.Esb;
using log4net;
using System.Threading;

namespace CustomLogMonitorJob
{
    /// <summary>
    /// Reads tail of a log file and sends email on specific search term using job scheduler of Peregrine MS.
    /// </summary>
    public class LogTailMonitor : IPeregrineJob
    {
        public JobKey JobKey { get; set; }
        public ILog Log { get; set; }
        public ESBConfiguration Configuration { get; set; }
        public SerializableDictionary<string, object> JobDataDictionary { get; set; }
        public string Name { get; set; }
        public string Group { get; set; }
        public string Description { get; set; }
        public string TriggerName { get; set; }
        public string EntityName { get; set; }
        public string MessageId { get; set; }
        public string TransactionID { get; set; }
        public string Response { get; set; }
        public string Request { get; set; }
        public DateTime LastFinishTimeUTC { get; set; }
        public TimeSpan LastExecutionTime { get; set; }
        public string LastException { get; set; }
        public bool IsCanceled { get ; set ; }
        public string CancellationDetail { get ; set; }

        public Task Execute(IJobExecutionContext context)
        {
           
            //arguments in JobDataMap.  Create a free test SMTP account using MailTrap.io for Smtp testing
            var rootLogFolder = context.MergedJobDataMap.GetString("RootLogFolder"); // @"C:\Program Files\Neudesic\Neuron ESB v3\logs\DEFAULT"
            var searchPattern = context.MergedJobDataMap.GetString("SearchPattern"); //"*INTERNAL_ManagementService*.log"
            var searchTerm = context.MergedJobDataMap.GetString("SearchTerm"); //"DEBUG"
            var lineLengthOffset = context.MergedJobDataMap.GetIntValue("LineLengthOffset"); // Gets number of characters before and after search term must be int
            var smtpserver = context.MergedJobDataMap.GetString("SMTPServer");  // "your smtp server"
            var userid = context.MergedJobDataMap.GetString("UserId"); //"youruserid"
            var password = context.MergedJobDataMap.GetString("Password"); //"yourpasword"
            var smtpPort = context.MergedJobDataMap.GetIntValue("smtpPort");  //2525  must be int
            var from = context.MergedJobDataMap.GetString("From");//"john.doe@neudesic.com"
            var to = context.MergedJobDataMap.GetString("To"); //"jane.doe@hotmail.com"
            var tailBytes = context.MergedJobDataMap.GetIntValue("TailBytes"); // No of bytes to read from tail must be int 
            var IsComplete = false;
            
            while (!context.CancellationToken.IsCancellationRequested)
            {
                    var di = new DirectoryInfo(rootLogFolder).GetDirectories().OrderByDescending(d => d.LastWriteTimeUtc).First();

                    var fileList = di.GetFiles(searchPattern);

                    var errorline = string.Empty;

                    foreach (var file in fileList)
                    {

                        string fileName = file.FullName;
                        errorline += fileName;
                        var s = ReadTail(fileName, tailBytes);

                        try
                        {
                            if (s.Contains(searchTerm))
                                errorline += s.Substring(s.LastIndexOf(searchTerm) - lineLengthOffset, s.LastIndexOf("\r\n") - s.LastIndexOf(searchTerm) + lineLengthOffset);
                        }
                        catch //will ignore if the linelength offset is out of bounds. This is for testing and demo purpose
                        { }
                        if (context.CancellationToken.IsCancellationRequested) break;
                    }
                try
                {
                    var smtpClient = new SmtpClient(smtpserver, smtpPort)
                    {
                        Credentials = new NetworkCredential(userid, password),
                        EnableSsl = true
                    };

                    smtpClient.Send(from, to, "Errors in " + searchPattern, errorline);
                } catch
                {
                    Log.Error("Error sending email");
                }
                //Uncomment to Pass some time to allow cancellation testing
                //Thread.Sleep(30000);
                //if you don't have email, comment the above block the following line will put the result in response field
                Response = "Success with Errors found in " + searchPattern + " " + errorline;
                IsComplete = !context.CancellationToken.IsCancellationRequested ? true : false;
                break;
            }
            if (IsComplete)
                return Task.CompletedTask;
            else
            {
               //Do any cleanup heere if required
                IsCanceled = true;
                CancellationDetail = "Log monitor job was cancelled by scheduler";
                Log.Warn(CancellationDetail);
                return Task.CompletedTask;
                //Return this if you want to show the job as failed when it is cancelled.
                //return Task.FromException(new Neuron.Esb.ScheduledJobCancellationException("Job was cancelled by scheduler"));
            }
        }

        static string ReadTail(string filename, int tailBytes)
        {
            string s = string.Empty;
            using (FileStream fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                try
                {
                    // Seek 1024 bytes from the end of the file
                    fs.Seek(tailBytes, SeekOrigin.End);
                    // read 1024 bytes
                    byte[] bytes = new byte[tailBytes];
                    fs.Read(bytes, 0, tailBytes);
                    // Convert bytes to string
                    s = Encoding.Default.GetString(bytes);
                }
                catch
                {

                }
                return s;
            }
        }
    }
}
