using EventArgsLibrary;
using HerkulexManagerNS;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Timers;
using static HerkulexManagerNS.HerkulexEventArgs;

namespace XBoxControllerNS
{
    public class XBoxController
    {
        int robotId = 0;
        Controller controller;
        Gamepad gamepad;
        public bool connected = false;
        public int deadband = 7000;
        public float leftTrigger, rightTrigger;
        double Vtheta;
        double VxRampe = 0;
        double VyRampe = 0;
        double VthetaRampe = 0;
        bool stopped = false;
        bool turbineEnabled = false;
        bool turbineFrontEnabled = false;
        bool turbineFrontIsLow = false;
        bool turbineFrontIsLowLow = false;
        Timer timerGamepad = new Timer(50);

        double t = 0;

        public XBoxController(int id)
        {
            robotId = id;
            controller = new Controller(UserIndex.One);
            connected = controller.IsConnected;

            timerGamepad.Elapsed += TimerGamepad_Elapsed;
            timerGamepad.Start();
        }

        private void TimerGamepad_Elapsed(object sender, ElapsedEventArgs e)
        {
            double VLinMax = 6;   //1.2 ~= 0.3m/s
            double VThetaMax = 1.5* Math.PI;
            double valeurRampe = 0.6;
            double Vx;
            double Vy;

            double vitessePriseBalle;
            if (controller.IsConnected)
            {
                gamepad = controller.GetState().Gamepad;

                if (gamepad.LeftThumbY > deadband)
                    Vx = gamepad.LeftThumbY - deadband;
                else if (gamepad.LeftThumbY < -deadband)
                    Vx = gamepad.LeftThumbY + deadband;
                else
                    Vx = 0;
                Vx = Vx / short.MaxValue * VLinMax;

                //Inversion sur Vy pour avoir Vy positif quand on va vers la gauche.
                double gamePadVy = -gamepad.LeftThumbX;
                if (gamePadVy > deadband)
                    Vy = gamePadVy - deadband;
                else if (gamePadVy < -deadband)
                    Vy = gamePadVy + deadband;
                else
                    Vy = 0;
                Vy = Vy / short.MaxValue * VLinMax;


                //Inversion sur VTHeta pour avoir VTheta positif quand on va vers la gauche.
                double gamePadVTheta = -gamepad.RightThumbX;
                if (gamePadVTheta > deadband)
                    Vtheta = gamePadVTheta - deadband;
                else if (gamePadVTheta < -deadband)
                    Vtheta = gamePadVTheta + deadband;
                else
                    Vtheta = 0;
                Vtheta = Vtheta / short.MaxValue * VThetaMax;

                //Console.WriteLine("Gamepad Vx : " + Vx + " Vy : "+Vy +" VTheta : "+Vtheta);
                vitessePriseBalle = (float)(gamepad.RightTrigger) / 2.55;
                if (gamepad.Buttons.HasFlag(GamepadButtonFlags.X))
                {
                    OnTirToRobot(robotId, 100, 100, 0, 0, 100, 0, 0);
                }


                if (gamepad.Buttons.HasFlag(GamepadButtonFlags.B))
                {
                    if (!turbineEnabled)
                    {
                        //On active la turbine
                        OnPololuToRobot(11, 1500);
                        turbineEnabled = true;
                    }
                }
                else
                {
                    if(turbineEnabled)
                    {
                        //On éteint la turbine
                        OnPololuToRobot(11, 1000);
                        turbineEnabled = false;
                    }
                }
                if (gamepad.Buttons.HasFlag(GamepadButtonFlags.Y))
                {
                    if (!turbineFrontEnabled)
                    {
                        //On active la turbine
                        OnPololuToRobot(17, 1350);
                        turbineFrontEnabled = true;
                    }
                }
                else
                {
                    if (turbineFrontEnabled)
                    {
                        //On éteint la turbine
                        OnPololuToRobot(17, 1000);
                        turbineFrontEnabled = false;
                    }
                }

                if (gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder))
                {
                    if (!turbineFrontIsLowLow)
                    {
                        //On descend la turbine
                        Dictionary<ServoId, int> dict = new Dictionary<ServoId, int>();
                        dict.Add(ServoId.Turbine_0, 780);
                        OnHerkulexFromManetteToRobot(dict);
                        turbineFrontIsLowLow = true;
                    }
                }
                else
                {
                    if (turbineFrontIsLowLow)
                    {
                        //On remonte la turbine
                        Dictionary<ServoId, int> dict = new Dictionary<ServoId, int>();
                        dict.Add(ServoId.Turbine_0, 512);
                        OnHerkulexFromManetteToRobot(dict);
                        turbineFrontIsLowLow = false;
                    }
                }


                if (gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder))
                {
                    if (!turbineFrontIsLow)
                    {
                        //On descend la turbine
                        Dictionary<ServoId, int> dict = new Dictionary<ServoId, int>();
                        dict.Add(ServoId.Turbine_0, 850);
                        OnHerkulexFromManetteToRobot(dict);
                        turbineFrontIsLow = true;
                    }
                }
                else
                {
                    if (turbineFrontIsLow)
                    {
                        //On remonte la turbine
                        Dictionary<ServoId, int> dict = new Dictionary<ServoId, int>();
                        dict.Add(ServoId.Turbine_0, 512);
                        OnHerkulexFromManetteToRobot(dict);
                        turbineFrontIsLow = false;
                    }
                }

