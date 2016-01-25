using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArduinoCommunicator;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
            
        {
            SerialCommunicator sc = new SerialCommunicator("COM22",115200 ,System.IO.Ports.Parity.Even, 8, System.IO.Ports.StopBits.Two);            
            sc.ArduinoStateReceived += Sc_ArduinoStateUpdated;
            while (true) ;            
        }

        private static ArduinoState _state = null;
        private static void Sc_ArduinoStateUpdated(object sender, ArduinoUpdateEventArgs e)
        {
            if (!e.State.Equals(_state))
                Console.WriteLine(e.State.digitalRead(2));
            _state = e.State;
        }
    }
}
