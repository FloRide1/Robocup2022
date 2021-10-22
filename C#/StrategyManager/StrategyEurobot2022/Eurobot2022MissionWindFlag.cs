using Constants;
using EventArgsLibrary;
using HerkulexManagerNS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilities;
using static HerkulexManagerNS.HerkulexEventArgs;

namespace StrategyManagerNS
{
    public class Eurobot2022MissionWindFlag: MissionBase
    {
        MissionWindFlagStates missionWindFlagState = MissionWindFlagStates.Idle;
        DateTime timestamp;
        Eurobot2022MancheAir MancheAirCourante;

        double timoutIndicatif;

        double angleBras = -3 * Math.PI / 4;

        StrategyEurobot2022 parentStrategie;
        public Eurobot2022MissionWindFlag() : base()
        { }

        public Eurobot2022MissionWindFlag(StrategyGenerique sg) : base(sg)
        {
            parentStrategie = sg as StrategyEurobot2022;
        }

        public override void Init()
        {
            missionWindFlagState = MissionWindFlagStates.Idle;
            ResetSubState();
            parentStrategie.taskBrasDeclencheur.Init();
            isFinished = false;
        }

        public void Start(Eurobot2022MancheAir mancheAir)
        {
            MancheAirCourante = mancheAir;
            missionWindFlagState = MissionWindFlagStates.PreDeplacementToFlag;
            isFinished = false;
            ResetSubState();
        }

        enum MissionWindFlagStates
        {
            Init,
            Idle,
            PreDeplacementToFlag,
            DeplacementToFlag,
            PrepareServo,
            PushFlag,
            Degagement,
        }

        public void Pause()
        {
            missionWindFlagState = MissionWindFlagStates.Idle;
            isFinished = false;
        }