                if (gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp))
                {
                    OnMoveTirUpToRobot();
                }
                if (gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown))
                {
                    OnMoveTirDownToRobot();
                }
                if (gamepad.Buttons.HasFlag(GamepadButtonFlags.Start))
                {
                    if (stopped)
                    {
                        OnStopToRobot(false);
                        stopped = false;
                    }
                    else
                    {
                        OnStopToRobot(true);
                        stopped = true;
                    }

                }

                VxRampe = Vx;
                VyRampe = Vy;
                VthetaRampe = Vtheta;

                //t += 0.02;
                //VxRampe = 1 * Math.Sin(3.14 * t);


                OnSpeedConsigneToRobot(robotId, VxRampe, VyRampe, VthetaRampe);
                //OnPriseBalleToRobot(2, (float)(Vx*33.3));
                OnPriseBalleToRobot(5, vitessePriseBalle);
                OnPriseBalleToRobot(6, -vitessePriseBalle);
            }
        }

        //Events générés en sortie
        public delegate void SpeedConsigneEventHandler(object sender, PolarSpeedArgs e);
        public event EventHandler<PolarSpeedArgs> OnSpeedConsigneEvent;
        public virtual void OnSpeedConsigneToRobot(int id, double vx, double vy, double vtheta)
        {
            var handler = OnSpeedConsigneEvent;
            if (handler != null)
            {
                handler(this, new PolarSpeedArgs { RobotId = id, Vx = vx, Vy = vy, Vtheta = vtheta });
            }
        }

        public delegate void OnTirEventHandler(object sender, TirEventArgs e);
        public event EventHandler<TirEventArgs> OnTirEvent;
        public virtual void OnTirToRobot(int id, ushort coil1ms, ushort coil2ms, ushort coil3ms, ushort coil4ms, ushort coil2offsetms, ushort coil3offsetms, ushort coil4offsetms)
        {
            var handler = OnTirEvent;
            if (handler != null)
            {
                handler(this, new TirEventArgs {  coil1MS=coil1ms, coil2MS=coil2ms, coil3MS=coil3ms, coil4MS=coil4ms, coil2OffsetMS=coil2offsetms, coil3OffsetMS=coil3offsetms, coil4OffsetMS=coil4offsetms });
            }
        }

        public event EventHandler<PololuServoUsArgs> OnPololuFromManetteEvent;
        public virtual void OnPololuToRobot(byte servoChan, ushort servoPeriodUs)
        {
            var handler = OnPololuFromManetteEvent;
            if (handler != null)
            {
                handler(this, new PololuServoUsArgs { servoChannel = servoChan, servoUs = servoPeriodUs });
            }
        }

        public event EventHandler<HerkulexPositionsArgs> OnHerkulexFromManetteEvent;
        public virtual void OnHerkulexFromManetteToRobot(Dictionary<ServoId,int> dict)
        {
            var handler = OnHerkulexFromManetteEvent;
            if (handler != null)
            {
                handler(this, new HerkulexPositionsArgs { servoPositions=dict  });
            }
        }
        public delegate void OnStopEventHandler(object sender, BoolEventArgs e);
        public event EventHandler<BoolEventArgs> OnStopEvent;
        public virtual void OnStopToRobot(bool stop)
        {
            var handler = OnStopEvent;
            if (handler != null)
            {
                handler(this, new BoolEventArgs());
            }
        }

        public delegate void OnMoveTirUpEventHandler(object sender, EventArgs e);
        public event EventHandler<EventArgs> OnMoveTirUpEvent;
        public virtual void OnMoveTirUpToRobot()
        {
            var handler = OnMoveTirUpEvent;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        public delegate void OnMoveTirDownEventHandler(object sender, EventArgs e);
        public event EventHandler<EventArgs> OnMoveTirDownEvent;
        public virtual void OnMoveTirDownToRobot()
        {
            var handler = OnMoveTirDownEvent;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        public event EventHandler<SpeedConsigneToMotorArgs> OnPriseBalleEvent;
        public virtual void OnPriseBalleToRobot(byte motorNumber, double vitesse)
        {
            OnPriseBalleEvent?.Invoke(this, new SpeedConsigneToMotorArgs { MotorNumber = motorNumber, V = vitesse });
        }
    }
}
