using System.IO;
using MidiParser.Entities;
using MidiParser.Entities.Enums;
using MidiParser.Exceptions;
using MidiParser.Extensions;

namespace MidiParser
{
    public class MidiReader
    {
        private const string MidiFileFormatIdentifier = "MThd";
        private const string MidiTrackChunkIdentifier = "MTrk";

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
            ReadTrackCount();
            ReadTimeDivision();
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

        private void ReadTrackCount()
        {
            var trackCount = _fileReader.ReadBytes(2).ConvertToInt();
            _midiFile.MidiHeader.NumberOfTracks = trackCount;
        }

        private void ReadTimeDivision()
        {
            var timeDivision = _fileReader.ReadBytes(2).ConvertToInt();
            //_midiFile.MidiHeader.NumberOfTracks = trackCount;
        }

        private void ReadTrackChunks()
        {
            for (var i = 0; i < _midiFile.MidiHeader.NumberOfTracks; i++)
            {
                var track = new MidiTrack();

                ReadTrackChunkId(track);
                ReadTrackChunkSize(track);
                
                using (var memoryStream = new MemoryStream(_fileReader.ReadBytes(track.TrackLength)))
                {
                    ParseTrack(memoryStream, track);
                }

                _midiFile.Tracks.Add(track);
            }
        }

        private void ReadTrackChunkSize(MidiTrack track)
        {
            var trackLength = _fileReader.ReadBytes(4).ConvertToInt();
            track.TrackLength = trackLength;
        }

        private void ReadTrackChunkId(MidiTrack track)
        {
            var trackHeaderIdentifier = _fileReader.ReadBytes(4).ConvertToString();

            if (!string.Equals(trackHeaderIdentifier, MidiTrackChunkIdentifier))
                throw new InvalidChunkHeaderException();

            track.TrackHeader.TrackHeaderIdentifier = trackHeaderIdentifier;
        }

        private void ParseTrack(MemoryStream memoryStream, MidiTrack track)
        {
            while (memoryStream.Position != memoryStream.Length)
            {
                var deltaTime = GetDeltaTime(memoryStream);
                var midiEvent = GetMidiEventType(memoryStream);
            }
        }

        private long GetDeltaTime(MemoryStream memoryStream)
        {
            var currentByte = memoryStream.ReadByte();
            long result = currentByte;

            if ((currentByte & 0x80) != 0)
            {
                // Clear the "more data ahead" Bit
                result &= 0x7f;

                do
                {
                    currentByte = memoryStream.ReadByte();
                    result = (result << 7) + (currentByte & 0x7f);
                }
                while (memoryStream.Position < memoryStream.Length && ((currentByte & 0x80) != 0));
            }

            return result;
        }

        private MidiChannelEventTypeEnum GetMidiEventType(MemoryStream memoryStream)
        {
            throw new System.NotImplementedException();
        }
    }
}
