using System;
using System.Collections.Generic;
using SerialPortNET;
using UnityEngine;

namespace UnitySerialPort
{
    internal class SerialPortAndroid : ILowLevelSerialPort
    {
        #region Public Constructors

        public SerialPortAndroid()
        {
            using (AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                activityContext = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
                LowLevelPort = new AndroidJavaObject("com.example.alavis.lowlevelusbserial.LowLevelPort", activityContext);
                LowLevelPort.Call("init");
                serialPort = LowLevelPort.Get<AndroidJavaObject>("serialPort");
            }
        }

        #endregion Public Constructors

        #region Public Methods

        public void Close()
        {
            serialPort.Call("syncClose");
            isOpen = false;
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        public void DisposeUnmanagedResources()
        {

        }

        public void Flush(FlushMode mode)
        {
            switch (mode)
            {
                case FlushMode.Input:
                    inputBuffer.Clear();
                    break;
                case FlushMode.Output:
                    outputBuffer.Clear();
                    break;
                default:
                    inputBuffer.Clear();
                    outputBuffer.Clear();
                    break;
            }
        }

        public Dictionary<string, string> GetPortNames()
        {
            throw new NotImplementedException();
        }

        public void Open()
        {
            isOpen = serialPort.Call<Boolean>("syncOpen");
        }

        public void Read(byte[] buffer, int offset, int count)
        {
            lock (inputBuffer)
            {
                refreshReadBuffer();
                for (int i = 0; i < Math.Min(count, inputBuffer.Count); i++)
                    buffer[i + offset] = inputBuffer.Dequeue();
            }
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            lock (outputBuffer)
            {
                for (int i = offset; i < count; i++)
                    outputBuffer.Enqueue(buffer[i]);
            }
        }

        #endregion Public Methods

        #region Protected Methods

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }
        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SerialPortAndroid() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }
        #endregion Protected Methods

        #region Private Methods

        private void refreshReadBuffer()
        {
            byte[] temp = new byte[DEFAULT_READ_BUFFER_SIZE];
            serialPort.Call("syncRead", temp, TIMEOUT);
            foreach (byte b in temp)
                this.inputBuffer.Enqueue(b);
        }

        #endregion Private Methods

        #region Public Properties

        public int BaudRate
        {
            get
            {
                return serialPort.Call<int>("getBaudRate");
            }

            set
            {
                serialPort.Call("setBaudRate", value);
            }
        }

        public int BytesToRead
        {
            get
            {
                lock (inputBuffer)
                    return inputBuffer.Count;
            }
        }

        public int BytesToWrite
        {
            get
            {
                lock (outputBuffer)
                    return outputBuffer.Count;
            }
        }

        public byte DataBits
        {
            get
            {
                return (byte)serialPort.Call<int>("getDataBits");
            }

            set
            {
                serialPort.Call("setDataBits", (int)value);
            }
        }

        public DtrControl DtrControl
        {
            get
            {
                return (DtrControl)serialPort.Call<int>("getFlowControl");
            }

            set
            {
                serialPort.Call("setFlowControl", (int)value);
            }
        }

        public bool IsOpen
        {
            get
            {
                return isOpen;
            }
        }

        public Parity Parity
        {
            get
            {
                return (Parity)serialPort.Call<int>("getParity");
            }

            set
            {
                serialPort.Call("setParity", (int)value);
            }
        }

        public string PortName
        {
            get
            {
                return LowLevelPort.Get<String>("getDeviceName");
            }

            set
            {

            }
        }

        public StopBits StopBits
        {
            get
            {
                return (StopBits)serialPort.Call<int>("getStopBits");
            }

            set
            {
                serialPort.Call("setStopBits", (int)value);
            }
        }

        #endregion Public Properties

        #region Private Fields

        private const int DEFAULT_READ_BUFFER_SIZE = 16 * 1024;
        private const int DEFAULT_WRITE_BUFFER_SIZE = 16 * 1024;
        private readonly int TIMEOUT = 200; // milliseconds
        private AndroidJavaObject activityContext;
        private bool disposedValue = false;
        private Queue<byte> inputBuffer = new Queue<byte>();
        private bool isOpen;
        // To detect redundant calls
        private AndroidJavaObject LowLevelPort;

        private Queue<byte> outputBuffer = new Queue<byte>();
        private AndroidJavaObject serialPort;

        #endregion Private Fields

        
        
    }
}