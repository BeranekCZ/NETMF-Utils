using System;
using Microsoft.SPOT;
using System.IO.Ports;
using BeranekCZ.NETMF;
using System.Text;
using System.Threading;

namespace BeranekCZ.NETMF.Wireless
{


    public delegate void NewNetworkDataReceivedHandler(object sender, byte[] data, int clientId, int size);

    /// <summary>
    /// For espressif firmware version  00200.9.4
    /// https://github.com/espressif/esp8266_at/wiki/AT_Description
    /// </summary>
    public class Esp8266Wifi : IDisposable
    {
        private SerialPort _module;
        private int _packetSize;
        private bool _packetMode;
        private byte[] _receivedData;
        private bool _eventMode;
        private int _wholePacketReadTimeout = 5; //seconds

        /// <summary>
        /// New serial interface data (AT commands response...) .
        /// </summary>
        public event NewDataReceivedHandler NewDataReceived;

        public event NewDataReceivedHandler ConnectDisconnectClient;
        /// <summary>
        /// New data received from clients in server mode, New network data.
        /// </summary>
        public event NewNetworkDataReceivedHandler ServerDataReceived;


        public enum WORKING_MODE { Sta = 1, AP, Both }
        public enum ENCRYPTION { open, wpa_psk = 2, wpa2_psk, wpa_wpa2_psk }
        public enum MUX_MODE { singleConnection, multipleConnection }
        public enum TRANSFER_MODE { normal, unvarnished }

