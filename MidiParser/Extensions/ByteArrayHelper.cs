using System;
using System.Text;

namespace MidiParser.Extensions
{
    public static class ByteArrayHelper
    {
        public static string ConvertByteArrayToString(this byte[] bytes)
        {
            ASCIIEncoding enc = new ASCIIEncoding();
            return enc.GetString(bytes);
        }

        public static int ConvertByteArrayToInt(this byte[] bytes)
        {
            Array.Reverse(bytes);

            if (bytes.Length == 4)
                return BitConverter.ToInt32(bytes, 0);
            if (bytes.Length == 2)
                return BitConverter.ToInt16(bytes, 0);

            // ToDo: Need to check more cases?
            return 0;
        }
    }
}
