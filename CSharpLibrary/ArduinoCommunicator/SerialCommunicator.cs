using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        public SerialCommunicator(SerialPortNET sp)
        {
            connect(sp);
        }

        /// <summary>
        /// Automatically finds Arduino Board and connect to it
        /// </summary>
        /// <param name="arduinoState">Variable to store ArduinoStates</param>
        /// <param name="Async">If you want to work with Asynchronous (event based) or synchronuse (in an update loop e.g. Unity) mode. </param>
        public SerialCommunicator()
        {
            var portnames = new List<String>();

            foreach (var devicename in SerialPortNET.EnumerateSerialPorts())
                if (devicename.Key.Contains(USBSerialDeviceName))
                    portnames.Add(devicename.Value);

            foreach (var portname in portnames)
            {
                var tempPort = new SerialPortNET(portname, 115200, Parity.Even, 8, StopBits.Two);
                try
                {
                    connect(tempPort);
                    SerialPort = tempPort;
                    break;
                }
                catch
                {
                    tempPort.Stop();
                    tempPort.Close();
                }
            }

            if (SerialPort == null)
                throw new IOException("No arduino board found.");
        }

        #endregion Public Constructors

        #region Public Methods

        public void Close()
        {
            SerialPort.Close();
        }

        public void sendCommand(byte command, byte pin, byte value)
        {
            byte msb = (byte)((pin << 3) | (value % 0x80));
            byte lsb = (byte)(value % 0x80);
            sendMessage(new byte[] { command, msb, lsb });
        }

        public void sendMessage(byte[] message)
        {
            SerialPort.Write(SerialProtocol.AddCRC(message), 0, message.Length + 1);
        }

        public int sendRequest(byte[] message)
        {
            byte[] response = { 0x00, 0x00, 0x00 };
            // tries sendin request maximum of TRIALS if it fails
            int i = 0;
            for (; i < TRIALS; i++)
            {
                sendMessage(message);
                response = readMessage();
                // sanity check
                if (response[0] == message[0])
                {
                    break;
                }
                else
                    continue;   // A NACK or a corrupted NACK is received
            }
            var ret = ((int)response[1] * 128 + (int)response[2]);

            Debug.WriteLine(String.Format("{0} {1} {2}", response[0], response[1], response[2]));
            if (ret > 1023 || ret < 0)
                Debugger.Break();
            return (ret);
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Connect to a serial port. If no arduino board is found, it would throw and IOException error.
        /// </summary>
        /// <param name="sp">SerialPortNET to connect to.</param>
        /// <exception cref="IOException"></exception>
        private void connect(SerialPortNET sp)
        {
            SerialPort = sp;
            SerialPort.Open();
            // Wait for arduino to get ready
            Thread.Sleep(2000);
            // Just to test it's connected to Arduino
            try
            {
                sendRequest(new byte[] { SerialProtocol.ANALOG_READ, (byte)0, (byte)0 });
            }
            catch
            {
                throw new IOException("Cannot connect to Arduino using the given port. Make sure that Arduino is connected, serial port is not in use, and port settings and board name are selected correctly.");
            }
        }

        private byte[] readMessage()
        {
            byte[] rawData = new byte[SerialProtocol.IN_BUFFER_LENGTH];
            //var sp = sender as MonoSerialPort;
            SerialPort.Read(rawData, 0, SerialProtocol.IN_BUFFER_LENGTH);

            // sanity check
            if (SerialProtocol.IsValidMessage(rawData))
                return rawData.SkipR(1);
            else
                throw new SerialCommunicatorUnhandledException("Bad CRC");
        }

        private void sendACK()
        {
            SerialPort.WriteAll(SerialProtocol.AddCRC(SerialProtocol.ACK));
            //serialPort.WriteAll(SerialProtocol.ACK);
        }

        #endregion Private Methods

        //private int OUT_MESSAGE_LENGTH = 18; // bytes

        #region Public Properties

        public SerialPortNET SerialPort { get; private set; }

        #endregion Public Properties

        #region Private Fields

        private const int TRIALS = 5;
        private const string USBSerialDeviceName = "USBSER";

        #endregion Private Fields

        // bytes
    }
}