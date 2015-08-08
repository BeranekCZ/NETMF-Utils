using System;
using Microsoft.SPOT.Hardware;
using System.Threading;
using Microsoft.SPOT;

namespace BeranekCZ.NETMF.Displays
{
    //SSD1306 driver
    public class SSD1306 :ICanvas
    {

        private readonly int _width;
        private readonly int _height;
        private readonly int _pages;
        private byte[] buffer;
        private int _timeout = 200;
        private bool _externalVcc = false;

        private I2CDevice _device;


        public int Width
        {
            get
            {
                return _width;
            }
        }

        public int Height
        {
            get
            {
                return _height;
            }
        }

        public SSD1306(I2CDevice device,int width = 128,int height = 64)
        {
            
            
            this._device = device;
            this._width = width;
            this._height = height;
            this._pages = _height / 8;
            buffer = new byte[width * _pages];
           
        }

        public void Clear()
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = 0;
            }
        }

        private void send(byte data)
        {
            this._device.Execute(
                new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(new byte[] { data }) },
                _timeout
                );
        }

        private void send(byte[] data)
        {
            this._device.Execute(
                new I2CDevice.I2CTransaction[] { I2CDevice.CreateWriteTransaction(data) },
                _timeout
                );
        }

        private void sendCommand(byte cmd)
        {
            // Co = 0, D/C = 0
            send(new byte[] { 0x00, cmd });
        }

        private void sendData(byte data)
        {
            // Co = 0, D/C = 1
            send(new byte[] { 0x40, data });
        }

        public void Init()
        {
            // Init sequence for 128x64 OLED module
            sendCommand(0xae);                      // 0xAE display off
            sendCommand(0xD5);                      // 0xD5 set display clock div.
            sendCommand(0x80);                      // the suggested ratio 0x80
            sendCommand(0xA8);                      // 0xA8 set multiplex
            sendCommand(0x3F);
            sendCommand(0xD3);                      // 0xD3 set display offset
            sendCommand(0x0);                       // no offset
            sendCommand(0x40 | 0x0);                // line #0 set display start line
            sendCommand(0x8D);                      // 0x8D charge pump
            if (_externalVcc)
            { sendCommand(0x10); }                  //disable charge pump
            else
            { sendCommand(0x14); }                  //enable charge pump
            sendCommand(0x20);                      // 0x20 set memory address mode
            sendCommand(0x00);                      // 0x0 horizontal addressing mode
            sendCommand(0xA0 | 0x1);                // set segment re-map
            sendCommand(0xc8);                      // set com output scan direction
            sendCommand(0xDA);                      // 0xDA set COM pins HW configuration
            sendCommand(0x12);
            sendCommand(0x81);                      // 0x81 Set Contrast Control for BANK0
            if (_externalVcc)
            { sendCommand(0x9F); }
            else
            { sendCommand(0xCF); }
            sendCommand(0xd9);                      // 0xd9 Set Pre-charge Period
            if (_externalVcc)
            { sendCommand(0x22); }
            else
            { sendCommand(0xF1); }
            sendCommand(0xDB);                      // 0xDB Set VCOMH Deselect Level
            sendCommand(0x40);                      // set display start line
            sendCommand(0xA4);                      // 0xA4 display ON
            sendCommand(0xA6);                      // 0xA6 set normal display


            sendCommand(0xAF);                      //--turn on oled panel

            //Thread.Sleep(100);
        }

        /// <summary>
        /// Send buffer to display
        /// </summary>
        public void Display()
        {
            //set column address
            sendCommand(0x21);  
            sendCommand(0);
            sendCommand((byte)(_width - 1));

            //set page address
            sendCommand(0x22);
            sendCommand(0);
            sendCommand((byte)(_pages - 1));

            for (ushort i = 0; i < buffer.Length; i = (ushort)(i + 16))
            {
                sendCommand(0x40);
                SendArray(buffer, i, (ushort)(i + 16));
            }
        }

        /// <summary>
        /// Coordinates start with index 0
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color">0 = turn off pixel, else turn on</param>
        public void DrawPixel(int x, int y, ushort color)
        {
            DrawPixel((byte)x, (byte)y, color);
        }

        /// <summary>
        /// Coordinates start with index 0
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color">0 = turn off pixel, else turn on</param>
        public void DrawPixel(byte x, byte y, ushort color)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
            {
                Debug.Print("DrawPixel error: Out of borders");
                return;
            }
            int index = y / 8 * _width + x;
            if (color != 0) buffer[index] = (byte)(buffer[index] | (byte)(1 << (y % 8)));
            else buffer[index] = (byte)(buffer[index] & ~(byte)(1 << (y % 8)));
        }

        //public void DrawLine(int x0, int y0, int x1, int y1, bool colored = true)
        //{
        //    DrawLine(x0, y0, x1, y1, colored);
        //}

        public void DrawHLine(int x0, int y0, int w, ushort color)
        {
            DrawLine(x0, y0, x0 + w - 1, y0, color);
        }

        public void DrawVLine(int x0, int y0, int h, ushort color)
        {
            DrawLine(x0, y0, x0, y0 + h - 1, color);
        }

        /// <summary>
        /// Source http://ericw.ca/notes/bresenhams-line-algorithm-in-csharp.html
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="colored"></param>
        public void DrawLine(int x0, int y0, int x1, int y1, ushort color)
        {
            bool steep = System.Math.Abs(y1 - y0) > System.Math.Abs(x1 - x0);
            if (steep)
            {
                int t;
                t = x0; // swap x0 and y0
                x0 = y0;
                y0 = t;
                t = x1; // swap x1 and y1
                x1 = y1;
                y1 = t;
            }
            if (x0 > x1)
            {
                int t;
                t = x0; // swap x0 and x1
                x0 = x1;
                x1 = t;
                t = y0; // swap y0 and y1
                y0 = y1;
                y1 = t;
            }
            int dx = x1 - x0;
            int dy = System.Math.Abs(y1 - y0);
            int error = dx / 2;
            int ystep = (y0 < y1) ? 1 : -1;
            int y = y0;
            for (int x = x0; x <= x1; x++)
            {
                DrawPixel((steep ? y : x), (steep ? x : y), color);
                error = error - dy;
                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }

        }

