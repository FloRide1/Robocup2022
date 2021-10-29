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
    public class Eurobot2021MissionPhare : MissionBase
    {
        MissionPhareStates missionPhareState = MissionPhareStates.Idle;
        private Dictionary<ServoId, int> servoPositionsRequested;
        DateTime timestamp;
        Eurobot2021Phare PhareCourant;
        double indicativeTime;
        double angleBras = -3 * Math.PI / 4;

        StrategyEurobot2021 parentStrategie;
        public Eurobot2021MissionPhare() : base()
        { }

        public Eurobot2021MissionPhare(StrategyGenerique sg) : base(sg)
        {
            parentStrategie = sg as StrategyEurobot2021;
        }

        public override void Init()
        {
            missionPhareState = MissionPhareStates.Idle;
            ResetSubState();
            isFinished = false;
        }

        public void Start(Eurobot2021Phare phare)
        {
            PhareCourant = phare;
            missionPhareState = MissionPhareStates.PreDeplacementToPhare;
            isFinished = false;
            ResetSubState();
        }

        enum MissionPhareStates
        {
            Init,
            Idle,
            PreDeplacementToPhare,
            DeplacementToPhare,
            PrepareServo,
            PushPhare,
            Degagement,
            Retrait,
        }

        public void Pause()
        {
            missionPhareState = MissionPhareStates.Idle;
            isFinished = false;
        }


        public override void MissionStateMachine()
        {
            if (missionPhareState != MissionPhareStates.Idle)
                isRunning = true;
            else
                isRunning = false;

            switch (missionPhareState)
            {
                case MissionPhareStates.Init:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            timestamp = DateTime.Now;
                            parentStrategie.taskBrasDeclencheur.StartRemonteeBras();
                            break;
                        case SubTaskState.EnCours:
                            if (DateTime.Now.Subtract(timestamp).TotalMilliseconds > 1000.0)
                            {
                                ExitState();/// A appeler quand on souhaite passer à Exit       
                            }
                            break;
                        case SubTaskState.Exit:                             /// L'état suivant ne doit être défini que dans le substate Exit
                            //On a terminé l'action en cours, et le bras est vide et en position de depart
                            isRunning = false;
                            missionPhareState = MissionPhareStates.Idle;
                            break;
                    }
                    break;
                case MissionPhareStates.Idle: //On fait une tempo de 2 secondes
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            isFinished = true;
                            break;
                    }
                    break;
                case MissionPhareStates.PreDeplacementToPhare:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            ///Deplacement de pôsitionnement sans sortir le bras
                            if (parentStrategie.playingColor == StrategyEurobot2021.Eurobot2021SideColor.Blue)
                            {
                                indicativeTime = parentStrategie.SetRobotDestination(new PointD(PhareCourant.Pos.X + 0.06, PhareCourant.Pos.Y - parent.RayonRobot - 0.15), Toolbox.DegToRad(90) - angleBras);
                            }
                            else if (parentStrategie.playingColor == StrategyEurobot2021.Eurobot2021SideColor.Yellow)
                            {
                                indicativeTime = parentStrategie.SetRobotDestination(new PointD(PhareCourant.Pos.X - 0.06, PhareCourant.Pos.Y - parent.RayonRobot - 0.15), Toolbox.DegToRad(90) - angleBras);
                            }
                            timestamp = DateTime.Now;
                            break;
                        case SubTaskState.EnCours:
                            if (parentStrategie.isDeplacementFinished || DateTime.Now.Subtract(timestamp).TotalMilliseconds > indicativeTime)
                                ExitState();                                /// A appeler quand on souhaite passer à Exit
                            break;
                        case SubTaskState.Exit:
                            Console.WriteLine("Pré Déplacement vers flag terminé");
                            missionPhareState = MissionPhareStates.DeplacementToPhare;                    /// L'état suivant ne doit être défini que dans le substate Exit
                            break;
                    }
                    break;
                case MissionPhareStates.DeplacementToPhare:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            ///Deplacement
                            if (parentStrategie.playingColor == StrategyEurobot2021.Eurobot2021SideColor.Blue)
                            {
                                indicativeTime = parentStrategie.SetRobotDestination(new PointD(PhareCourant.Pos.X + 0.06, PhareCourant.Pos.Y - parent.RayonRobot - 0.05), Toolbox.DegToRad(90) - angleBras);
                            }
                            else if (parentStrategie.playingColor == StrategyEurobot2021.Eurobot2021SideColor.Yellow)
                            {
                                indicativeTime = parentStrategie.SetRobotDestination(new PointD(PhareCourant.Pos.X - 0.06, PhareCourant.Pos.Y - parent.RayonRobot - 0.05), Toolbox.DegToRad(90) - angleBras);
                            }
                            /// Positionnnement du servo
                            parentStrategie.taskBrasDeclencheur.StartPositionnementPhare();
                            timestamp = DateTime.Now;
                            break;
                        case SubTaskState.EnCours:
                            if (parentStrategie.isDeplacementFinished || DateTime.Now.Subtract(timestamp).TotalMilliseconds > indicativeTime)
                                ExitState();                                /// A appeler quand on souhaite passer à Exit
                            break;
                        case SubTaskState.Exit:
                            Console.WriteLine("Déplacement vers flag terminé avec sortie bras");
                            missionPhareState = MissionPhareStates.PushPhare;                    /// L'état suivant ne doit être défini que dans le substate Exit
                            break;
                    }
                    break;
                case MissionPhareStates.PushPhare:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            timestamp = DateTime.Now;
                            if (parentStrategie.playingColor == StrategyEurobot2021.Eurobot2021SideColor.Blue)
                            {
                                indicativeTime = parentStrategie.SetRobotDestination(new PointD(PhareCourant.Pos.X - 0.04, PhareCourant.Pos.Y - parent.RayonRobot - 0.05), Toolbox.DegToRad(90) - angleBras);
                            }
                            else if (parentStrategie.playingColor == StrategyEurobot2021.Eurobot2021SideColor.Yellow)
                            {
                                indicativeTime = parentStrategie.SetRobotDestination(new PointD(PhareCourant.Pos.X + 0.04, PhareCourant.Pos.Y - parent.RayonRobot - 0.05), Toolbox.DegToRad(90) - angleBras);
                            }
                            //parentStrategie.taskAvoidanceParametersModifiers.StartAvoidanceReduction(radiusObstacleFixe: 0.2, radiusObstacleMobile: 0.20, 4000);
                            break;
                        case SubTaskState.EnCours:
                            if (parentStrategie.isDeplacementFinished || DateTime.Now.Subtract(timestamp).TotalMilliseconds > indicativeTime)
                            {
                                ExitState();
                            }
                            break;
                        case SubTaskState.Exit:                             /// L'état suivant ne doit être défini que dans le substate Exit
                            Console.WriteLine("Phare : pushed");
                            missionPhareState = MissionPhareStates.Degagement;
                            PhareCourant.isAvailable = false;
                            break;
                    }
                    break;
                case MissionPhareStates.Degagement:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            timestamp = DateTime.Now;
                            if (parentStrategie.playingColor == StrategyEurobot2021.Eurobot2021SideColor.Blue)
                            {
                                indicativeTime = parentStrategie.SetRobotDestination(new PointD(PhareCourant.Pos.X - 0.08, PhareCourant.Pos.Y - parent.RayonRobot - 0.05), Toolbox.DegToRad(110) - angleBras);
                            }
                            else if (parentStrategie.playingColor == StrategyEurobot2021.Eurobot2021SideColor.Yellow)
                            {
                                indicativeTime = parentStrategie.SetRobotDestination(new PointD(PhareCourant.Pos.X + 0.08, PhareCourant.Pos.Y - parent.RayonRobot - 0.05), Toolbox.DegToRad(70) - angleBras);
                            }
                            //parentStrategie.taskAvoidanceParametersModifiers.StartAvoidanceReduction(radiusObstacleFixe: 0.2, radiusObstacleMobile: 0.20, 1000);
                            break;
                        case SubTaskState.EnCours:
                            if (parentStrategie.isDeplacementFinished || DateTime.Now.Subtract(timestamp).TotalMilliseconds > indicativeTime)
                            {
                                ExitState();
                            }
                            break;
                        case SubTaskState.Exit:                             /// L'état suivant ne doit être défini que dans le substate Exit
                            Console.WriteLine("Phare : dégagement effectué");
                            missionPhareState = MissionPhareStates.Retrait;
                            break;
                    }
                    break;
                case MissionPhareStates.Retrait:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            timestamp = DateTime.Now;
                            if (parentStrategie.playingColor == StrategyEurobot2021.Eurobot2021SideColor.Blue)
                            {
                                indicativeTime = parentStrategie.SetRobotDestination(new PointD(PhareCourant.Pos.X - 0.08, PhareCourant.Pos.Y - parent.RayonRobot - 0.05 - 0.10), Toolbox.DegToRad(110) - angleBras);
                            }
                            else if (parentStrategie.playingColor == StrategyEurobot2021.Eurobot2021SideColor.Yellow)
                            {
                                indicativeTime = parentStrategie.SetRobotDestination(new PointD(PhareCourant.Pos.X + 0.08, PhareCourant.Pos.Y - parent.RayonRobot - 0.05 - 0.10), Toolbox.DegToRad(70) - angleBras);
                            }
                            break;
                        case SubTaskState.EnCours:
                            if (parentStrategie.isDeplacementFinished || DateTime.Now.Subtract(timestamp).TotalMilliseconds > indicativeTime)
                            {
                                ExitState();
                            }
                            break;
                        case SubTaskState.Exit:                             /// L'état suivant ne doit être défini que dans le substate Exit
                            Console.WriteLine("Phare : retrait effectué");
                            parentStrategie.taskBrasDeclencheur.StartRemonteeBras();
                            missionPhareState = MissionPhareStates.Idle;
                            PhareCourant.isAvailable = false;
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
