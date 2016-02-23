namespace ArduinoCommunicator
{
    /// <summary>
    /// Represents pin mode 
    /// </summary>
    public enum PinMode
    {
        /// <summary>
        /// Digital input pin
        /// </summary>
        INPUT,
        /// <summary>
        /// Digital output pin
        /// </summary>
        OUTPUT,
        /// <summary>
        /// Digital input pin with a pull up resistor. Makes its default value High, unless externally grounded.
        /// </summary>
        INPUT_PULLUP
    }
}