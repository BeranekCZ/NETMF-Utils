using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Threading;

namespace BeranekCZ.NETMF.Displays
{

    //TODO: backlight
    /// <summary>
    /// Source: https://github.com/veccsolutions/Vecc.Netduino.Drivers.Ili9341/blob/master/src/Vecc.Netduino.Drivers.Ili9341/Driver.cs
    /// </summary>
    public class ILI9341 : ICanvas
    {
        private int _width;
        private int _height;
        private SPI _module;
        private const byte lcdPortraitConfig = 8;
        private const byte lcdLandscapeConfig = 44;
        private bool _isLandscape = true;

        //TOUCH
        private SPI.Configuration _touchSpi;
        private InterruptPort _irq;
        /// <summary>
        /// 0 command, 1 data
        /// </summary>
        private OutputPort _DCpin;
        private OutputPort _ResetPin;

        public int Width
        {
            get
            {
                if (_isLandscape) return _width;
                else return _height;
            }

        }

        public int Height
        {
            get
            {
                if (_isLandscape) return _height;
                else return _width;
            }

        }

        public void SetTouchConfig(SPI.Configuration config, Cpu.Pin irq)
        {
            _touchSpi = config;
            _irq = new InterruptPort(irq, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeLow);
            _irq.OnInterrupt += _irq_OnInterrupt;
        }


        void _irq_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            Debug.Print("TOUCH");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="module"></param>
        /// <param name="DCpin">Data/Command</param>
        /// <param name="width">Size in landscape mode</param>
        /// <param name="height">Size in landscape mode</param>
        public ILI9341(SPI module, Cpu.Pin dcPin, Cpu.Pin resetPin, int width = 320, int height = 240)
        {
            this._module = module;
            this._DCpin = new OutputPort(dcPin, false);
            this._ResetPin = new OutputPort(resetPin, true);

            this._width = width;
            this._height = height;
        }

        public void Init()
        {
            lock (this)
            {
                WriteReset(false);
                Thread.Sleep(10);
                WriteReset(true);
                SendCommand(Commands.SoftwareReset);    //0x01
                Thread.Sleep(10);
                SendCommand(Commands.DisplayOff);       //0x28

                SendCommand(Commands.MemoryAccessControl);  //0x36
                SendData(lcdLandscapeConfig);

                SendCommand(Commands.PixelFormatSet);       //0x3A
                SendData(0x55);//16-bits per pixel

                SendCommand(Commands.FrameControlNormal);   //frame rate
                SendData(new byte[] { 0x00, 0x1B });        //70fps

                SendCommand(Commands.GammaSet);             //0x26
                SendData(0x01);

                SendCommand(Commands.ColumnAddressSet); //0x2A, width of the screen
                SendData(new byte[] { 0x00, 0x00, 0x00, 0xEF });    //0 - 239

                SendCommand(Commands.PageAddressSet); //0x2B, height of the screen
                SendData(new byte[] { 0x00, 0x00, 0x01, 0x3F });    //0 - 319    

                SendCommand(Commands.EntryModeSet);     //0xB7
                SendData(0x07);

                SendCommand(Commands.DisplayFunctionControl);       //0xB6
                SendData(new byte[] { 0x0A, 0x82, 0x27, 0x00 });

                SendCommand(Commands.SleepOut);
                Thread.Sleep(120);

                SendCommand(Commands.DisplayOn);
                Thread.Sleep(100);

                SendCommand(Commands.MemoryWrite);      //0x2C

                SetOrientation(_isLandscape);
            }
        }

        private void SendData(byte[] data)
        {
            _DCpin.Write(true);
            this._module.Write(data);
        }

        private void SendData(ushort[] data)
        {
            _DCpin.Write(true);
            this._module.Write(data);
        }

        private void SendData(ushort data)
        {
            SendData(new ushort[] { data });
        }

        private void SendData(byte data)
        {
            _DCpin.Write(true);
            this._module.Write(new byte[] { data });
        }

        //private void SendData(int data)
        //{
        //    SendData((byte)data);
        //}

        private void SendCommand(Commands commands)
        {
            _DCpin.Write(false);
            WriteCommand((byte)commands);
        }

        private void WriteReset(bool reset)
        {
            _ResetPin.Write(reset);
        }



