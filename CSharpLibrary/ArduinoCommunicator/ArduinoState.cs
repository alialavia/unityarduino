using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ArduinoCommunicator
{
    /// <summary>
    /// Holds state of the Arduino board
    /// </summary>
    public class Arduino
    {
        #region Public Fields

        //public byte[] outBuffer { get; private set; }
        public Queue<byte[]> commandQueue = new Queue<byte[]>();

        #endregion Public Fields

        #region Public Constructors

        public Arduino(BoardName board, SerialPortNET serialPort, bool Async) : this(board)
        {
            sc = new SerialCommunicator(this, serialPort, Async);
            sc.ArduinoStateReceived += Sc_ArduinoStateReceived;
        }

        public Arduino(BoardName board, bool Async) : this(board)
        {
            sc = new SerialCommunicator(this, Async);
            sc.ArduinoStateReceived += Sc_ArduinoStateReceived;
        }

        #endregion Public Constructors

        #region Public Events

        public event EventHandler<ArduinoUpdateEventArgs> ArduinoStateReceived;

        #endregion Public Events

        #region Public Properties

        public ArduinoBoard Board { get; private set; }
        public bool IsValid { get; protected set; }

        #endregion Public Properties

        #region Public Methods

        public int analogRead(int pinNumber)
        {
            return analogValuesIn[pinNumber];
        }

        public void analogWrite(int pinNumber, byte value)
        {
            commandQueue.Enqueue(new byte[] { ANALOG_WRITE, (byte)pinNumber, value });
        }

        public void Close()
        {
            sc.Close();
        }

        public DigitalValue digitalRead(int pinNumber)
        {
            return DigitalValueIn[pinNumber];
        }

        public void digitalWrite(int pinNumber, DigitalValue value)
        {
            commandQueue.Enqueue(new byte[] { DIGITAL_WRITE, (byte)pinNumber, value });
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is Arduino))
                return false;
            var o = obj as Arduino;
            for (int i = 0; i < o.stateBuffer.Length; i++)
                if (o.stateBuffer[i] != this.stateBuffer[i])
                    return false;

            return true;
        }

        public override int GetHashCode()
        {
            return this.stateBuffer[0].GetHashCode() ^ this.stateBuffer[this.stateBuffer.Length - 1].GetHashCode();
        }

        public void getStates()
        {
            sc.getStates();
        }

        public void inBuffer(byte[] buffer)
        {
            IsValid = false;
            Debug.Write(String.Format("{0} {1}", Convert.ToString(buffer[0], 2).PadLeft(8, '0'), Convert.ToString(buffer[1], 2).PadLeft(8, '0')));
            stateBuffer = buffer;
            for (int i = 0; i < Board.NumberOfDigitalPins; i++)
            {
                int B = i / 8, // Byte # for digital pins
                    b = i % 8;                      // bit number
                DigitalValueIn[i] = ((buffer[B] & (1 << b)) == 0) ? DigitalValue.Low : DigitalValue.High;
            }

            for (int i = 0; i < Board.NumberOfAnalogPins; i++)
            {
                byte B0 = buffer[digitalBytes + i * 2];
                byte B1 = buffer[digitalBytes + i * 2 + 1];

                analogValuesIn[i] = B0 * 256 + B1;
            }

            /* Behaviours specific to each board which needs to be addressed after buffer is processed */
            switch (Board.BoardName)
            {
                case BoardName.UNO:
                    DigitalValueIn[0] = DigitalValueIn[1] = DigitalValue.Invalid;
                    break;

                default:
                    throw new NotImplementedException("The selected board is not implemented yet.");
            }
            IsValid = true;
        }

        public void pinMode(int pinNumber, PinMode pinMode)
        {
            commandQueue.Enqueue(new byte[] { PIN_MODE, (byte)pinNumber, (byte)(pinMode) });
        }

        public void Refresh()
        {
            sc.Refresh();
        }

        public void sendCommands()
        {
            sc.sendCommands();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(String.Format("Board: {0} \r\n", this.Board));
            for (int i = 0; i < Board.NumberOfDigitalPins; i++)
                sb.Append(String.Format("Digital Pin #{0}: {1} \r\n", i, this.digitalRead(i)));

            for (int i = 0; i < Board.NumberOfAnalogPins; i++)
                sb.Append(String.Format("Analog Pin #{0}: {1} \r\n", i, this.analogRead(i)));

            return sb.ToString();
        }

        #endregion Public Methods

        #region Private Fields

        private const byte ANALOG_WRITE = 0x03;
        private const byte DIGITAL_WRITE = 0x01;
        private const byte PIN_MODE = 0x02;

        private int digitalBytes = 2;
        private SerialCommunicator sc;
        private byte[] stateBuffer;

        #endregion Private Fields

        #region Private Constructors

        private Arduino(BoardName board)
        {
            IsValid = false;
            Board = new ArduinoBoard(board);
            /* Number of bytes required for the digital pins */
            digitalBytes = (int)Math.Ceiling(Board.NumberOfDigitalPins / 8d);
            DigitalValueIn = new DigitalValue[Board.NumberOfDigitalPins];
            analogValuesIn = new int[Board.NumberOfAnalogPins];
        }

        #endregion Private Constructors

        #region Private Properties

        private int[] analogValuesIn { get; set; }

        private DigitalValue[] DigitalValueIn { get; set; }

        #endregion Private Properties

        #region Private Methods

        private void Sc_ArduinoStateReceived(object sender, ArduinoUpdateEventArgs e)
        {
            EventHandler<ArduinoUpdateEventArgs> temp = ArduinoStateReceived;
            if (temp != null)
            {
                temp(this, e);
            }
        }

        #endregion Private Methods
    }
}