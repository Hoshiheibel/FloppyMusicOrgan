using System;
using System.IO;
using System.Windows.Forms;
using MidiParser;

namespace Midi_Streamer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var midiParser = new MidiReader();
            midiParser.Parse(Path.Combine(Application.StartupPath, "Resources", "TestFile.mid"));
        }
    }
}
