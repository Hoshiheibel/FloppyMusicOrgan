using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace ComOutput
{
    public class ComStreamer
    {
        private readonly List<string> _availableComPorts = new List<string>();
        private SerialPort _port;

        public void GetAvailableComPorts()
        {
            _availableComPorts.AddRange(SerialPort.GetPortNames());
        }

        public void SendResetDrivesCommand()
        {
            if (_port.IsOpen)
            {
                _port.Write(new byte[]{100, 0, 0}, 0, 3);
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
            if (_port.IsOpen)
                _port.Close();
        }

        public void SendCommand(byte pin, int periodData)
        {
            var message = new[] { pin, (byte)((periodData >> 8) & 0xFF), (byte)(periodData & 0xFF) };
            System.Diagnostics.Trace.WriteLine("\"" + (int)message[0] + "\" / " + "\"" + (int)message[1] + "\" / " + "\"" + (int)message[2] + "\" / ");
            _port.Write(message, 0, 3);
        }

        public void SendCommand(byte[] message)
        {
            _port.Write(message, 0, message.Length);
            System.Diagnostics.Trace.WriteLine("\"" + (int)message[0] + "\" / " + "\"" + (int)message[1] + "\" / " + "\"" + (int)message[2] + "\" / ");
        }

        public void Dispose()
        {
            if (_port != null)
            {
                _port.Dispose();
                _port = null;
            }
        }
    }
}
