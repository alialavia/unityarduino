using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using SerialPortNET;
namespace ArduinoCommunicator
{
    /// <summary>
    /// Communicates with Arduino.
    /// </summary>
    internal class SerialCommunicator
    {
        #region Public Constructors
        // To support multiple connected Arduinos
        private static List<String> connectedPortNames = new List<string>();
        /// <summary>
        /// Manually select Arduino port settings. Use it for multiple boards, or if you have changed Arduino connection settings.
        /// </summary>
        /// <param name="sp">Serial port to connect to</param>
        public SerialCommunicator(SerialPort sp)
        {
            SerialPort = sp;
            try
            {
                connect();
                connectedPortNames.Add(SerialPort.PortName);

            }
            catch (Exception ex)
            {
                SerialPort = null;
            }
        }

        /// <summary>
        /// Automatically finds Arduino Board and connect to it
        /// </summary>
        public SerialCommunicator()
        {
            var portnames = new List<String>();

            foreach (var devicename in SerialPort.EnumerateSerialPorts())
                if (devicename.Key.Contains(USBSerialDeviceName))
                    portnames.Add(devicename.Value);

            foreach (var portname in connectedPortNames)
                portnames.Remove(portname);

            foreach (var portname in portnames)
            {
                SerialPort = new SerialPort(portname, 115200, Parity.Even, 8, StopBits.Two);
                try
                {
                    connect();
                    break;
                }
                catch (Exception ex)
                {
                    SerialPort.Close();
                    SerialPort = null;
                }
            }

            if (SerialPort == null)
                throw new IOException("No arduino board found.");

            connectedPortNames.Add(SerialPort.PortName);
        }

        #endregion Public Constructors

        #region Public Methods

        public void Close()
        {
            SerialPort.Close();
        }

        public int readCommand(SerialProtocol.Commands command, byte pin, byte value = 0)
        {
            byte[] result = sendRequest(createCommand(command, pin, value));
            int val = result[2];
            if (command == SerialProtocol.Commands.ANALOG_READ)
                val = result[1] * 128 + result[2];

            if (val > 1023)
                Debugger.Break();
            return val;
        }

        private void sendCommand(SerialProtocol.Commands command, byte pin, byte value)
        {
            sendMessage(createCommand(command, pin, value));
        }

        public void sendMessage(byte[] message)
        {
            //Debug.Print(message[0])
            SerialPort.WriteAll(SerialProtocol.AddCRC(message));
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Connect to the Serial Port. If no arduino board is found, it would throw and IOException error.
        /// </summary>
        /// <exception cref="IOException"></exception>
        private void connect()
        {
            SerialPort.Open();                                 
            // Wait for arduino to get ready
            Thread.Sleep(2000);
            // Just to test it's connected to Arduino
            try
            {
                readCommand(SerialProtocol.Commands.DIGITAL_READ, 13);
            }
            catch (Exception ex)
            {
                throw new IOException("Cannot connect to Arduino using the given port. Make sure that Arduino is connected, serial port is not in use, and port settings and board name are selected correctly.");
            }
        }

        private byte[] createCommand(SerialProtocol.Commands command, byte pin, byte value)
        {
            byte msb = (byte)((pin << 3) | (value >> 7));
            byte lsb = (byte)(value % 0x80);

            return new byte[] { (byte)command, msb, lsb };
        }

        private byte[] readMessage()
        {
            byte[] rawData = new byte[SerialProtocol.IN_BUFFER_LENGTH];
            SerialPort.Read(rawData, 0, SerialProtocol.IN_BUFFER_LENGTH);

            // sanity check
            if (SerialProtocol.IsValidMessage(rawData))
                return Helper.SkipR(rawData, 1);
            else
                throw new SerialCommunicatorUnhandledException("Bad CRC");
        }

        private void sendACK()
        {
            SerialPort.WriteAll(SerialProtocol.AddCRC(SerialProtocol.ACK));
            //serialPort.WriteAll(SerialProtocol.ACK);
        }

        private byte[] sendRequest(byte[] message)
        {
            byte[] response = { 0x00, 0x00, 0x00 };
            // tries sending the request maximum of TRIALS if it fails
            int i = 0;
            // send and receive should happen synchronously
            lock (syncLock)
            {
                for (; i < TRIALS; i++)
                {
                    sendMessage(message);
                    waitForResponse();
                    response = readMessage();
#if DEBUG
                    if (response[0] == (byte)SerialProtocol.Commands.ANALOG_WRITE)
                        Debug.WriteLine(String.Format("{0}, {1}, {2}", response[0], response[1], response[2]));
#endif
                    // sanity check
                    if (response[0] == message[0])
                    {
                        break;
                    }
                    else
                        continue;   // A NACK or a corrupted NACK is received
                }
            }
            return response;
        }

        private void waitForResponse()
        {
            var t = DateTime.Now;
            while (SerialPort.BytesToRead < 4)
            {
                if (DateTime.Now.Ticks - t.Ticks > serialTimeout)
                    throw new SerialCommunicatorUnhandledException("SerialCommunicator timeout reached.");
            }
        }

        #endregion Private Methods

        #region Public Properties

        public SerialPort SerialPort { get; private set; }

        #endregion Public Properties

        #region Private Fields

        private const long serialTimeout = 1000000;

        //private int OUT_MESSAGE_LENGTH = 18; // bytes
        private const int TRIALS = 5;

        private const string USBSerialDeviceName = "USBSER";
        private object syncLock = new object();

        #endregion Private Fields

        // ticks, or 0.1 of uSec

        // bytes
    }
}