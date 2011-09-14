using System;

namespace SpeakWrite.API
{
    public abstract class RequestBase
    {

        private Guid _applicationId = new Guid("4ab34c28-5306-4e47-ba35-827e81e478f8");
        /// <summary>
        /// ApplicationID used to identify requesting application.  Default is provided.
        /// </summary>
        public Guid ApplicationID
        {
            get { return _applicationId; }
            set{ _applicationId = value; }
        }

        /// <summary>
        /// Your SpeakWrite account number (you can obtain a SpeakWrite account by signing up)
        /// </summary>
        public string AccountNumber { get; set; }

        /// <summary>
        /// Your SpeakWrite PIN number / password
        /// </summary>
        public string PIN { get; set; }
    }
}
