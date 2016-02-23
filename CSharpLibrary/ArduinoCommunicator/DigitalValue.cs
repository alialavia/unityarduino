namespace ArduinoCommunicator
{
    /// <summary>
    /// Represents valid and invalid digital values (High, Low and Invalid)
    /// Implicitly casted to and from bool and int. 
    /// </summary>
    public class DigitalValue
    {
        #region Private Constructors

        private DigitalValue()
        {
        }

        #endregion Private Constructors

        #region Public Methods

        /// <summary>
        /// Returns true if digitalValue is High and false if it's Low or Invalid.
        /// </summary>
        /// <param name="digitalValue">DigitalValue to cast to bool</param>
        /// <returns>bool</returns>
        public static implicit operator bool(DigitalValue digitalValue)
        {
            if (digitalValue == DigitalValue.High)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Casts DigitalValue to byte.
        /// </summary>
        /// <param name="digitalValue">DigitalValue to cast to byte</param>
        /// <returns>Returns 1 if digitalValue is High and 0 if it's Low or Invalid</returns>
        public static implicit operator byte(DigitalValue digitalValue)
        {
            if (digitalValue == DigitalValue.High)
                return 1;
            else
                return 0;
        }

        /// <summary>
        /// Casts bool to DigitalValue.
        /// </summary>
        /// <param name="b">Boolean value to cast to DigitalValue</param>
        /// <returns>Returns DigitalValue.High if b is true and DigitalValue.Low otherwise.</returns>
        public static implicit operator DigitalValue(bool b)
        {
            if (b)
                return DigitalValue.High;
            else
                return DigitalValue.Low;
        }

        /// <summary>
        /// Casts int to DigitalValue.
        /// </summary>
        /// <param name="i">Integer value to cast to DigitalValue</param>
        /// <returns>Returns DigitalValue.High if i > 0 and DigitalValue.Low otherwise</returns>
        public static implicit operator DigitalValue(int i)
        {
            if (i > 0)
                return DigitalValue.High;
            else
                return DigitalValue.Low;
        }

        /// <summary>
        /// Converts DigitalValue to String.
        /// </summary>
        /// <returns>Returns High, Low or Invalid for respected values of this instance.</returns>
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

        /// <summary>
        /// Digital value High.
        /// </summary>
        public static DigitalValue High = new DigitalValue();

        /// <summary>
        /// Digital value invalid. Typically used for pins which values cannot be read.
        /// </summary>
        public static DigitalValue Invalid = new DigitalValue();

        /// <summary>
        /// Digital value Low.
        /// </summary>
        public static DigitalValue Low = new DigitalValue();

        #endregion Public Fields
    }
}