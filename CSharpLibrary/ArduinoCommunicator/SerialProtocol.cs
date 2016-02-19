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
            return (byte)(Crc8(data, data.Length) & 0x7F);
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

        public enum Commands : byte
        {
            PIN_MODE = 0xF0,
            DIGITAL_WRITE = 0xF1,
            ANALOG_WRITE = 0xF2,
            DIGITAL_READ = 0xF3,
            ANALOG_READ = 0xF4
        }
        public const int IN_BUFFER_LENGTH = 0x04;

        public static readonly byte[] ACK = new byte[] { 0xFE, 0xFF, 0xFF };
        public static readonly byte[] BOF = new byte[] { 0xFF };
        public static readonly byte[] NACK = new byte[] { 0xFD, 0xFF, 0xFF };

        #endregion Public Fields
    }
}