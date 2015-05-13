using System;
using Microsoft.SPOT;

namespace BeranekCZ.NETMF.Sensors.GpsData
{
    public class GsvData
    {
        public int TotalNumberOfMessages;
        public int NumberOfMessages;
        public int NumberOfSatellitesInView;

        public SateliteInfo[] SatellitesInfo;    //up to 4 satelites

        public GsvData()
        {
        }

        /// <summary>
        /// Sentence example $GPGSV,2,1,08,01,40,083,46,02,17,308,41,12,07,344,39,14,22,228,45
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public static GsvData TryParse(string sentence)
        {
            //if (sentence[sentence.Length - 1] != '\n') return false;
            string[] splitData = sentence.Split(',');
            GsvData data = new GsvData();
            data.TotalNumberOfMessages = int.Parse(splitData[1]);
            data.NumberOfMessages = int.Parse(splitData[2]);
            data.TotalNumberOfMessages = int.Parse(splitData[3]);

            int numberOfInfo = (splitData.Length - 4) / 4; 
            SateliteInfo info;
            data.SatellitesInfo = new SateliteInfo[numberOfInfo];

            for (int i = 0; i < numberOfInfo; i++)
            {
                if (!splitData[4 + (i * 4)].Equals(""))
                    info.SatelitePrnNumber = int.Parse(splitData[4 + (i * 4)]);
                else info.SatelitePrnNumber = -1;

                if (!splitData[5 + (i * 4)].Equals(""))
                    info.Elevation = int.Parse(splitData[5 + (i * 4)]);
                else info.Elevation = -1;

                if (!splitData[6 + (i * 4)].Equals(""))
                    info.Azimut = int.Parse(splitData[6 + (i * 4)]);
                else info.Azimut = -1;
                
                if (!splitData[7 + (i * 4)].Equals(""))
                    info.Snr = int.Parse(splitData[7 + (i * 4)]);
                else info.Snr = -1;

                data.SatellitesInfo[i] = info;
            }

            return data;
        }

    }
}
