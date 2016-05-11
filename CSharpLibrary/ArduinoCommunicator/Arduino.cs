using System;
using System.IO;
using SerialPortNET;

namespace ArduinoCommunicator
{
    /// <summary>
    /// Holds state of the Arduino board
    /// </summary>

    public class Arduino<T> where T : class, ISerialPort, new()
    {
        #region Public Constructors
        /// <summary>
        /// Creates a new instance of Arduino board with the specified board type and connects to it on the serialPort.
        /// </summary>
        /// <param name="board">Arduino board type</param>
        /// <param name="serialPort">Serial port to which the Arduino is connected</param>
        /// <exception cref="IOException">Throws IOException if connection fails.</exception>
        public Arduino(BoardType board, T serialPort)
        {
            this.SerialPort = serialPort;
            BoardType = new BoardInfo(board);
            analogBuffer = new int[BoardType.NumberOfAnalogInputPins];
            IsConnected = false;
            try
            {
                if (this.SerialPort == null)
                {
                    sc = new SerialCommunicator<T>();
                    this.SerialPort = sc.serialPort;
                }
                else
                    sc = new SerialCommunicator<T>(this.SerialPort);
            }
            catch (IOException)
            {
                if (serialPort == null)
                    throw new IOException("Cannot find an Arduino connected to this computer.");
                else
                    throw new IOException("Cannot connect to Arduino on port " + serialPort.PortName);

            }

            IsConnected = true;
        }

        /// <summary>
        /// Creates a new instance of Arduino board with the specified board type, and automatically connects.
        /// </summary>
        /// <param name="board">Arduino board name</param>
        /// <exception cref="IOException">Throws IOException if connection fails.</exception>
        public Arduino(BoardType board = ArduinoCommunicator.BoardType.UNO) : this(board, (T)null)
        {

        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>
        /// Read the analog value on pinNumber. If an error occurs, it will return the previously successfully read number, or zero if no successful previous analogRead operation.
        /// </summary>
        /// <param name="pinNumber">Number of analog pin to read from (Usually written as A0, A1, ... on the board)</param>
        /// <returns>An integer value between 0 to 1023</returns>
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


        private int analogReadEx(int pinNumber)
        {
            var ret = sc.readCommand(SerialProtocol.Commands.ANALOG_READ, (byte)pinNumber);
            if (ret > 1023 || ret < 0)
                throw new ArgumentOutOfRangeException(pinNumber.ToString(), ret.ToString());
            return ret;
        }

        /// <summary>
        /// Writes to the analog pin.
        /// </summary>
        /// <param name="pinNumber">Pin number.</param>
        /// <param name="value">Analog value to write to.</param>
        public void analogWrite(int pinNumber, byte value)
        {
            sc.readCommand(SerialProtocol.Commands.ANALOG_WRITE, (byte)pinNumber, value);
        }

        /// <summary>
        /// Reads digital value of the pin.
        /// </summary>
        /// <param name="pinNumber">Pin number to read its digital value.</param>
        /// <returns>DigitalValue read on the pin.</returns>
        public DigitalValue digitalRead(int pinNumber)
        {
            return sc.readCommand(SerialProtocol.Commands.DIGITAL_READ, (byte)pinNumber);
        }

        /// <summary>
        /// Writes digital value to the pin.
        /// </summary>
        /// <param name="pinNumber">Digital pin to write to.</param>
        /// <param name="value">Digital value to write to the pin.</param>
        public void digitalWrite(int pinNumber, DigitalValue value)
        {
            sc.readCommand(SerialProtocol.Commands.DIGITAL_WRITE, (byte)pinNumber, value);
        }

        /// <summary>
        /// Sets pin mode.
        /// </summary>
        /// <param name="pinNumber">Pin number to set the mode on.</param>
        /// <param name="pinMode">Either PinMode.OUTPUT, PinMode.INPUT, or PinMode.INPUT_PULLUP</param>
        public void pinMode(int pinNumber, PinMode pinMode)
        {
            sc.readCommand(SerialProtocol.Commands.PIN_MODE, (byte)pinNumber, (byte)(pinMode));
        }

        #endregion Public Methods

        /// <summary>
        /// Close the connection.
        /// </summary>
        public void Close()
        {
            sc.Close();
        }
        #region Private Methods

        #endregion Private Methods

        #region Public Properties

        /// <summary>
        /// Board type of this instance 
        /// </summary>
        public BoardInfo BoardType { get; private set; }

        /// <summary>
        /// True if instance is connected to Arduino board. False otherwise.
        /// </summary>
        public bool IsConnected { get; protected set; }

        /// <summary>
        /// The serial port object that Arduino is connected to. Null if not connected.
        /// </summary>
        public T SerialPort { get; private set; }


        #endregion Public Properties

        #region Private Fields

        private int[] analogBuffer;
        private SerialCommunicator<T> sc;

        #endregion Private Fields
    }

    /// <summary>
    /// Arduino class with a SerialPort object as its underlying communication class
    /// </summary>
    public class Arduino : Arduino<SerialPort>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="board"></param>
        public Arduino(BoardType board = ArduinoCommunicator.BoardType.UNO) : base(board) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="board"></param>
        /// <param name="serialPort"></param>
        public Arduino(BoardType board, SerialPort serialPort) : base(board, serialPort) { }


    }
}