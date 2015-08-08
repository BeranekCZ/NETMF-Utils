using System;
using Microsoft.SPOT;
using BeranekCZ.NETMF.Expanders;
using Microsoft.SPOT.Hardware;
using System.Threading;

namespace BeranekCZ.NETMF.Displays
{
    /// <summary>
    /// Character LCD display with HD44780 driver. Controled by I2C I/O expander.
    /// </summary>
    public class LcdCharacter_I2C
    {
        private PCF8574 _expander;
        private static byte RS_PIN_MASK = 0x01;
        private static byte RW_PIN_MASK = 0x02;
        private static byte E_PIN_MASK = 0x04;
        private static byte BACKLIGHT_PIN_MASK = 0x08;
        private static byte D4_PIN_MASK = 0x10;
        private static byte D5_PIN_MASK = 0x20;
        private static byte D6_PIN_MASK = 0x40;
        private static byte D7_PIN_MASK = 0x80;

        private int _width;
        private int _height;
        private bool _backLight = true;

        public int Width
        {
            get { return _width; }
        }

        public int Height
        {
            get { return _height; }
        }

        public LcdCharacter_I2C(I2CDevice device,I2CDevice.Configuration configuration,int numberOfLines, int numberOfCharInLine)
        {
            _expander = new PCF8574(device,configuration);
            _width = numberOfCharInLine;
            _height = numberOfLines;
        }

        private byte[] makePacket(bool RS, bool RW, byte data)
        {
            byte packet1 = 0;
            if (RS) packet1 |= RS_PIN_MASK;
            if (RW) packet1 |= RW_PIN_MASK;
            if (_backLight) packet1 |= BACKLIGHT_PIN_MASK;

            
            if ((data & 128) != 0) packet1 |= D7_PIN_MASK;
            if ((data & 64) != 0) packet1 |= D6_PIN_MASK;
            if ((data & 32) != 0) packet1 |= D5_PIN_MASK;
            if ((data & 16) != 0) packet1 |= D4_PIN_MASK;

            byte packet2 = 0;
            if (RS) packet2 |= RS_PIN_MASK;
            if (RW) packet2 |= RW_PIN_MASK;
            if (_backLight) packet2 |= BACKLIGHT_PIN_MASK;

            
            if ((data & 8) != 0) packet2 |= D7_PIN_MASK;
            if ((data & 4) != 0) packet2 |= D6_PIN_MASK;
            if ((data & 2) != 0) packet2 |= D5_PIN_MASK;
            if ((data & 1) != 0) packet2 |= D4_PIN_MASK;
            
            return new byte[] { packet1, packet2 };
        }

        private void sendData(byte data)
        {
            this._expander.SetPins(data);
            Thread.Sleep(1);
            this._expander.SetPins((byte)(data | E_PIN_MASK));
            Thread.Sleep(1);
            this._expander.SetPins(data);
            Thread.Sleep(1);
        }

        private void sendData(byte[] data)
        {
            sendData(data[0]);
            sendData(data[1]);
        }

        public void Init()
        {
            Thread.Sleep(50);
            sendData(0x00);
            Thread.Sleep(1);

            byte[] commType = makePacket(false, false, 0x32);
            sendData(commType[0]);
            Thread.Sleep(5);
            sendData(commType[0]);
            Thread.Sleep(5);
            sendData(commType);
            Thread.Sleep(1);

            ResetDisplay();
            SetCursorDirection(true);
            SetupDisplay(true, true, true);
        }

        /// <summary>
        /// Clear display and set cursor to start position
        /// </summary>
        public void ResetDisplay()
        {
            sendData(makePacket(false, false, 0x01));
            Thread.Sleep(2);
        }

        public void SetCursorToStart()
        {
            sendData(makePacket(false, false, 0x02));
            Thread.Sleep(2);
        }

        public void SetCursorDirection(bool right)
        {
            if(right) sendData(makePacket(false, false, 0x06));
            else sendData(makePacket(false, false, 0x04));
           
        }

        public void SetupDisplay(bool turnOnDisplay, bool turnOnCursor, bool blinkingCursor)
        {
            byte data=0x08;
            if(turnOnDisplay) data |= 0x04;  
            if(turnOnCursor) data |=  0x02;
            if(blinkingCursor) data |=  0x01;

            sendData(makePacket(false, false, data));
          
        }

        public void MoveCursor(bool right)
        {
            if (right) sendData(makePacket(false, false, 0x14));
            else sendData(makePacket(false, false, 0x10));
        }

        public void MoveText(bool right)
        {
            if (right) sendData(makePacket(false, false, 0x1C));
            else sendData(makePacket(false, false, 0x18));
        }

        public void WriteChar(char c)
        {
            sendData(makePacket(true, false, (byte)c));
        }

        public void WriteText(char[] text)
        {
            foreach (char c in text)
            {
                WriteChar(c);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="row">Indexed from 0</param>
        /// <param name="column">Indexed from 0</param>
        public void SetCursorPosition(int row, int column)
        {
            if (row >= _height || column >= _width) return;

            //0 40 14 54
            int rowAddress = 0;
            switch (row)
            {
                case 1:
                    rowAddress = 0x40;
                    break;
                case 2:
                    rowAddress = 0x14;
                    break;
                case 3:
                    rowAddress = 0x54;
                    break;
            }
            sendData(makePacket(false, false, (byte)((rowAddress + column) | 128)));
        }
    }
}
