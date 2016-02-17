using System;
using System.Collections.Generic;
using System.Text;

namespace ArduinoCommunicator
{
    public class ArduinoBoard
    {
        public ArduinoBoard() : this(BoardName.UNO) { }
        public ArduinoBoard(BoardName board)
        {
            BoardName = board;
            switch (board)
            {
                case BoardName.UNO:
                    NumberOfDigitalPins = 14;
                    NumberOfAnalogPins = 6;
                    break;

                default:
                    throw new NotImplementedException("The selected board is not implemented yet.");
            }
        }

        public BoardName BoardName { get; private set; }
        public int NumberOfAnalogPins { get; private set; }
        public int NumberOfDigitalPins { get; private set; }
    }
}
