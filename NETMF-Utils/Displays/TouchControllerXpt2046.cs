using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace BeranekCZ.NETMF.Displays
{
    public class TouchControllerXpt2046
    {
        private SPI _module;
        private InterruptPort _irq;

        public TouchControllerXpt2046(SPI device,Cpu.Pin irq)
        {
            _module = device;
            _irq = new InterruptPort(irq,false,Port.ResistorMode.PullDown,Port.InterruptMode.InterruptEdgeHigh);
            _irq.OnInterrupt += _irq_OnInterrupt;
        }

        void _irq_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            Debug.Print("TOUCH");
        }
    }
}