        public static ushort ColorFromRgb(byte r, byte g, byte b)
        {
            return (ushort)((r << 11) | (g << 5) | b);
        }


        public void FillScreen(int x0, int y0, int x1, int y1, ushort color)
        {
            lock (this)
            {
                SetWindow(x0, y0, x1, y1);
                var buffer = new ushort[Width];

                if (color != 0)
                {
                    for (var i = 0; i < Width; i++)
                    {
                        buffer[i] = color;
                    }
                }

                for (int y = 0; y < Height; y++)
                {
                    SendData(buffer);
                }
            }
        }

        public void DrawBuffer(int x0, int y0, CanvasBuffer buffer)
        {
            lock (this)
            {
                Debug.Print(Debug.GC(true).ToString());
                SetWindow(x0, y0, x0+buffer.Width-1, y0+buffer.Height-1);
                SendData(buffer.GetBuffer());
                buffer.Dispose();
                //Debug.GC(true);
                Debug.Print(Debug.GC(true).ToString());
                //ushort buff;
                //for (int i = 0; i < buffer.Height; i++)
                //{
                //    //ushort[] b = buffer.GetBufferPart(i * buffer.Width, i * buffer.Width + buffer.Width);
                //    SendData(buffer.GetBuffer());

                //}
            }
        }


        //public void SetAddrWindow(int x0, int y0, int x1, int y1)
        //{

        //    WriteCommand(0x2A); // Column addr set
        //    WriteData((byte)(x0 >> 8));
        //    WriteData((byte)(x0 & 0xFF));     // XSTART 
        //    WriteData((byte)(x1 >> 8));
        //    WriteData((byte)(x1 & 0xFF));     // XEND

        //    WriteCommand(0x2B); // Row addr set
        //    WriteData((byte)(y0 >> 8));
        //    WriteData((byte)y0);     // YSTART
        //    WriteData((byte)(y1 >> 8));
        //    WriteData((byte)y1);     // YEND

        //    WriteCommand(0x2C); // write to RAM
        //}

        void SetWindow(int x0, int y0, int x1, int y1)
        {
            lock (this)
            {
                SendCommand(Commands.ColumnAddressSet);
                SendData(new byte[]{(byte)((x0 >> 8) & 0xFF),
                         (byte)(x0 & 0xFF),
                         (byte)((x1 >> 8) & 0xFF),
                         (byte)(x1 & 0xFF)});
                SendCommand(Commands.PageAddressSet);
                SendData(new byte[]{(byte)((y0 >> 8) & 0xFF),
                         (byte)(y0 & 0xFF),
                         (byte)((y1 >> 8) & 0xFF),
                         (byte)(y1 & 0xFF)});
                SendCommand(Commands.MemoryWrite);
            }
        }

        public void WriteCommand(byte command)
        {
            this._DCpin.Write(false);
            this._module.Write(new byte[] { command });
        }

        public void WriteData(byte data)
        {
            this._DCpin.Write(true);
            this._module.Write(new byte[] { data });
        }

        public void WriteData(ushort[] data)
        {
            this._DCpin.Write(true);
            this._module.Write(data);
        }


        public void DrawPixel(int x, int y, ushort color)
        {
            if ((x < 0) || (x >= Width) || (y < 0) || (y >= Height)) return;

            lock (this)
            {
                SetWindow(x, y, x, y);
                SendData(color);
            }
        }


