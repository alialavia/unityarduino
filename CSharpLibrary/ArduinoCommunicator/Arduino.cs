using System;
using System.Text;

namespace ArduinoCommunicator
{
    /// <summary>
    /// Holds state of the Arduino board
    /// </summary>
    public class Arduino
    {
        #region Public Constructors

        public Arduino(BoardName board, SerialPortNET serialPort) : this(board)
        {
            sc = new SerialCommunicator(serialPort);
        }

        #endregion Public Constructors

        #region Private Constructors

        private Arduino(BoardName board)
        {
            IsConnected = false;
            Board = new ArduinoBoard(board);
        }

        #endregion Private Constructors

        #region Public Methods

        public int analogRead(int pinNumber)
        {
            return sc.sendRequest(new byte[] { SerialProtocol.ANALOG_READ, (byte)pinNumber, (byte)0 });
        }

        public void analogWrite(int pinNumber, byte value)
        {
            sc.sendCommand(new byte[] { SerialProtocol.ANALOG_WRITE, (byte)pinNumber, value });
        }

        public void Close()
        {
            sc.Close();
        }

        public DigitalValue digitalRead(int pinNumber)
        {
            return (DigitalValue)sc.sendRequest(new byte[] { SerialProtocol.DIGITAL_READ, (byte)pinNumber, (byte)0 });
        }

        public void digitalWrite(int pinNumber, DigitalValue value)
        {
            sc.sendCommand(new byte[] { SerialProtocol.DIGITAL_WRITE, (byte)pinNumber, value });
        }

        public void pinMode(int pinNumber, PinMode pinMode)
        {
            sc.sendCommand(new byte[] { SerialProtocol.PIN_MODE, (byte)pinNumber, (byte)(pinMode) });
        }

        #endregion Public Methods

        #region Public Properties

        public ArduinoBoard Board { get; private set; }

        public bool IsConnected { get; protected set; }

        #endregion Public Properties

        #region Private Fields

        private SerialCommunicator sc;

        #endregion Private Fields
    }
}