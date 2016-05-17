using System;
using System.Collections.Generic;
using System.Text;
using SerialPortNET;

namespace UnitySerialPort
{

    public class AndroidSerialPort : AbstractSerialPort
    {
        public AndroidSerialPort(string portName, int baudRate, Parity parity, byte dataBits, StopBits stopBits)
            : base(portName, baudRate, parity, dataBits, stopBits,
                      (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.Android
                      ? new SerialPortAndroid()
                      : null)
                  )
        {
        }
        public AndroidSerialPort() : this("COM1", 115200, Parity.Even, 8, StopBits.Two) { }

    }
}
