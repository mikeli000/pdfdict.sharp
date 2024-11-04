using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PDFDict.SDK.Sharp.Core
{
    public class NativeStringReader
    {
        public static string UnsafeRead_UTF16_LE(Func<IntPtr, int, int> nativeFunc)
        {
            unsafe
            {
                int defaultByteCount = 512;
                IntPtr ptr = Marshal.AllocHGlobal(defaultByteCount);
                try
                {
                    var len = nativeFunc(ptr, defaultByteCount);

                    if (len > defaultByteCount)
                    {
                        Marshal.FreeHGlobal(ptr);
                        ptr = Marshal.AllocHGlobal(len);
                        len = nativeFunc(ptr, len);
                    }
                    if (len == 0)
                    {
                        return string.Empty;
                    }

                    if (len % 2 != 0)
                    {
                        throw new ArgumentException($"Bytes buf(encoded in UTF-16LE) length must be even instead of {len}");
                    }

                    if (len == 2)
                    {
                        return string.Empty;
                    }
                    string s = Marshal.PtrToStringUni(ptr, len / 2 - 1);
                    return s;
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
        }

        public static string UnsafeRead_UTF16_LE_2(Func<IntPtr, int, int> nativeFuncWithCharCountReturn)
        {
            unsafe
            {
                int defaultByteCount = 512;
                IntPtr ptr = Marshal.AllocHGlobal(defaultByteCount);
                try
                {
                    var charCount = nativeFuncWithCharCountReturn(ptr, defaultByteCount);
                    charCount = charCount * 2; // charCount is the number of characters, not bytes

                    if (charCount > defaultByteCount)
                    {
                        Marshal.FreeHGlobal(ptr);
                        ptr = Marshal.AllocHGlobal(charCount);
                        charCount = nativeFuncWithCharCountReturn(ptr, charCount);
                    }
                    if (charCount == 0)
                    {
                        return string.Empty;
                    }

                    if (charCount % 2 != 0)
                    {
                        throw new ArgumentException($"Bytes buf(encoded in UTF-16LE) length must be even instead of {charCount}");
                    }

                    string s = Marshal.PtrToStringUni(ptr, charCount / 2);
                    return s;
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
        }

        public static string UnsafeRead_UTF8(Func<IntPtr, int, int> nativeFunc)
        {
            unsafe
            {
                int defaultByteCount = 512;
                IntPtr ptr = Marshal.AllocHGlobal(defaultByteCount);
                try
                {
                    var len = nativeFunc(ptr, defaultByteCount);

                    if (len > defaultByteCount)
                    {
                        Marshal.FreeHGlobal(ptr);
                        ptr = Marshal.AllocHGlobal(len);
                        len = nativeFunc(ptr, len);
                    }
                    if (len == 0)
                    {
                        return string.Empty;
                    }

                    string s = Marshal.PtrToStringUTF8(ptr, len);
                    return s;
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
        }
    }
}
