using System;
using System.Collections.Generic;
using System.Text;

namespace ArduinoCommunicator
{
    /// <summary>
    /// Retrieve information about the Arduino board.
    /// </summary>
    public class BoardInfo
    {
        /// <summary>
        /// Creates a new instance for Arduino UNO.
        /// </summary>
        public BoardInfo() : this(BoardType.UNO) { }

        /// <summary>
        /// Creates a news instance for the specific BoardType.
        /// </summary>
        /// <param name="board">Board type to retrieve its information.</param>
        public BoardInfo(BoardType board)
        {
            BoardName = board;
            switch (board)
            {
                case BoardType.UNO:
                    NumberOfDigitalPins = 14;
                    NumberOfAnalogInputPins = 6;
                    AnalogOutPins = new List<int>(new int[] { 3, 5, 6, 9, 10, 11 });
                    break;

                default:
                    throw new NotImplementedException("The selected board is not implemented yet.");
            }
        }

        /// <summary>
        /// Board type of this instance.
        /// </summary>
        public BoardType BoardName { get; private set; }
        /// <summary>
        /// Number of analog input pins (usually denoted by 'ANALOG IN' on the board)
        /// </summary>
        public int NumberOfAnalogInputPins { get; private set; }

        /// <summary>
        /// List of analog (PWM) output pins (usually denoted by a ~ sign next to the pin number on the board)
        /// </summary>
        public List<int> AnalogOutPins { get; private set; }

        /// <summary>
        /// Number of digital pins (usually denoted by 'DIGITAL' on the board)
        /// </summary>
        public int NumberOfDigitalPins { get; private set; }
    }
}
