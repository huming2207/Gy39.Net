using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using Gy39Sensor.Core.Model;

namespace Gy39Sensor.Core
{
    public class Gy39
    {
        private readonly SerialPort _serialPort;

        public event EventHandler<SensorDataReceivedEventArgs> SensorDataReceived;

        /// <summary>
        /// Constructor for GY-39 Library. User need to specify a port and a baud rate (either 9600 or 115200)
        /// </summary>
        /// <param name="port">Serial port of the sensor</param>
        /// <param name="baudRate">Baud rate of the sensor, should be 9600 or 115200</param>
        public Gy39(string port, int baudRate)
        {
            _serialPort = new SerialPort(port, baudRate, Parity.None, 8, StopBits.One);
            _serialPort.DataReceived += SerialPort_DataReceived;
        }

        /// <summary>
        /// Minimal constructor for GY-39 Library. User need to specify a port with default baud rate at 9600.
        /// </summary>
        /// <param name="port">Serial port of the sensor</param>
        public Gy39(string port)
        {
            _serialPort = new SerialPort(port, 9600, Parity.None, 8, StopBits.One);
            _serialPort.DataReceived += SerialPort_DataReceived;
        }

        public void Start()
        {
            _serialPort.Open();
        }

        public void Stop()
        {
            _serialPort.Close();
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var eventArgs = new SensorDataReceivedEventArgs();
            var dataSets = GetDataFromSerial();
            if(dataSets == null) return;

            foreach (var dataSet in dataSets)
            {
                if (dataSet[2] == 0x15) eventArgs.Brightness = SensorDataReader.GetBrightnessFromData(dataSet);
                if (dataSet[2] == 0x45) eventArgs.Weather = SensorDataReader.GetWeatherFromData(dataSet);
            }
            
            OnDataConverted(eventArgs);
        }

        private IEnumerable<byte[]> GetDataFromSerial()
        {
            // Create a buffer and fill it in with incoming data
            var buffer = new byte[_serialPort.BytesToRead];
            _serialPort.Read(buffer, 0, _serialPort.BytesToRead);

            // Truncate the buffer based on the sensor spec, 
            // where it starts with two 0x5A bytes, followed by a type byte and a length byte.
            var packetBeginIndex = 0;
            var truncatedDataSets = new List<byte[]>();
            while(packetBeginIndex >= 0)
            {
                // Get the index of the first 0x5A for each round
                packetBeginIndex = Array.IndexOf(buffer, 0x5A, packetBeginIndex);

                // According to the datasheet, a set of data should be at least 8 bytes long.
                // If the buffer length is shorter than 8 bytes, it must be wrong.
                if (packetBeginIndex + 7 > buffer.Length) return null;

                // DataSetLength = DataHeaderLength + DataLength
                var dataSetLength = 4 + buffer[packetBeginIndex + 3];
                var truncatedDataSet = new byte[dataSetLength];
                Array.Copy(buffer, packetBeginIndex, truncatedDataSet, 0, dataSetLength);
                truncatedDataSets.Add(truncatedDataSet);
            }

            return truncatedDataSets;
        }

        protected virtual void OnDataConverted(SensorDataReceivedEventArgs eventArgs)
        {
            var handler = SensorDataReceived;
            handler?.Invoke(this, eventArgs); // Same as "if (handler != null) handler(this, eventArgs);"
        }
    }
}
