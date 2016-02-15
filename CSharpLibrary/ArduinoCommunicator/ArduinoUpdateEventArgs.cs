using System;
using ArduinoCommunicator;

namespace ArduinoCommunicator
{
    public class ArduinoUpdatesEventArgs : EventArgs
    {
        public ArduinoUpdatesEventArgs(Arduino state)
        {
            State = state;
        }
        public Arduino State { get; private set; }
    }
}