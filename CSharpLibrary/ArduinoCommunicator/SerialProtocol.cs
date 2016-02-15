using System;
using System.Collections.Generic;
using System.Text;

namespace ArduinoCommunicator
{
    public static class SerialProtocol
    {
        public const int IN_MESSAGE_LENGTH = 16;
        public static readonly byte[] BOF = new byte[] { 0xFF };
        public static readonly byte[] ACK = new byte[] { 0xFE, 0xFF, 0xFF };

        public static byte CRC(byte[] data, int len)
        {
            return Crc8(data, len);
        }
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

        public static bool IsValidMessage(byte[] rawData)
        {
            return (
                    rawData[0] == SerialProtocol.BOF[0]
                    &&
                    (SerialProtocol.CRC(getData(rawData), SerialProtocol.IN_MESSAGE_LENGTH - 2)
                    ==
                    rawData[SerialProtocol.IN_MESSAGE_LENGTH - 1])
                    );
        }

        public static byte[] getData(byte[] rawData)
        {
            byte[] data = new byte[SerialProtocol.IN_MESSAGE_LENGTH - 2];
            for (int i = 0; i < data.Length; i++)
                data[i] = rawData[i + 1];
            return data;
        }
    }
}
