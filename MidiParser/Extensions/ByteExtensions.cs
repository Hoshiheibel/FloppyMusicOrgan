using System;
using System.Collections.Generic;
using System.Text;

namespace MidiParser.Extensions
{
    public static class ByteExtensions
    {
        public static string ConvertToString(this byte[] bytes)
        {
            ASCIIEncoding enc = new ASCIIEncoding();
            return enc.GetString(bytes);
        }

        public static IEnumerable<int> ConvertByteListToIntList(this IEnumerable<byte> bytes)
        {
            foreach (var b in bytes)
            {
                yield return BitConverter.ToInt16(new[] {(byte)0, b}, 0);
            }
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
    }
}
