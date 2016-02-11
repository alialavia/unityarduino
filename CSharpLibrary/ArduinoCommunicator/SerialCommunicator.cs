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

        public SerialCommunicator(Arduino arduinoState, MonoSerialPort sp, bool Async)
        {
            serialPort = sp;
            ArduinoState = arduinoState;
            serialPort.ReceivedBytesThreshold = IN_MESSAGE_LENGTH;
            

            serialPort.Open();
            // Wait for arduino to get ready
            Thread.Sleep(2000);
            if (Async)
            {
                serialPort.Async = true;
                serialPort.DataReceived += Sp_DataReceived;
                serialPort.Run();
            }
            
        }

        public void Start()
        {
            //sendACK();
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
            Refresh();
        }

        private object sync = new object();
        public void Refresh()
        {
            // Two refreshes cannot be called simultanously
            lock (sync)
            {
                sendACK();
                UpdateState();
                sendCommands();
            }
        }

        private void sendCommands()
        {
            while (ArduinoState.commandQueue.Count > 0)
            {
                byte[] nextCommand = ArduinoState.commandQueue.Dequeue();
                serialPort.Write(nextCommand, 0, nextCommand.Length);
            }
        }

        private void UpdateState()
        {
            byte[] temp = new byte[IN_MESSAGE_LENGTH];
            //var sp = sender as MonoSerialPort;
            Debug.Assert(serialPort.BytesToRead >= IN_MESSAGE_LENGTH);
            serialPort.Read(temp, 0, IN_MESSAGE_LENGTH);

            byte[] data = new byte[IN_MESSAGE_LENGTH - 2];
            for (int i = 0; i < data.Length; i++)
                data[i] = temp[i + 1];

            // sanity check
            if (temp[0] == BOF && Crc8(data, IN_MESSAGE_LENGTH - 2) == temp[IN_MESSAGE_LENGTH - 1])
            {
                ArduinoState.inBuffer(data);
                OnArduinoStateReceived(new ArduinoUpdateEventArgs(ArduinoState));
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
