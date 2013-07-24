using System.IO;
using MidiParser.Entities;
using MidiParser.Exceptions;
using MidiParser.Extensions;

namespace MidiParser
{
    public class MidiReader
    {
        private const string MidiFileFormatIdentifier = "MThd";
        private MidiFile _midiFile;
        private BinaryReader _fileReader;
        
        public MidiReader()
        {
            _midiFile = new MidiFile();
        }

        public void Parse(string fileName)
        {
            using (_fileReader = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                ReadHeaderChunk();
                ReadTrackChunks();
            }
        }

        private static bool IsInvalidMidiFormatType(int fileType)
        {
            return fileType < 0 || fileType > 2;
        }

        private void ReadHeaderChunk()
        {
            ReadHeaderChunkId();
            ReadHeaderLength();
            ReadMidiFormatType();
        }

        private void ReadHeaderChunkId()
        {
            var fileIdentifier = _fileReader.ReadBytes(4).ConvertToString();

            if (!string.Equals(fileIdentifier, MidiFileFormatIdentifier))
                throw new InvalidMidiHeaderException();

            _midiFile.MidiHeader.FileIdentifier = fileIdentifier;
        }

        private void ReadHeaderLength()
        {
            var headerLength = _fileReader.ReadBytes(4).ConvertToInt();

            if (headerLength != 6)
                throw new InvalidHeaderLengthException();

            _midiFile.MidiHeader.HeaderLength = headerLength;
        }

        private void ReadMidiFormatType()
        {
            var formatType = _fileReader.ReadBytes(2).ConvertToInt();

            if (IsInvalidMidiFormatType(formatType))
                throw new InvalidMidiFormatTypeException();

            _midiFile.MidiHeader.Format = (MidiFormatTypeEnum) formatType;
        }

        private bool ReadTrackChunks()
        {
            return true;
        }

        private bool ParseTracks()
        {
            return true;
        }
    }
}
