using System;
using Microsoft.SPOT;
using BeranekCZ.NETMF.Displays;
using Microsoft.SPOT.Hardware;
using System.Threading;
using BeranekCZ.NETMF.Expanders;
using BeranekCZ.NETMF.Wireless;
using System.IO.Ports;
using System.Text;
using BeranekCZ.NETMF;
using BeranekCZ.NETMF.Utils;
using BeranekCZ.NETMF.Sensors;


namespace TestProject
{
    public class Program
    {

        static BtModuleHC05 bt;
        static Esp8266Wifi wifi;
        static Bmp180 sensor;
        static GpsNeo6M gps;

        static byte val; 
        public static void Main()
        {
            //I2CScanner.ScanAddresses(50, 80);
            //testSSD1306_128x64();
            //test1602();
            //testBT();
            //testWifi();
            //testBmp180();
            testGPS();

            Thread.Sleep(Timeout.Infinite);      


        }

        private static void testGPS()
        {
            gps = new GpsNeo6M(new SerialPort("COM1",9600,Parity.None,8,StopBits.One),true);
            gps.NewGpsMainData += gps_NewGpsMainDataHandler;
            //gps.GetUbxPosition(new TimeSpan(0,0,0,5));
            //gps.ReguestUbxPosition();
            gps.RequestSentence("RMC");

            
        }

        static void gps_NewGpsMainDataHandler(object sender, BeranekCZ.NETMF.Sensors.GpsData.GpsMainData data)
        {
            Debug.Print(data.Latitude.ToString());
            Debug.Print(data.Longtitude.ToString());
        }

        private static void testBmp180()
        {
            //I2CScanner.ScanAddresses(1, 140);
            I2CDevice.Configuration conf = new I2CDevice.Configuration(119, 400);
            sensor = new Bmp180(new I2CDevice(conf));
            Debug.Print(sensor.ReadCalibrationData().ToString());
            
            Debug.Print(sensor.GetRawTemperature().ToString());
            Debug.Print(sensor.GetTemperature()*0.1+"°C" );
            long pressure = sensor.GetPressure(Bmp180.PresureAccurancyMode.Standard);
            Debug.Print(pressure / 100 + "hPa");

            Debug.Print(sensor.GetAltitude(pressure) + "m");       
        }

        private static void testWifi()
        {
            //wifi = new Esp8266Wifi(new SerialPort("COM1", 9600, Parity.None, 8, StopBits.One));
            wifi = new Esp8266Wifi(new SerialPort("COM1", 115200, Parity.None, 8, StopBits.One));

            wifi.Open(true);
            wifi.NewDataReceived += wifi_NewDataReceived;
            wifi.ConnectDisconnectClient += wifi_ConnectDisconnectClient;
            wifi.ServerDataReceived += wifi_ServerDataReceived;

            //ByteUtils.PrintDataToConsole(wifi.PingModule());
            //ByteUtils.PrintDataToConsole(wifi.GetVersion());

            //ByteUtils.PrintDataToConsole(wifi.GetWorkingMode());
            ByteUtils.PrintDataToConsole(wifi.SetWorkingMode(Esp8266Wifi.WORKING_MODE.Sta));
            ByteUtils.PrintDataToConsole(wifi.SetMUX(Esp8266Wifi.MUX_MODE.multipleConnection));
            //wifi.Close();
            //wifi.Open(true);
            //ByteUtils.PrintDataToConsole(wifi.GetSSIDList());

            //ByteUtils.PrintDataToConsole(wifi.GetConnectedNetworkName());
            ByteUtils.PrintDataToConsole(wifi.ConnectToNetwork("", ""));
            ByteUtils.PrintDataToConsole(wifi.GetConnectedNetworkName());

            //ByteUtils.PrintDataToConsole(wifi.DisconnectFromNetwork());
            //ByteUtils.PrintDataToConsole(wifi.GetConnectedNetworkName());

            //ByteUtils.PrintDataToConsole(wifi.GetSoftAPConf());
            //ByteUtils.PrintDataToConsole(wifi.SetSoftAPConf("TempWifi","temptemp",5,Esp8266Wifi.ENCRYPTION.wpa2_psk));
            //ByteUtils.PrintDataToConsole(wifi.GetSoftAPConf());
            //ByteUtils.PrintDataToConsole(wifi.SetDHCP(Esp8266Wifi.WORKING_MODE.Both, true));
            //Thread.Sleep(60000);//time to connect,1 minute
            //ByteUtils.PrintDataToConsole(wifi.GetClientsIP());

            //ByteUtils.PrintDataToConsole(wifi.GetStationMacAddress());
            //ByteUtils.PrintDataToConsole(wifi.GetSoftAPMacAddress());

            //ByteUtils.PrintDataToConsole(wifi.GetStationIPAddress());
            //ByteUtils.PrintDataToConsole(wifi.GetSoftAPIPAddress());

            //ByteUtils.PrintDataToConsole(wifi.GetConnectionInfo());
            ByteUtils.PrintDataToConsole(wifi.GetLocalIPAddress());

            ByteUtils.PrintDataToConsole(wifi.SetTransferMode(Esp8266Wifi.TRANSFER_MODE.normal));
            
            //ByteUtils.PrintDataToConsole(wifi.EstablishConnection("TCP","192.168.0.102","3000"));
            //ByteUtils.PrintDataToConsole(wifi.SendData(Encoding.UTF8.GetBytes("AHOJ\r\n")));

            //ByteUtils.PrintDataToConsole(wifi.SendBigData(Encoding.UTF8.GetBytes("1234567890")));
            ByteUtils.PrintDataToConsole(wifi.GetConnectionInfo());

            ByteUtils.PrintDataToConsole(wifi.StartServer(333));

            //ByteUtils.PrintDataToConsole(wifi.GetConnectionInfo());

            

        }

