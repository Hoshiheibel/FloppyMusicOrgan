using System;
using System.IO;
using MidiParser.Entities.Enums;
using MidiParser.Entities.MidiEvents;
using MidiParser.Entities.MidiFile;
using MidiParser.Exceptions;
using MidiParser.Extensions;

namespace MidiParser
{
    public class Parser
    {
        private const string MidiFileFormatIdentifier = "MThd";
        private const string MidiTrackChunkIdentifier = "MTrk";

        private MidiEventTypeEnum _currentEventType;
        private MidiFile _midiFile;
        private BinaryReader _fileReader;
        private byte _lastStatus;

        public MidiFile Parse(string fileName)
        {
            _midiFile = new MidiFile();

            using (_fileReader = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                ReadHeaderChunk();
                ReadTrackChunks();
            }

            return _midiFile;
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

            _midiFile.FileHeader.FileIdentifier = fileIdentifier;
        }

        private void ReadHeaderLength()
        {
            var headerLength = _fileReader.ReadBytes(4).ConvertToInt();

            if (headerLength != 6)
                throw new InvalidHeaderLengthException();

            _midiFile.FileHeader.HeaderLength = headerLength;
        }

        private void ReadMidiFormatType()
        {
            var formatType = _fileReader.ReadBytes(2).ConvertToInt();

            if (IsInvalidMidiFormatType(formatType))
                throw new InvalidMidiFormatTypeException();

            _midiFile.FileHeader.Format = (MidiFormatTypeEnum) formatType;
        }

        private void ReadTrackCount()
        {
            var trackCount = _fileReader.ReadBytes(2).ConvertToInt();
            _midiFile.FileHeader.NumberOfTracks = trackCount;
        }

        private void ReadTimeDivision()
        {
            // ToDo: parse FramesPerSecond correctly, only works for TicksPerBeat at the moment
            var timeDivision = _fileReader.ReadBytes(2);
            _midiFile.TimeDivisionType = (TimeDivisionTypeEnum) (timeDivision[0] & 0x80);
            _midiFile.FileHeader.TimeDivision = timeDivision.ConvertToInt();
        }

        private void ReadTrackChunks()
        {
            for (var i = 0; i < _midiFile.FileHeader.NumberOfTracks; i++)
            {
                var track = new Track();

                ReadTrackChunkId(track);
                ReadTrackChunkSize(track);
                
                using (var memoryStream = new MemoryStream(_fileReader.ReadBytes(track.TrackLength)))
                {
                    ParseTrack(memoryStream, track);
                }

                _midiFile.Tracks.Add(track);
            }
        }

        private void ReadTrackChunkSize(Track track)
        {
            var trackLength = _fileReader.ReadBytes(4).ConvertToInt();
            track.TrackLength = trackLength;
        }

        private void ReadTrackChunkId(Track track)
        {
            var trackHeaderIdentifier = _fileReader.ReadBytes(4).ConvertToString();

            if (!string.Equals(trackHeaderIdentifier, MidiTrackChunkIdentifier))
                throw new InvalidChunkHeaderException();

            track.TrackHeader.TrackHeaderIdentifier = trackHeaderIdentifier;
        }

