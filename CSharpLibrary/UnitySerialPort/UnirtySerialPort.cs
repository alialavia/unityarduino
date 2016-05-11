using System;
using System.Collections.Generic;
using System.Text;
using SerialPortNET;

namespace UnitySerialPort
{
    public class AndroidSerialPort : SerialPort, ISerialPort
    {
        static AndroidSerialPort()
        {
            PlatformSpecificImplementation = new HandleUnimplementedPlatforms(implementAndroid);
        }

        public AndroidSerialPort(string portName, int baudRate, Parity parity, byte dataBits, StopBits stopBits) : base(portName, baudRate, parity, dataBits, stopBits)
        {
        }
        public AndroidSerialPort() : base() { }

        private static ILowLevelSerialPort implementAndroid(Platform p)
        {
            if (p == Platform.Android)
                return new SerialPortAndroid();
            else
                throw new NotImplementedException();
        }
    }
}
