using System;
using ClassLibrary1;

namespace ArduinoCommunicator
{
    public class ArduinoUpdateEventArgs : EventArgs
    {
        public ArduinoUpdateEventArgs(byte[] stateBuffer, BoardName board = BoardName.UNO)
        {
            State = new ArduinoState(stateBuffer, board);
        }
        public ArduinoState State { get; private set; }
    }
}