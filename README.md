# Gy39.Net

.NET library and demo WPF app for GY-39 sensor (UART Serial port only)

## How to use this library

```csharp
var gy39 = new Gy39("COM3", 9600);
Console.WriteLine(gy39.QueryWeather().Temperature); // For temperature
Console.WriteLine(gy39.QueryWeather().Humidity); // For humidity
Console.WriteLine(gy39.QueryBrightness().Brightness); // For brightness
```

## Intro for GY-39 communication protocol

This protocol spec is translated from the original data sheet in Chinese. Here is the Baidu CloudDisk link: [GY-39系列](https://pan.baidu.com/s/1hrOnGTe#list/path=%2FGy%E7%B3%BB%E5%88%97%2FGY-39&parentPath=%2FGy%E7%B3%BB%E5%88%97)

## Jumper settings

- When S0 is connected to GND, the microcontroller is set to IIC mode, where the CT Pin is SCL, and DR is SDA.
- When S0 is disconnected, the microcontroller is set to UART mode, where the CT pin is TxD and DR is RxD.
- S1 is the microcontroller enable pin. If it is connected to GND, then the microcontroller will not be initialised. The user can access the sensors via the "raw sensor IIC" with SDA/SCL pins.

## UART

Default UART setting is 9600,8,n,1. The user can set the baud rate to 115200.

The default setting for the module is automatically sending two sets of data (two frames) at the same time on every second. 

### Request data (for setting adjustments or data query)

#### UART settings

- Byte 0: 0xA5, "Frame head", fixed value
- Byte 1:

| Bits | Bit 7 | Bit 6 | Bit 5 | Bit 4 | Bit 3 | Bit 2 | Bit 1 | Bit 0 |
|------|-------|-------|-------|-------|-------|-------|-------|-------|
| Data | AUTO  | 0     | 0     | 0     | 0     | 0     | BME   | MAX   |

    * If AUTO is 1, then the sensor module will save as default setting and automatically sends the data after it boots up.
    * If BME is 1, then the weather sensor (for temperature/humidity/atmosphere) will be selected
    * If MAX is 1, then the brightness sensor will be selected

- Byte 3: the checksum for Byte #1 plus Byte #2.

For example, if the user hopes to temporarily shut up the sensor module for automatically sending the sensor data, then send:

```
0xA5, 0x00, 0xA5
```

To restore the default settings, send:

```
0xA5, 0x03, 0xA8
```

#### IIC address configuration

Send:

```
0xAA, ADDR, SUM
```
...where ADDR is the IIC address and SUM is the checksum of Byte #1 plus Byte #2.

### Sensor data query

For brightness data, send:

```
0xA5, 0x51, 0xF6
```

For weather data, send:

```
0xA5, 0x52, 0xF7
```

### Baud rate configuration

For 9600bps, send:

```
0xA5, 0xAE, 0x53
```

For 115200bps, send:

```
0xA5, 0xAF, 0x54
```

### Response data

- Byte 0: 0x5A, "Frame head", fixed value
- Byte 1: 0x5A, "Frame head", fixed value
- Byte 2: 0x15 or 0x45, Frame type, 0x15 for brightness data, 0x45 for temperature/humidity/atmostphere data
- Byte 3: 0x04 Data length, starts from Byte 4 to Byte N.
- Byte 4 to N, 0x00~0xFF: Sensor data payload
- Byte N+1: 0x00~0xFF Checksum for the payload (LSB only)

#### Brightness data

The data format for brightness sensor is 4 bytes, from Byte #4 to #7, in Lux:

```
data_in_lux = ((Byte[4] << 24) | (Byte[5] << 16) | (Byte[6] << 8) | Byte[7])/100
```

For example, if a data frame received like this:

```5A-5A-15-04-00-00-FE-40-0B```

We can calculate the data like this:

```
data=((0x00<<24)|(0x00<<16)|(0xFE<<8)|0x40)/100
```

#### Temperature data

The data format for temperature is 2 bytes, from Byte #4 to #5, in Celsius:

```
data_in_c = ((Byte[4] << 8) | Byte[5])/100
```

#### Atmosphere data

The data format for atmosphere is 4 bytes, from Byte #6 to #9, in Pascal:

```
data_in_pa = ((Byte[6] << 24) | (Byte[7] << 16) | (Byte[8] << 8) | Byte[9])/100
```

#### Humidity data

The data format for temperature is 2 bytes, from Byte #10 to #11, in RH%:

```
data_in_rh_percentage = ((Byte[10] << 8) | Byte[11])/100
```
## IIC mode

Default 7-bit address is 0x5B, 8-bit address is 0xB6.

**Byte position**|**Name**|**Description**
:-----:|:-----:|:-----:
0x00|H\_LUX\_H|Brightness MSB (high)
0x01|H\_LUX\_L|Brightness MSB (low)
0x02|L\_LUX\_H|Brightness LSB (high)
0x03|L\_LUX\_L|Brightness LSB (low)
0x04|T\_H|Temperature MSB
0x05|T\_L|Temperature LSB
0x06|H\_P\_H|Atmosphere MSB (high)
0x07|H\_P\_L|Atmosphere MSB (low)
0x08|L\_P\_H|Atmosphere LSB (high)
0x09|L\_P\_L|Atmosphere LSB (low)
0x0a|HUM\_H|Humidity MSB
0x0b|HUM\_L|Humidity LSB
0x0c|H\_H|Altitude MSB
0x0d|H\_L|Altitude LSB

