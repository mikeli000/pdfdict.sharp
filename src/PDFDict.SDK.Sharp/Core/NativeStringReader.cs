using SixLabors.ImageSharp.Memory;
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
                    if (s.EndsWith('\0'))
                    {
                        s = s.Substring(0, s.Length - 1);
                    }
                    return s;
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
        }

        public static string UnsafeRead_UTF16_LE_Char(Func<IntPtr, int, int> nativeFunc)
        {
            unsafe
            {
                int charBufCount = 512;
                IntPtr ptr = Marshal.AllocHGlobal(charBufCount);
                try
                {
                    var charReadCount = nativeFunc(ptr, charBufCount);

                    if (charReadCount > charBufCount)
                    {
                        Marshal.FreeHGlobal(ptr);
                        ptr = Marshal.AllocHGlobal(charReadCount);
                        charReadCount = nativeFunc(ptr, charReadCount);
                    }
                    if (charReadCount == 0)
                    {
                        return string.Empty;
                    }

                    if (charBufCount > charReadCount)
                    {
                        charReadCount -= 1; // remove the last null char
                    }
                    string s = Marshal.PtrToStringUni(ptr, charReadCount);
                    return s;
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
        }

        public static string UnsafeRead_UTF16_LE_2(Func<IntPtr, int, int> nativeFuncWithCharCountReturn, int charCount)
        {
            unsafe
            {
                int byteCount = charCount * 2;
                IntPtr ptr = Marshal.AllocHGlobal(byteCount);
                try
                {
                    int read = nativeFuncWithCharCountReturn(ptr, charCount);
                    if (read == 0)
                    {
                        return string.Empty;
                    }

                    string s = Marshal.PtrToStringUni(ptr, read);
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
                IntPtr ptr = IntPtr.Zero;
                try
                {
                    int read = nativeFuncWithCharCountReturn(ptr, 0);
                    if (read <= 0)
                    {
                        return string.Empty;
                    }

                    ptr = Marshal.AllocHGlobal(read * 2);
                    nativeFuncWithCharCountReturn(ptr, read);

                    string s = Marshal.PtrToStringUni(ptr, read - 1);
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

        public static string UnsafeRead_UTF8_Twice(Func<IntPtr, int, int> nativeFunc)
        {
            unsafe
            {
                int defaultByteCount = 0;
                IntPtr ptr = IntPtr.Zero;
                try
                {
                    var len = nativeFunc(ptr, defaultByteCount);
                    if (len == 0)
                    {
                        return string.Empty;
                    }

                    Marshal.FreeHGlobal(ptr);
                    ptr = Marshal.AllocHGlobal(len);
                    len = nativeFunc(ptr, len);

                    string s = Marshal.PtrToStringUTF8(ptr, len - 1);
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
