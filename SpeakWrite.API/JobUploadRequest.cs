using System.IO;

namespace SpeakWrite.API
{
    /// <summary>
    /// Request to upload a job
    /// </summary>
    public class JobUploadRequest : RequestBase
    {
        /// <summary>
        /// Required: The source audio file.  This will be sent via https to SpeakWrite for transcription
        /// </summary>
        public FileInfo AudioFile { get; set; }

        /// <summary>
        /// Optional: you can specify a custom file name which SpeakWrite will keep so you can reference
        /// your submitted material against an internally generated identifier.  Note: you can always use
        /// the SpeakWrite FileName as it is gaurenteed to be unique.
        /// </summary>
        public string CustomFileName { get; set; }
    }
}