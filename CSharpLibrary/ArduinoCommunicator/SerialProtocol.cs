using System;
using System.Collections.Generic;
using System.Text;

namespace ArduinoCommunicator
{
    public static class SerialProtocol
    {
        #region Public Methods

        public static byte[] AddCRC(byte[] data)
        {
            var ret = new byte[data.Length + 1];
            data.CopyTo(ret, 0);
            ret[ret.Length - 1] = CRC(data);
            return ret;
        }

        public static byte CRC(byte[] data)
        {
            return Crc8(data, data.Length);
        }

        public static byte[] getData(byte[] rawData)
        {
            byte[] data = new byte[SerialProtocol.IN_MESSAGE_LENGTH - 2];
            for (int i = 0; i < data.Length; i++)
                data[i] = rawData[i + 1];
            return data;
        }

        public static bool IsValidMessage(byte[] rawData)
        {
            return (SerialProtocol.CRC(rawData.SkipR(1)) == rawData[rawData.Length - 1]);
        }

        #endregion Public Methods

        #region Private Methods

        private static byte Crc8(byte[] data, int len)
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

        #endregion Private Methods

        #region Public Fields

        public const byte ANALOG_READ = 0x04;
        public const byte ANALOG_WRITE = 0x03;
        public const byte DIGITAL_READ = 0x05;
        public const byte DIGITAL_WRITE = 0x01;
        public const int IN_BUFFER_LENGTH = 0x04;
        public const int IN_MESSAGE_LENGTH = 16;
        public const byte PIN_MODE = 0x02;
        public static readonly byte[] ACK = new byte[] { 0xFE, 0xFF, 0xFF };
        public static readonly byte[] BOF = new byte[] { 0xFF };
        public static readonly byte[] NACK = new byte[] { 0xFD, 0xFF, 0xFF };

        #endregion Public Fields
    }
}