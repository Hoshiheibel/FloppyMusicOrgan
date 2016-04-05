using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using MidiToArduinoConverter;

namespace ComOutput
{
    public class ComStreamer
    {
        private const byte ResetDriveCommand = 100;
        private const byte PowerOffCommand = 126;
        private const byte PowerOnCommand = 127;
        private const int BaudRate = 115200;

        private SerialPort _port;

        public ComStreamer()
        {
            GetAvailableComPorts();
        }

        public List<string> AvailableComPorts { get; private set; }

        public bool IsConnected => _port != null && _port.IsOpen;

        public void SendResetDrivesCommand()
        {
            if (_port == null || !_port.IsOpen)
                return;

            _port.Write(new byte[] { ResetDriveCommand, 0, 0}, 0, 3);
            Thread.Sleep(500);
        }

        public bool Connect(string portName)
        {
            try
            {
                if (_port == null)
                    _port = new SerialPort(portName, BaudRate, Parity.None, 8, StopBits.One);

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
            if (_port != null && _port.IsOpen)
                _port.Close();

            Dispose();
        }

        public void SendCommand(byte[] message)
        {
            _port.Write(message, 0, message.Length);
        }

        public void Dispose()
        {
            if (_port == null)
                return;

            _port.Dispose();
            _port = null;
        }

        public void SendStopCommand()
        {
            if (!IsConnected)
                return;

            var message = new ArduinoMessage
            {
                ComMessage = Enumerable
                    .Repeat((byte)0, 48)
                    .ToArray()
            };
            
            SendCommand(message.ComMessage);
        }

        private void GetAvailableComPorts()
        {
            AvailableComPorts = new List<string>(SerialPort.GetPortNames());
        }

        public void SendPowerOffCommand()
        {
            _port.Write(new byte[] { PowerOffCommand, 0, 0 }, 0, 3);
            Thread.Sleep(500);
        }

        public void SendPowerOnCommand()
        {
            _port.Write(new byte[] { PowerOnCommand, 0, 0 }, 0, 3);
            Thread.Sleep(500);
        }
    }
}