#region moved to Painter class
        //public void DrawCircle(int centerX, int centerY, int radius, bool colored = true)
        //{
        //    radius--;
        //    int d = (5 - radius * 4) / 4;
        //    int x = 0;
        //    int y = radius;
        //    do
        //    {

        //        DrawPixel(centerX + x, centerY + y, colored);
        //        DrawPixel(centerX + x, centerY - y, colored);
        //        DrawPixel(centerX - x, centerY + y, colored);
        //        DrawPixel(centerX - x, centerY - y, colored);
        //        DrawPixel(centerX + y, centerY + x, colored);
        //        DrawPixel(centerX + y, centerY - x, colored);
        //        DrawPixel(centerX - y, centerY + x, colored);
        //        DrawPixel(centerX - y, centerY - x, colored);
        //        if (d < 0)
        //        {
        //            d += 2 * x + 1;
        //        }
        //        else
        //        {
        //            d += 2 * (x - y) + 1;
        //            y--;
        //        }
        //        x++;

        //    } while (x <= y);
        //}

        //public void DrawFilledCircle(int centerX, int centerY, int radius, bool colored = true)
        //{
        //    radius--;
        //    int d = (5 - radius * 4) / 4;
        //    int x = 0;
        //    int y = radius;
        //    do
        //    {
        //        DrawLine(centerX + x, centerY + y, centerX - x, centerY + y, colored);
        //        DrawLine(centerX + x, centerY - y, centerX - x, centerY - y, colored);

        //        DrawLine(centerX - y, centerY + x, centerX + y,centerY + x, colored);
        //        DrawLine(centerX - y, centerY - x, centerX + y, centerY - x, colored);
        //        if (d < 0)
        //        {
        //            d += 2 * x + 1;
        //        }
        //        else
        //        {
        //            d += 2 * (x - y) + 1;
        //            y--;
        //        }
        //        x++;
        //    } while (x <= y);
        //}

        //public void DrawRectangle(int xLeft, int yTop, int width, int height, bool colored = true)
        //{
        //    width--;
        //    height--;
        //    DrawLine(xLeft, yTop, xLeft + width, yTop, colored);
        //    DrawLine(xLeft + width, yTop, xLeft + width, yTop + height, colored);
        //    DrawLine(xLeft + width, yTop + height, xLeft, yTop + height, colored);
        //    DrawLine(xLeft, yTop, xLeft, yTop + height, colored);

        //}

        //public void DrawFilledRectangle(int xLeft, int yTop, int width, int height, bool colored = true)
        //{
        //    width--;
        //    height--;
        //    for (int i = 0; i <= height; i++)
        //    {
        //        DrawLine(xLeft, yTop + i, xLeft + width, yTop + i, colored);
        //    }
        //}

        //Draw a rounded rectangle
        //public void DrawRoundRect(int x, int y, int w, int h, int r, bool colored = true)
        //{
        //    // smarter version
        //    DrawHLine(x + r, y, w - 2 * r, colored); // Top
        //    DrawHLine(x + r, y + h - 1, w - 2 * r, colored); // Bottom
        //    DrawVLine(x, y + r, h - 2 * r, colored); // Left
        //    DrawVLine(x + w - 1, y + r, h - 2 * r, colored); // Right
        //    // draw four corners
        //    drawCircleHelper(x + r, y + r, r, 1, colored);
        //    drawCircleHelper(x + w - r - 1, y + r, r, 2, colored);
        //    drawCircleHelper(x + w - r - 1, y + h - r - 1, r, 4, colored);
        //    drawCircleHelper(x + r, y + h - r - 1, r, 8, colored);
        //}

        //public void DrawRoundFilledRect(int x, int y, int w, int h, int r, bool colored = true)
        //{

        //    // smarter version
        //    //fillRect(x+r, y, w-2*r, h, color);
        //    for (int i = x + r; i < x + r + (w - 2 * r); i++)
        //    {
        //        DrawVLine(i, y, h, colored);
        //    }

        //    // draw four corners
        //    fillCircleHelper(x + w - r - 1, y + r, r, 1, h - 2 * r - 1, colored);
        //    fillCircleHelper(x + r, y + r, r, 2, h - 2 * r - 1, colored);

        //}

        //private void drawCircleHelper(int x0, int y0, int r, int cornername, bool colored = true)
        //{
        //    int f = 1 - r;
        //    int ddF_x = 1;
        //    int ddF_y = -2 * r;
        //    int x = 0;
        //    int y = r;

        //    while (x < y)
        //    {
        //        if (f >= 0)
        //        {
        //            y--;
        //            ddF_y += 2;
        //            f += ddF_y;
        //        }
        //        x++;
        //        ddF_x += 2;
        //        f += ddF_x;
        //        if ((cornername & 0x4) != 0)
        //        {
        //            DrawPixel(x0 + x, y0 + y, colored);
        //            DrawPixel(x0 + y, y0 + x, colored);
        //        }
        //        if ((cornername & 0x2) != 0)
        //        {
        //            DrawPixel(x0 + x, y0 - y, colored);
        //            DrawPixel(x0 + y, y0 - x, colored);
        //        }
        //        if ((cornername & 0x8) != 0)
        //        {
        //            DrawPixel(x0 - y, y0 + x, colored);
        //            DrawPixel(x0 - x, y0 + y, colored);
        //        }
        //        if ((cornername & 0x1) != 0)
        //        {
        //            DrawPixel(x0 - y, y0 - x, colored);
        //            DrawPixel(x0 - x, y0 - y, colored);
        //        }
        //    }
        //}

        //private void fillCircleHelper(int x0, int y0, int r, int cornername, int delta, bool colored)
        //{

        //    int f = 1 - r;
        //    int ddF_x = 1;
        //    int ddF_y = -2 * r;
        //    int x = 0;
        //    int y = r;

        //    while (x < y)
        //    {
        //        if (f >= 0)
        //        {
        //            y--;
        //            ddF_y += 2;
        //            f += ddF_y;
        //        }
        //        x++;
        //        ddF_x += 2;
        //        f += ddF_x;

        //        if ((cornername & 0x1) != 0)
        //        {
        //            DrawVLine(x0 + x, y0 - y, 2 * y + 1 + delta, colored);
        //            DrawVLine(x0 + y, y0 - x, 2 * x + 1 + delta, colored);
        //        }
        //        if ((cornername & 0x2) != 0)
        //        {
        //            DrawVLine(x0 - x, y0 - y, 2 * y + 1 + delta, colored);
        //            DrawVLine(x0 - y, y0 - x, 2 * x + 1 + delta, colored);
        //        }
        //    }
        //}


        ////// Draw a triangle
        //public void DrawTriangle(int x0, int y0, int x1, int y1, int x2, int y2, bool colored = true)
        //{
        //    DrawLine(x0, y0, x1, y1, colored);
        //    DrawLine(x1, y1, x2, y2, colored);
        //    DrawLine(x2, y2, x0, y0, colored);
        //}

        //public void DrawChar(int x, int y, char c, int size = 1, bool colored = true)
        //{

        //    if ((x >= _width) || // Clip right
        //       (y >= _height) || // Clip bottom
        //       ((x + 6 * size - 1) < 0) || // Clip left
        //       ((y + 8 * size - 1) < 0))   // Clip top
        //        return;

        //    for (byte i = 0; i < 6; i++)
        //    {
        //        byte line;
        //        if (i == 5)
        //            line = 0x0;
        //        else
        //            //line = pgm_read_byte(font+(c*5)+i);
        //            line = Glcfont.MEM[(c * 5) + i];

        //        for (byte j = 0; j < 8; j++)
        //        {
        //            if ((line & 0x1) != 0)
        //            {
        //                if (size == 1) // default size
        //                    DrawPixel(x + i, y + j, colored);
        //                else
        //                {  // big size
        //                    DrawFilledRectangle(x + (i * size), y + (j * size), size, size, colored);
        //                }
        //            }
        //            else
        //            {
        //                if (size == 1) // default size
        //                    DrawPixel(x + i, y + j, !colored);
        //                else
        //                {  // big size
        //                    DrawFilledRectangle(x + i * size, y + j * size, size, size, !colored);
        //                }
        //            }
        //            line >>= 1;
        //        }
        //    }
        //}



        //public void DrawText(int x,int y,char[] text, int size = 1, bool colored = true,bool wrap=true)
        //{
        //    int cursorX = x;
        //    int cursorY = y;
            

        //    foreach (char c in text)
        //    {
        //        if (c == '\n')
        //        {
        //            cursorY += size * 8;
        //            cursorX = 0;
        //        }
        //        else if (c == '\r')
        //        {
        //            // skip em
        //        }
        //        else
        //        {
        //            DrawChar(cursorX, cursorY, c, size, colored);
        //            cursorX += size * 6;
        //            if (wrap && (cursorX > (_width - size * 6)))
        //            {
        //                cursorY += size * 8;
        //                cursorX = 0;
        //            }
        //        }
        //    }

        //}