        public void DrawLine(int x0, int y0, int x1, int y1, ushort color)
        {
            //horizontal line
            if (y0 == y1)
            {
                if (x0 > x1) swap(ref x0,ref x1);
                ushort[] buff = new ushort[x1 - x0];
                for (int i = 0; i < buff.Length; i++) buff[i] = color;
                this.SetWindow(x0, y0, x1, y1);
                this.SendData(buff);
                return;
            }

            //vertical line
            if (x0 == x1)
            {
                if (y0 > y1) swap(ref y0, ref y1);
                ushort[] buff = new ushort[y1 - y0];
                for (int i = 0; i < buff.Length; i++) buff[i] = color;
                this.SetWindow(x0, y0, x1, y1);
                this.SendData(buff);
                return;
            }

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

        public void SetOrientation(bool isLandscape)
        {
            lock (this)
            {
                _isLandscape = isLandscape;
                SendCommand(Commands.MemoryAccessControl);

                if (isLandscape)
                {
                    SendData(lcdLandscapeConfig);
                }
                else
                {
                    SendData(lcdPortraitConfig);
                }

                SetWindow(0, Width - 1, 0, Height - 1);
            }
        }


        public void InversionMode(bool inversion)
        {
            if (inversion)
            {
                WriteCommand(0x21);
            }
            else
            {
                WriteCommand(0x20);
            }
        }


        private void swap(ref int x,ref int y)
        {
            int tmp = x;
            x = y;
            y = tmp;
        }
    }

    
    /// <summary>
    /// Added type byte. It saves some space.
    /// Source: https://github.com/veccsolutions/Vecc.Netduino.Drivers.Ili9341/blob/master/src/Vecc.Netduino.Drivers.Ili9341/Driver.cs
    /// </summary>
    public enum Commands : byte
    {
        NoOp = 0x00,
        /// <summary>
        /// <para>
        ///     When the Software Reset command is written, it causes a software reset. It resets the commands and parameters to their
        ///     S/W Reset default values. (See default tables in each command description.)
        /// </para>
        /// <para>
        ///     Note: The Frame Memory contents are unaffected by this command
        /// </para>
        /// <para>
        /// It will be necessary to wait 5msec before sending new command following software reset. The display module loads all display
        /// supplier factory default values to the registers during this 5msec. If Software Reset is applied during Sleep Out mode, it will be
        /// necessary to wait 120msec before sending Sleep out command. Software Reset Command cannot be sent during Sleep Out
        /// sequence
        /// </para>
        /// </summary>
        SoftwareReset = 0x01,
        ReadDisplayInformation = 0x04,
        ReadDisplayStatus = 0x09,
        ReadDisplayPowerMode = 0x0A,
        ReadDisplayMADCTL = 0x0B,
        ReadDisplayPixelFormat = 0x0C,
        ReadDisplayImageFormat = 0x0D,
        ReadDisplaySignalMode = 0x0E,
        ReadDisplaySelfDiagnosticResult = 0x0F,
        /// <summary>
        /// <para>This command causes the LCD module to enter the minimum power consumption mode.</para>
        /// <para>In this mode e.g. the DC/DC converter is stopped, Internal oscillator is stopped, and panel scanning is stopped.</para>
        /// <para>MCU interface and memory are still working and the memory keeps its contents. </para>
        /// </summary>
        /// <remarks>
        /// This command has no effect when module is already in sleep in mode. Sleep In Mode can only be left by the Sleep Out
        /// Command (11h). It will be necessary to wait 5msec before sending next to command, this is to allow time for the supply
        /// voltages and clock circuits to stabilize. It will be necessary to wait 120msec after sending Sleep Out command (when in Sleep
        /// In Mode) before Sleep In command can be sent
        /// </remarks>
        EnterSleepMode = 0x10,
        /// <summary>
        /// This command turns off sleep mode.
        /// In this mode e.g. the DC/DC converter is enabled, Internal oscillator is started, and panel scanning is started.
        /// </summary>
        /// <remarks>
        /// This command has no effect when module is already in sleep out mode. Sleep Out Mode can only be left by the Sleep In
        /// Command (10h). It will be necessary to wait 5msec before sending next command, this is to allow time for the supply voltages
        /// and clock circuits stabilize. The display module loads all display supplier’s factory default values to the registers during this
        /// 5msec and there cannot be any abnormal visual effect on the display image if factory default and register values are same
        /// when this load is done and when the display module is already Sleep Out –mode. The display module is doing self-diagnostic
        /// functions during this 5msec. It will be necessary to wait 120msec after sending Sleep In command (when in Sleep Out mode)
        /// before Sleep Out command can be sent.
        /// </remarks>
        SleepOut = 0x11,
        PartialModeOn = 0x12,
        NormalDisplayModeOn = 0x13,
        DisplayInversionOff = 0x20,
        DisplayInversionOn = 0x21,
        GammaSet = 0x26,
        DisplayOff = 0x28,
        DisplayOn = 0x29,
        ColumnAddressSet = 0x2A,
        PageAddressSet = 0x2B,
        MemoryWrite = 0x2C,
        ColorSet = 0x2D,
        MemoryRead = 0x2E,
        ParialArea = 0x30,
        VerticalScrollingDefinition = 0x33,
        TearingEffectLineOff = 0x34,
        TearingEffectLineOn = 0x35,
        MemoryAccessControl = 0x36,
        VerticalScrollingStartAddress = 0x37,
        IdleModeOff = 0x38,
        IdleModeOn = 0x39,
        PixelFormatSet = 0x3A,
        WriteMemoryContinue = 0x3C,
        ReadMemoryContinue = 0x3E,
        SetTearScanLine = 0x44,
        GetScanLine = 0x45,
        WriteDisplayBrightness = 0x51,
        ReadDisplayBrightness = 0x52,
        WriteCtrlDisplay = 0x53,
        ReadCtrlDisplay = 0x54,
        WriteContentAdaptiveBrightnessControl = 0x55,
        ReadContentAdaptiveBrightnessControl = 0x56,
        WriteCabcMinimumBrightness = 0x5E,
        ReadCabcMinimumBrightness = 0x5F,
        /// <summary>
        /// Manufacturer only
        /// </summary>
        RgbInterfaceSignalControl = 0xB0,
        /// <summary>
        /// Manufacturer only
        /// </summary>
        FrameControlNormal = 0xB1,
        /// <summary>
        /// Manufacturer only
        /// </summary>
        FrameControlIdle = 0xB2,
        /// <summary>
        /// Manufacturer only
        /// </summary>
        FrameControlPartial = 0xB3,
        /// <summary>
        /// Manufacturer only
        /// </summary>
        DisplayInversionControl = 0xB4,
        /// <summary>
        /// Manufacturer only
        /// </summary>
        BlankingPorchControl = 0xB5,
        /// <summary>
        /// Manufacturer only
        /// </summary>
        DisplayFunctionControl = 0xB6,
        /// <summary>
        /// Manufacturer only
        /// </summary>
        EntryModeSet = 0xB7,
        /// <summary>
        /// Manufacturer only
        /// </summary>
        BacklightControl1 = 0xB8,
        /// <summary>
        /// Manufacturer only
        /// </summary>
        BacklightControl2 = 0xB9,
        /// <summary>
        /// Manufacturer only
        /// </summary>
        BacklightControl3 = 0xBA,
        /// <summary>
        /// Manufacturer only
        /// </summary>
        BacklightControl4 = 0xBB,
        /// <summary>
        /// Manufacturer only
        /// </summary>
        BacklightControl5 = 0xBC,
        /// <summary>
        /// Manufacturer only
        /// </summary>
        BacklightControl6 = 0xBD,// BacklightControl6 did not exist in the Ilitek documentation, BD is assumed
        /// <summary>
        /// Manufacturer only
        /// </summary>
        BacklightControl7 = 0xBE,
        /// <summary>
        /// Manufacturer only
        /// </summary>
        BacklightControl8 = 0xBF,
        /// <summary>
        /// Manufacturer only
        /// </summary>
        PowerControl1 = 0xC0,
        /// <summary>
        /// Manufacturer only
        /// </summary>
        PowerControl2 = 0xC1,
        /// <summary>
        /// Manufacturer only
        /// </summary>
        VCOMControl1 = 0xC5,
        /// <summary>
        /// Manufacturer only
        /// </summary>
        VCOMControl2 = 0xC7,
        /// <summary>
        /// Manufacturer only
        /// </summary>
        NVMemoryWrite = 0xD0,
        /// <summary>
        /// Manufacturer only
        /// </summary>
        NVMemoryProtectionKey = 0xD1,
        /// <summary>
        /// Manufacturer only
        /// </summary>
        NVMemoryStatusRead = 0xD2,
        /// <summary>
        /// Manufacturer only
        /// </summary>
        ReadId4 = 0xD3,
        ReadId1 = 0xDA,
        ReadId2 = 0xDB,
        ReadId3 = 0xDC,
        PositiveGammaCorrection = 0xE0,
        NegativeGammaCorrection = 0xE1,
        DigitalGammaControl1 = 0xE2,
        DigitalGammaControl2 = 0xE3,
        InterfaceControl = 0xF6
    }
}
