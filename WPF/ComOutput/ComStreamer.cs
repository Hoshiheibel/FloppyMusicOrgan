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
        private SerialPort _port;

        public ComStreamer()
        {
            GetAvailableComPorts();
        }

        public List<string> AvailableComPorts { get; private set; }

        public bool IsConnected
        {
            get { return _port != null && _port.IsOpen; }
        }

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
                if (_port == null)
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
            if (_port != null)
            {
                _port.Dispose();
                _port = null;
            }
        }

        public void SendStopCommand()
        {
            if (!IsConnected)
                return;

            var message = new ArduinoMessage
            {
                ComMessage = new byte[0]
            };

            for (var i = 0; i < 16; i++)
            {
                message.ComMessage = message.ComMessage.Concat(new[]
                    {
                        (byte) (i + 1),
                        (byte) 0,
                        (byte) 0
                    })
                    .ToArray();
            }
            
            SendCommand(message.ComMessage);
        }

        private void GetAvailableComPorts()
        {
            AvailableComPorts = new List<string>();
            AvailableComPorts.AddRange(SerialPort.GetPortNames());
        }
    }
}
