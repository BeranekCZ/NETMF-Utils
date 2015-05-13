using System;
using Microsoft.SPOT;

namespace BeranekCZ.NETMF.Sensors.GpsData
{
    public struct SateliteInfo
    {
        public int SatelitePrnNumber;
        public int Elevation;
        public int Azimut;
        public int Snr;     //Signal to Noise Ratio, higher is better
    }
}
