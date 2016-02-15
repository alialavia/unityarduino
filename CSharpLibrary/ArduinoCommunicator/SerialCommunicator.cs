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
                if (devicename.Key.Contains("USBSER"))
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

        public event EventHandler<ArduinoUpdateEventArgs> ArduinoStateReceived;

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

        public void Start()
        {
            //sendACK();
        }

        #endregion Public Methods

        #region Protected Methods

        protected virtual void OnArduinoStateReceived(ArduinoUpdateEventArgs e)
        {
            EventHandler<ArduinoUpdateEventArgs> temp = ArduinoStateReceived;
            if (temp != null)
            {
                temp(this, e);
            }
        }

        #endregion Protected Methods

        #region Private Fields

        private const int IN_MESSAGE_LENGTH = 16;

        // bytes
        private readonly object serialLock = new object();

        private byte ACK = 0xfe;

        private Arduino ArduinoState;

        private byte BOF = 0xff;

        private Queue<byte> dataFrame = new Queue<byte>(IN_MESSAGE_LENGTH);

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

        private void connect(SerialPortNET sp)
        {
            serialPort = sp;
            serialPort.ReceivedBytesThreshold = IN_MESSAGE_LENGTH;
            serialPort.Open();
            // Wait for arduino to get ready
            Thread.Sleep(2000);
            getStates();
            if (!ArduinoState.IsValid)
                throw new IOException("Cannot find connect to Arduino using the given port and state. Make sure that Arduino is connected, serial port is not in use, and port settings and board name are selected correctly.");
        }

        private byte Crc8(byte[] data, int len)
        {
            //const byte *data = vptr;
            uint crc = 0;
            int i, j;
            for (j = 0; j < len; j++)
            {
                crc ^= (uint)(data[j] << 8);
                for (i = 8; i > 0; i--)
                {
                    if ((crc & 0x8000) != 0)
                        crc ^= (0x1070 << 3);
                    crc <<= 1;
                }
            }
            return (byte)(crc >> 8);
        }
        //private int OUT_MESSAGE_LENGTH = 18; // bytes

        private void sendACK()
        {
            serialPort.Write(new byte[] { ACK, 0xFF, 0xFF }, 0, 3);
        }

        private void sendBOF()
        {
            serialPort.Write(new byte[] { BOF }, 0, 1);
        }

        private void Sp_DataReceived(object sender, SerialPortNETDataReceivedEventArgs e)
        {
            if (e.EventType == SerialData.Eof)
                return;
            Refresh();
        }
        private void UpdateState()
        {
            byte[] temp = new byte[IN_MESSAGE_LENGTH];
            //var sp = sender as MonoSerialPort;
            Debug.Assert(serialPort.BytesToRead >= IN_MESSAGE_LENGTH);
            serialPort.Read(temp, 0, IN_MESSAGE_LENGTH);

            byte[] data = new byte[IN_MESSAGE_LENGTH - 2];
            for (int i = 0; i < data.Length; i++)
                data[i] = temp[i + 1];

            // sanity check
            if (temp[0] == BOF && Crc8(data, IN_MESSAGE_LENGTH - 2) == temp[IN_MESSAGE_LENGTH - 1])
            {
                ArduinoState.inBuffer(data);
                OnArduinoStateReceived(new ArduinoUpdateEventArgs(ArduinoState));
            }
        }

        #endregion Private Methods
    }
}