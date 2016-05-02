using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SpeakWrite.API
{
    public interface IClient
    {
        /// <summary>
        /// Submit a new job via the API to SpeakWrite for transcription
        /// </summary>
        /// <param name="request">The JobUploadRequest to submit</param>
        /// <returns>A response with status of the operation</returns>
        JobUploadResponse UploadJob(JobUploadRequest request);

        /// <summary>
        /// Download a completed job from SpeakWrite's API
        /// </summary>
        /// <param name="request">The request containing information on which job to download</param>
        /// <returns>A response with status of the operation</returns>
        JobDownloadResponse Download(JobDownloadRequest request);
        CompletedJobsResponse GetCompletedJobs(CompletedJobsRequest request);
    }

    /// <summary>
    /// SpeakWrite API Client
    /// </summary>
    public class Client : IClient
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static Uri BaseUri { get; set; }

        /// <summary>
        /// Submit a new job via the API to SpeakWrite for transcription
        /// </summary>
        /// <param name="request">Object containing request parameters</param>
        /// <returns>A response with status of the operation</returns>
        public JobUploadResponse UploadJob(JobUploadRequest request)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create(BaseUri + "submitjob.ashx");
            var formParameters = new NameValueCollection
                                     {
                                         {"applicationid", request.ApplicationID.ToString()},
                                         {"customFilename", request.CustomFileName},
                                         {"accountnumber", request.AccountNumber},
                                         {"pin", request.PIN},
                                         {"isGroupConversation", request.IsGroupConversation.ToString()},               
                                     };
            return UploadFile(httpRequest, request.AudioFile, "audiofile", "audio/mp3", formParameters);
        }

        /// <summary>
        /// Download a completed job from SpeakWrite's API
        /// </summary>
        /// <param name="request">Object containing request parameters</param>
        /// <returns>A response with status of the operation</returns>
        public JobDownloadResponse Download(JobDownloadRequest request)
        {
            var formParameters = new NameValueCollection
                                     {
                                         {"applicationid", request.ApplicationID.ToString()},
                                         {"accountnumber", request.AccountNumber},
                                         {"pin", request.PIN},
                                         {"filetype", request.Type == JobDownloadRequest.DownloadType.Document ? "document" : "audio-source"}
                                     };
            if(String.IsNullOrEmpty(request.FileName))
            {
                if(String.IsNullOrEmpty(request.CustomFileName))
                {
                    throw new ArgumentException("Must provide either FileName or CustomFile name of file to download");
                }
                formParameters.Add("customfilename", request.CustomFileName);
            }
            else
            {
                formParameters.Add("filename", request.FileName);
            }

            var webRequest = CreatePostRequest(BaseUri + "download.ashx", formParameters);
            var webResponse = GetResponse(webRequest);
            if(webResponse.StatusCode != HttpStatusCode.OK)
            {
                return DecodeResponse<JobDownloadResponse>(webResponse);
            }

            using (var responseStream = webResponse.GetResponseStream())
            {
                var outfile = File.Create(request.DestinationFileName); var buffer = new byte[4096];
                var bytesRead = 0;
                while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    outfile.Write(buffer, 0, bytesRead);
                }
            }
            return new JobDownloadResponse();
        }

        /// <summary>
        /// Gets a list of completed jobs which can be downloaded via <see cref="Download"/> Client#Download
        /// </summary>
        /// <param name="request">Object containing request parameters</param>
        /// <returns>A response with status of the operation</returns>
        public CompletedJobsResponse GetCompletedJobs(CompletedJobsRequest request)
        {
            var parameters = new NameValueCollection
                                 {
                                     {"applicationid", request.ApplicationID.ToString()},
                                     {"accountnumber", request.AccountNumber},
                                     {"pin", request.PIN}
                                 };
            if(request.MaxAge.HasValue)
            {
                parameters.Add("maxage", request.MaxAge.Value.ToLongTimeString());
            }
            var address = BaseUri + "completedjobs.ashx";
            Log.DebugFormat("Requesting completed jobs: {0}", address);

            HttpWebRequest webRequest = CreatePostRequest(address, parameters);
            return DecodeResponse<CompletedJobsResponse>(GetResponse(webRequest));
        }

        private static HttpWebRequest CreatePostRequest(string address, NameValueCollection parameters)
        {
            LogFormParameters(parameters);
            var webRequest = (HttpWebRequest)WebRequest.Create(address);
            webRequest.Method = "POST";
            webRequest.KeepAlive = true;
            webRequest.ContentType = "application/x-www-form-urlencoded";
            var encodedParameters = parameters.AllKeys.Select(key => HttpUtility.UrlEncode(key) + "=" + HttpUtility.UrlEncode(parameters[(string) key]));
            var content = String.Join("&", encodedParameters);
            using (var requestStream = webRequest.GetRequestStream())
            {
                Write(requestStream, content);
            }
            return webRequest;
        }

        private static void LogFormParameters(NameValueCollection formparameters)
        {
            if(Log.IsDebugEnabled)
            {
                foreach (var key in formparameters.AllKeys)
                {
                    var value = formparameters[key];
                    Log.DebugFormat("Form Parameter: {0} => {1}", key, key.Equals("pin", StringComparison.OrdinalIgnoreCase) ? "XXXX" : value);
                }
            }
        }

        private static void Write(Stream stream, string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static JobUploadResponse UploadFile(HttpWebRequest request, FileInfo file, string paramName, string contentType, NameValueCollection formValues)
        {
            Log.DebugFormat("UploadFile request to {0}", request.RequestUri);
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Method = "POST";
            request.KeepAlive = true;

            using (var requestStream = request.GetRequestStream())
            {
                LogFormParameters(formValues);
                foreach (string key in formValues.Keys)
                {
                    requestStream.Write(boundarybytes, 0, boundarybytes.Length);
                    var value = HttpUtility.UrlEncode(formValues[key]);
                
                    var paramString = string.Format(
                        "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", HttpUtility.UrlEncode(key), value);
                    Write(requestStream, paramString);
                }
                requestStream.Write(boundarybytes, 0, boundarybytes.Length);


                const string format = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
                string header = string.Format(format, paramName, file.Name, contentType);
            
                Write(requestStream, header);
                Log.DebugFormat("Attaching file: parameter name => {0}, file name => {1}, file size => {2}KB", paramName, file.Name, file.Length/1000);

                using (var fileStream = file.OpenRead())
                {
                    var buffer = new byte[4096];
                    var bytesRead = 0;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        requestStream.Write(buffer, 0, bytesRead);
                    }
                }
            

                Write(requestStream, "\r\n--" + boundary + "--\r\n");
            }

            HttpWebResponse response = GetResponse(request);
            return DecodeResponse<JobUploadResponse>(response);
        }

        private static T DecodeResponse<T>(HttpWebResponse response)
        {
            using (var stream = response.GetResponseStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    var body = reader.ReadToEnd();
                    Log.DebugFormat("Received response content: {0}", body);
                    using (var jsonReader = new JsonTextReader(new StringReader(body)))
                    {
                        var seraizlier = new JsonSerializer{ ContractResolver = new CamelCasePropertyNamesContractResolver() };
                        return seraizlier.Deserialize<T>(jsonReader);
                    }
                }
            }
        }

        private static HttpWebResponse GetResponse(HttpWebRequest request)
        {
            Log.Debug("Getting response");
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch(WebException ex)
            {
                //don't worry about protocol errors (404, 500), but throw everything else
                if(ex.Status != WebExceptionStatus.ProtocolError)
                {
                    throw;
                }
                response = (HttpWebResponse)ex.Response;
            }
            Log.DebugFormat("Received response status: {0}", response.StatusCode);
            return response;
        }
    }
}
