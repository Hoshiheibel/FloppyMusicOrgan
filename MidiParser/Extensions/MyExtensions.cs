using System;
using System.IO;
using System.Text;

namespace MidiParser.Extensions
{
    public static class MyExtensions
    {
        public static string ConvertToString(this byte[] bytes)
        {
            ASCIIEncoding enc = new ASCIIEncoding();
            return enc.GetString(bytes);
        }

        public static int ConvertToInt(this byte[] bytes)
        {
            Array.Reverse(bytes);
            if (bytes.Length == 4)
                return BitConverter.ToInt32(bytes, 0);
            if (bytes.Length == 2)
                return BitConverter.ToInt16(bytes, 0);

            // ToDo: Need to check more cases?
            return -1;
        }

        public static byte GetFirstNibble(this byte value)
        {
            return (byte)(value & 0xF0);
        }

        public static byte GetSecondNibble(this byte value)
        {
            return (byte)(value & 0x0F);
        }

        public static T ConvertToEnum<T>(this object source) where T : struct 
        {
            var type = typeof(T);

            if (!type.IsEnum)
                throw new InvalidOperationException();

            if (!Enum.IsDefined(type, source))
                throw new InvalidCastException();

            return (T)source;
        }

        public static void SkipBytes(this Stream stream, int byteCountToSkip)
        {
            var buffer = new byte[byteCountToSkip];
            stream.Read(buffer, 0, byteCountToSkip);
        }
    }
}
