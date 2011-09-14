using System;
using System.Configuration;
using System.IO;
using System.Linq;
using log4net;
using log4net.Config;
using SpeakWrite.API;

namespace SpeakWrite.Console
{
    /// <summary>
    /// Reference command line application using the SpeakWrite.API library
    /// </summary>
    class Program
    {
        //get logger
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));
        static void Main(string[] args)
        {
            //default, simple log4net implementation for demo purposes
            BasicConfigurator.Configure();
            //configuration of base client URL (value found in app.config)
            Client.BaseUri = new Uri(ConfigurationManager.AppSettings["API_BASE_URL"]);
            if(args.Length < 1)
            {
                System.Console.WriteLine("Usage: sw upload... / sw completed... / sw download...");
                return;
            }
            try
            {
                var action = args[0].ToLowerInvariant();
                switch (action)
                {
                    case "upload":
                        Upload(args);
                        break;
                    case "completed":
                        Completed(args);
                        break;
                    case "download":
                        Download(args);
                        break;
                    default:
                        System.Console.WriteLine("Unrecognized action: '{0}'", action);
                        break;
                }
            }
            catch(Exception ex)
            {
                Log.Error("Unhandled error", ex);
            }
        }

        private static void Download(string[] args)
        {
            if (args.Length < 4)
            {
                System.Console.WriteLine("Usage: sw download <account_number> <pin> <filename>");
                return;
            }
            //simple download request
            var request = new JobDownloadRequest();
            //your SpeakWrite account number
            request.AccountNumber = args[1];
            //your SpeakWrite account pin
            request.PIN = args[2];
            //the SpeakWrite Filename of the job to download
            request.FileName = args[3];
            //download type is the completed transcription document (this is default)
            request.Type = JobDownloadRequest.DownloadType.Document;
            //where should the download file be saved?
            request.DestinationFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, request.FileName + ".doc");
            //create client instance
            var client = new Client();
            //issue request...currently doing nothing with response
            var response = client.Download(request);
        }

        private static void Completed(string[] args)
        {
            if(args.Length < 3)
            {
                System.Console.WriteLine("Usage: sw completed <account_number> <pin>");
                return;
            }
            var request = new CompletedJobsRequest();
            //your SpeakWrite account number
            request.AccountNumber = args[1];
            //your SpeakWrite pin
            request.PIN = args[2];
            //create client instance
            var client = new Client();
            //issue request
            var response = client.GetCompletedJobs(request);
            //write success to console
            //generally you'd want to do something more interesting with the results
            if(response.Success)
            {
                System.Console.WriteLine("Successfully retrieved {0} completed jobs", response.Jobs.Count());
            }
        }

        private static void Upload(string[] args)
        {
            var request = new JobUploadRequest();
            if(args.Length < 4)
            {
                System.Console.WriteLine("Usage: sw upload <account_number> <pin> <path_to_file> <custom_file_name>(optional)");
                return;
            }
            //your SpeakWrite account number
            request.AccountNumber = args[1];
            //your SpeakWrite account pin
            request.PIN = args[2];
            //FileInfo object pointing to audio file to upload
            request.AudioFile = new FileInfo(args[3]);
            //currently there is little argument checking within the SpeakWrite.API library
            if(!request.AudioFile.Exists)
            {
                throw new FileNotFoundException("Must provide path to valid audio file", request.AudioFile.Name);
            }
            //optional, custom file name which SpeakWrite will track and return
            //with a call to "GetCompletedJobs" so you can use an internal tracking number
            if(args.Length > 4)
            {
                request.CustomFileName = args[4];
            }
            var client = new Client();
            //issue request
            var result = client.UploadJob(request);
            if(result.Success)
            {
                System.Console.WriteLine("Successfully created job with filename " + result.FileName);
            }
        }
    }
}
