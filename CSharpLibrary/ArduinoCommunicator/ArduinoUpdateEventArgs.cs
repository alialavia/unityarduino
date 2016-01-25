using System;
using ArduinoCommunicator;

namespace ArduinoCommunicator
{
    public class ArduinoUpdateEventArgs : EventArgs
    {
        public ArduinoUpdateEventArgs(Arduino state)
        {
            State = state;
        }
        public Arduino State { get; private set; }
    }
}