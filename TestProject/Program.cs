using System;
using Microsoft.SPOT;
using BeranekCZ.NETMF.Utils;
using BeranekCZ.NETMF.Displays;
using Microsoft.SPOT.Hardware;
using System.Threading;
using BeranekCZ.NETMF.Expanders;


namespace TestProject
{
    public class Program
    {

   
        static byte val; 
        public static void Main()
        {
            //I2CScanner.ScanAddresses(50, 80);


            InterruptPort btn = new InterruptPort(GHI.Pins.Generic.GetPin('B', 8), true, Port.ResistorMode.PullUp,Port.InterruptMode.InterruptEdgeLow);
            btn.OnInterrupt += btn_OnInterrupt;
            //testSSD1306_128x64();
            test1602();
            


        }

        static void btn_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            Debug.Print("posun");
            val = (byte)(val << 1);
            reg.SetPins(val);
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
