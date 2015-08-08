using System;
using Microsoft.SPOT;

namespace BeranekCZ.NETMF.Displays
{
    public class Painter
    {
        public static void DrawCircle(ICanvas canvas, int centerX, int centerY, int radius, ushort color)
        {
            radius--;
            int d = (5 - radius * 4) / 4;
            int x = 0;
            int y = radius;
            do
            {

                canvas.DrawPixel(centerX + x, centerY + y, color);
                canvas.DrawPixel(centerX + x, centerY - y, color);
                canvas.DrawPixel(centerX - x, centerY + y, color);
                canvas.DrawPixel(centerX - x, centerY - y, color);
                canvas.DrawPixel(centerX + y, centerY + x, color);
                canvas.DrawPixel(centerX + y, centerY - x, color);
                canvas.DrawPixel(centerX - y, centerY + x, color);
                canvas.DrawPixel(centerX - y, centerY - x, color);
                if (d < 0)
                {
                    d += 2 * x + 1;
                }
                else
                {
                    d += 2 * (x - y) + 1;
                    y--;
                }
                x++;

            } while (x <= y);
        }

        public static void DrawFilledCircle(ICanvas canvas, int centerX, int centerY, int radius, ushort color)
        {
            radius--;
            int d = (5 - radius * 4) / 4;
            int x = 0;
            int y = radius;
            do
            {
                canvas.DrawLine(centerX + x, centerY + y, centerX - x, centerY + y, color);
                canvas.DrawLine( centerX + x, centerY - y, centerX - x, centerY - y, color);

                canvas.DrawLine( centerX - y, centerY + x, centerX + y, centerY + x, color);
                canvas.DrawLine( centerX - y, centerY - x, centerX + y, centerY - x, color);
                if (d < 0)
                {
                    d += 2 * x + 1;
                }
                else
                {
                    d += 2 * (x - y) + 1;
                    y--;
                }
                x++;
            } while (x <= y);
        }

        public static void DrawRectangle(ICanvas canvas, int xLeft, int yTop, int width, int height, ushort color)
        {
            width--;
            height--;
            canvas.DrawLine(xLeft, yTop, xLeft + width, yTop, color);
            canvas.DrawLine(xLeft + width, yTop, xLeft + width, yTop + height, color);
            canvas.DrawLine(xLeft + width, yTop + height, xLeft, yTop + height, color);
            canvas.DrawLine(xLeft, yTop, xLeft, yTop + height, color);

        }

        public static void DrawFilledRectangle(ICanvas canvas, int xLeft, int yTop, int width, int height, ushort color)
        {
            width--;
            height--;
            for (int i = 0; i <= height; i++)
            {
                canvas.DrawLine(xLeft, yTop + i, xLeft + width, yTop + i, color);
            }
        }

        public static void DrawChar(ICanvas canvas,int x, int y, char c, ushort color, int size = 1)
        {

            if ((x >=  canvas.Width) || // Clip right
               (y >= canvas.Height) || // Clip bottom
               ((x + 6 * size - 1) < 0) || // Clip left
               ((y + 8 * size - 1) < 0))   // Clip top
                return;

            for (byte i = 0; i < 6; i++)
            {
                byte line;
                if (i == 5)
                    line = 0x0;
                else
                    //line = pgm_read_byte(font+(c*5)+i);
                    line = Glcfont.MEM[(c * 5) + i];

                for (byte j = 0; j < 8; j++)
                {
                    if ((line & 0x1) != 0)
                    {
                        if (size == 1) // default size
                            canvas.DrawPixel(x + i, y + j, color);
                        else
                        {  // big size
                            DrawFilledRectangle(canvas,x + (i * size), y + (j * size), size, size, color);
                        }
                    }
                    //else
                    //{
                    //    if (size == 1) // default size
                    //        canvas.DrawPixel(x + i, y + j, !color);
                    //    else
                    //    {  // big size
                    //        DrawFilledRectangle(x + i * size, y + j * size, size, size, !colored);
                    //    }
                    //}
                    line >>= 1;
                }
            }
        }

        public static void DrawText(ICanvas canvas, int x, int y, char[] text, ushort color,int size = 1, bool wrap = true)
        {
            int cursorX = x;
            int cursorY = y;


            foreach (char c in text)
            {
                if (c == '\n')
                {
                    cursorY += size * 8;
                    cursorX = 0;
                }
                else if (c == '\r')
                {
                    // skip em
                }
                else
                {
                    DrawChar(canvas, cursorX, cursorY, c, color, size);
                    cursorX += size * 6;
                    if (wrap && (cursorX > (canvas.Width - size * 6)))
                    {
                        cursorY += size * 8;
                        cursorX = 0;
                    }
                }
            }

        }

