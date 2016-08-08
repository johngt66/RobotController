using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using RobotController.TB6612FNG;

namespace RobotController
{
    public class Program
    {
        static Cpu.PWMChannel LeftPWM = PWMChannels.PWM_PIN_D9;
        static Cpu.Pin LeftDir1 = Pins.GPIO_PIN_D7;
        static Cpu.Pin LeftDir2 = Pins.GPIO_PIN_D8;
        static Cpu.PWMChannel RightPWM = PWMChannels.PWM_PIN_D5;
        static Cpu.Pin RightDir1 = Pins.GPIO_PIN_D3;
        static Cpu.Pin RightDir2 = Pins.GPIO_PIN_D4;
        static Cpu.Pin StandBy = Pins.GPIO_PIN_D0;
        public static void Main()
        {
            //MotorTest();

            MotorControllerTest();
        }

        private static void MotorControllerTest()
        {
            using (var ctrl = InitMotorController())
            {
                for (var i = -10; i <= 10; i++)
                {
                    ctrl.Go(i * 10);
                    Debug.Print(i.ToString());
                    Thread.Sleep(1000);
                }
                var speed = 70;

                Debug.Print("Forward: " + speed.ToString());
                ctrl.Stop();
                ctrl.Go(speed);
                Thread.Sleep(2500);

                Debug.Print("Right Only: " + speed.ToString());
                ctrl.Stop();
                ctrl.SetRight(speed);
                Thread.Sleep(2500);

                Debug.Print("Left Only: " + speed.ToString());
                ctrl.Stop();
                ctrl.SetLeft(speed);
                Thread.Sleep(2500);

                Debug.Print("Back: " + speed.ToString());
                ctrl.Stop();
                ctrl.Go(-speed);
                Thread.Sleep(2500);

                Debug.Print("Right turn: " + speed.ToString());
                ctrl.Stop();
                ctrl.Turn(speed, -speed);
                Thread.Sleep(2500);

                Debug.Print("Left Turn: " + speed.ToString());
                ctrl.Stop();
                ctrl.Turn(-speed, speed);
                Thread.Sleep(2500);
            }
        }

        private static void MotorTest()
        {
            var stdb = new OutputPort(StandBy, true);
            using (var ctrl = new Motor(LeftPWM, LeftDir1, LeftDir2))
            {
                //ctrl.Stop();
                ctrl.SetSpeed(0);
                for (var i = -10; i <= 10; i++)
                {
                    var speed = i * 10;
                    ctrl.SetSpeed(speed);
                    Debug.Print(speed.ToString());
                    Thread.Sleep(1000);
                }
                //ctrl.Stop();
                ctrl.SetSpeed(0);
            }
            using (var ctrl = new Motor(RightPWM, RightDir1, RightDir2))
            {
                //ctrl.Stop();
                ctrl.SetSpeed(0);
                for (var i = -10; i <= 10; i++)
                {
                    var speed = i * 10;
                    ctrl.SetSpeed(speed);
                    Debug.Print(speed.ToString());
                    Thread.Sleep(1000);
                }
                //ctrl.Stop();
                ctrl.SetSpeed(0);
            }
            stdb.Write(false);
        }

        private static MotorController2WD InitMotorController()
        {
            var ctrl = new MotorController2WD(LeftPWM, LeftDir1, LeftDir2, RightPWM, RightDir1, RightDir2, StandBy);
            return ctrl;
        }
    }
}
