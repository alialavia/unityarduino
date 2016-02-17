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

        public static implicit operator bool(DigitalValue dv)
        {
            if (dv == DigitalValue.High)
                return true;
            else if (dv == DigitalValue.Low)
                return false;
            else
                throw new System.Exception("Cannot convert Invalid digital value to boolean!");
        }

        public static implicit operator byte(DigitalValue dv)
        {
            if (dv == DigitalValue.High)
                return 1;
            else if (dv == DigitalValue.Low)
                return 0;
            else
                throw new System.Exception("Cannot convert Invalid digital value to boolean!");
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