using System;
using Gy39Sensor.Core.Model;

namespace Gy39Sensor.Core
{
    public static class SensorDataReader
    {
        public static Weather GetWeatherFromData(byte[] data)
        {   
            // As datasheet said, the weather data set must be longer than 8 bytes
            if (data.Length < 13) return null;
            
            // The third byte is the type of data, 0x45 represents the weather data (temp, humidity, atmosphere)
            if (data[2] != 0x45) return null;


            return new Weather
            {
                // Byte 4 and 5 is the temperature data, e.g. 2345 is 23.45 ℃
                Temperature = (data[4] << 8 | data[5]) / 100.0,
                
                // Byte 6 to 9 is the barometer data (atmosphere), e.g.9988877 is 99888.77Pa
                Atmosphere = (data[6] << 24 | data[7] << 16 | data[8] << 8 | data[9]) / 100.0,
                
                // Byte 10 and 11 is the humidity data, e.g. 5678 is 56.78%
                Humidity = (data[10] << 8 | data[11]) / 100.0
            };
        }

        public static Brightness GetBrightnessFromData(byte[] data)
        {
            // As datasheet said, the brightness data set must be longer than 8 bytes
            if (data.Length < 8) return null;
            
            // The third byte is the type of data, 0x45 represents the weather data (temp, humidity, atmosphere)
            if (data[2] != 0x15) return null;

            return new Brightness
            {
                Lux = (data[4] << 24 | data[5] << 16 | data[6] << 8 | data[7]) / 100.0
            };
        }
    }
}