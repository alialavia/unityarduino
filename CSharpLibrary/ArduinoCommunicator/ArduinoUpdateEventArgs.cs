using System;
using ArduinoCommunicator;

namespace ArduinoCommunicator
{
    public class ArduinoUpdatesEventArgs : EventArgs
    {
        #region Public Constructors

        public ArduinoUpdatesEventArgs(Arduino state)
        {
            State = state;
        }

        #endregion Public Constructors

        #region Public Properties

        public Arduino State { get; private set; }

        #endregion Public Properties
    }
}