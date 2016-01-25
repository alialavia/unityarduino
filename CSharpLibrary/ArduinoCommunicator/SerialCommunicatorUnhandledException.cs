using System;
using System.IO;
using System.Runtime.Serialization;

namespace ArduinoCommunicator
{
    [Serializable]
    internal class SerialCommunicatorUnhandledException : IOException
    {
        public SerialCommunicatorUnhandledException()
        {
        }

        public SerialCommunicatorUnhandledException(string message) : base(message)
        {
        }

        public SerialCommunicatorUnhandledException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SerialCommunicatorUnhandledException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}