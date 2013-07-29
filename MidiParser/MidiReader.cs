using System;
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
            _midiFile.MidiHeader.TimeDivision = timeDivision;
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
            long pos = 0; // current position in data
            bool running = false; // whether we're in running status
            int status = 0; // the current status byte
            bool sysExContinue = false; // whether we're in a multi-segment system exclusive message
            byte[] sysExData = null; // system exclusive data up to this point from a multi-segment message

            try
            {
                // Process all bytes, turning them into events
                while (memoryStream.Position < memoryStream.Length)
                {
                    var deltaTime = GetDeltaTime(memoryStream);
                    var nextValue = (byte) memoryStream.ReadByte();

                    if (nextValue == 0xFF)
                    {
                        ReadMetaEvent(memoryStream);
                    }
                    else
                    {
                        switch (nextValue.GetFirstNibble())
                        {
                            case 0x80:  // Note Off

                                break;

                            case 0x90:  // Note on
                                ParseNoteOnEvent(memoryStream, nextValue.GetSecondNibble());
                                break;

                            case 0xA0:  // Key atertouch
                                break;

                            case 0xB0:  // Controller Change event
                                break;

                            case 0xC0:  // Program change
                                ParseProgramChangeEvent(memoryStream, nextValue.GetSecondNibble());
                                break;

                            case 0xD0:  // Channel Aftertouch event
                                break;

                            case 0xE0:  // Pitch Bend event
                                ParsePitchBendEvent(memoryStream, nextValue.GetSecondNibble());
                                break;

                            case 0xF0:
                                break;
                        }

                        //nextValue.GetFirstNibble() == 0xF0;
                    }
                }
            }
                //        // Are we continuing a sys ex?  If so, the next value better be 0x7F
                //        if (sysExContinue && (nextValue != 0x7f)) throw new MidiParserException("Expected to find a system exclusive continue byte.", pos);

                //        // Are we in running status?  Determine whether we're running and
                //        // what the current status byte is.
                //        if ((nextValue & 0x80) == 0)
                //        {
                //            // We're now in running status... if the last status was 0, uh oh!
                //            if (status == 0) throw new MidiParserException("Status byte required for running status.", pos);

                //            // Keep the last iteration's status byte, and now we're in running mode
                //            running = true;
                //        }
                //        else
                //        {
                //            // Not running, so store the current status byte and mark running as false
                //            status = nextValue;
                //            running = false;
                //        }

                //        // Grab the 4-bit identifier
                //        byte messageType = (byte)((status >> 4) & 0xF);

                //        MidiEvent tempEvent = null;

                //        // Handle voice events
                //        if (messageType >= 0x8 && messageType <= 0xE)
                //        {
                //            if (!running) pos++; // if we're running, we don't advance; if we're not running, we do
                //            byte channel = (byte)(status & 0xF); // grab the channel from the status byte
                //            tempEvent = ParseVoiceEvent(deltaTime, messageType, channel, data, ref pos);
                //        }
                //        // Handle meta events
                //        else if (status == 0xFF)
                //        {
                //            pos++;
                //            byte eventType = data[pos];
                //            pos++;
                //            tempEvent = ParseMetaEvent(deltaTime, eventType, data, ref pos);
                //        }
                //        // Handle system exclusive events
                //        else if (status == 0xF0)
                //        {
                //            pos++;
                //            long length = ReadVariableLength(data, ref pos); // figure out how much data to read

                //            // If this is single-segment message, process the whole thing
                //            if (data[pos + length - 1] == 0xF7)
                //            {
                //                sysExData = new byte[length - 1];
                //                Array.Copy(data, (int)pos, sysExData, 0, (int)length - 1);
                //                tempEvent = new SystemExclusiveMidiEvent(deltaTime, sysExData);
                //            }
                //            // It's multi-segment, so add the new data to the previously aquired data
                //            else
                //            {
                //                // Add to previously aquired sys ex data
                //                int oldLength = (sysExData == null ? 0 : sysExData.Length);
                //                byte[] newSysExData = new byte[oldLength + length];
                //                if (sysExData != null) sysExData.CopyTo(newSysExData, 0);
                //                Array.Copy(data, (int)pos, newSysExData, oldLength, (int)length);
                //                sysExData = newSysExData;
                //                sysExContinue = true;
                //            }
                //            pos += length;
                //        }
                //        // Handle system exclusive continuations
                //        else if (status == 0xF7)
                //        {
                //            if (!sysExContinue) sysExData = null;

                //            // Figure out how much data there is
                //            pos++;
                //            long length = ReadVariableLength(data, ref pos);

                //            // Add to previously aquired sys ex data
                //            int oldLength = (sysExData == null ? 0 : sysExData.Length);
                //            byte[] newSysExData = new byte[oldLength + length];
                //            if (sysExData != null) sysExData.CopyTo(newSysExData, 0);
                //            Array.Copy(data, (int)pos, newSysExData, oldLength, (int)length);
                //            sysExData = newSysExData;

                //            // Make it a system message if necessary (i.e. if we find an end marker)
                //            if (data[pos + length - 1] == 0xF7)
                //            {
                //                tempEvent = new SystemExclusiveMidiEvent(deltaTime, sysExData);
                //                sysExData = null;
                //                sysExContinue = false;
                //            }
                //        }
                //        // Nothing we know about
                //        else throw new MidiParserException("Invalid status byte found.", pos);

                //        // Add the newly parsed event if we got one
                //        if (tempEvent != null) track.Events.Add(tempEvent);
                //    }

                //    // Return the newly populated track
                //    return track;
                //}

            catch
                (Exception)
            {
            }
        }

        private void ParseNoteOnEvent(MemoryStream memoryStream, int channelNumber)
        {
            var noteNumber = memoryStream.ReadByte();
            var velocity = memoryStream.ReadByte();
        }

        private void ParsePitchBendEvent(MemoryStream memoryStream, int channelNumber)
        {
            memoryStream.ReadByte();
            memoryStream.ReadByte();
        }

        private void ParseProgramChangeEvent(MemoryStream memoryStream, int channelNumber)
        {
            // Ignore this event for the moment, maybe implement it later
            var programNumber = memoryStream.ReadByte();
        }

        private void ReadMetaEvent(MemoryStream memoryStream)
        {
            var metaEventType = (byte) memoryStream.ReadByte(); // ignored for the moment
            var length = GetVariableLengthValue(memoryStream);
            memoryStream.Seek(length, SeekOrigin.Current);
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

        private long GetVariableLengthValue(MemoryStream memoryStream)
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
