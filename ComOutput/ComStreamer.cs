using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using MidiParser.Extensions;

namespace ComOutput
{
    public class ComStreamer
    {
        private SerialPort _port;

        public ComStreamer()
        {
            GetAvailableComPorts();
        }

        public List<string> AvailableComPorts { get; set; }

        public void SendResetDrivesCommand()
        {
            if (_port != null && _port.IsOpen)
            {
                _port.Write(new byte[] {100, 0, 0}, 0, 3);
                Thread.Sleep(500);
            }
        }

        public bool Connect(string portName)
        {
            try
            {
                _port = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One);
                _port.Open();
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public void Disconnect()
        {
            if (_port != null && _port.IsOpen)
                _port.Close();
        }

        public void SendCommand(byte pin, int periodData)
        {
            var message = new[] { pin, (byte)((periodData >> 8) & 0xFF), (byte)(periodData & 0xFF) };
            _port.Write(message, 0, 3);
        }

        public void SendCommand(byte[] message)
        {
            _port.Write(message, 0, message.Length);
        }

        public void Dispose()
        {
            if (_port != null)
            {
                _port.Dispose();
                _port = null;
            }
        }

        public void SendStopCommand()
        {
            // ToDo: Send NoteOff command to all drives
            SendResetDrivesCommand();
        }

        private void GetAvailableComPorts()
        {
            AvailableComPorts = new List<string>();
            AvailableComPorts.AddRange(SerialPort.GetPortNames());
        }
    }
}
