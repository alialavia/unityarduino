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
                throw new IOException("Cannot connect to arduino on port" + serialPort.portName);
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
            var ret = sc.sendRequest(new byte[] { SerialProtocol.ANALOG_READ, (byte)(pinNumber << 3), (byte)0 });
            if (ret > 1023 || ret < 0)
                throw new ArgumentOutOfRangeException(pinNumber.ToString(), ret.ToString());
            return ret;
        }
        
        public void analogWrite(int pinNumber, byte value)
        {
            sc.sendCommand(SerialProtocol.ANALOG_WRITE, (byte)pinNumber, value);
        }


        public DigitalValue digitalRead(int pinNumber)
        {            
            return (DigitalValue)sc.sendRequest(new byte[] { SerialProtocol.DIGITAL_READ, (byte)(pinNumber << 3), (byte)0 });
        }

        public void digitalWrite(int pinNumber, DigitalValue value)
        {
            sc.sendCommand(SerialProtocol.DIGITAL_WRITE, (byte)pinNumber, value);
        }

        public void pinMode(int pinNumber, PinMode pinMode)
        {
            sc.sendCommand(SerialProtocol.PIN_MODE, (byte)pinNumber, (byte)(pinMode));
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