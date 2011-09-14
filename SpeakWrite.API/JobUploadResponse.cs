namespace SpeakWrite.API
{
    /// <summary>
    /// Response of job upload & submission process
    /// </summary>
    public class JobUploadResponse : ResponseBase
    {
        /// <summary>
        /// The uniqe, SpeakWrite generated FileName for your transcription job
        /// </summary>
        public string FileName { get; set; }
    }
}