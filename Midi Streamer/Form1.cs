using System;
using System.IO;
using System.Windows.Forms;
using MidiParser;
using MidiParser.Entities;

namespace Midi_Streamer
{
    public partial class Form1 : Form
    {
        private ParsedMidiFile _parsedMidiFile;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var midiParser = new MidiReader();
            _parsedMidiFile = midiParser.Parse(Path.Combine(Application.StartupPath, "Resources", "_TestFile.mid"));
        }
    }
}
