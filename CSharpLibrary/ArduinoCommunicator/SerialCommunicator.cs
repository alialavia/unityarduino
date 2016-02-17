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
                var tempPort = new SerialPortNET(portname, 115200, Parity.None, 8, StopBits.One);
                try
                {
                    connect(tempPort);
                    serialPort = tempPort;
                    break;
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

        #region Public Methods

        public void Close()
        {
            serialPort.Close();
        }

        public void sendCommand(byte[] command)
        {
            serialPort.Write(SerialProtocol.AddCRC(command), 0, command.Length + 1);
        }

        public int sendRequest(byte[] command)
        {
            sendCommand(command);
            byte[] msg = readMessage();
            // sanity check
            if (msg[0] != command[0])
                throw new SerialCommunicatorUnhandledException("Sent and received commands do not match.");
            return ((int)msg[1] * 256 + (int)msg[2]);
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
            serialPort = sp;
            serialPort.Open();
            // Wait for arduino to get ready
            Thread.Sleep(2000);
            // Just to test it's connected to Arduino
            try
            {
                sendRequest(new byte[] { SerialProtocol.ANALOG_READ, (byte)13, (byte)0 });
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
            Debug.Assert(serialPort.BytesToRead >= SerialProtocol.IN_BUFFER_LENGTH);
            serialPort.Read(rawData, 0, SerialProtocol.IN_BUFFER_LENGTH);

            // sanity check
            if (SerialProtocol.IsValidMessage(rawData))
                return rawData.SkipR(1);
            else
                throw new SerialCommunicatorUnhandledException("Bad CRC");
        }

        private void sendACK()
        {
            serialPort.WriteAll(SerialProtocol.AddCRC(SerialProtocol.ACK));
            //serialPort.WriteAll(SerialProtocol.ACK);
        }

        #endregion Private Methods

        //private int OUT_MESSAGE_LENGTH = 18; // bytes

        #region Private Fields

        private const string USBSerialDeviceName = "USBSER";

        // bytes

        private SerialPortNET serialPort;

        #endregion Private Fields
    }
}