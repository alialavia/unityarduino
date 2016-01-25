using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using ArduinoCommunicator;

namespace ArduinoCommunicator
{
    /// <summary>
    /// Holds state of the Arduino board
    /// </summary>
    public class Arduino
    {        
        public bool IsValid { get; protected set; }

        //public byte[] outBuffer { get; private set; }
        public Queue<byte[]> commandQueue = new Queue<byte[]>();
        /* getters */
        public DigitalValue digitalRead(int pinNumber)
        {
            return digitalValuesIn[pinNumber];
        }
        byte DIGITAL_WRITE = 0x01;
        byte PIN_MODE = 0x02;
        byte ANALOG_WRITE = 0x03;
        public void digitalWrite(int pinNumber, DigitalValue value)
        {
            commandQueue.Enqueue(new byte[] { DIGITAL_WRITE, (byte)pinNumber, (byte)(value == DigitalValue.High ? 1:0) });
        }

        public void analogWrite(int pinNumber, byte value)
        {
            commandQueue.Enqueue(new byte[] { ANALOG_WRITE, (byte)pinNumber, (byte)(value) });
        }

        public void pinMode(int pinNumber, PinMode pinMode)
        {
            commandQueue.Enqueue(new byte[] { PIN_MODE, (byte)pinNumber, (byte)(pinMode) });
        }
        public int analogRead(int pinNumber)
        {
            return analogValuesIn[pinNumber];
        }

        public ArduinoBoard Board { get; private set; }

        SerialCommunicator sc;
        public Arduino(BoardName board, MonoSerialPort serialPort /*, String portName = "COM22", int baudRate = 115200, Parity parity = Parity.Even, int dataBits = 8, StopBits stopBits = StopBits.Two*/)
        {
            //stateBuffer = inBuffer;
            IsValid = false;    
            Board = new ArduinoBoard(board);
            sc = new SerialCommunicator(this, serialPort);
            /* Number of bytes required for the digital pins */
            digitalBytes = (int)Math.Ceiling(Board.NumberOfDigitalPins / 8d);
            digitalValuesIn = new DigitalValue[Board.NumberOfDigitalPins];
            analogValuesIn = new int[Board.NumberOfAnalogPins];
            sc.Start();
            sc.ArduinoStateReceived += Sc_ArduinoStateReceived;
            //outBuffer = new byte[digitalBytes + Board.NumberOfDigitalPins];

        }
        public event EventHandler<ArduinoUpdateEventArgs> ArduinoStateReceived;

        private void Sc_ArduinoStateReceived(object sender, ArduinoUpdateEventArgs e)
        {
            EventHandler<ArduinoUpdateEventArgs> temp = ArduinoStateReceived;
            if (temp != null)
            {
                temp(this, e);
            }
        }

        public void inBuffer(byte[] buffer)
        {            
            Debug.Write(String.Format("{0} {1}", Convert.ToString(buffer[0], 2).PadLeft(8, '0'), Convert.ToString(buffer[1], 2).PadLeft(8, '0')));
            stateBuffer = buffer;
            for (int i = 0; i < Board.NumberOfDigitalPins; i++)
            {
                int B = i / 8, // Byte # for digital pins
                    b = i % 8;                      // bit number 
                digitalValuesIn[i] = ((buffer[B] & (1 << b)) == 0) ? DigitalValue.Low : DigitalValue.High;
            }

            for (int i = 0; i < Board.NumberOfAnalogPins; i++)
            {
                byte B0 = buffer[digitalBytes + i * 2];
                byte B1 = buffer[digitalBytes + i * 2 + 1];

                analogValuesIn[i] = B0*256 + B1;
            }

            /* Behaviours specific to each board which needs to be addressed after buffer is processed */
            switch (Board.BoardName)
            {
                case BoardName.UNO:
                    digitalValuesIn[0] = digitalValuesIn[1] = DigitalValue.Invalid;
                    break;

                default:
                    throw new NotImplementedException("The selected board is not implemented yet.");
            }            
            IsValid = true;
        }

        #region Equals and GetHashCode overrides
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
            return this.stateBuffer[0].GetHashCode() ^ this.stateBuffer[this.stateBuffer.Length-1].GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(String.Format("Board: {0} \r\n", this.Board));
            for (int i = 0; i<Board.NumberOfDigitalPins; i++)            
                sb.Append(String.Format("Digital Pin #{0}: {1} \r\n", i , this.digitalRead(i)));

            for (int i = 0; i < Board.NumberOfAnalogPins; i++)
                sb.Append(String.Format("Analog Pin #{0}: {1} \r\n", i,  this.analogRead(i)));

            return sb.ToString();
        }
        #endregion

        #region Private constants and buffers for holding frame data and state                
        private int digitalBytes = 2;
        private byte[] stateBuffer;
        private DigitalValue[] digitalValuesIn { get; set; }
        private int[] analogValuesIn { get; set; }
        #endregion

    }
}