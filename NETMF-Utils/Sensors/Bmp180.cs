using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Threading;

namespace BeranekCZ.NETMF.Sensors
{

    /*
     *  ultra low power mode 3 4.5 ms
        tc_p_std standard mode 5 7.5 ms
        tc_p_hr high resolution mode 9 13.5 ms
        tc_p_luhr ultra high res. mode 17 25.5 ms
        tc_p_ar Advanced res. mode 51 76.5 ms
        Conversion time
        temperature tC_temp standard mode 3 4.5 ms        UP = pressure data (16 to 19 bit)
        UT = temperature data (16 bit)     *  I2C up to 3.4Mbit/sec    */
    /// <summary>
    /// digital barometric pressure sensor
    /// Call ReadCalibrationData before getting temp and press. !
    /// </summary>
    public class Bmp180
    {
        private I2CDevice _device;
        private int _timeout = 200;

        public enum PresureAccurancyMode { UltraLowPower, Standard, HighResolution, UltraHighResolution }
        //calibration coefficients
        private short _AC1;
        private short _AC2;
        private short _AC3;
        private ushort _AC4;
        private ushort _AC5;
        private ushort _AC6;
        private short _B1;
        private short _B2;
        private short _MB;
        private short _MC;
        private short _MD;

        private long _X1;
        private long _X2;
        private long _B5;

        /// <summary>
        /// Call ReadCalibrationData before getting temp and press. !
        /// </summary>
        /// <param name="device"></param>
        public Bmp180(I2CDevice device)
        {
            _device = device;


        }

        private void write(byte data)
        {
            this._device.Execute(
                new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(new byte[] { data }) },
                _timeout
                );

        }

