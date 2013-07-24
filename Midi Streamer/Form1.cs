using System;
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
            midiParser.Parse(@"C:\Users\b.bandleon\Downloads\midi\Red_OctoberD.MID");
        }
    }
}
