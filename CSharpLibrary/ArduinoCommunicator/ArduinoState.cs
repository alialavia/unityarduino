using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ClassLibrary1;

namespace ArduinoCommunicator
{
    /// <summary>
    /// Holds state of the Arduino board
    /// </summary>
    public class ArduinoState
    {
        public bool IsValid { get; protected set; }        
        public BoardName Board { get; private set; }
        public int NumberOfDigitalPins { get; private set; }
        public int NumberOfAnalogPins { get; private set; }

        /* getters */
        public DigitalValue digitalRead(int pinNumber)
        {
            return digitalValues[pinNumber];
        }

        public int analogRead(int pinNumber)
        {
            return analogValues[pinNumber];
        }

        public ArduinoState(byte[] buffer, BoardName board = BoardName.UNO)
        {
            stateBuffer = buffer;
            IsValid = false;    
            Board = board;

            switch (board)
            {
                case BoardName.UNO:
                    NumberOfDigitalPins = 14;
                    NumberOfAnalogPins = 6;
                    break;

                default:
                    throw new NotImplementedException("The selected board is not implemented yet.");
            }

            digitalBytes = (int)Math.Ceiling(NumberOfDigitalPins / 8d);
            analogBufferOffset = digitalBufferOffset + digitalBytes;
            digitalValues = new DigitalValue[NumberOfDigitalPins];
            analogValues = new int[NumberOfAnalogPins];

            processBuffer(buffer);
            IsValid = true;
        }

        private void processBuffer(byte[] buffer)
        {
            Debug.Write(String.Format("{0} {1}", Convert.ToString(buffer[digitalBufferOffset], 2).PadLeft(8, '0'), Convert.ToString(buffer[digitalBufferOffset+1], 2).PadLeft(8, '0')));

            stateBuffer = buffer;
            for (int i = 0; i < NumberOfDigitalPins; i++)
            {
                int B = i / 8, // Byte # for digital pins
                    b = i % 8;                      // bit number 
                digitalValues[i] = ((buffer[B + digitalBufferOffset] & (1 << b)) == 0) ? DigitalValue.Low : DigitalValue.High;
            }

            for (int i = 0; i < NumberOfAnalogPins; i++)
            {
                byte B0 = buffer[analogBufferOffset + i * 2];
                byte B1 = buffer[analogBufferOffset + i * 2 + 1];

                analogValues[i] = B0*256 + B1;
            }

            /* Behaviours specific to each board which needs to be addressed after buffer is processed */
            switch (Board)
            {
                case BoardName.UNO:
                    digitalValues[0] = digitalValues[1] = DigitalValue.Invalid;
                    break;

                default:
                    throw new NotImplementedException("The selected board is not implemented yet.");
            }
        }


        #region Equals and GetHashCode overrides
        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is ArduinoState))
                return false;            

            return (obj as ArduinoState).stateBuffer.SequenceEqual(this.stateBuffer);
        }

        public override int GetHashCode()
        {
            return this.stateBuffer.First().GetHashCode() ^ this.stateBuffer.Last().GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(String.Format("Board: {0} \r\n", this.Board));
            for (int i = 0; i<NumberOfDigitalPins; i++)            
                sb.Append(String.Format("Digital Pin #{0}: {1} \r\n", i , this.digitalRead(i)));

            for (int i = 0; i < NumberOfAnalogPins; i++)
                sb.Append(String.Format("Analog Pin #{0}: {1} \r\n", i,  this.analogRead(i)));

            return sb.ToString();
        }
        #endregion

        #region Private constants and buffers for holding frame data and state
        private const int frameHeaderLength = 1;
        private const int digitalBufferOffset = frameHeaderLength;
        private int digitalBytes = 2;
        private int analogBufferOffset = 3;
        private byte[] stateBuffer;
        private DigitalValue[] digitalValues { get; set; }
        private int[] analogValues { get; set; }
        #endregion

    }
}