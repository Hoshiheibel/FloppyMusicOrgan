using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiParser.Entities
{
    public class ArduinoMessage
    {
        public long DeltaTime { get; set; }
        public byte[] ComMessage { get; set; }
    }
}
