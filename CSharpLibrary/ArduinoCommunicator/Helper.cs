using System;
using System.Collections.Generic;
using System.Text;

namespace ArduinoCommunicator
{
    internal static class Helper
    {
        #region Public Methods

        public static byte[] Skip(this byte[] bytes, int skip)
        {
            byte[] ret = new byte[bytes.Length - skip];
            for (int i = 0; i < ret.Length; i++)
                ret[i] = bytes[i + skip];

            return ret;
        }

        public static byte[] SkipR(this byte[] bytes, int skip)
        {
            byte[] ret = new byte[bytes.Length - skip];
            for (int i = 0; i < ret.Length; i++)
                ret[i] = bytes[i];

            return ret;
        }

        #endregion Public Methods
    }
}

// To make extension methods available
namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Used to enable extension methods for .Net 2.0 
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class
         | AttributeTargets.Method)]        
    public sealed class ExtensionAttribute : Attribute { }
}