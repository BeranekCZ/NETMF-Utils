using System;
using Microsoft.SPOT;
using System.IO.Ports;
using BeranekCZ.NETMF.Sensors.GpsData;
using Microsoft.SPOT.Hardware;
using System.Text;

namespace BeranekCZ.NETMF.Sensors
{

    public delegate void NewGpsMainDataHandler(object sender, GpsMainData data);
    public delegate void NewGpsGsvDataHandler(object sender, GsvData data);
    public delegate void NewGpsGsaDataHandler(object sender, GsaData data);
    public delegate void NewGpsGgaDataHandler(object sender, GgaData data);
    public delegate void NewGpsRmcDataHandler(object sender, RmcData data);


    /// <summary>
    /// Protocol NMEA
    /// </summary>
    public class GpsNeo6M:IDisposable
    {
        private SerialPort _module;
        private byte[] _receivedData;

        public event NewGpsMainDataHandler NewGpsMainData;
        public event NewGpsGsvDataHandler NewGsvData;
        public event NewGpsGsaDataHandler NewGsaData;
        public event NewGpsGgaDataHandler NewGgaData;
        public event NewGpsRmcDataHandler NewRmcData;


        private bool _eventMode;

        //byte quality;

        public GpsNeo6M(SerialPort module,bool eventMode = false)
        {
            this._module = module;
            this._eventMode = eventMode;
            if(eventMode) this._module.DataReceived += _module_DataReceived;
            _module.Open();
            SetAutomaticSending(false);

        }

        /// <summary>
        /// if true, module automatically send information sentences
        /// </summary>
        /// <param name="turnOn"></param>
        public void SetAutomaticSending(bool turnOn)
        {
            int set = 0;
            if (turnOn) set = 1;
            if (!turnOn && _module.IsOpen)
            {
                _module.DiscardInBuffer();
            }

            writeCommand(makeSentence("PUBX,40,GLL,0," + set + ",0,0"));
            writeCommand(makeSentence("PUBX,40,GGA,0," + set + ",0,0"));
            writeCommand(makeSentence("PUBX,40,GSA,0," + set + ",0,0"));
            writeCommand(makeSentence("PUBX,40,RMC,0," + set + ",0,0"));
            writeCommand(makeSentence("PUBX,40,GSV,0," + set + ",0,0"));
            writeCommand(makeSentence("PUBX,40,VTG,0," + set + ",0,0"));
        }

        /// <summary>
        /// Send request to the GPS module. Module will return sentence.
        /// Non blocking. Response will be returned in event.
        /// </summary>
        /// <param name="sentenceId">Example: GLL, GGA, RSA, RMC, GSV, VTG </param>
        public void RequestSentence(string sentenceId)
        {
            writeCommand(makeSentence("EIGPQ," + sentenceId));
        }

        /// <summary>
        /// Send request to the GPS module. Module will return sentence.
        /// Blocking.
        /// </summary>
        /// <param name="sentenceId">Example: GLL, GGA, RSA, RMC, GSV, VTG </param>
        //public void GetSentence(string sentenceId)
        //{
        //    writeCommand(makeSentence("EIGPQ," + sentenceId));
        //}



        /// <summary>
        /// Add $ and CRC to the sentence
        /// </summary>
        /// <param name="text">Example: PUBX,40,GLL,0,0,0,0</param>
        /// <returns>Example: $PUBX,40,GLL,0,0,0,0*5C</returns>
        private byte[] makeSentence(string text)
        {
            int crc = getCrc(System.Text.UTF8Encoding.UTF8.GetBytes(text), 0, text.Length);
            crc.ToString("X");

            string sent = "$" + text + "*" + crc.ToString("X")+"\r\n";
            return System.Text.UTF8Encoding.UTF8.GetBytes(sent);
        }

        /// <summary>
        /// Send request to the GPS module. Module will return PUBX,00 sentence.  
        /// Non blocking request.  Response will be returned in event.
        /// PUBX,00,222557.00,5007.90318,N,01546.81763,E,270.108,G3,4.3,5.0,0.219,29.99,0.028,,0.89,1.07,0.68,11,0,0
        /// </summary>
        public void ReguestUbxPosition()
        {
            writeCommand(System.Text.UTF8Encoding.UTF8.GetBytes("$PUBX,00*33\r\n"));
        }


