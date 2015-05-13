using System;
using Microsoft.SPOT;

namespace BeranekCZ.NETMF.Sensors.GpsData
{
    /// <summary>
    /// Global positioning system fix data
    /// </summary>
    public class GgaData
    {
        public TimeSpan Time;
        public GpsCoordinate Latitude;
        public GpsCoordinate Longtitude;
        public byte PositionFixIndicator;
        public byte SatellitesUsed;
        public float HDOP;  //horizontal dillution of precision
        
        public float Altitude;
        public char AltitudeUnits;

        public float GeoidSeparation;
        public char GeoidSeparationUnits;

        /// <summary>
        /// -1 if DGPS is not used
        /// </summary>
        public int AgeOfDifferentialCorrections;
        public int DiffReferenceStationId;

        public static GgaData TryParse(string sentence)
        {
            string[] splitData = sentence.Split(',');
            GgaData data = new GgaData();
            try
            {
                data.Latitude = GpsCoordinate.TryParseNmeaFormat(splitData[2], splitData[3][0]);
                data.Longtitude = GpsCoordinate.TryParseNmeaFormat(splitData[4], splitData[5][0]);

                int time = (int)double.Parse(splitData[1]);
                data.Time = new TimeSpan(time / 10000, (time % 10000) / 100, time % 100);

                data.PositionFixIndicator = byte.Parse(splitData[6]);
                data.SatellitesUsed = byte.Parse(splitData[7]);

                data.HDOP = (float)double.Parse(splitData[8]);
                data.Altitude = (float)double.Parse(splitData[9]);
                data.GeoidSeparation = (float)double.Parse(splitData[11]);
                if (!splitData[10].Equals("")) data.AltitudeUnits = splitData[10][0];
                if (!splitData[12].Equals("")) data.GeoidSeparation = splitData[12][0];

                if (splitData[13].Equals("")) data.AgeOfDifferentialCorrections = -1;
                data.AgeOfDifferentialCorrections = int.Parse(splitData[13]);
            
                data.DiffReferenceStationId = int.Parse(splitData[14]);
            }
            catch
            {
                return null;
            }
            return data;            
            
        }
    }
}
