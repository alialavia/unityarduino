using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace ArduinoCommunicator
{
    /// <summary>
    /// Communicates with Arduino.
    /// </summary>
    public class SerialCommunicator
    {
        #region Public Constructors

        /// <summary>
        /// Manually select Arduino port settings. Use it for multiple bords, or if you have changed Arduino connection settings.
        /// </summary>
        /// <param name="arduinoState">Variable to store ArduinoStates</param>
        /// <param name="sp"></param>
        /// <param name="Async">If you want to work with Asynchronous (event based) or synchronuse (in an update loop e.g. Unity) mode. </param>
        public SerialCommunicator(Arduino arduinoState, SerialPortNET sp, bool Async) : this(arduinoState)
        {
            connect(sp);
            if (Async)
            {
                serialPort.Async = true;
                serialPort.DataReceived += Sp_DataReceived;
                serialPort.Run();
            }
        }

        /// <summary>
        /// Automatically finds Arduino Board and connect to it
        /// </summary>
        /// <param name="arduinoState">Variable to store ArduinoStates</param>
        /// <param name="Async">If you want to work with Asynchronous (event based) or synchronuse (in an update loop e.g. Unity) mode. </param>
        public SerialCommunicator(Arduino arduinoState, bool Async) : this(arduinoState)
        {
            var portnames = new List<String>();

            foreach (var devicename in SerialPortNET.EnumerateSerialPorts())
                if (devicename.Key.Contains(USBSerialDeviceName))
                    portnames.Add(devicename.Value);

            foreach (var portname in portnames)
            {
                var tempPort = new SerialPortNET(portname, 115200, Parity.None, 8, StopBits.One);
                try
                {
                    connect(tempPort);
                    serialPort = tempPort;
                }
                catch
                {
                    tempPort.Stop();
                    tempPort.Close();
                }
            }

            if (serialPort == null)
                throw new IOException("No arduino board found.");
        }

        #endregion Public Constructors

        #region Public Events

        public event EventHandler<ArduinoUpdatesEventArgs> ArduinoStateReceived;

        #endregion Public Events

        #region Public Methods

        public void Close()
        {
            serialPort.Close();
        }

        public void getStates()
        {
            sendACK();
            UpdateState();
        }

        public void Refresh()
        {
            // Two refreshes cannot be called simultanously
            lock (sync)
            {
                getStates();
                sendCommands();
            }
        }

        public void sendCommands()
        {
            while (ArduinoState.commandQueue.Count > 0)
            {
                byte[] nextCommand = ArduinoState.commandQueue.Dequeue();
                serialPort.Write(nextCommand, 0, nextCommand.Length);
            }
        }

        #endregion Public Methods

        #region Protected Methods

        protected virtual void OnArduinoStateReceived(ArduinoUpdatesEventArgs e)
        {
            EventHandler<ArduinoUpdatesEventArgs> temp = ArduinoStateReceived;
            if (temp != null)
            {
                temp(this, e);
            }
        }

        #endregion Protected Methods

        #region Private Fields

        private const string USBSerialDeviceName = "USBSER";

        // bytes
        private readonly object serialLock = new object();

        private Arduino ArduinoState;

        private Queue<byte> dataFrame = new Queue<byte>(SerialProtocol.IN_MESSAGE_LENGTH);

        private SerialPortNET serialPort;

        private object sync = new object();

        #endregion Private Fields

        #region Private Constructors

        private SerialCommunicator(Arduino arduinoState)
        {
            ArduinoState = arduinoState;
        }

        #endregion Private Constructors

        #region Private Methods

        /// <summary>
        /// Connect to a serial port. If no arduino board is found, it would throw and IOException error.
        /// </summary>
        /// <param name="sp">SerialPortNET to connect to.</param>
        /// <exception cref="IOException"></exception>
        private void connect(SerialPortNET sp)
        {
            serialPort = sp;
            serialPort.ReceivedBytesThreshold = SerialProtocol.IN_MESSAGE_LENGTH;
            serialPort.Open();
            // Wait for arduino to get ready
            Thread.Sleep(2000);
            getStates();
            if (!ArduinoState.IsValid)
                throw new IOException("Cannot find connect to Arduino using the given port and state. Make sure that Arduino is connected, serial port is not in use, and port settings and board name are selected correctly.");
        }

        //private int OUT_MESSAGE_LENGTH = 18; // bytes

        private void sendACK()
        {
            serialPort.WriteAll(SerialProtocol.ACK);
        }

        private void sendBOF()
        {
            serialPort.WriteAll(SerialProtocol.BOF);
        }

        private void Sp_DataReceived(object sender, SerialPortNETDataReceivedEventArgs e)
        {
            if (e.EventType == SerialData.Eof)
                return;
            Refresh();
        }

        private void UpdateState()
        {
            byte[] rawData = new byte[SerialProtocol.IN_MESSAGE_LENGTH];
            //var sp = sender as MonoSerialPort;
            Debug.Assert(serialPort.BytesToRead >= SerialProtocol.IN_MESSAGE_LENGTH);
            serialPort.Read(rawData, 0, SerialProtocol.IN_MESSAGE_LENGTH);

            // sanity check
            if (SerialProtocol.IsValidMessage(rawData))
            {
                ArduinoState.inBuffer(SerialProtocol.getData(rawData));
                OnArduinoStateReceived(new ArduinoUpdatesEventArgs(ArduinoState));
            }
        }

        #endregion Private Methods
    }
}