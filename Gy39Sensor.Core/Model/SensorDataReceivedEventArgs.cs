using System;

namespace Gy39Sensor.Core.Model
{
    public class SensorDataReceivedEventArgs : EventArgs
    {
        public Weather Weather { get; set; }
        public Brightness Brightness { get; set; }
    }
}