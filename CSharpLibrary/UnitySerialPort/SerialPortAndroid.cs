using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using SerialPortNET;
using UnityEngine;
namespace UnitySerialPort
{
    internal class SerialPortAndroid : ILowLevelSerialPort
    {
        #region Public Constructors
        int _baudRate;
        Parity _parity;
        byte _dataBits;
        DtrControl _dtrControl;
        StopBits _stopBits;
        AndroidJavaClass activityClass;
        /// <summary>
        /// How long should I busy wait for USB access, in seconds? 0 for unlimited.
        /// </summary>
        public static int WaitForAccessTimeOut = 30;
        public SerialPortAndroid(bool waitForAccess = true)
        {
            IsOpen = false;
            activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            activityContext = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
            var t = DateTime.Now.Ticks;
            if (waitForAccess)
            {
                while (!AccessGranted)
                {
                    if (WaitForAccessTimeOut > 0 &&
                        new TimeSpan(DateTime.Now.Ticks - t).TotalSeconds > WaitForAccessTimeOut
                        )
                        break;
                };
                // if it wasn't timed out
                if (AccessGranted)
                {
                    serialPort = activityContext.Call<AndroidJavaObject>("getSerialPort");
                    service = activityContext.Call<AndroidJavaObject>("getService");
                }
                else
                    throw new TimeoutException("Access grant timed out!");
            }
        }

        public bool AccessGranted { get { return activityContext.Call<bool>("isUSBAccessGranted"); } }
        public SerialPortAndroid(String portName, int baudRate, Parity parity, byte dataBits, StopBits stopBits) : this()
        {
            // parameters
            this._baudRate = baudRate;
            this._parity = parity;
            this._dataBits = dataBits;
            this._stopBits = stopBits;
            this._dtrControl = DtrControl.Enable; // Default
        }
        #endregion Public Constructors

        #region Public Methods

        public void Close()
        {
            serialPort.Call("close");
            IsOpen = false;
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
                    var temp = new byte[BytesToRead];
                    Read(temp, 0, temp.Length);
                    break;
                case FlushMode.Output:
                    outputBuffer.Clear();
                    break;
                default:
                    Flush(FlushMode.Input);
                    Flush(FlushMode.Output);
                    break;
            }
        }

        public Dictionary<string, string> GetPortNames()
        {
            var temp = new Dictionary<string, string>();
            temp.Add(PortName, "USBSER");
            return temp;
        }

        public void Open()
        {
            IsOpen = serialPort.Call<Boolean>("open");
            BaudRate = _baudRate;
            Parity = _parity;
            DataBits = _dataBits;
            StopBits = _stopBits;
            DtrControl = _dtrControl;
        }

        public void Read(byte[] buffer, int offset, int count)
        {
            var temp = service.Call<byte[]>("getInputBuffer");
            for (int i = 0; i < temp.Length; i++)
                buffer[i] = temp[i];
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            lock (outputBuffer)
            {
                serialPort.Call("write", buffer);
                //TODO: Think about any outputBuffer and is it necessary?
                outputBuffer.Clear();
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



        #endregion Private Methods

        #region Public Properties

        public int BaudRate
        {
            get
            {
                return _baudRate;
            }

            set
            {
                _baudRate = value;
                serialPort.Call("setBaudRate", _baudRate);
            }
        }

        public int BytesToRead
        {
            get
            {
                return service.Call<int>("numberOfBytes");
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
            get { return _dataBits; }
            set
            {
                _dataBits = value;
                serialPort.Call("setDataBits", (int)_dataBits);
            }
        }

        public DtrControl DtrControl
        {
            get { return _dtrControl; }
            set
            {
                _dtrControl = value;
                serialPort.Call("setFlowControl", (int)_dtrControl);
            }
        }

        public bool IsOpen
        {
            get; protected set;
        }

        public Parity Parity
        {
            get { return _parity; }
            set
            {
                _parity = value;
                serialPort.Call("setParity", (int)_parity);
            }
        }

        public string PortName
        {
            get
            {
                return "USBSER";
            }

            set
            {

            }
        }
        private int convertFromStopBits(StopBits sb)
        {
            if (sb == StopBits.One)
                return 1;
            else if (sb == StopBits.Two)
                return 2;
            else
                return 3;


        }

        private StopBits convertToStopBits(int sb)
        {
            if (sb == 1)
                return StopBits.One;
            else if (sb == 2)
                return StopBits.Two;
            else
                return StopBits.OnePointFive;


        }
        public StopBits StopBits
        {
            get { return _stopBits; }
            set
            {
                _stopBits = value;
                serialPort.Call("setStopBits", convertFromStopBits(_stopBits));
            }
        }

        #endregion Public Properties

        #region Private Fields

        private const int DEFAULT_READ_BUFFER_SIZE = 16 * 1024;
        private const int DEFAULT_WRITE_BUFFER_SIZE = 16 * 1024;
        private readonly int TIMEOUT = 200; // milliseconds
        private AndroidJavaObject activityContext;
        private bool disposedValue = false;
        // To detect redundant calls

        private Queue<byte> outputBuffer = new Queue<byte>();
        public AndroidJavaObject serialPort;
        public AndroidJavaObject service;

        #endregion Private Fields



    }
}