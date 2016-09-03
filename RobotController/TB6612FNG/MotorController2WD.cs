using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using static Microsoft.SPOT.Hardware.Cpu;

namespace RobotController.TB6612FNG
{
    public class MotorController2WD : IDisposable
    {
        private Motor LeftMotor = null;
        private Motor RightMotor = null;

        private OutputPort StandBy = null;
        public bool Enabled
        {
            get
            {
                return LeftMotor.Enabled && RightMotor.Enabled;
            }
            set
            {
                LeftMotor.Enabled = value;
                RightMotor.Enabled = value;
            }
        }
        public MotorController2WD(PWMChannel leftPWM, Pin leftDir1, Pin leftDir2, PWMChannel rightPWM, Pin rightDir1, Pin rightDir2, Pin standby)
        {
            StandBy = new OutputPort(standby, true);

            LeftMotor = new Motor(leftPWM, leftDir1, leftDir2);
            RightMotor = new Motor(rightPWM, rightDir1, rightDir2);
            Enabled = true;
        }

        public void SetLeft(int speed)
        {
            if (speed >= -100 && speed <= 100)
                LeftMotor.SetSpeed(speed);
        }
        public void SetRight(int speed)
        {
            if (speed >= -100 && speed <= 100)
                RightMotor.SetSpeed(speed);
        }
        public void Stop()
        {
            Go(0);
        }
        public void Go(int speed)
        {
            SetLeft(speed);
            SetRight(speed);
        }

        public void Turn(int leftSpeed, int rightSpeed)
        {
            SetLeft(leftSpeed);
            SetRight(rightSpeed);
        }

        public void Dispose()
        {
            LeftMotor.Dispose();
            RightMotor.Dispose();
            StandBy.Write(false);
            StandBy.Dispose();
        }
    }
}
