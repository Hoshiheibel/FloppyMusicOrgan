using System;
using System.IO;
using System.Windows;
using System.Windows.Documents;
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
        private const int ExpectedMidiHeaderLength = 6;

        private MidiFile _midiFile;
        private BinaryReader _fileReader;
        ////private byte _lastStatus;

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

            if (fileIdentifier != MidiFileFormatIdentifier)
                throw new InvalidMidiHeaderException();

            _midiFile.FileHeader.FileIdentifier = fileIdentifier;
        }

        private void ReadHeaderLength()
        {
            var headerLength = _fileReader.ReadBytes(4).ConvertToInt();

            if (headerLength != ExpectedMidiHeaderLength)
                throw new InvalidHeaderLengthException();

            _midiFile.FileHeader.HeaderLength = headerLength;
        }

        private void ReadMidiFormatType()
        {
            MidiFormatTypeEnum formatType;

            try
            {
                formatType = _fileReader.ReadBytes(2).ConvertToInt().ConvertToEnum<MidiFormatTypeEnum>();
            }
            catch (InvalidCastException ex)
            {
                throw new InvalidMidiFormatTypeException(ex);
            }

            if (formatType == MidiFormatTypeEnum.MultipleTracksAsynchronously)
                throw new NotSupportedException("MidiFormatType \"MultipleTracksAsynchronously (2)\" is not supported!");

            _midiFile.FileHeader.Format = formatType;
        }

        private void ReadTrackCount()
        {
            var trackCount = _fileReader.ReadBytes(2).ConvertToInt();
            _midiFile.FileHeader.NumberOfTracks = trackCount;
        }

        private void ReadTimeDivision()
        {
            var timeDivision = _fileReader.ReadBytes(2);
            _midiFile.TimeDivisionType = (TimeDivisionTypeEnum) (timeDivision[0] & 0x80);

            // ToDo: parse FramesPerSecond correctly, only works for TicksPerBeat at the moment
            if (_midiFile.TimeDivisionType == TimeDivisionTypeEnum.FramesPerSecond)
                throw new NotImplementedException("TimeDivision type \"FramesPerSecond\" not yet implemented");

            _midiFile.FileHeader.TimeDivision = timeDivision.ConvertToInt();
        }

        private void ReadTrackChunks()
        {
            for (var i = 0; i < _midiFile.FileHeader.NumberOfTracks; i++)
            {
                // ToDo: Own method "ReadTrackChunk"
                var track = new Track();

                ReadTrackChunkId(track);
                ReadTrackChunkSize(track);
                
                // ToDo: pass _fileReader directly to ParseTrack()
                using (var memoryStream = new MemoryStream(_fileReader.ReadBytes(track.TrackLength)))
                    ParseTrack(memoryStream, track);

                _midiFile.Tracks.Add(track);
            }
        }

        private void ReadTrackChunkSize(Track track)
        {
            track.TrackLength = _fileReader.ReadBytes(4).ConvertToInt();
        }

        private void ReadTrackChunkId(Track track)
        {
            var trackHeaderIdentifier = _fileReader.ReadBytes(4).ConvertToString();

            if (trackHeaderIdentifier != MidiTrackChunkIdentifier)
                throw new InvalidChunkHeaderException();

            track.TrackHeader.TrackHeaderIdentifier = trackHeaderIdentifier;
        }

        private void ParseTrack(Stream memoryStream, Track track)
        {
            var lastStatus = new byte();

            try
            {
                // Process all bytes, turning them into events
                while (memoryStream.Position < memoryStream.Length)
                {
                    var deltaTime = GetVariableLengthValue(memoryStream);
                    var status = (byte) memoryStream.ReadByte();

                    var running = (status & 0x80) == 0;

                    if (running)
                    {
                        status = lastStatus;
                        memoryStream.Seek(- 1, SeekOrigin.Current);
                    }

                    if (!running)
                        lastStatus = status;

                    if ((MidiEventType)status == MidiEventType.MetaEvent)
                        ReadMetaEvent(memoryStream, deltaTime, track);
                    else if ((MidiEventType)status == MidiEventType.SysExContinuationEvent)
                        ReadSysExEvent(memoryStream, deltaTime, track, MidiEventType.SysExContinuationEvent);
                    else if ((MidiEventType)status == MidiEventType.SysExEvent)
                        ReadSysExEvent(memoryStream, deltaTime, track, MidiEventType.SysExEvent);
                    else
                    {
                        switch ((MidiEventType)status.GetFirstNibble())
                        {
                            // ToDo: Handle "All Notes Off"
                            case MidiEventType.AllNotesOff:
                                break;

                            case MidiEventType.NoteOff:  // Note Off
                                ParseNoteOffEvent(memoryStream, deltaTime, status.GetSecondNibble(), track);
                                break;

                            case MidiEventType.NoteOn:  // Note on
                                ParseNoteOnEvent(memoryStream, deltaTime, status.GetSecondNibble(), track);
                                break;

                            case MidiEventType.KeyAftertouch:  // Key aftertouch
                                ParseKeyAftertouchEvent(memoryStream, deltaTime, status.GetSecondNibble(), track);
                                break;

                            case MidiEventType.ControllerChange:  // Controller Change event
                                ParseControllerChangeEvent(memoryStream, deltaTime, status.GetSecondNibble(), track);
                                break;

                            case MidiEventType.ProgramChange:  // Program change
                                ParseProgramChangeEvent(memoryStream, deltaTime, status.GetSecondNibble(), track);
                                break;

                            case MidiEventType.ChannelAftertouch:  // Channel Aftertouch event
                                break;

                            case MidiEventType.PitchBend:  // Pitch Bend event
                                ParsePitchBendEvent(memoryStream, status.GetSecondNibble());
                                break;
                        }

                        //nextValue.GetFirstNibble() == 0xF0;
                    }
                }
            }
            // ToDo: Catch outside
            catch
                (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
        }

        // ToDo: All parse methods should return result, adding should happen outside
        private static void ParseControllerChangeEvent(Stream stream, long deltaTime, int channelNumber, Track track)
        {
            FakeReadEvent(stream, deltaTime, channelNumber, track);
        }

        private static void FakeReadEvent(Stream stream, long deltaTime, int channelNumber, Track track)
        {
            stream.SkipBytes(2);

            track.TrackEventChain.Add(new BaseMidiChannelEvent
            {
                ChannelNumber = channelNumber,
                DeltaTime = deltaTime
            });
        }

        private static void ParseKeyAftertouchEvent(Stream stream, long deltaTime, int channelNumber, Track track)
        {
            FakeReadEvent(stream, deltaTime, channelNumber, track);
        }

        private static void ReadSysExEvent(Stream stream, long deltaTime, Track track, MidiEventType currentEventType)
        {
            var length = GetVariableLengthValue(stream);
            var offsetIncrement = currentEventType == MidiEventType.SysExContinuationEvent ? 0 : 1;
            stream.Seek(length + offsetIncrement, SeekOrigin.Current);

            track.TrackEventChain.Add(new BaseMidiChannelEvent
            {
                DeltaTime = deltaTime
            });
        }

        private static void ParseTempoChangeEvent(Stream stream, long deltaTime, Track track)
        {
            const int timeMultiplier = 60 * 1000 * 1000;

            var microsecondsPerQuarterNote = new byte[4];
            stream.Read(microsecondsPerQuarterNote, 1, 3);

            track.TrackEventChain.Add(new TempoChangeEvent
            {
                BPM = timeMultiplier / microsecondsPerQuarterNote.ConvertToInt(),
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
            // ToDo: Create only if needed
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
            stream.SkipBytes(2);
        }

        private static void ParseProgramChangeEvent(Stream stream, long deltaTime, int channelNumber, Track track)
        {
            track.TrackEventChain.Add(new BaseMidiChannelEvent
            {
                ChannelNumber = channelNumber,
                DeltaTime = deltaTime
            });

            stream.SkipBytes(1);
        }

        private static void ReadMetaEvent(Stream stream, long deltaTime, Track track)
        {
            var eventType = (byte)stream.ReadByte();
            var length = GetVariableLengthValue(stream);

            switch ((MidiEventType)eventType)
            {
                case MidiEventType.TempoChange:
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

        private static long GetVariableLengthValue(Stream stream)
        {
            var currentByte = stream.ReadByte();

            if ((currentByte & 0x80) == 0)
                return currentByte;

            // Clear the "more data ahead" Bit
            long deltaTime = currentByte & 0x7f;

            do
            {
                currentByte = stream.ReadByte();
                deltaTime = (deltaTime << 7) + (currentByte & 0x7f);
            }
            while (stream.Position < stream.Length && ((currentByte & 0x80) != 0));

            return deltaTime;
        }
    }
}
