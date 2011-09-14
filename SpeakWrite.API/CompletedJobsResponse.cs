using System.Collections.Generic;

namespace SpeakWrite.API
{
    /// <summary>
    /// Response containing information about the CompletedJobRequest
    /// </summary>
    public class CompletedJobsResponse : ResponseBase
    {
        /// <summary>
        /// Enumerable of jobs which are completed and can be downloaded
        /// </summary>
        public IEnumerable<CompletedJob> Jobs { get; set; }

        /// <summary>
        /// Completed Job data object
        /// </summary>
        public class CompletedJob
        {
            /// <summary>
            /// The custome file name supplied during creation of this job
            /// note: this can be used by you to track files within your system
            /// </summary>
            public string CustomFileName { get; set; }

            /// <summary>
            /// The uniqe, SpeakWrite assigned filename for this job
            /// </summary>
            public string FileName { get; set; }

            /// <summary>
            /// The account number of the owner of this job
            /// </summary>
            public string AccountNumber { get; set; }
        }
    }
}