using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

namespace ArduinoCommunicator
{
    /// <summary>
    /// Communicates with Arduino.
    /// </summary>
    public class SerialCommunicator
    {
        private byte Crc8(byte[] data, int len)
        {
            //const byte *data = vptr;
            uint crc = 0;
            int i, j;
            for (j = 0; j < len; j++)
            {
                crc ^= (uint)(data[j] << 8);
                for (i = 8; i > 0; i--)
                {
                    if ((crc & 0x8000) != 0)
                        crc ^= (0x1070 << 3);
                    crc <<= 1;
                }
            }
            return (byte)(crc >> 8);
        }
        private MonoSerialPort serialPort;

        public void Close()
        {
            serialPort.Close();
        }

        public SerialCommunicator(Arduino arduinoState, MonoSerialPort sp)
        {
            serialPort = sp;
            ArduinoState = arduinoState;
            serialPort.ReceivedBytesThreshold = IN_MESSAGE_LENGTH;

            serialPort.DataReceived += Sp_DataReceived;
            // Wait for arduino to get ready
            Thread.Sleep(2000);
            serialPort.Open();
        }
        //public SerialCommunicator(Arduino arduinoState)
        //{
            
        //}
        //public SerialCommunicator(Arduino arduinoState, String portName = "COM1", int baudRate = 115200, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        //{
        //    serialPort = new MonoSerialPort(portName, baudRate, parity, dataBits, stopBits);
        //}

        public void Start()
        {
            sendACK();
        }

        private void Sp_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            Debug.WriteLine("ERROR:" + e.ToString());
        }
        private byte BOF = 0xff;
        private byte ACK = 0x01;
        private const int IN_MESSAGE_LENGTH = 16; // bytes
        private readonly object serialLock = new object();
        Queue<byte> dataFrame = new Queue<byte>(IN_MESSAGE_LENGTH);
        private Arduino ArduinoState;

        //private int OUT_MESSAGE_LENGTH = 18; // bytes

        private void Sp_DataReceived(object sender, MonoSerialDataReceivedEventArgs e)
        {
            if (e.EventType == SerialData.Eof)
                return;

            byte[] temp = new byte[IN_MESSAGE_LENGTH];
            //var sp = sender as MonoSerialPort;
            Debug.Assert(serialPort.BytesToRead >= IN_MESSAGE_LENGTH);
            serialPort.Read(temp, 0, IN_MESSAGE_LENGTH);

            byte[] data = new byte[IN_MESSAGE_LENGTH - 2];
            for (int i = 0; i < data.Length; i++)
                data[i] = temp[i + 1];

            // sanity check
            if (temp[0] == BOF && Crc8(data, IN_MESSAGE_LENGTH - 2) == temp[IN_MESSAGE_LENGTH-1])
            {
                ArduinoState.inBuffer(data);
                OnArduinoStateReceived(new ArduinoUpdateEventArgs(ArduinoState));
            }
            sendACK();
            //sendBOF();
            while (ArduinoState.commandQueue.Count > 0)
            {
                byte[] nextCommand = ArduinoState.commandQueue.Dequeue();
                serialPort.Write(nextCommand, 0, nextCommand.Length);
            }

        }

        private void sendACK()
        {
            serialPort.Write(new byte[] { ACK }, 0, 1);
        }

        private void sendBOF()
        {
            serialPort.Write(new byte[] { BOF }, 0, 1);
        }
        protected virtual void OnArduinoStateReceived(ArduinoUpdateEventArgs e)
        {
            EventHandler<ArduinoUpdateEventArgs> temp = ArduinoStateReceived;
            if (temp != null)
            {
                temp(this, e);
            }
        }

        public event EventHandler<ArduinoUpdateEventArgs> ArduinoStateReceived;

    }
}
