using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Text;
using System.Timers;

namespace ArduinoCommunicator
{
    /// <summary>
    /// Mono implementation of SerialPort is incomplete. This is to make up for that.
    /// </summary>
    public class MonoSerialPort
    {
        private int baudRate;
        private int dataBits;
        private Parity parity;
        private string portName;
        private StopBits stopBits;
        private SerialPort sp;
        private BackgroundWorker bgWorker = new BackgroundWorker();

        public MonoSerialPort(SerialPort sp)
        {
            this.sp = sp;
            bgWorker.DoWork += new DoWorkEventHandler(BgWorker_DoWork);
            bgWorker.ProgressChanged += BgWorker_ProgressChanged;
        }

        public  MonoSerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits): 
            this(new SerialPort(portName, baudRate, parity, dataBits, stopBits))
        {
            this.portName = portName;
            this.baudRate = baudRate;
            this.parity = parity;
            this.dataBits = dataBits;
            this.stopBits = stopBits;
        }

        private void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (sp.BytesToRead >= this.ReceivedBytesThreshold)
                {
                    OnDataReceived();
                }
            }
        }

        protected virtual void OnDataReceived()
        {
            
            EventHandler<MonoSerialDataReceivedEventArgs> handler = DataReceived;
            if (handler != null)
                handler(this, new MonoSerialDataReceivedEventArgs(SerialData.Chars));
        }

        public event EventHandler<MonoSerialDataReceivedEventArgs> DataReceived;
        public int ReceivedBytesThreshold { get; set; }
        public int BytesToRead { get { return sp.BytesToRead; } }

        public void Open()
        {
            sp.Open();
            bgWorker.RunWorkerAsync();
        }

        public void Write(byte[] v1, int v2, int v3)
        {
            sp.Write(v1, v2, v3);
        }

        internal void Read(byte[] buffer, int v, int len)
        {
            sp.Read(buffer, v, len);
        }
    }
}
