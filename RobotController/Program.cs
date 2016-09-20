using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using RobotController.TB6612FNG;
using Microsoft.SPOT.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;

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

        static Cpu.Pin ReadyLight = Pins.ONBOARD_LED;

        //static event EventHandler MsgReceived = null;

        public static void Main()
        {
            //MotorTest();

            //MotorControllerTest();
            //MsgReceived += Program_MsgReceived1; ;
            StartListening();
        }

        private static void StartListening()
        {
            Socket socket = null;
            try
            {
                //Initialize Socket class
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //Request and bind to an IP from DHCP server
                socket.Bind(new IPEndPoint(IPAddress.Any, 8080));
                //Debug print our IP address
                string address = NetworkInterface.GetAllNetworkInterfaces()[0].IPAddress;
                var config = Wireless80211.GetAllNetworkInterfaces()[0] as Wireless80211;

                for (var i = 0; i < 10; i++)
                {
                    if (address != "192.168.5.100" && address != "0.0.0.0")
                        break;
                    address = NetworkInterface.GetAllNetworkInterfaces()[0].IPAddress;
                    Thread.Sleep(500);
                    Debug.Print("step " + i.ToString());
                }
                Debug.Print(address);
                Debug.Print(config.IPAddress);
                Debug.Print(config.ToString());
                FlashReady(address);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                throw;
            }
            try
            {
                while (true)
                {
                    //Start listen for web requests
                    socket.Listen(10);
                    ListenForRequest(socket);
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                throw;
            }
        }

        private static void FlashReady(string address)
        {
            var ready = new OutputPort(ReadyLight, false);
            for (var i = 0; i < 5; i++)
            {
                ready.Write(true);
                Thread.Sleep(500);
                ready.Write(false);
                Thread.Sleep(250);
            }
        }

        public static void ListenForRequest(Socket socket)
        {
            using (var ctrl = InitMotorController())
            {
                ctrl.Enabled = false;
                using (Socket clientSocket = socket.Accept())
                {
                    //Get clients IP
                    IPEndPoint clientIP = clientSocket.RemoteEndPoint as IPEndPoint;
                    EndPoint clientEndPoint = clientSocket.RemoteEndPoint;

                    if (clientSocket.Poll(-1, SelectMode.SelectRead))
                    {
                        while (true)
                        {
                            //int byteCount = cSocket.Available;
                            int bytesReceived = clientSocket.Available;
                            if (bytesReceived > 0)
                            {
                                //Get request
                                byte[] buffer = new byte[bytesReceived];
                                int byteCount = clientSocket.Receive(buffer, bytesReceived, SocketFlags.None);
                                string request = new string(Encoding.UTF8.GetChars(buffer));
                                Debug.Print(request);
                                //Compose a response
                                string response = "OK: " + request;
                                //string header = "HTTP/1.0 200 OK\r\nContent-Type: text; charset=utf-8\r\nContent-Length: " + response.Length.ToString() + "\r\nConnection: close\r\n\r\n";
                                //clientSocket.Send(Encoding.UTF8.GetBytes(header), header.Length, SocketFlags.None);
                                clientSocket.Send(Encoding.UTF8.GetBytes(response), response.Length, SocketFlags.None);

                                if (!Parse(request, ctrl))
                                    break;
                            }
                            else
                                Thread.Sleep(100);
                        }
                    }
                    clientSocket.Close();
                }
            }
        }

        private static bool Parse(string request, MotorController2WD ctrl)
        {
            var result = true;
            request = request.ToUpper();
            var rex = Regex.Match(request, @"^L([+-]?\d{3}),R([+-]?\d{3})$");
            if (rex.Success)
            {
                var left = int.Parse(rex.Groups[1].Value);
                var right = int.Parse(rex.Groups[2].Value);

                ctrl.Turn(left, right);
            }
            else if (request.Equals("MOTOR-START"))
                ctrl.Enabled = true;
            else if (request.Equals("MOTOR-STOP"))
                ctrl.Enabled = false;
            else if (request.Equals("DISCONNECT"))
                result = false;
            return result;
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
