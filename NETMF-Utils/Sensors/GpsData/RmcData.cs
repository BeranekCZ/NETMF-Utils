using System;
using Microsoft.SPOT;

namespace BeranekCZ.NETMF.Sensors.GpsData
{
    /// <summary>
    /// Recommended Minimum data
    /// </summary>
    public class RmcData
    {
        public DateTime DateAndTime;
        public char Status;
        public GpsCoordinate Latitude;
        public GpsCoordinate Longtitude;
        public float SpeedOverGround;
        public float CourseOverGround;
        /// <summary>
        /// A = fix
        /// </summary>
        public char PositionFix;


        public static RmcData TryParse(string sentence)
        {
            string[] splitData = sentence.Split(',');
            if (splitData[2].Equals("V"))
            {
                return new RmcData() { Status = 'V' };
            }

            RmcData data = new RmcData();
            data.Latitude = GpsCoordinate.TryParseNmeaFormat(splitData[3], splitData[3][0]);
            data.Longtitude = GpsCoordinate.TryParseNmeaFormat(splitData[5], splitData[6][0]);

            data.SpeedOverGround = (float)double.Parse(splitData[7]);
            data.CourseOverGround = (float)double.Parse(splitData[8]);

            int time = (int)double.Parse(splitData[1]);
            int date = int.Parse(splitData[9]);
            data.DateAndTime = new DateTime(2000 + date % 100, date / 100 % 100, date / 10000, time / 10000, (time % 10000) / 100, time % 100);

            data.PositionFix = splitData[12][0];
            return data;
        }
    }
}
