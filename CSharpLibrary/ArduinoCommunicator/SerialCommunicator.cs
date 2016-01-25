using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System.Threading;

namespace ArduinoCommunicator
{
    /// <summary>
    /// Communicates with Arduino. Upstream (PC to Arduino) uses BSON serialization, while downstream (Arduino to PC) uses 
    /// </summary>
    public class SerialCommunicator
    {
        private byte Crc8(byte[] data, int len)
        {            
            //const byte *data = vptr;
            uint crc = 0;
            int i, j;
            for (j = len; j != 0; j--)
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
        private SerialPort serialPort;
        public SerialCommunicator(String portName = "COM1", int baudRate = 115200, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            serialPort.ReceivedBytesThreshold = MESSAGE_LENGTH;
            //serialPort.ReadBufferSize = 4096 * 4096;
            serialPort.DataReceived += Sp_DataReceived;
            serialPort.ErrorReceived += Sp_ErrorReceived;
            serialPort.Open();
        }

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
        private const int MESSAGE_LENGTH = 16; // bytes
        private readonly object serialLock = new object();
        Queue<byte> dataFrame = new Queue<byte>(MESSAGE_LENGTH);
        private void Sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (e.EventType == SerialData.Eof)
                return;

            byte[] temp = new byte[MESSAGE_LENGTH];
            var sp = sender as SerialPort;
            Debug.Assert(sp.BytesToRead >= MESSAGE_LENGTH);
            sp.Read(temp, 0, MESSAGE_LENGTH);
            OnArduinoStateReceived(new ArduinoUpdateEventArgs(temp));
            sendACK();
        }

        private void sendACK()
        {
            //Thread.Sleep(5);
            serialPort.Write(new byte[] { ACK }, 0, 1);
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

        /*
private int bytesRead = 0;
private int messageLength = 0;
private void Sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
{
var sp = sender as SerialPort;
byte[] temp;
// First 4 bytes of every packet is the BSON length
if (bytesRead == 0)
{
// At least 4 bytes should be available
if (sp.BytesToRead < 4)
   return;

temp = new byte[4];
if (sp.Read(temp, 0, 4) != 4)
   throw new SerialCommunicatorUnhandledException("Cannot read message length. Make sure that the device is connected");

messageLength = BitConverter.ToInt32(temp, 0);
bsonBuffer = new byte[messageLength];
temp.CopyTo(bsonBuffer, 0);
bytesRead += 4;
}

if (sp.BytesToRead == 0)
return;

bytesRead += sp.Read(bsonBuffer, bytesRead, Math.Min(sp.BytesToRead, messageLength - bytesRead));
if (bytesRead == messageLength)
{
bytesRead = 0;
messageLength = 0;
processBson(bsonBuffer);
}
}

private void processBson(byte[] bsonBuffer)
{
BsonReader bsonReader = new BsonReader(new MemoryStream(bsonBuffer));
JsonSerializer serializer = new JsonSerializer();
serializer.Deserialize<ArduinoState>(bsonReader);
OnArduinoStateUpdated(null);
}
*/
    }
}
