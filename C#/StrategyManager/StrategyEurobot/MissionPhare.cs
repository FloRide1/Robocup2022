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
    public class MissionPhare : MissionBase
    {
        MissionPhareStates missionPhareState = MissionPhareStates.Idle;
        private Dictionary<ServoId, int> servoPositionsRequested;
        DateTime timestamp;
        Phare PhareCourant;
        double timoutIndicatif;
        double angleBras = -3 * Math.PI / 4;

        StrategyEurobot2021 parentStrategie;
        public MissionPhare() : base()
        { }

        public MissionPhare(StrategyGenerique sg) : base(sg)
        {
            parentStrategie = sg as StrategyEurobot2021;
        }

        public override void Init()
        {
            missionPhareState = MissionPhareStates.Idle;
            ResetSubState();
            isFinished = false;
        }

        public void Start(Phare phare)
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
                            if (parentStrategie.playingColor == StrategyEurobot2021.SideColor.Blue)
                            {
                                timoutIndicatif = parentStrategie.SetRobotDestination(new PointD(PhareCourant.Pos.X + 0.12, PhareCourant.Pos.Y - parent.RayonRobot - 0.15), Toolbox.DegToRad(90) - angleBras);
                            }
                            else if (parentStrategie.playingColor == StrategyEurobot2021.SideColor.Yellow)
                            {
                                timoutIndicatif = parentStrategie.SetRobotDestination(new PointD(PhareCourant.Pos.X - 0.12, PhareCourant.Pos.Y - parent.RayonRobot - 0.15), Toolbox.DegToRad(90) - angleBras);
                            }
                            timestamp = DateTime.Now;
                            break;
                        case SubTaskState.EnCours:
                            if (parentStrategie.isDeplacementFinished || DateTime.Now.Subtract(timestamp).TotalMilliseconds > timoutIndicatif)
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
                            if (parentStrategie.playingColor == StrategyEurobot2021.SideColor.Blue)
                            {
                                timoutIndicatif = parentStrategie.SetRobotDestination(new PointD(PhareCourant.Pos.X + 0.12, PhareCourant.Pos.Y - parent.RayonRobot - 0.05), Toolbox.DegToRad(90) - angleBras);
                            }
                            else if (parentStrategie.playingColor == StrategyEurobot2021.SideColor.Yellow)
                            {
                                timoutIndicatif = parentStrategie.SetRobotDestination(new PointD(PhareCourant.Pos.X - 0.12, PhareCourant.Pos.Y - parent.RayonRobot - 0.05), Toolbox.DegToRad(90) - angleBras);
                            }
                            /// Positionnnement du servo
                            parentStrategie.taskBrasDeclencheur.StartPositionnementPhare();
                            timestamp = DateTime.Now;
                            break;
                        case SubTaskState.EnCours:
                            if (parentStrategie.isDeplacementFinished || DateTime.Now.Subtract(timestamp).TotalMilliseconds > timoutIndicatif)
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
                            if (parentStrategie.playingColor == StrategyEurobot2021.SideColor.Blue)
                            {
                                timoutIndicatif = parentStrategie.SetRobotDestination(new PointD(PhareCourant.Pos.X - 0.12, PhareCourant.Pos.Y - parent.RayonRobot - 0.03), Toolbox.DegToRad(90) - angleBras);
                            }
                            else if (parentStrategie.playingColor == StrategyEurobot2021.SideColor.Yellow)
                            {
                                timoutIndicatif = parentStrategie.SetRobotDestination(new PointD(PhareCourant.Pos.X + 0.12, PhareCourant.Pos.Y - parent.RayonRobot - 0.03), Toolbox.DegToRad(90) - angleBras);
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
                                Console.WriteLine("Déplacement vers Phare FAILED");
                                if (parentStrategie.playingColor == StrategyEurobot2021.SideColor.Blue)
                                    PhareCourant.PriorityBlue -= 10;
                                if (parentStrategie.playingColor == StrategyEurobot2021.SideColor.Yellow)
                                    PhareCourant.PriorityYellow -= 10;
                                missionPhareState = MissionPhareStates.Idle;                     /// L'état suivant ne doit être défini que dans le substate Exit
                            }
                            else
                            {
                                Console.WriteLine("Phare : SUCCESS");
                                missionPhareState = MissionPhareStates.Degagement;
                                PhareCourant.isAvailable = false;
                            }
                            break;
                    }
                    break;
                case MissionPhareStates.Degagement:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            timestamp = DateTime.Now;
                            if (parentStrategie.playingColor == StrategyEurobot2021.SideColor.Blue)
                            {
                                timoutIndicatif = parentStrategie.SetRobotDestination(new PointD(PhareCourant.Pos.X - 0.12, PhareCourant.Pos.Y - parent.RayonRobot - 0.03), Toolbox.DegToRad(65) - angleBras);
                            }
                            else if (parentStrategie.playingColor == StrategyEurobot2021.SideColor.Yellow)
                            {
                                timoutIndicatif = parentStrategie.SetRobotDestination(new PointD(PhareCourant.Pos.X + 0.12, PhareCourant.Pos.Y - parent.RayonRobot - 0.03), Toolbox.DegToRad(115) - angleBras);
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
