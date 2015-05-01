using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace BeranekCZ.NETMF.Utils
{
    public class I2CScanner
    {
        /// <summary>
        /// Scan range of addresses and print devices to debug output.
        /// </summary>
        /// <param name="startAddress">Start of scanning (included)</param>
        /// <param name="endAddress">End of scanning (included)</param>
        /// <param name="clockRateKhz">frequency in Khz</param>
        public static void ScanAddresses(ushort startAddress, ushort endAddress, ushort clockRateKhz = 100)
        {
            Debug.Print("Scanning...");
            for (ushort adr = startAddress; adr <= endAddress; adr++)
            {

                I2CDevice device = new I2CDevice(new I2CDevice.Configuration(adr, clockRateKhz));
                byte[] buff = new byte[1];
                try
                {
                    I2CDevice.I2CReadTransaction read = I2CDevice.CreateReadTransaction(buff);
                    var ret = device.Execute(new I2CDevice.I2CTransaction[] { read }, 1000);
                    if(ret > 0) Debug.Print("Device on address: "+adr+ " (0x"+adr.ToString("X")+")");
                    
                }
                catch (Exception){              
                    continue;
                }
                finally
                {
                    //otestovat yda se dela pokazde
                    device.Dispose();
                    device = null; 
                }
            }
            Debug.Print("Scanning finished.");
        }
    }
}
