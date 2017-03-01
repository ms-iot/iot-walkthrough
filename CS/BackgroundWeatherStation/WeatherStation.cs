using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace BackgroundWeatherStation
{
    class WeatherStation
    {
        private abstract class Htu21dDefinitions
        {
            public const int ADDRESS = 0x40;
            public readonly static byte[] TEMPERATURE_COMMAND = { 0xE3 };
            public readonly static byte[] HUMIDITY_COMMAND = { 0xE5 };
        }

        private abstract class Mpl3115a2Definitions
        {
            // Datasheet available at http://www.nxp.com/assets/documents/data/en/data-sheets/MPL3115A2.pdf
            public const int ADDRESS = 0x60;
            public const int WHO_AM_I_ID = 0xC4;
            public const int CTRL_REG1 = 0x26;
            public readonly static byte[] PRESSURE_COMMAND = { 0x01 };
            public readonly static byte[] WHO_AM_I = { 0x0C };
        }

        private I2cDevice htu21d, mpl3115a2;

        public async Task InitAsync()
        {
            var controller = await I2cController.GetDefaultAsync();
            if (controller == null)
            {
                throw new Exception("No I2C controller found");
            }

            htu21d = controller.GetDevice(new I2cConnectionSettings(Htu21dDefinitions.ADDRESS));
            mpl3115a2 = controller.GetDevice(new I2cConnectionSettings(Mpl3115a2Definitions.ADDRESS));
            if (htu21d == null || mpl3115a2 == null)
            {
                throw new Exception("Failed to open I2C device. Make sure the bus is not in use.");
            }
            int who_am_i_id;
            try
            {
                who_am_i_id = WriteRead8(mpl3115a2, Mpl3115a2Definitions.WHO_AM_I);
            }
            catch (FileNotFoundException)
            {
                // First I2C operation might fail if some slave was in a bad state
                who_am_i_id = WriteRead8(mpl3115a2, Mpl3115a2Definitions.WHO_AM_I);
            }
            if (who_am_i_id != Mpl3115a2Definitions.WHO_AM_I_ID)
            {
                throw new Exception("MP13115A2 fails WHO_AM_I test");
            }
            // 0x39 = barometer mode, oversampling of 128 samples, ACTIVE mode
            // For a full list of flags, see page 33 of the datasheet
            byte[] activateCmd = { Mpl3115a2Definitions.CTRL_REG1, 0x39 };
            mpl3115a2.Write(activateCmd);
        }

        public double ReadTemperature()
        {
            /// <summary>Read temperature reading from HTU21D.
            /// <para>Temperature is returned in Celsius degrees.</para>
            /// </summary>
            return -46.85 + (175.72 * WriteRead16(htu21d, Htu21dDefinitions.TEMPERATURE_COMMAND) / 65536.0);
        }

        public double ReadHumidity()
        {
            /// <summary>Read humidity reading from HTU21D.
            /// <para>Humidity is returned in %.</para>
            /// </summary>
            return -6 + (125 * WriteRead16(htu21d, Htu21dDefinitions.HUMIDITY_COMMAND) / 65536.0);
        }

        public double ReadPressure()
        {
            /// <summary>Read pressure from MPL3115A2.
            /// <para>Pressure is returned in Pa.</para>
            /// </summary>
            int rawReading = WriteRead24(mpl3115a2, Mpl3115a2Definitions.PRESSURE_COMMAND);
            return (rawReading >> 6) + ((rawReading >> 4) & 3) / 4.0;
        }

        private int WriteRead24(I2cDevice sensor, byte[] command)
        {
            byte[] data = new byte[3];
            sensor.WriteRead(command, data);
            return data[0] << 16 | data[1] << 8 | data[2];
        }

        private int WriteRead16(I2cDevice sensor, byte[] command)
        {
            byte[] data = new byte[2];
            sensor.WriteRead(command, data);
            return data[0] << 8 | data[1];
        }

        private int WriteRead8(I2cDevice sensor, byte[] command)
        {
            byte[] data = new byte[1];
            sensor.WriteRead(command, data);
            return data[0];
        }
    }
}