        private void ParseTrack(Stream memoryStream, Track track)
        {
            //long pos = 0; // current position in data
            //bool running = false; // whether we're in running status
            //int status = 0; // the current status byte
            //bool sysExContinue = false; // whether we're in a multi-segment system exclusive message
            //byte[] sysExData = null; // system exclusive data up to this point from a multi-segment message

            try
            {
                // Process all bytes, turning them into events
                while (memoryStream.Position < memoryStream.Length)
                {
                    var deltaTime = GetDeltaTime(memoryStream);
                    var status = (byte) memoryStream.ReadByte();
                    
                    if (status == 0xFF)
                    {
                        // Meta Event
                        _currentEventType = MidiEventTypeEnum.MetaEvent;
                        ReadMetaEvent(memoryStream, deltaTime, track);
                    }
                    else if (status == 0xF7)
                    {
                        // SysEx continuation
                        _currentEventType = MidiEventTypeEnum.SysExContinuationEvent;
                        ReadSysEx(memoryStream, deltaTime, track);
                    }
                    else if (status == 0xF0)
                    {
                        // SysEx
                        _currentEventType = MidiEventTypeEnum.SysExEvent;
                        ReadSysEx(memoryStream, deltaTime, track);
                    }
                    else
                    {
                        // if (status != 0) // with running status, 'status' can be zero.
                        // Midi Event
                        if ((status & 0x80) == 0)
                            status = _lastStatus;

                        switch (status.GetFirstNibble())
                        {
                            case 0x80:  // Note Off
                                ParseNoteOffEvent(memoryStream, deltaTime, status.GetSecondNibble(), track);
                                break;

                            case 0x90:  // Note on
                                ParseNoteOnEvent(memoryStream, deltaTime, status.GetSecondNibble(), track);
                                break;

                            case 0xA0:  // Key aftertouch
                                ParseKeyAftertouchEvent(memoryStream, deltaTime, status.GetSecondNibble(), track);
                                break;

                            case 0xB0:  // Controller Change event
                                ParseControllerChangeEvent(memoryStream, deltaTime, status.GetSecondNibble(), track);
                                break;

                            case 0xC0:  // Program change
                                ParseProgramChangeEvent(memoryStream, deltaTime, status.GetSecondNibble(), track);
                                break;

                            case 0xD0:  // Channel Aftertouch event
                                break;

                            case 0xE0:  // Pitch Bend event
                                ParsePitchBendEvent(memoryStream, status.GetSecondNibble());
                                break;

                            case 0xF0:
                                break;
                        }

                        //nextValue.GetFirstNibble() == 0xF0;
                    }
                }
            }
            catch
                (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
        }

        private void ParseControllerChangeEvent(Stream stream, long deltaTime, int channelNumber, Track track)
        {
            stream.ReadByte();
            stream.ReadByte();

            track.TrackEventChain.Add(new BaseMidiChannelEvent
            {
                ChannelNumber = channelNumber,
                DeltaTime = deltaTime
            });
        }

        private void ParseKeyAftertouchEvent(Stream stream, long deltaTime, int channelNumber, Track track)
        {
            stream.ReadByte();
            stream.ReadByte();

            track.TrackEventChain.Add(new BaseMidiChannelEvent
            {
                ChannelNumber = channelNumber,
                DeltaTime = deltaTime
            });
        }

        private void ReadSysEx(Stream stream, long deltaTime, Track track)
        {
            var length = GetVariableLengthValue(stream);

            if (_currentEventType == MidiEventTypeEnum.SysExContinuationEvent)
                stream.Seek(length, SeekOrigin.Current);
            else
                stream.Seek(length + 1, SeekOrigin.Current);

            track.TrackEventChain.Add(new BaseMidiChannelEvent
            {
                DeltaTime = deltaTime
            });
        }

        private static void ParseTempoChangeEvent(Stream stream, long deltaTime, Track track)
        {
            var microsecondsPerQuarterNote = new byte[4];
            stream.Read(microsecondsPerQuarterNote, 1, 3);

            track.TrackEventChain.Add(new TempoChangeEvent
            {
                BPM = 60 * 1000 * 1000 / microsecondsPerQuarterNote.ConvertToInt(),
                DeltaTime = deltaTime
            });
        }

        private static void ParseNoteOffEvent(Stream stream, long deltaTime, int channelNumber, Track track)
        {
            track.TrackEventChain.Add(new NoteOffEvent
            {
                Note = stream.ReadByte(),
                Velocity = stream.ReadByte(),
                ChannelNumber = channelNumber,
                DeltaTime = deltaTime
            });
        }

        private static void ParseNoteOnEvent(Stream stream, long deltaTime, int channelNumber, Track track)
        {
            var noteEvent = new NoteOnEvent
            {
                Note = stream.ReadByte(),
                Velocity = stream.ReadByte(),
                ChannelNumber = channelNumber,
                DeltaTime = deltaTime
            };

            if (noteEvent.Velocity != 0)
            {
                track.TrackEventChain.Add(noteEvent);
            }
            else
            {
                track.TrackEventChain.Add(new NoteOffEvent
                {
                    ChannelNumber = noteEvent.ChannelNumber,
                    DeltaTime = noteEvent.DeltaTime,
                    Note = noteEvent.Note,
                    Velocity = noteEvent.Velocity
                });
            }
        }

        private static void ParsePitchBendEvent(Stream stream, int channelNumber)
        {
            // Ignore this event for the moment, maybe implement it later
            var x = stream.ReadByte();
            x = stream.ReadByte();
        }

        private static void ParseProgramChangeEvent(Stream stream, long deltaTime, int channelNumber, Track track)
        {
            track.TrackEventChain.Add(new BaseMidiChannelEvent
            {
                ChannelNumber = channelNumber,
                DeltaTime = deltaTime
            });

            stream.ReadByte();
        }

        private static void ReadMetaEvent(Stream stream, long deltaTime, Track track)
        {
            var eventType = (byte)stream.ReadByte();
            var length = GetVariableLengthValue(stream);

            switch (eventType)
            {
                case 0x51:
                    ParseTempoChangeEvent(stream, deltaTime, track);
                    break;

                default:
                    stream.Seek(length, SeekOrigin.Current);
                    
                    track.TrackEventChain.Add(new BaseMidiChannelEvent
                    {
                        DeltaTime = deltaTime
                    });

                    break;
            }
        }

        private static long GetDeltaTime(Stream stream)
        {
            var currentByte = stream.ReadByte();
            long result = currentByte;

            if ((currentByte & 0x80) != 0)
            {
                // Clear the "more data ahead" Bit
                result &= 0x7f;

                do
                {
                    currentByte = stream.ReadByte();
                    result = (result << 7) + (currentByte & 0x7f);
                }
                while (stream.Position < stream.Length && ((currentByte & 0x80) != 0));
            }

            return result;
        }

        private static long GetVariableLengthValue(Stream stream)
        {
            var currentByte = stream.ReadByte();
            long result = currentByte;

            if ((currentByte & 0x80) != 0)
            {
                // Clear the "more data ahead" Bit
                result &= 0x7f;

                do
                {
                    currentByte = stream.ReadByte();
                    result = (result << 7) + (currentByte & 0x7f);
                }
                while (stream.Position < stream.Length && ((currentByte & 0x80) != 0));
            }

            return result;
        }
    }
}