#endregion

        private bool isInScreen(int x, int y)
        {
            if (x < 0 || x >= _width) return false;
            if (y < 0 || y >= _height) return false;
            return true;
        }

        private void SendArray(byte[] array, ushort startIndex, ushort endIndex)
        {
            for (int i = startIndex; i < endIndex; i++)
            {
                sendData(array[i]);
            }
        }

        public void SetContrast(byte value = 0xFF)
        {
            //sendCommand(new byte[]{0x81,value});
            sendCommand(0x81);
            sendCommand(value);
        }

        public void SetInverseDisplay(bool inverse)
        {
            if (inverse) sendCommand(0xA7);
            else sendCommand(0xA6);
        }

        public void SetEntireDisplayON(bool setOn)
        {
            if (setOn) sendCommand(0xA5);
            else sendCommand(0xA4);
        }

        public void SetMemoryAddressingMode()
        {
            //TODO another modes
            sendCommand(0x20);
            sendCommand(0x00);
        }

        private void setColumnAddress(byte start = 0, byte end = 127)
        {
            //sendCommand(new byte[] { 0x21, start, end });
            sendCommand(0x21);
            sendCommand(start);
            sendCommand(end);
        }

        private void setPageAddress(byte start = 0, byte end = 7)
        {
            sendCommand(0x22);
            sendCommand(start);
            sendCommand(end);
        }

        /// <summary>
        /// Start horizontall scrolling
        /// </summary>
        /// <param name="left">true = scrolling to left, false = scrollong to right</param>
        /// <param name="start">Start page index</param>
        /// <param name="stop">Stop page index</param>
        public void StartScrollHorizontally(bool left, byte start, byte stop)
        {
            DeactivateScroll();

            if (left) sendCommand(0x27);
            else sendCommand(0x26);

            sendCommand(0x00);
            sendCommand(start); //start page index
            sendCommand(0x00);  //scroll interval in frames
            sendCommand(stop);  //end page index
            sendCommand(0x00);
            sendCommand(0xFF);

            sendCommand(0x2F);  //start scroll
        }

        /// <summary>
        /// Start vert. and hor. scrolling == diagonal
        /// </summary>
        /// <param name="left">true = scrolling to left, false = scrollong to right</param>
        /// <param name="start">Start page index</param>
        /// <param name="stop">Stop page index</param>
        /// <param name="verticalOffset"></param>
        public void StartScrollVerticallyHorizontally(bool left, byte start, byte stop,byte verticalOffset)
        {
            DeactivateScroll();

            if (left) sendCommand(0x2A);
            else sendCommand(0x29);

            sendCommand(0x00);
            sendCommand(start); //start page index
            sendCommand(0x00);  //scroll interval in frames
            sendCommand(stop);  //end page index
            sendCommand(verticalOffset);  //vertical scrolling offset

            sendCommand(0x2F);  //start scroll
        }

        /// <summary>
        /// Turn off scrolling
        /// </summary>
        public void DeactivateScroll()
        {
            sendCommand(0x2E);
        }

        public void SetVerticalScrollArea(byte topRow, byte numberOfRows)
        {
            DeactivateScroll();
            sendCommand(0xA3);
            sendCommand(topRow);
            sendCommand(numberOfRows);
            sendCommand(0x2F);
        }


        
    }
}
