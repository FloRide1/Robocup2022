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
using static StrategyManagerNS.StrategyEurobot2021;

namespace StrategyManagerNS
{
    public class Eurobot2021MissionZoneMouillage : MissionBase
    {
        MissionZoneMouillageState missionZoneMouillage = MissionZoneMouillageState.Idle;
        DateTime timestamp;
        double indicativeTime;

        StrategyEurobot2021 parentStrategie;
        public Eurobot2021MissionZoneMouillage() : base()
        { }

        public Eurobot2021MissionZoneMouillage(StrategyGenerique sg) : base(sg)
        {
            parentStrategie = sg as StrategyEurobot2021;
        }

        public override void Init()
        {
            missionZoneMouillage = MissionZoneMouillageState.Idle;
            ResetSubState();
            isFinished = false;
        }

        public void Start()
        {
            missionZoneMouillage = MissionZoneMouillageState.DeplacementToZoneMoullage;
            isFinished = false;
            ResetSubState();
        }

        enum MissionZoneMouillageState
        {
            Init,
            Idle,
            DeplacementToZoneMoullage,
        }

        public void Pause()
        {
            missionZoneMouillage = MissionZoneMouillageState.Idle;
            isFinished = false;
        }


        public override void MissionStateMachine()
        {
            if (missionZoneMouillage != MissionZoneMouillageState.Idle)
                isRunning = true;
            else
                isRunning = false;

            switch (missionZoneMouillage)
            {
                case MissionZoneMouillageState.Init:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            timestamp = DateTime.Now;
                            break;
                        case SubTaskState.EnCours:
                            if (DateTime.Now.Subtract(timestamp).TotalMilliseconds > 1000.0)
                            {
                                ExitState();/// A appeler quand on souhaite passer à Exit       
                            }
                            break;
                        case SubTaskState.Exit:                             /// L'état suivant ne doit être défini que dans le substate Exit
                            isRunning = false;
                            missionZoneMouillage = MissionZoneMouillageState.Idle;
                            break;
                    }
                    break;
                case MissionZoneMouillageState.Idle: //On fait une tempo de 2 secondes
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            isFinished = true;
                            break;
                    }
                    break;
                case MissionZoneMouillageState.DeplacementToZoneMoullage:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            ///Deplacement de pôsitionnement sans sortir le bras
                            foreach(var task in parentStrategie.listTasks)
                            {
                                task.Init();
                            }

                            if (parentStrategie.playingColor == Eurobot2021SideColor.Blue)
                            {
                                parentStrategie.obstacleFixeListAdditional.Add(new LocationExtended(-1.1, 0.485, 0, ObjectType.Obstacle));
                                parentStrategie.obstacleFixeListAdditional.Add(new LocationExtended(-1.25, 0.485, 0, ObjectType.Obstacle));
                                parentStrategie.obstacleFixeListAdditional.Add(new LocationExtended(-1.4, 0.485, 0, ObjectType.Obstacle));
                                parentStrategie.obstacleFixeListAdditional.Add(new LocationExtended(-1.1, -0.085, 0, ObjectType.Obstacle));
                                parentStrategie.obstacleFixeListAdditional.Add(new LocationExtended(-1.25, -0.085, 0, ObjectType.Obstacle));
                                parentStrategie.obstacleFixeListAdditional.Add(new LocationExtended(-1.4, -0.085, 0, ObjectType.Obstacle));
                                if (parentStrategie.robotType == Eurobot2021RobotType.RobotNord)
                                {
                                    indicativeTime = parentStrategie.SetRobotDestination(new PointD(-1.150, 0.77), Math.PI);
                                }
                                else
                                {
                                    indicativeTime = parentStrategie.SetRobotDestination(new PointD(-1.200, -0.45), Math.PI);
                                }
                            }
                            else if (parentStrategie.playingColor == Eurobot2021SideColor.Yellow)
                            {
                                parentStrategie.obstacleFixeListAdditional.Add(new LocationExtended(1.1, 0.485, 0, ObjectType.Obstacle));
                                parentStrategie.obstacleFixeListAdditional.Add(new LocationExtended(1.25, 0.485, 0, ObjectType.Obstacle));
                                parentStrategie.obstacleFixeListAdditional.Add(new LocationExtended(1.4, 0.485, 0, ObjectType.Obstacle));
                                parentStrategie.obstacleFixeListAdditional.Add(new LocationExtended(1.1, -0.085, 0, ObjectType.Obstacle));
                                parentStrategie.obstacleFixeListAdditional.Add(new LocationExtended(1.25, -0.085, 0, ObjectType.Obstacle));
                                parentStrategie.obstacleFixeListAdditional.Add(new LocationExtended(1.4, -0.085, 0, ObjectType.Obstacle));
                                if (parentStrategie.robotType == Eurobot2021RobotType.RobotNord)
                                {
                                    indicativeTime = parentStrategie.SetRobotDestination(new PointD(1.150, 0.77), Math.PI);
                                }
                                else
                                {
                                    indicativeTime = parentStrategie.SetRobotDestination(new PointD(1.200, -0.45), Math.PI);
                                }
                            }
                            timestamp = DateTime.Now;
                            break;
                        case SubTaskState.EnCours:
                            if (parentStrategie.isDeplacementFinished || DateTime.Now.Subtract(timestamp).TotalMilliseconds > 5000)
                                ExitState();                                /// A appeler quand on souhaite passer à Exit
                            break;
                        case SubTaskState.Exit:
                            Console.WriteLine("Déplacement vers zone de mouillage terminé");
                            missionZoneMouillage = MissionZoneMouillageState.Idle;                    /// L'état suivant ne doit être défini que dans le substate Exit
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
