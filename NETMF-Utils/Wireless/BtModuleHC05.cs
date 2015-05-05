using System;
using Microsoft.SPOT;
using System.IO.Ports;
using System.Threading;
using System.Text;

namespace BeranekCZ.NETMF.Wireless
{
    public delegate void NewDataReceivedHandler(object sender, byte[] data);
    //HC05
    /// <summary>
    /// Default baudrate 9600, for AT command mode 38400, stop bit 1, parity NONE
    /// </summary>
    public class BtModuleHC05: IDisposable
    {
        private SerialPort _module;
        private byte[] _receivedData;
        private int _packetSize;
        private bool _packetMode;
        private int _dataLoaded;

        public event NewDataReceivedHandler NewDataReceived;
        
        /// <summary>
        /// packetMode and packetSize working only in receiveEvent mode (method Open).
        /// </summary>
        /// <param name="module"></param>
        /// <param name="packetMode">Data are grouped in packet. Event is fired when packet is full</param>
        /// <param name="packetSize">Size of packet in bytes</param>
        public BtModuleHC05(SerialPort module, bool packetMode=false, int packetSize=0)
        {
            this._module = module;
            this._packetMode = packetMode;
            this._packetSize = packetSize;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="receiveEvent">If true -> received data are put in NewDataReceived event. If false -> you have to read data manualy</param>
        /// <param name="ATmode">You have to boot BT module to AT mode and set AT mode baudrate in constructor</param>
        public void Open(bool receiveEvent,int readTimeout=200, Handshake handshake = Handshake.None, bool ATmode=false)
        {

            if (!ATmode && receiveEvent) { 
                _module.ErrorReceived += _module_ErrorReceived;
                _module.DataReceived += _module_DataReceived;
            }
            _module.ReadTimeout = readTimeout;
            _module.Handshake = handshake;
            _module.Open();
        }

        public void Close()
        {
            _module.ErrorReceived -= _module_ErrorReceived;
            _module.DataReceived -= _module_DataReceived;
            if (_module.IsOpen) _module.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="waitingTime">waiting time in ms. Its used in Thread.Sleep after write data.</param>
        /// <returns></returns>
        public byte[] WriteAndRead(byte[] data,int waitingTime)
        {
            //_module.ReadTimeout = readTimeout;
            Write(data);
            Thread.Sleep(waitingTime);
            int count = _module.BytesToRead;
            return Read(count);
        }

        public void Write(byte[] data)
        {
            _module.Flush();
            _module.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Read data. Waiting time is set in constructor (readTimeout).
        /// </summary>
        /// <param name="size">Number of bytes</param>
        /// <returns></returns>
        public byte[] Read(int size)
        {
            byte[] tmp = new byte[size];
            _module.Read(tmp, 0, tmp.Length);
            return tmp;
        }

        public byte[] ATCommand(string command)
        {
            if (!_module.IsOpen || !_module.CanWrite) return null;
            _module.Flush();
            byte[] data = Encoding.UTF8.GetBytes(command + "\r\n");
            _module.Write(data,0,data.Length);

            Thread.Sleep(100);

            int count = _module.BytesToRead;
            byte[] dataOut = new byte[count];

            _module.Read(dataOut, 0, count);
            return dataOut;

        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>"OK" if connection is OK</returns>
        public byte[] CheckConnectionAT()
        {
            return ATCommand("AT");
        }

        /// <summary>
        /// Param2:stop bit:
        /// 0----1 bit
        /// 1----2 bits
        /// Param3: parity bit 
        /// 0----None
        /// 1----Odd parity
        /// 2----Even parity
        //Default: 9600, 0, 0 
        /// </summary>
        /// <returns>baud rate, stop bit, parity </returns>
        public byte[] GetBaudrateAT()
        {
            return ATCommand("AT+ UART?");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baudrate"></param>
        /// <param name="stopBit">0----1 bit;
        /// 1----2 bits</param>
        /// <param name="parity">0----None;
        /// 1----Odd; parity
        /// 2----Even parity</param>
        /// <returns>succes -> "OK"</returns>
        public byte[] SetBaudrateAT(String baudrate, String stopBit, String parity)
        {
            return ATCommand("AT+UART=" + baudrate + "," + stopBit + "," + parity);
        }

        public byte[] GetPasswordAT()
        {
            return ATCommand("AT+ PSWD?");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pswd">New password</param>
        /// <returns>return "OK" if password was changed</returns>
        public byte[] SetPasswordAT(String pswd)
        {
            return ATCommand("AT+PSWD="+pswd);
        }

        public byte[] GetNameAT()
        {
            return ATCommand("AT+NAME?");
        }

        public byte[] SetNameAT(String name)
        {
            return ATCommand("AT+NAME=" + name);
        }

        //public byte[] ResetAndExitATMode()
        //{
        //    return ATCommand("AT+RESET");
        //}

        public byte[] ResetToFactorySettingsAT()
        {
            return ATCommand("AT+ORGL");
        }

        public byte[] GetAddressAT()
        {
            return ATCommand("AT+ADDR");
        }

        void _module_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            Debug.Print(e.ToString());
        }


        /// <summary>
        /// Make packets of defined size and fire events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _module_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_packetMode && _packetSize > 0)
            {
                int count = _module.BytesToRead;
                byte[] dataOut = new byte[count];
                _module.Read(dataOut, 0, count);
                if (_receivedData == null) _receivedData = dataOut;
                else _receivedData = ByteUtils.AppendArrays(_receivedData, dataOut);

                if (_receivedData.Length >= _packetSize)
                {
                    int numberOfPackets = _receivedData.Length / _packetSize;
                    for (int i = 0; i < numberOfPackets; i++)
                    {
                        byte[] ret = ByteUtils.CopyArrays(_receivedData, i * _packetSize, i * _packetSize + _packetSize - 1);
                        NewDataReceived(this, ret);
                    }

                    _receivedData = ByteUtils.CopyArrays(_receivedData, numberOfPackets * _packetSize, _receivedData.Length - 1);
                }
                //NewDataReceived(this, dataOut);
            }
            else
            {
                int count = _module.BytesToRead;
                byte[] dataOut = new byte[count];
                _module.Read(dataOut, 0, count);
                NewDataReceived(this, dataOut);

            }

        }

        public void ClearPacket()
        {
            _receivedData = null;
        }

        public void Dispose()
        {
            _module.ErrorReceived -= _module_ErrorReceived;
            _module.DataReceived -= _module_DataReceived;
            if (_module.IsOpen) _module.Close();
           
        }
    }
}
