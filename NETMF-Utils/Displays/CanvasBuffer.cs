using System;
using Microsoft.SPOT;

namespace BeranekCZ.NETMF.Displays
{
    public class CanvasBuffer: ICanvas, IDisposable
    {
        private int _width;
        private int _height;
        private ushort[] buffer;

        public int Width
        {
            get { return _width; }
        }

        public int Height
        {
            get { return _height; }
        }

        public CanvasBuffer(int width, int height, ushort initColor)
        {
            _width = width;
            _height = height;
            buffer = new ushort[Width*Height];
            for (int i=0;i<Width*Height; i++)
            {
                buffer[i] = initColor;
            }
        }

        public ushort[] GetBuffer()
        {
            return buffer;
        }

        public ushort[] GetBufferPart(int startIndex, int endIndex)
        {
            ushort[] buff = new ushort[endIndex-startIndex];
            for (int i = startIndex; i < endIndex; i++)
            {
                buff[i - startIndex] = buffer[i];
            }
            return buff;
        }

        public void DrawPixel(int x, int y, bool colored)
        {
            throw new NotImplementedException();
        }

        public void DrawPixel(int x, int y, ushort color)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return;
      
            int index = y * Width + x;
            buffer[index] = color;
        }

        public void Dispose()
        {
            buffer = null;
            
        }

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
            for (int x = x0; x < x1; x++)
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

        public void DrawLine(int x0, int y0, int x1, int y1, bool colored)
        {
            throw new NotImplementedException();
        }
    }
}
