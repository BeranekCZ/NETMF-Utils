using System;
using Microsoft.SPOT;

namespace BeranekCZ.NETMF.Sensors.GpsData
{
    public class GpsMainData
    {
        public TimeSpan Time;
        public GpsCoordinate Latitude;
        public GpsCoordinate Longtitude;
        public float Altitude;
        public string NavigationStatus;
        public float HorizontalAccuracyEstimate;
        public float VerticallAccuracyEstimate;
        public float SpeedOverGround;
        public float CourseOverGround;
        public float VerticalVelocity;  //positive=downwards

        public float HDOP;  //horizontal dillution of precision
        public float VDOP;  //vert. dillution of precision
        public float TDOP;  //time dillution of precision

        public byte NumberOfGpsSatellites;
        public byte NumberOfGlonassSatellites;
        public bool DrUsed;
        public int AgeOfRecentDgpsCorrections;


        /// <summary>
        /// Input = sentence from $ to * 
        /// PUBX,00,222557.00,5007.90318,N,01546.81763,E,270.108,G3,4.3,5.0,0.219,29.99,0.028,,0.89,1.07,0.68,11,0,0
        /// </summary>
        public static GpsMainData TryParseFromUbx00(string sentence)
        {
            string[] splitData = sentence.Split(',');
            if (splitData.Length != 21 || !splitData[0].Equals("PUBX") || !splitData[1].Equals("00")) return null;

            GpsMainData data = new GpsMainData();
            
            //parse time
            int time = (int)double.Parse(splitData[2]);
            data.Time = new TimeSpan(time / 10000, (time % 10000) / 100, time % 100);
            
            double outputVal;
            bool ret;

            data.Latitude = new GpsCoordinate(splitData[3], splitData[4][0]);
            data.Longtitude = new GpsCoordinate(splitData[5], splitData[6][0]);
            if (!data.Longtitude.DataOK || !data.Latitude.DataOK) return null;

            ret = double.TryParse(splitData[7], out outputVal);
            if (!ret) return null;
            data.Altitude = (float)outputVal;

            data.NavigationStatus = splitData[8];

            ret = double.TryParse(splitData[9], out outputVal);
            if (!ret) return null;
            data.HorizontalAccuracyEstimate = (float)outputVal;

            ret = double.TryParse(splitData[10], out outputVal);
            if (!ret) return null;
            data.VerticallAccuracyEstimate = (float)outputVal;

            ret = double.TryParse(splitData[11], out outputVal);
            if (!ret) return null;
            data.SpeedOverGround = (float)outputVal;

            ret = double.TryParse(splitData[12], out outputVal);
            if (!ret) return null;
            data.CourseOverGround = (float)outputVal;

            ret = double.TryParse(splitData[13], out outputVal);
            if (!ret) return null;
            data.VerticalVelocity = (float)outputVal;

            if (!splitData[14].Equals("")) data.AgeOfRecentDgpsCorrections = int.Parse(splitData[14]);
            else data.AgeOfRecentDgpsCorrections = -1;

            ret = double.TryParse(splitData[15], out outputVal);
            if (!ret) return null;
            data.HDOP = (float)outputVal;

            ret = double.TryParse(splitData[16], out outputVal);
            if (!ret) return null;
            data.VDOP = (float)outputVal;

            ret = double.TryParse(splitData[17], out outputVal);
            if (!ret) return null;
            data.TDOP = (float)outputVal;

            data.NumberOfGpsSatellites = byte.Parse(splitData[18]);
            data.NumberOfGlonassSatellites = byte.Parse(splitData[19]);

            if (splitData[20].Equals("0")) data.DrUsed = false;
            else data.DrUsed = true;

            return data;
        }
    }
}
