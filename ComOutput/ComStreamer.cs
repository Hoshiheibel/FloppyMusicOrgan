using System;
using System.Collections.Generic;
using System.IO.Ports;
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
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public void Disconnect()
        {
            if (_port.IsOpen)
                _port.Close();

            _port.Dispose();
        }

        public void SendCommand(byte pin, byte x, byte y)
        {
            var message = new[] {pin, x, y};
            _port.Write(message, 0, 3);
        }
    }
}
