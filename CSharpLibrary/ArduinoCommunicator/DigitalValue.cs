namespace ArduinoCommunicator
{
    public class DigitalValue
    {
        public static implicit operator bool (DigitalValue dv)
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

        public override string ToString()
        {
            if (this == DigitalValue.High)
                return "High";
            if (this == DigitalValue.Low)
                return "Low";
            if (this == DigitalValue.Invalid)
                return "Invalid";

            throw new System.Exception("Digital Value out of range.");
        }

        private DigitalValue() { }
        public static DigitalValue High = new DigitalValue();
        public static DigitalValue Low = new DigitalValue();
        public static DigitalValue Invalid = new DigitalValue();
    }

}