using System;
using System.IO;
using System.Runtime.Serialization;

namespace ArduinoCommunicator
{
    [Serializable]
    internal class SerialCommunicatorUnhandledException : IOException
    {
        #region Public Constructors

        public SerialCommunicatorUnhandledException()
        {
        }

        public SerialCommunicatorUnhandledException(string message) : base(message)
        {
        }

        public SerialCommunicatorUnhandledException(string message, Exception innerException) : base(message, innerException)
        {
        }

        #endregion Public Constructors

        #region Protected Constructors

        protected SerialCommunicatorUnhandledException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        #endregion Protected Constructors
    }
}