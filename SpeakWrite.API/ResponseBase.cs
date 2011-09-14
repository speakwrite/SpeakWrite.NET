namespace SpeakWrite.API
{
    public abstract class ResponseBase
    {
        /// <summary>
        /// True if succesful, False if failure.  If False the Message property will have more information
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Nulll if response was sucessful.  Otherwise, should contain more detailed error message
        /// </summary>
        public string Message { get; set; }
    }
}