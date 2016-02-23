using System;
using System.Collections.Generic;
using System.Text;

namespace ArduinoCommunicator
{
    internal static class Helper
    {
        #region Public Methods

        public static byte[] Skip(byte[] bytes, int skip)
        {
            byte[] ret = new byte[bytes.Length - skip];
            for (int i = 0; i < ret.Length; i++)
                ret[i] = bytes[i + skip];

            return ret;
        }

        public static byte[] SkipR(byte[] bytes, int skip)
        {
            byte[] ret = new byte[bytes.Length - skip];
            for (int i = 0; i < ret.Length; i++)
                ret[i] = bytes[i];

            return ret;
        }

        #endregion Public Methods
    }
}