        private static void drawCircleHelper(ICanvas canvas, int x0, int y0, int r, int cornername, ushort color)
        {
            int f = 1 - r;
            int ddF_x = 1;
            int ddF_y = -2 * r;
            int x = 0;
            int y = r;

            while (x < y)
            {
                if (f >= 0)
                {
                    y--;
                    ddF_y += 2;
                    f += ddF_y;
                }
                x++;
                ddF_x += 2;
                f += ddF_x;
                if ((cornername & 0x4) != 0)
                {
                    canvas.DrawPixel(x0 + x, y0 + y, color);
                    canvas.DrawPixel(x0 + y, y0 + x, color);
                }
                if ((cornername & 0x2) != 0)
                {
                    canvas.DrawPixel(x0 + x, y0 - y, color);
                    canvas.DrawPixel(x0 + y, y0 - x, color);
                }
                if ((cornername & 0x8) != 0)
                {
                    canvas.DrawPixel(x0 - y, y0 + x, color);
                    canvas.DrawPixel(x0 - x, y0 + y, color);
                }
                if ((cornername & 0x1) != 0)
                {
                    canvas.DrawPixel(x0 - y, y0 - x, color);
                    canvas.DrawPixel(x0 - x, y0 - y, color);
                }
            }
        }

        private static void fillCircleHelper(ICanvas canvas, int x0, int y0, int r, int cornername, int delta, ushort color)
        {

            int f = 1 - r;
            int ddF_x = 1;
            int ddF_y = -2 * r;
            int x = 0;
            int y = r;

            while (x < y)
            {
                if (f >= 0)
                {
                    y--;
                    ddF_y += 2;
                    f += ddF_y;
                }
                x++;
                ddF_x += 2;
                f += ddF_x;

                if ((cornername & 0x1) != 0)
                {
                    canvas.DrawLine(x0 + x, y0 - y, x0 + x, y0 + y + 1 + delta, color);
                    canvas.DrawLine(x0 + y, y0 - x, x0 + y, y0 + x + 1 + delta, color);
                }
                if ((cornername & 0x2) != 0)
                {
                    canvas.DrawLine(x0 - x, y0 - y, x0 - x, y0 + y + 1 + delta, color);
                    canvas.DrawLine(x0 - y, y0 - x, x0 - y, y0 + x + 1 + delta, color);
                    //DrawVLine(x0 - x, y0 - y, 2 * y + 1 + delta, color);
                    //DrawVLine(x0 - y, y0 - x, 2 * x + 1 + delta, color);
                }
            }
        }



        public static void DrawRoundRect(ICanvas canvas, int x, int y, int w, int h, int r, ushort color)
        {
            // smarter version
            canvas.DrawLine(x + r, y, x  + w - r, y, color); // Top
            canvas.DrawLine(x + r, y + h-1, x + w - r,y+h-1, color); // Bottom
            canvas.DrawLine(x, y + r,x,y + r + h - 2 * r , color); // Left
            canvas.DrawLine(x + w - 1, y + r, x + w - 1, y + h - r, color); // Right

            // draw four corners
            drawCircleHelper(canvas,x + r, y + r, r, 1, color);
            drawCircleHelper(canvas,x + w - r - 1 , y + r, r, 2, color);
            drawCircleHelper(canvas,x + w - r - 1 , y + h - r - 1, r, 4, color);
            drawCircleHelper(canvas,x + r, y + h - r - 1, r, 8, color);
        }

        public static void DrawRoundFilledRect(ICanvas canvas, int x, int y, int w, int h, int r, ushort color)
        {

            // smarter version
            for (int i = x + r; i < x + r + (w - 2 * r); i++)
            {
                canvas.DrawLine(i, y, i,y+h, color);
            }

            // draw four corners
            fillCircleHelper(canvas,x + w - r - 1, y + r, r, 1, h - 2 * r - 1, color);
            fillCircleHelper(canvas,x + r, y + r, r, 2, h - 2 * r - 1, color);

        }

        ////// Draw a triangle
        public static void DrawTriangle(ICanvas canvas, int x0, int y0, int x1, int y1, int x2, int y2, ushort color)
        {
            canvas.DrawLine(x0, y0, x1, y1, color);
            canvas.DrawLine(x1, y1, x2, y2, color);
            canvas.DrawLine(x2, y2, x0, y0, color);
        }


    }
}
