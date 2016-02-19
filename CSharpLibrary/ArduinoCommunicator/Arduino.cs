using System;
using System.IO;
using System.Text;

namespace ArduinoCommunicator
{
    /// <summary>
    /// Holds state of the Arduino board
    /// </summary>
    public class Arduino
    {
        #region Public Constructors

        public Arduino(BoardName board, SerialPortNET serialPort)
        {
            this.SerialPort = serialPort;
            Board = new ArduinoBoard(board);
            analogBuffer = new int[Board.NumberOfAnalogPins];
            IsConnected = false;
            try
            {
                if (this.SerialPort == null)
                {
                    sc = new SerialCommunicator();
                    this.SerialPort = sc.SerialPort;
                }
                else
                    sc = new SerialCommunicator(this.SerialPort);
            }
            catch
            {
                if (serialPort == null)
                    throw new IOException("Cannot find an Arduino connected to this computer.");
                else
                    throw new IOException("Cannot connect to Arduino on port " + serialPort.portName);
            }
            IsConnected = true;
        }

        public Arduino(BoardName board) : this(board, null)
        {
        }

        #endregion Public Constructors

        #region Public Methods

        public int analogRead(int pinNumber)
        {
            try
            {
                analogBuffer[pinNumber] = analogReadEx(pinNumber);
            }
            catch (ArgumentOutOfRangeException)
            {

            }

            return analogBuffer[pinNumber];
        }

        public int analogReadEx(int pinNumber)
        {
            var ret = sc.readCommand(SerialProtocol.Commands.ANALOG_READ, (byte)pinNumber);
            if (ret > 1023 || ret < 0)
                throw new ArgumentOutOfRangeException(pinNumber.ToString(), ret.ToString());
            return ret;
        }
        
        public void analogWrite(int pinNumber, byte value)
        {
            sc.readCommand(SerialProtocol.Commands.ANALOG_WRITE, (byte)pinNumber, value);
        }


        public DigitalValue digitalRead(int pinNumber)
        {            
            return (DigitalValue)sc.readCommand(SerialProtocol.Commands.DIGITAL_READ, (byte)pinNumber);
        }

        public void digitalWrite(int pinNumber, DigitalValue value)
        {
            sc.readCommand(SerialProtocol.Commands.DIGITAL_WRITE, (byte)pinNumber, value);
        }

        public void pinMode(int pinNumber, PinMode pinMode)
        {
            sc.readCommand(SerialProtocol.Commands.PIN_MODE, (byte)pinNumber, (byte)(pinMode));
        }

        #endregion Public Methods

        public void Close()
        {
            sc.Close();
        }
        #region Private Methods

        private void Connect(SerialPortNET serialPort)
        {
        }

        #endregion Private Methods

        #region Public Properties

        public ArduinoBoard Board { get; private set; }
        public bool IsConnected { get; protected set; }
        public SerialPortNET SerialPort { get; private set; }

        #endregion Public Properties

        #region Private Fields

        private int[] analogBuffer;
        private SerialCommunicator sc;

        #endregion Private Fields
    }
}