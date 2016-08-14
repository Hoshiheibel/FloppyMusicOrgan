# FloppyMusicOrgan

## About

FloppyMusicOrgan is a tool to play music files on floppy drives with an [Arduino Mega 2560](http://arduino.cc/en/Main/ArduinoBoardMega2560) microcontroller.
The software parses Midi-files, converts them to a custom format and sends commands to an Arduino controller which controls the floppy drives.
The project is being developed with VisualStudio in C#.

The software is currently under development, thus still contains a few bugs and is not yet feature complete, but playing music works fine in most cases. Midi parsing only handles NoteOn, NoteOff and TempoChange events so far. It already works well with "good" midi files, but there are still problems with a few special cases.



## Project structure


### ComOutput

Handles communication between PC and the controller via USB.


### FloppyMusicOrgan

The WPF UI.


### MidiParser

Handles parsing of the midi files. The MidiParser project is completely independent of all other projects and can easily be reused in other projects to parse midi files.


### MidiPlayer

Handles the playback of the midi files. Acts as an interface between the UI and the ComOutput projects.


### MidiToArduionoConverter

Handles conversion of the parsed midi files to a custom format which is sent to the microcontroller.


## Project Dependencies

In order to build this project you only need to install the Visual Studio Extension "NuGet Package Manager". Once you build the project for the first time, all external dependencies should be downloaded automatically.


## How it works

When you open a MIDI file, the parser converts the notes from the MIDI file to instructions that the arduino can

use. For exemple, when you read a midi file and there is a NOTE_ON command, the parser reads it and tells the arduino

to play this note, on the right channel and with the right frequency.

The arduino play the note and counts how many pulse they are so it knows when the reverse

the direction of the motor.