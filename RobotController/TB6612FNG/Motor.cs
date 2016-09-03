using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using static Microsoft.SPOT.Hardware.Cpu;
using System.Threading;

namespace RobotController.TB6612FNG
{
    public class Motor : IDisposable
    {
        private PWM MotorSpeed = null;
        private OutputPort DirectionPin1 = null;
        private OutputPort DirectionPin2 = null;
        public bool Enabled = true;

        public Motor(PWMChannel pwm, Pin directionPin1, Pin directionPin2)
        {
            var freq = 10000d;
            var cycle = 0d;
            MotorSpeed = new PWM(pwm, freq, cycle, false);
            DirectionPin1 = new OutputPort(directionPin1, false);
            DirectionPin2 = new OutputPort(directionPin2, false);
            MotorSpeed.Start();
            Enabled = true;
        }

        public void SetSpeed(int speed)
        {
            if (Enabled && speed <= 100 && speed >= -100)
            {
                var d1 = (speed > 0);
                var d2 = (speed < 0);
                var cycle = (double)System.Math.Abs(speed) / 100d;

                DirectionPin1.Write(d1);
                DirectionPin2.Write(d2);
                MotorSpeed.DutyCycle = cycle;
            }
        }

        public void Dispose()
        {
            MotorSpeed.Stop();
            SetSpeed(0);
            MotorSpeed.Dispose();
            DirectionPin1.Dispose();
            DirectionPin2.Dispose();
        }
    }
}
