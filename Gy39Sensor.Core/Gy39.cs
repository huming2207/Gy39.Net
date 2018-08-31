using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gy39Sensor.Core.Model;

namespace Gy39Sensor.Core
{
    public class Gy39 : IDisposable
    {
        private readonly SerialPort _serialPort;

        /// <summary>
        /// Constructor for GY-39 Library. User need to specify a port and a baud rate (either 9600 or 115200)
        /// Meanwhile, start the serial port, send 0xA5, 0x00, 0xA5 to disable the auto output
        /// as it's really a headache for handling data.
        /// It's hard to judge whether the data is valid with the auto mode (maybe polluted with junk data).
        /// </summary>
        /// <param name="port">Serial port of the sensor</param>
        /// <param name="baudRate">Baud rate of the sensor, should be 9600 or 115200</param>
        public Gy39(string port, int baudRate)
        {
            _serialPort = new SerialPort(port, baudRate, Parity.None, 8, StopBits.One);
            _serialPort.Open();
            _serialPort.Write(new byte[] { 0xA5, 0x00, 0xA5 }, 0, 3);
        }

        /// <summary>
        /// Minimal constructor for GY-39 Library. User need to specify a port with default baud rate at 9600.
        /// </summary>
        /// <param name="port">Serial port of the sensor</param>
        public Gy39(string port)
        {
            _serialPort = new SerialPort(port, 9600, Parity.None, 8, StopBits.One);
            _serialPort.Open();
            _serialPort.Write(new byte[] { 0xA5, 0x00, 0xA5 }, 0, 3);
        }

        public Weather QueryWeather()
        {
            var buffer = PerformQuery(new byte[] {0xA5, 0x52, 0xF7}, 15);
            return SensorDataReader.GetWeatherFromData(buffer);
        }

        public Brightness QueryBrightness()
        {
            var buffer = PerformQuery(new byte[] {0xA5, 0x51, 0xF6}, 9);
            return SensorDataReader.GetBrightnessFromData(buffer);
        }

        public byte[] PerformQuery(byte[] commandData, int length)
        {
            // Send weather query command   
            _serialPort.Write(commandData, 0, commandData.Length);
            var buffer = new byte[length];

            // What for a while, the sensor need some time to respond.
            Thread.Sleep(50);

            _serialPort.Read(buffer, 0, length);
            return buffer;
        }

        public void Dispose()
        {
            _serialPort.Close();
            _serialPort?.Dispose();
        }
    }
}