        private void write(byte[] data)
        {
            this._device.Execute(
                new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(data) },
                _timeout
                );
        }


        public bool ReadCalibrationData()
        {
            _AC1 = read2B(0xAA);
            if (_AC1 == 0x0 || _AC1 == 0xffff) return false; //ERROR
            _AC2 = read2B(0xAC);
            if (_AC2 == 0 || _AC2 == 0xffff) return false; //ERROR
            _AC3 = read2B(0xAE);
            if (_AC3 == 0 || _AC3 == 0xffff) return false; //ERROR
            _AC4 = read2BU(0xB0);
            if (_AC4 == 0 || _AC4 == 0xffff) return false; //ERROR
            _AC5 = read2BU(0xB2);
            if (_AC5 == 0 || _AC5 == 0xffff) return false; //ERROR
            _AC6 = read2BU(0xB4);
            if (_AC6 == 0 || _AC6 == 0xffff) return false; //ERROR
            _B1 = read2B(0xB6);
            if (_B1 == 0 || _B1 == 0xffff) return false; //ERROR
            _B2 = read2B(0xB8);
            if (_B2 == 0 || _B2 == 0xffff) return false; //ERROR
            _MB = read2B(0xBA);
            if (_MB == 0 || _MB == 0xffff) return false; //ERROR
            _MC = read2B(0xBC);
            if (_MC == 0 || _MC == 0xffff) return false; //ERROR
            _MD = read2B(0xBE);
            if (_MD == 0 || _MD == 0xffff) return false; //ERROR

            return true;
        }

        /// <summary>
        /// Read 2B from MsbAddress and MsbAddress+1
        /// </summary>
        /// <param name="MsbAddress"></param>
        /// <returns></returns>
        private short read2B(byte MsbAddress)
        {
            write(MsbAddress);
            byte[] readedVal = read(2);
            return (short)((readedVal[0] << 8) | readedVal[1]);
        }


        /// <summary>
        /// Read 2B unsigned from MsbAddress and MsbAddress+1
        /// </summary>
        /// <param name="MsbAddress"></param>
        /// <returns></returns>
        private ushort read2BU(byte MsbAddress)
        {
            write(MsbAddress);
            byte[] readedVal = read(2);
            return (ushort)((readedVal[0] << 8) | readedVal[1]);
        }

        /// <summary>
        /// Read defined number of bytes
        /// </summary>
        /// <param name="numberOfbytes"></param>
        /// <returns>Return null if numberOfReadedBytes != numberOfbytes</returns>
        private byte[] read(int numberOfbytes)
        {
            byte[] ret = new byte[numberOfbytes];
            int numberOfReadedBytes = this._device.Execute(
                new I2CDevice.I2CTransaction[] { I2CDevice.CreateReadTransaction(ret) },
                _timeout
                );
            if (numberOfReadedBytes == numberOfbytes) return ret;
            return null;
        }

        private byte readByte()
        {
            byte[] ret = new byte[1];
            this._device.Execute(
                new I2CDevice.I2CTransaction[] { I2CDevice.CreateReadTransaction(ret) },
                _timeout
                );
            return ret[0];
        }

        private byte ReadId()
        {
            write(0xD0);
            return readByte();
        }

        public bool Ping()
        {
            return ReadId() == 85;
        }

        private void WriteCommand(byte data)
        {
            write(new byte[] { 0xF4, data });   //0xF4 is command register address
        }

        public void Reset()
        {
            write(new byte[] {0xE0, 0xB6});
        }

        public long GetRawTemperature()
        {

            WriteCommand(0x2E);
            Thread.Sleep(5);    //defined waiting time
            write(0xF6);
            byte[] readedVal = read(2);
            return (long)((readedVal[0] << 8) | readedVal[1]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Temp in 0.1°C</returns>
        public long GetTemperature()
        {
            long ut = GetRawTemperature();
            _X1 = (ut - _AC6) * _AC5 / 32768;//(1 << 15); //(1 << 15) == 2 on 15
            _X2 = _MC * (2048) / (_X1 + _MD);
            _B5 = _X1 + _X2;
            return (_B5 + 8) / 16;
        }

        public long GetRawPressure(PresureAccurancyMode mode)
        {
            int data = 0x34 + ((byte)mode << 6);
            int waitingTime = 5;
            switch (mode)
            {
                case PresureAccurancyMode.UltraLowPower:
                    waitingTime = 5;
                    break;
                case PresureAccurancyMode.Standard:
                    waitingTime = 8;
                    break;
                case PresureAccurancyMode.HighResolution:
                    waitingTime = 14;
                    break;
                case PresureAccurancyMode.UltraHighResolution:
                    waitingTime = 26;
                    break;
            }
            WriteCommand((byte)data);
            Thread.Sleep(waitingTime);    //defined waiting time
            write(0xF6);
            byte[] readedVal = read(3);

            return (long)(((readedVal[0] << 16) | readedVal[1] << 8 | readedVal[2]) >> (8 - (int)mode));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode"></param>
        /// <returns>Presure in [Pa]</returns>
        public long GetPressure(PresureAccurancyMode mode)
        {
            GetTemperature();
            int oss = (int)mode;
            long UP = GetRawPressure(mode);
            long b6 = _B5 - 4000;
            _X1 = (_B2 * (b6 * b6 / (1 << 12))) / (1 << 11);
            _X2 = _AC2 * b6 / (1 << 11);
            long x3 = _X1 + _X2;
            long b3 = (((_AC1 * 4 + x3) << oss) + 2) / 4;
            _X1 = _AC3 * b6 / (1 << 13);
            _X2 = (_B1 * (b6 * b6 / (1 << 12))) / (1 << 16);
            x3 = ((_X1 + _X2) + 2) / 4;

            ulong b4 = _AC4 * (ulong)(x3 + 32768) / (1 << 15);
            ulong b7 = (ulong)((UP - b3) * (50000 >> oss));
            long p;

            if (b7 < 0x80000000) { p = (long)((b7 * 2) / b4); }
            else { p = (long)((b7 / b4) * 2); }

            _X1 = (p / 256) * (p / 256);
            _X1 = (_X1 * 3038) / (1 << 16);
            _X2 = (-7357 * p) / (1 << 16);
            p = p + (_X1 + _X2 + 3791) / 16;
            return p;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pressure">pressure in hPa</param>
        /// <returns>Altitude [m]</returns>
        public double GetAltitude(long pressure)
        {
            return 44330 * (1 - System.Math.Pow((pressure / 100 / 1013.25), 1 / 5.255));
        }
    }
}
