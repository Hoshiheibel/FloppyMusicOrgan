# FloppyMusicOrgan

## About

FloppyMusicOrgan is a tool to play music files on floppy drives with an [Arduino Mega 2560](http://arduino.cc/en/Main/ArduinoBoardMega2560) microcontroller.
The software parses Midi-files, converts them to a custom format and sends commands to an Arduino controller which controls the floppy drives.
The project is being developed with VisualStudio in C#.

The software is currently under development, thus still contains a few bugs and is not yet feature complete, but playing music works fine in most cases. Midi parsing only handles NoteOn, NoteOff and TempoChange events so far. It already works well with "good" midi files, but there are still problems with a few special cases.


# Project structure

## ComOutput

Handles communication between PC and the controller via USB.


## FloppyMusicOrgan

The WPF UI.


## MidiParser

Handles parsing of the midi files. The MidiParser project is completely independent of all other projects and can easily be reused in other projects to parse midi files.


## MidiPlayer

Handles the playback of the midi files. Acts as an interface between the UI and the ComOutput projects.


## MidiToArduionoConverter

Handles conversion of the parsed midi files to a custom format which is sent to the microcontroller.


# Architecture Diagram

[![architecture diagram](ArchitectureGraph_For_Floppy_Music_Organ.png?raw=true)]