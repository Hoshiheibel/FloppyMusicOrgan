using System.IO;
using System.Text;
using MidiParser.Entities;
using MidiParser.Exceptions;
using System.Linq;
using MidiParser.Extensions;

namespace MidiParser
{
    public class MidiReader
    {
        private MidiFile _midiFile;
        private BinaryReader _fileReader;
        
        public MidiReader()
        {
            _midiFile = new MidiFile();
        }

        public bool Parse(string fileName)
        {
            _fileReader = new BinaryReader(File.Open(fileName, FileMode.Open));

            ReadHeaderChunk();
            ReadTrackChunks();

            _fileReader.Close();
            _fileReader.Dispose();

            return true;
        }

        private void ReadHeaderChunk()
        {
            ReadHeaderChunkId();
            ReadHeaderLength();
            ReadMidiFormatType();
        }

        private void ReadHeaderChunkId()
        {
            var fileIdentifier = _fileReader.ReadBytes(4).ConvertByteArrayToString();

            if (!string.Equals(fileIdentifier, "MThd"))
                throw new InvalidMidiHeaderException();

            _midiFile.MidiHeader.FileIdentifier = fileIdentifier;
        }

        private void ReadHeaderLength()
        {
            var headerLength = _fileReader.ReadBytes(4).ConvertByteArrayToInt();

            if (headerLength != 6)
                throw new InvalidHeaderLengthException();

            _midiFile.MidiHeader.HeaderLength = headerLength;
        }

        private void ReadMidiFormatType()
        {
            var fileType = _fileReader.ReadBytes(2).ConvertByteArrayToInt();

            if (fileType < 0 || fileType > 2)
                throw new InvalidMidiFormatTypeException();

            _midiFile.MidiHeader.MidiFormatType = (MidiFormatTypeEnum) fileType;
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