        public override void MissionStateMachine()
        {
            if (missionWindFlagState != MissionWindFlagStates.Idle)
                isRunning = true;
            else
                isRunning = false;

            switch (missionWindFlagState)
            {
                case MissionWindFlagStates.Idle: //On fait une tempo de 2 secondes
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            isFinished = true;
                            break;
                    }
                    break;
                case MissionWindFlagStates.PreDeplacementToFlag:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            ///Deplacement de pôsitionnement sans sortir le bras
                            if (parentStrategie.playingColor == Eurobot2022SideColor.Blue)
                            {
                                timoutIndicatif = parentStrategie.SetRobotDestination(new PointD(MancheAirCourante.Pos.X - 0.08, MancheAirCourante.Pos.Y + parent.RayonRobot + 0.15), Toolbox.DegToRad(-90) - angleBras);
                            }
                            else if (parentStrategie.playingColor == Eurobot2022SideColor.Yellow)
                            {
                                timoutIndicatif = parentStrategie.SetRobotDestination(new PointD(MancheAirCourante.Pos.X + 0.08, MancheAirCourante.Pos.Y + parent.RayonRobot + 0.15), Toolbox.DegToRad(-90) - angleBras);
                            }
                            timestamp = DateTime.Now;
                            break;
                        case SubTaskState.EnCours:
                            if (parentStrategie.isDeplacementFinished || DateTime.Now.Subtract(timestamp).TotalMilliseconds > timoutIndicatif)
                                ExitState();                                /// A appeler quand on souhaite passer à Exit
                            break;
                        case SubTaskState.Exit:
                            Console.WriteLine("Pré Déplacement vers flag terminé");
                            missionWindFlagState = MissionWindFlagStates.DeplacementToFlag;                    /// L'état suivant ne doit être défini que dans le substate Exit
                            break;
                    }
                    break;
                case MissionWindFlagStates.DeplacementToFlag:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            ///Deplacement
                            if (parentStrategie.playingColor == Eurobot2022SideColor.Blue)
                            {
                                timoutIndicatif = parentStrategie.SetRobotDestination(new PointD(MancheAirCourante.Pos.X - 0.08, MancheAirCourante.Pos.Y + parent.RayonRobot + 0.03), Toolbox.DegToRad(-90) - angleBras);
                            }
                            else if (parentStrategie.playingColor == Eurobot2022SideColor.Yellow)
                            {
                                timoutIndicatif = parentStrategie.SetRobotDestination(new PointD(MancheAirCourante.Pos.X + 0.08, MancheAirCourante.Pos.Y + parent.RayonRobot + 0.03), Toolbox.DegToRad(-90) - angleBras);
                            }
                            /// Positionnnement du servo
                            parentStrategie.taskBrasDeclencheur.StartPositionnementMancheAir();
                            timestamp = DateTime.Now;
                            break;
                        case SubTaskState.EnCours:
                            if (parentStrategie.isDeplacementFinished || DateTime.Now.Subtract(timestamp).TotalMilliseconds > timoutIndicatif)
                                ExitState();                                /// A appeler quand on souhaite passer à Exit
                            break;
                        case SubTaskState.Exit:
                            Console.WriteLine("Déplacement vers flag terminé avec sortie bras");
                            missionWindFlagState = MissionWindFlagStates.PushFlag;                    /// L'état suivant ne doit être défini que dans le substate Exit
                            break;
                    }
                    break;
                //case MissionWindFlagStates.PrepareServo:
                //    switch (subState)
                //    {
                //        case SubTaskState.Entry:
                //            timestamp = DateTime.Now;
                //            servoPositionsRequested = new Dictionary<ServoId, int>();
                //            servoPositionsRequested.Add(ServoId.Bras_225, (int)TaskBrasPositions.Push);
                //            parentStrategie.OnHerkulexSetPosition(servoPositionsRequested);
                //            break;
                //        case SubTaskState.EnCours:
                //            if (DateTime.Now.Subtract(timestamp).TotalMilliseconds > 200.0)
                //                ExitState();                                /// A appeler quand on souhaite passer à Exit
                //            break;
                //        case SubTaskState.Exit:
                //            Console.WriteLine("Manche à Air : Servos deployes");
                //            missionWindFlagState = MissionWindFlagStates.PushFlag;                    /// L'état suivant ne doit être défini que dans le substate Exit
                //            break;
                //    }
                //    break;
                case MissionWindFlagStates.PushFlag:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            timestamp = DateTime.Now;
                            if (parentStrategie.playingColor == Eurobot2022SideColor.Blue)
                            {
                                timoutIndicatif = parentStrategie.SetRobotDestination(new PointD(MancheAirCourante.Pos.X + 0.12, MancheAirCourante.Pos.Y + parent.RayonRobot + 0.03), Toolbox.DegToRad(-90) - angleBras);
                            }
                            else if (parentStrategie.playingColor == Eurobot2022SideColor.Yellow)
                            {
                                timoutIndicatif = parentStrategie.SetRobotDestination(new PointD(MancheAirCourante.Pos.X - 0.12, MancheAirCourante.Pos.Y + parent.RayonRobot + 0.03), Toolbox.DegToRad(-90) - angleBras);
                            }
                            parentStrategie.taskParametersModifiers.StartAvoidanceReduction(radiusObstacleFixe: 0.05, radiusObstacleMobile: 0.20, 4000);
                            break;
                        case SubTaskState.EnCours:
                            if (parentStrategie.isDeplacementFinished || DateTime.Now.Subtract(timestamp).TotalMilliseconds > timoutIndicatif)
                            {
                                ExitState();
                            }
                            break;
                        case SubTaskState.Exit:                             /// L'état suivant ne doit être défini que dans le substate Exit
                            if (DateTime.Now.Subtract(timestamp).TotalMilliseconds > timoutIndicatif)
                            {
                                /// Failed : on revient à Idle sans cocher la case
                                /// On remet en jeu les windflags considérés avec un priorité de 1
                                /// Ils seront gérés par l'algo ensuite
                                Console.WriteLine("Déplacement vers manche à air : FAILED");
                                if (parentStrategie.playingColor == Eurobot2022SideColor.Blue)
                                    MancheAirCourante.PriorityBlue -= 10;
                                if (parentStrategie.playingColor == Eurobot2022SideColor.Yellow)
                                    MancheAirCourante.PriorityYellow -= 10;
                                missionWindFlagState = MissionWindFlagStates.Idle;                     /// L'état suivant ne doit être défini que dans le substate Exit
                            }
                            else
                            {
                                Console.WriteLine("Manche à air : SUCCESS");
                                missionWindFlagState = MissionWindFlagStates.Degagement;
                                MancheAirCourante.isAvailable = false;
                            }
                            break;
                    }
                    break;
                case MissionWindFlagStates.Degagement:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            timestamp = DateTime.Now;
                            if (parentStrategie.playingColor == Eurobot2022SideColor.Blue)
                            {
                                timoutIndicatif = parentStrategie.SetRobotDestination(new PointD(MancheAirCourante.Pos.X + 0.12, MancheAirCourante.Pos.Y + parent.RayonRobot + 0.03), Toolbox.DegToRad(-65) - angleBras);
                            }
                            else if (parentStrategie.playingColor == Eurobot2022SideColor.Yellow)
                            {
                                timoutIndicatif = parentStrategie.SetRobotDestination(new PointD(MancheAirCourante.Pos.X - 0.12, MancheAirCourante.Pos.Y + parent.RayonRobot + 0.03), Toolbox.DegToRad(-115) - angleBras);
                            }
                            parentStrategie.taskParametersModifiers.StartAvoidanceReduction(radiusObstacleFixe: 0.05, radiusObstacleMobile: 0.20, 4000);
                            break;
                        case SubTaskState.EnCours:
                            if (parentStrategie.isDeplacementFinished || DateTime.Now.Subtract(timestamp).TotalMilliseconds > timoutIndicatif)
                            {
                                ExitState();
                            }
                            break;
                        case SubTaskState.Exit:                             /// L'état suivant ne doit être défini que dans le substate Exit
                            Console.WriteLine("Manche à air : pushed");
                            parentStrategie.taskBrasDeclencheur.StartRemonteeBras();
                            missionWindFlagState = MissionWindFlagStates.Idle;
                            MancheAirCourante.isAvailable = false;
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        public event EventHandler<SpeedConsigneToMotorArgs> OnPilotageVentouseEvent;
        public virtual void OnPilotageVentouse(byte motorNumber, double vitesse)
        {
            OnPilotageVentouseEvent?.Invoke(this, new SpeedConsigneToMotorArgs { MotorNumber = motorNumber, V = vitesse });
        }
        public void OnMotorCurrentReceive(object sender, MotorsCurrentsEventArgs e)
        {
        }

        public event EventHandler<HerkulexPositionsArgs> OnHerkulexPositionRequestEvent;
        public virtual void OnHerkulexPositionRequest(Dictionary<ServoId, int> positionDictionary)
        {
            OnHerkulexPositionRequestEvent?.Invoke(this, new HerkulexPositionsArgs { servoPositions = positionDictionary });
        }
    }
}