        public Esp8266Wifi(SerialPort module, bool packetMode = false, int packetSize = 0)
        {
            this._module = module;
            this._packetMode = packetMode;
            this._packetSize = packetSize;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventMode">If true -> received data are put in NewDataReceived,ServerDataReceived and ConnectDisconnectClient event. If false -> you have to read data manualy</param>
        /// <param name="ATmode">You have to boot BT module to AT mode and set AT mode baudrate in constructor</param>
        public void Open(bool eventMode, int readTimeout = 5000, Handshake handshake = Handshake.None, bool ATmode = false)
        {

            if (!ATmode && eventMode)
            {
                //_module.ErrorReceived += _module_ErrorReceived;
                _module.DataReceived += _module_DataReceived;
            }
            _eventMode = eventMode;
            _module.ReadTimeout = readTimeout;
            _module.Handshake = handshake;
            _module.Open();
            _module.DiscardInBuffer();
            _module.DiscardOutBuffer();
        }

        public void EventMode(bool eventMode)
        {
            if (eventMode) _module.DataReceived += _module_DataReceived;
            else _module.DataReceived -= _module_DataReceived;
        }

        public void Close()
        {
            //_module.ErrorReceived -= _module_ErrorReceived;
            _module.DataReceived -= _module_DataReceived;
            if (_module.IsOpen) _module.Close();
        }

        public byte[] WriteAndRead(byte[] data, int waitingTime)
        {
            //_module.ReadTimeout = readTimeout;
            _module.Write(data, 0, data.Length);
            Thread.Sleep(waitingTime);
            int count = _module.BytesToRead;
            return Read(count);
        }

        public byte[] Read(int size)
        {
            byte[] tmp = new byte[size];
            _module.Read(tmp, 0, tmp.Length);
            return tmp;
        }

        public byte[] ATCommand(string command, int waitingTime = 1000)
        {
            //Thread.Sleep(1000);
            while (!_module.CanWrite) { }
            if (!_module.IsOpen || !_module.CanWrite) return null;
            _module.Flush();
            byte[] data = Encoding.UTF8.GetBytes(command + "\r\n");
            _module.Write(data, 0, data.Length);
            Thread.Sleep(waitingTime);
            if (_eventMode) return null;

            int count = _module.BytesToRead;
            byte[] dataOut = new byte[count];

            _module.Read(dataOut, 0, count);
            return dataOut;

        }

        public byte[] GetVersion()
        {
            return ATCommand("AT+GMR");
        }

        public byte[] RestartModule()
        {
            return ATCommand("AT + RST");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>"OK" if everything is ok</returns>
        public byte[] PingModule()
        {
            return ATCommand("AT");
        }

        public byte[] GetWorkingMode()
        {
            return ATCommand("AT+CWMODE?");
        }

        public byte[] SetWorkingMode(WORKING_MODE mode)
        {
            return ATCommand("AT+CWMODE=" + mode.ToString());
        }

        public byte[] SetDHCP(WORKING_MODE mode, bool enabled)
        {
            int en = 1;
            if (enabled) en = 0;
            return ATCommand("AT+CWMODE=" + mode + "," + en);
        }

        /// <summary>
        /// First number is security
        /// 0 OPEN, 1 WEP, 2 WPA_PSK, 3 WPA2_PSK, 4 WPA_WPA2_PSK
        /// </summary>
        /// <returns></returns>
        public byte[] GetSSIDList()
        {
            return ATCommand("AT+CWLAP", 5000);
        }

        public byte[] ConnectToNetwork(string SSID, string password)
        {
            return ATCommand("AT+CWJAP=\"" + SSID + "\",\"" + password + "\"", 10000);
        }

        /// <summary>
        /// Return name of wifi network which is using.
        /// </summary>
        /// <returns></returns>
        public byte[] GetConnectedNetworkName()
        {
            return ATCommand("AT+CWJAP?");
        }

        public byte[] DisconnectFromNetwork()
        {
            return ATCommand("AT+CWQAP");
        }

        public byte[] GetSoftAPConf()
        {
            return ATCommand("AT+CWSAP?");
        }

        public byte[] SetSoftAPConf(String SSID, String password, int channel, ENCRYPTION encryption)
        {
            return ATCommand("AT+CWSAP=\"" + SSID + "\",\"" + password + "\"," + channel + "," + encryption);
        }

        /// <summary>
        /// Return all connected IP
        /// </summary>
        /// <returns></returns>
        public byte[] GetClientsIP()
        {
            return ATCommand("AT+CWLIF");
        }

        public byte[] GetStationMacAddress()
        {
            return ATCommand("AT+CIPSTAMAC?");
        }

        public byte[] SetStationMacAddress(String newAddress)
        {
            return ATCommand("AT+CIPSTAMAC=\"" + newAddress + "\"");
        }

        public byte[] GetSoftAPMacAddress()
        {
            return ATCommand("AT+CIPAPMAC?");
        }

        public byte[] GetSoftAPMacAddress(String newAddress)
        {
            return ATCommand("AT+CIPAPMAC=\"" + newAddress + "\"");
        }

        public byte[] GetStationIPAddress()
        {
            return ATCommand("AT+CIPSTA?");
        }

        public byte[] SetStationMIPAddress(String newAddress)
        {
            return ATCommand("AT+CIPSTA=\"" + newAddress + "\"");
        }

        public byte[] GetSoftAPIPAddress()
        {
            return ATCommand("AT+CIPAP?");
        }

        public byte[] GetSoftAPIPAddress(String newAddress)
        {
            return ATCommand("AT+CIPAP=\"" + newAddress + "\"");
        }

        //TCPIP commands

        /// <summary>
        /// Get infotmation about connection
        /// Response:STATUS:stat +CIPSTATUS:id,type,addr,port,tetype OK
        /// stat:
        /// 2: Got IP
        /// 3: Connected
        /// 4: Disconnected
        /// id:id of the connection (0-4), for multi-connect
        /// type: string, TCP or UDP
        /// addr: string, IP address.
        /// port: port number
        /// tetype:
        /// 0: ESP8266 runs as client
        /// 1: ESP8266 runs as server
        /// </summary>
        /// <returns></returns>
        public byte[] GetConnectionInfo()
        {
            return ATCommand("AT+CIPSTATUS");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">UDP or TCP</param>
        /// <param name="addr"></param>
        /// <param name="port"></param>
        /// <param name="id">Connection id => values 0-4, -1 for single connection mode</param>
        /// <returns></returns>
        public byte[] EstablishConnection(string type, string addr, string port, int id = -1)
        {
            string idString = "";
            if (id != -1) idString = id.ToString() + ",";

            //return ATCommand("AT+CIPSTART=" + idString + "\"" + type + "\",\"" + addr + "\",\"" + port + "\"",5000);
            return ATCommand("AT+CIPSTART=" + idString + "\"" + type + "\",\"" + addr + "\"," + port, 5000);


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Id 5 close all connection in multi connection mode. -1  (or nothing) for close connection in single connection mode </param>
        /// <returns></returns>
        public byte[] CloseConnection(int id = -1)
        {
            string idString = "";
            if (id != -1) idString = "=" + id.ToString();

            return ATCommand("AT+CIPCLOSE" + idString);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="length"> MAX length 2048 bytes </param>
        /// <param name="id">Connection ID = 0~4. -1 (or nothing) for single connection</param>
        /// <returns></returns>
        public byte[] SendData(byte[] data, int id = -1)
        {
            string idString = "";
            if (id != -1) idString = id.ToString() + ",";

            ATCommand("AT+CIPSEND=" + idString + data.Length.ToString());

            //Thread.Sleep(5000);
            return WriteAndRead(data, 300);

        }

        /// <summary>
        /// Send data bigger than 2048B.
        /// Device have to be in Unvarnished Transmission Mode!
        /// </summary>
        /// <param name="length"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public byte[] SendBigData(byte[] data)
        {
            byte[] ret = ATCommand("AT+CIPSEND", 100);
            string str = new string(System.Text.Encoding.UTF8.GetChars(ret));
            if (str == null) return null;
            if (str.IndexOf('>') == -1) return null;

            for (int i = 0; i <= data.Length / 2048; i++)
            {
                Thread.Sleep(20);
                int lastIndex = i * 2048 + 2048;
                if (lastIndex > data.Length) lastIndex = data.Length;
                byte[] packet = ByteUtils.CopyArrays(data, i * 2048, lastIndex - 1);
                _module.Write(packet, 0, packet.Length);
            }
            //_module.Write(Encoding.UTF8.GetBytes("+++"), 0, 3);
            Thread.Sleep(50);
            byte[] r = WriteAndRead(Encoding.UTF8.GetBytes("+++"), 100);
            Thread.Sleep(50);
            return r;


        }

        public byte[] SetTransferMode(TRANSFER_MODE mode)
        {
            return ATCommand("AT+CIPMODE=" + mode);
        }

        /// <summary>
        /// 1.Server can only be created when AT+CIPMUX=1
        /// 2. Server monitor will automatically be created when Server is created.
        /// 3. When a client is connected to the server, it will take up one connection, be gave an id.
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public byte[] StartServer(int port)
        {
            return ATCommand("AT+CIPSERVER=1," + port);
        }

        /// <summary>
        /// Need to follow by restart
        /// </summary>
        /// <returns></returns>
        public byte[] DeleteServer()
        {
            return ATCommand("AT+CIPSERVER=0");
        }


        /// <summary>
        /// IP_address:
        /// IP address of ESP8266 softAP
        /// IP address of ESP8266 station
        /// </summary>
        /// <returns></returns>
        public byte[] GetLocalIPAddress()
        {
            return ATCommand("AT+CIFSR");
        }

        /// <summary>
        /// Enable multiple connections or not.
        /// This mode can only be changed after all connections are disconnected. 
        /// If server is started, reboot is required.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public byte[] SetMUX(MUX_MODE mode)
        {
            return ATCommand("AT+CIPMUX=" + mode);
        }

        public byte[] GetServerTimeout()
        {
            return ATCommand("AT+CIPSTO?");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout">range 0~7200 seconds</param>
        /// <returns></returns>
        public byte[] SetServerTimeout(int timeout)
        {
            return ATCommand("AT+CIPSTO=" + timeout);
        }

        /// <summary>
        /// At your own risk.
        /// 1: found server
        /// 2: connect server
        /// 3: got edition
        /// 4: start update
        /// </summary>
        /// <returns></returns>
        public byte[] UpdateDevicesFirmwareFromCloud()
        {
            return ATCommand("AT+CIUPDATE", 5000);
        }

        public byte[] SetBaudrate(int baudrate)
        {
            return ATCommand("AT+IPR=" + baudrate, 2000);
        }

        /// <summary>
        /// Make packets of defined size and fire events. Read data line by line.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _module_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int count = _module.BytesToRead;

            byte[] dataOut = null;
            if (count >= 4)
            {
                dataOut = new byte[4];
                _module.Read(dataOut, 0, 4);
                //read IPD data
                if (dataOut[0] == (byte)'+' && dataOut[1] == (byte)'I' && dataOut[2] == (byte)'P' && dataOut[3] == (byte)'D')
                {
                    int b = -1;
                    dataOut = null;
                    while ((b = _module.ReadByte()) != 58) //ASCII ":"
                    {
                        dataOut = ByteUtils.AppendToArray(dataOut, (byte)b);
                    }

                    int commaIndex = -1;
                    int id = -1;
                    int size;
                    if ((commaIndex = ByteUtils.GetIndexOf(dataOut, (byte)',', 1)) == -1)
                    {
                        //without ID
                        size = int.Parse(new String(System.Text.Encoding.UTF8.GetChars(dataOut, 1, dataOut.Length - 1))); //1 because have to skip first ","
                    }
                    else
                    {
                        //with ID
                        id = int.Parse(new String(System.Text.Encoding.UTF8.GetChars(dataOut, 1, commaIndex - 1))); //1 because have to skip first ","
                        size = int.Parse(new String(System.Text.Encoding.UTF8.GetChars(dataOut, commaIndex + 1, dataOut.Length - commaIndex - 1)));
                    }

                    dataOut = new byte[size];
                    //int readed = _module.Read(dataOut, 0, size);
                    DateTime start = DateTime.Now;
                    TimeSpan timeout = new TimeSpan(0, 0, _wholePacketReadTimeout);
                    int dateOutIndex = 0;
                    while (DateTime.Now - start < timeout && dateOutIndex < size)
                    {
                        int readedByte = _module.ReadByte();
                        if (readedByte != -1)
                        {
                            dataOut[dateOutIndex] = (byte)readedByte;
                            dateOutIndex++;
                        }

                    }
                    if (ServerDataReceived != null) ServerDataReceived(this, dataOut, id, size);
                    return;
                }
                else
                {
                    //return data for further processing
                    count = count - 4;
                }
            }


            byte[] readedData = new byte[count];
            _module.Read(readedData, 0, count);
            dataOut = ByteUtils.AppendArrays(dataOut, readedData);


            if (_receivedData == null) _receivedData = dataOut;
            else _receivedData = ByteUtils.AppendArrays(_receivedData, dataOut);

            int index = -1;
            while ((index = ByteUtils.GetIndexOf(_receivedData, (byte)'\n')) != -1)
            {
                if (index + 1 < _receivedData.Length && _receivedData[index + 1] == (byte)'\r') index++;
                byte[] ret = ByteUtils.CopyArrays(_receivedData, 0, index);


                if (ret.Length > 7 && ret.Length < 15 && (ByteUtils.Find(ret, ",CONN") || ByteUtils.Find(ret, ",CLOS")))
                {
                    if (ConnectDisconnectClient != null) ConnectDisconnectClient(this, ret);
                }
                else
                {
                    if (NewDataReceived != null) NewDataReceived(this, ret);

                }

                _receivedData = ByteUtils.CopyArrays(_receivedData, index + 1, _receivedData.Length - 1);
            }
        }

        public void Dispose()
        {
            Close();
        }
    }
}
