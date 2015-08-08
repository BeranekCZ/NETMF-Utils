using System;
using Microsoft.SPOT;

namespace BeranekCZ.NETMF.Displays
{
    public interface ICanvas
    {
         int Width { get; }
         int Height { get; }
         void DrawPixel(int x, int y, ushort color);
         void DrawLine(int x0, int y0, int x1, int y1, ushort color);
 
    }
}
