using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace RobotController
{
    public class Class1 : IDisposable
    {
        private OutputPort DirectionPin1 = null;
        private OutputPort DirectionPin2 = null;

        public Class1(Cpu.PWMChannel pwm, Cpu.Pin directionPin1, Cpu.Pin directionPin2)
        {
            var freq = 10000d;
            var cycle = 0d;
            DirectionPin1 = new OutputPort(directionPin1, false);
            DirectionPin2 = new OutputPort(directionPin2, false);
        }

        public void SetSpeed(int speed)
        {
            if (speed <= 100 && speed >= -100)
            {
                var d1 = (speed > 0);
                var d2 = (speed < 0);
                var cycle = (double)speed / 100d;

                DirectionPin1.Write(d1);
                DirectionPin2.Write(d2);
            }
        }

        public void Dispose()
        {
            SetSpeed(0);
            DirectionPin1.Dispose();
            DirectionPin2.Dispose();
        }
    }
}