        /// <summary>
        /// Send request to the GPS module. Module will return PUBX,00 sentence.  
        /// Blocking request.
        /// PUBX,00,222557.00,5007.90318,N,01546.81763,E,270.108,G3,4.3,5.0,0.219,29.99,0.028,,0.89,1.07,0.68,11,0,0
        /// </summary>
        /// <returns></returns>
        public GpsMainData GetUbxPosition(TimeSpan timeout)
        {
            _module.DiscardInBuffer();
            writeCommand(System.Text.UTF8Encoding.UTF8.GetBytes("$PUBX,00*33\r\n"));
            
            TimeSpan diff;
            int index = 0;
            byte[] buffer = null;
            DateTime start = DateTime.Now;
            while ((diff = DateTime.Now - start) < timeout && (index = ByteUtils.GetIndexOf(buffer, (byte)'\n')) == -1)
            {
                int count = _module.BytesToRead;
                byte[] dataOut = new byte[count];
                _module.Read(dataOut, 0, count);

                if (buffer == null) buffer = dataOut;
                else buffer = ByteUtils.AppendArrays(buffer, dataOut);
            }

            if (diff > timeout || index == 0) return null;

            byte[] ret = ByteUtils.CopyArrays(buffer, 0, index);
            //ByteUtils.PrintDataToConsole(ret);
            checkSentece(ret);
            return GpsMainData.TryParseFromUbx00(new string(System.Text.UTF8Encoding.UTF8.GetChars(ret, 1, ret.Length - 6)));

        }

        private bool checkSentece(byte[] sentence)
        {
            int loadedCrc = Convert.ToInt32(new string(System.Text.UTF8Encoding.UTF8.GetChars(sentence, sentence.Length - 4, 2)), 16);
            int crc = getCrc(sentence, 1, sentence.Length - 6);

            return loadedCrc == crc && sentence[0] == '$' && sentence[sentence.Length - 5] == '*';
        }

        

        private void writeCommand(byte[] data)
        {
            if (!_module.IsOpen)
            {
                Debug.Print("Serial port is close!");
                return;
            }
            _module.Write(data, 0, data.Length);
        }

        void _module_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int count = _module.BytesToRead;
            byte[] dataOut = new byte[count];
            _module.Read(dataOut, 0, count);
            

            if (_receivedData == null) _receivedData = dataOut;
            else _receivedData = ByteUtils.AppendArrays(_receivedData, dataOut);

            int index = -1;
            while ((index = ByteUtils.GetIndexOf(_receivedData, (byte)'\n')) != -1)
            {
                //if (index + 1 < _receivedData.Length && _receivedData[index + 1] == (byte)'\r') index++;
                byte[] ret = ByteUtils.CopyArrays(_receivedData, 0, index);
                if (ret == null) continue;
                //ByteUtils.PrintDataToConsole(ret);
                if (ret[0] != '$' || ret[ret.Length-5] != '*')
                {
                    _receivedData = ByteUtils.CopyArrays(_receivedData, index + 1, _receivedData.Length - 1);
                    continue;
                }

                ByteUtils.PrintDataToConsole(ret);

                //int astIndex = ByteUtils.GetIndexOf(ret,(byte)'*');
                int loadedCrc = Convert.ToInt32(new string(System.Text.UTF8Encoding.UTF8.GetChars(ret, ret.Length - 4, 2)), 16);
                int crc = getCrc(ret, 1, ret.Length - 6);

                if (loadedCrc != crc)
                {
                    _receivedData = ByteUtils.CopyArrays(_receivedData, index + 1, _receivedData.Length - 1);
                    continue;
                }

                string sentence = new string(System.Text.UTF8Encoding.UTF8.GetChars(ret,1,ret.Length - 6));

                if (sentence[2] == 'R' && sentence[3] == 'M' && sentence[4] == 'C')
                    NewRmcData(this,RmcData.TryParse(sentence));
                else if (sentence[2] == 'G' && sentence[3] == 'G' && sentence[4] == 'A')
                    NewGgaData(this, GgaData.TryParse(sentence));
                else if (sentence[2] == 'G' && sentence[3] == 'S' && sentence[4] == 'A')
                    NewGsaData(this, GsaData.TryParse(sentence));
                else if (sentence[2] == 'G' && sentence[3] == 'S' && sentence[4] == 'V')
                    NewGsvData(this, GsvData.TryParse(sentence));
                else if (sentence[0] == 'P' && sentence[1] == 'U' && sentence[2] == 'B')
                    NewGpsMainData(this, GpsMainData.TryParseFromUbx00(sentence));
                else ByteUtils.PrintDataToConsole(ret);

                _receivedData = ByteUtils.CopyArrays(_receivedData, index + 1, _receivedData.Length - 1);
            }
            
            
        }


        private int getCrc(byte[] data,int offset, int length)
        {
            int crc = data[offset];
            for (int i = offset+1; i < length+offset; i++)
            {
                //if (data[i] == (byte)',') continue;
                crc ^= data[i]; 
            }
            return crc;
        }

        public void Dispose()
        {
            this._module.DataReceived -= _module_DataReceived;
            if (_module.IsOpen) _module.Close();
        }
    }
}
