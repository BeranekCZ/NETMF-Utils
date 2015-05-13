using System;
using Microsoft.SPOT;


namespace BeranekCZ.NETMF.Sensors.GpsData
{
    public struct GpsCoordinate
    {
        public int Degrees;
        public float Minutes;
        public char Direction;
        public bool DataOK;

        public GpsCoordinate(int degrees, float minutes, char direction)
        {
            Degrees = degrees;
            Minutes = minutes;
          
            Direction = direction;
            DataOK = true;
        }


        public GpsCoordinate(double value,char direction)
        {
            Degrees = (int)value / 100;
            Minutes = (float)(value % 100);
            Direction = direction;
            DataOK = true;          
        }

        public GpsCoordinate(string nmeaValue, char direction)
        {
            int index = nmeaValue.IndexOf('.');
            Degrees = int.Parse(nmeaValue.Substring(0, index - 2));
            Minutes = (float)double.Parse(nmeaValue.Substring(index - 2, nmeaValue.Length - (index - 2)));
            Direction = direction;
            DataOK = true;
        }

        public override string ToString()
        {
            return Degrees + "°" + Minutes + "'" + Direction;
        }

        public static GpsCoordinate TryParseNmeaFormat(string nmeaValue, char direction)
        {
            int index = nmeaValue.IndexOf('.');
            
            GpsCoordinate coord = new GpsCoordinate();
            if (index == -1)
            {
                coord.DataOK = false;
                return coord;
            }

            try
            {
                coord.Degrees = int.Parse(nmeaValue.Substring(0, index - 2));
                coord.Minutes = (float)double.Parse(nmeaValue.Substring(index - 2, nmeaValue.Length - (index - 2)));
                coord.Direction = direction;
            }catch{
                coord.DataOK = false;
                return coord;
            }

            coord.DataOK = true;
            return coord;
        }

    }
}
