using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace BeranekCZ.NETMF.Expanders
{
    /// <summary>
    /// I/O 8-bit expander
    /// </summary>
    public class PCF8574
    {
        private I2CDevice _device;
        private I2CDevice.Configuration _config;
        private int _timeout;

        public PCF8574(I2CDevice device,I2CDevice.Configuration configuration)
        {
            //if (device.Config.ClockRateKhz > 100) throw new ArgumentOutOfRangeException("device.Config.ClockRateKhz", "ClockRateKhz have to be 100 (maximum)");
            this._device = device;
            this._timeout = 200;
            this._config = configuration;
        }

        public void SetPins(byte data)
        {
            this._device.Config = _config;
            this._device.Execute(
                new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(new byte[] { data }) },
                _timeout
                );
        }

    }
}
