namespace ArduinoCommunicator
{
    public class DigitalValue
    {
        #region Private Constructors

        private DigitalValue()
        {
        }

        #endregion Private Constructors

        #region Public Methods

        /// <summary>
        /// Returns true if digitalValue is High and false if it's Low or Invalid
        /// </summary>
        /// <param name="digitalValue"></param>
        public static implicit operator bool(DigitalValue digitalValue)
        {
            if (digitalValue == DigitalValue.High)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Returns true if digitalValue is 1 and 0 if it's Low or Invalid
        /// </summary>
        /// <param name="digitalValue"></param>
        public static implicit operator byte(DigitalValue digitalValue)
        {
            if (digitalValue == DigitalValue.High)
                return 1;
            else
                return 0;
        }

        public static implicit operator DigitalValue(bool b)
        {
            if (b)
                return DigitalValue.High;
            else
                return DigitalValue.Low;
        }

        public static implicit operator DigitalValue(int i)
        {
            if (i > 0)
                return DigitalValue.High;
            else
                return DigitalValue.Low;
        }

        public override string ToString()
        {
            if (this == DigitalValue.High)
                return "High";
            else if (this == DigitalValue.Low)
                return "Low";
            else
                return "Invalid";
        }

        #endregion Public Methods

        #region Public Fields

        public static DigitalValue High = new DigitalValue();
        public static DigitalValue Invalid = new DigitalValue();
        public static DigitalValue Low = new DigitalValue();

        #endregion Public Fields
    }
}