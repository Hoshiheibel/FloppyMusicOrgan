using System;

namespace MidiParser.Exceptions
{
    public class InvalidMidiFormatTypeException : Exception
    {
        public InvalidMidiFormatTypeException(Exception innerException) : base(string.Empty, innerException)
        {
        }
    }
}