        static void wifi_ServerDataReceived(object sender, byte[] data, int clientId, int size)
        {
            Debug.Print("Client ID = "+clientId);
            Debug.Print("size = " + size);
            Debug.Print("real size = " + data.Length);
            ByteUtils.PrintDataToConsole(data);
            Esp8266Wifi wifi = sender as Esp8266Wifi;
            if (wifi != null) wifi.SendData(Encoding.UTF8.GetBytes("Thanks for your data"), clientId);
        }

        static void wifi_ConnectDisconnectClient(object sender, byte[] data)
        {
            ByteUtils.PrintDataToConsole(data);
        }

        static void wifi_NewDataReceived(object sender, byte[] data)
        {
            ByteUtils.PrintDataToConsole(data);
        }

        private static void testBT()
        {

            //bt = new BtModuleLC07(new SerialPort("COM1",38400,Parity.None,8,StopBits.One));
           
            bt = new BtModuleHC05(new SerialPort("COM1", 9600, Parity.None, 8, StopBits.One),true,4);
            bt.Open(true);
            bt.NewDataReceived += bt_NewDataReceived;

            
        }

        static void bt_NewDataReceived(object sender, byte[] data)
        {
            ByteUtils.PrintDataToConsole(data);
        }



        private static void test1602()
        {
            //I2CScanner.ScanAddresses(0, 120);
            //address 39
            I2CDevice.Configuration conf = new I2CDevice.Configuration(39, 100);

            LcdCharacter_I2C lcd = new LcdCharacter_I2C(new I2CDevice(conf),4,20);
            lcd.Init();

            lcd.WriteText("TEST ZAPISU delsi".ToCharArray());
            lcd.SetCursorDirection(false);
            lcd.SetCursorPosition(1, 3);
            Thread.Sleep(5000);
            //lcd.MoveText(true);
            Thread.Sleep(5000);
            lcd.SetupDisplay(true, true, false);

            lcd.SetCursorPosition(0, 19);
            lcd.WriteChar('E');
            lcd.SetCursorPosition(1, 19);
            lcd.WriteChar('E');
            lcd.SetCursorPosition(2, 19); 
            lcd.WriteChar('E');
            lcd.SetCursorPosition(3, 19);
            lcd.WriteChar('E');


            //lcd.ResetDisplay();
            //lcd.WriteChar('A');


        }

        private static void testSSD1306_128x64()
        {
            I2CDevice.Configuration conf = new I2CDevice.Configuration(60, 400);
            SSD1306 oled = new SSD1306(new I2CDevice(conf), 128, 64);
            oled.Init();

            //two circles
            oled.DrawCircle(31, 31, 30);
            oled.DrawFilledCircle(97, 31, 30);

            oled.Display();
            Thread.Sleep(5000);
            oled.Clear();

            //archery target
            oled.DrawFilledCircle(63, 31, 30);
            oled.DrawCircle(63, 31, 25, false);
            oled.DrawCircle(63, 31, 20, false);
            oled.DrawCircle(63, 31, 15, false);
            oled.DrawCircle(63, 31, 10, false);
            oled.DrawCircle(63, 31, 5, false);

            oled.Display();
            Thread.Sleep(5000);
            oled.Clear();

            //invert 
            oled.SetInverseDisplay(true);
            Thread.Sleep(5000);
            oled.SetInverseDisplay(false);

            //rectangles 
            oled.DrawRectangle(5, 20, 40, 40);
            oled.DrawFilledRectangle(50, 20, 50, 40);

            oled.Display();
            Thread.Sleep(5000);
            oled.Clear();

            //rounded rect
            oled.DrawRoundRect(5, 20, 40, 40, 10);
            oled.DrawRoundFilledRect(50, 20, 50, 40, 10);

            oled.Display();
            Thread.Sleep(5000);
            oled.Clear();

            oled.StartScrollHorizontally(true, 0, 0xff);
            Thread.Sleep(5000);

            oled.StartScrollHorizontally(false, 0, 0xff);
            Thread.Sleep(5000);
            oled.StartScrollVerticallyHorizontally(true, 0, 0xff, 0x02);
            Thread.Sleep(5000);
            oled.StartScrollVerticallyHorizontally(true, 0, 0xff, 0x0A);
            Thread.Sleep(5000);
            oled.DeactivateScroll();

            oled.Clear();


            //triangle + lines
            oled.DrawLine(10, 10, oled.Width - 10, oled.Height - 10);
            oled.DrawTriangle(5, 20, 5, 60, 63, 60);

            oled.Display();
            Thread.Sleep(5000);
            oled.Clear();


            //text
            oled.DrawText(0, 0, "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray(), 1);

            oled.Display();
            Thread.Sleep(5000);
            oled.Clear();

            //text
            oled.DrawText(0, 0, "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray(), 2);

            oled.Display();
            Thread.Sleep(5000);
            oled.Clear();
            return;
        }
    }
}
