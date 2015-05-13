using System;
using Microsoft.SPOT;

namespace BeranekCZ.NETMF.Sensors.GpsData
{
    public class GsaData
    {
        public char SelectionMode;  // A - auto., M - manual 
        public byte Mode;   // 1 - no fix, 2 - 2D fix, 3 - 3D fix
        public byte[] SatelitesIds;
        public float Pdop;
        public float Hdop;  //Horizontal dilution of precision
        public float Vdop;  //Vertical dilution of precision


        public static GsaData TryParse(string sentence)
        {
            string[] splitData = sentence.Split(',');
            GsaData data = new GsaData();

            byte[] satelites = new byte[12];
            int lastIndex = 0;

            for (int i = 3; i <= 14; i++)
            {
                if (splitData[i].Equals("")) continue;

                satelites[lastIndex] = byte.Parse(splitData[i]);
                lastIndex++;
            }

            data.SatelitesIds = satelites;
            if (!splitData[1].Equals("")) data.SelectionMode = splitData[1].ToCharArray()[0];
            if (!splitData[2].Equals("")) data.Mode = byte.Parse(splitData[2]);

            double val;
            double.TryParse(splitData[15], out val);
            data.Pdop = (float)val;
            double.TryParse(splitData[16], out val);
            data.Hdop = (float)val;
            double.TryParse(splitData[17], out val);
            data.Vdop = (float)val;

            return data;
        }
    }
}
