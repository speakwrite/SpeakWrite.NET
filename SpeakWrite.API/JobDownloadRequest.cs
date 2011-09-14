namespace SpeakWrite.API
{
    /// <summary>
    /// Request to download a job
    /// </summary>
    public class JobDownloadRequest : RequestBase
    {
        /// <summary>
        /// The unique, SpeakWrite assigned filename of this job
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Path to local file name to create and download the remote file into
        /// </summary>
        public string DestinationFileName { get; set; }

        /// <summary>
        /// Type of download
        /// </summary>
        public DownloadType Type { get; set; }

        /// <summary>
        /// Type of completed job file to download
        /// </summary>
        public enum DownloadType
        {
            /// <summary>
            /// Default: download the completed, typed document
            /// </summary>
            Document = 0,

            /// <summary>
            /// Download the submitted source audio
            /// </summary>
            SourceAudio = 1
        }
    }
}