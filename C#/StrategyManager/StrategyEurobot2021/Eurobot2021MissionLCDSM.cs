using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StrategyManagerEurobotNS;
using Utilities;

namespace StrategyManagerNS
{
    public class Eurobot2021MissionLCDSM : MissionBase
    {
        PositionTerrain Cible;

        private enum MissionLCDSMState
        {
            Idle,
            GOTO_Point
        }

        public Eurobot2021MissionLCDSM() : base()
        { }

        StrategyEurobot2021 parentStrategie;
        public Eurobot2021MissionLCDSM(StrategyGenerique sg) : base(sg)
        {
            parentStrategie = sg as StrategyEurobot2021;
        }

        public override void Init()
        {
            state = MissionLCDSMState.Idle;
            parent.OnPilotageTurbine((byte)PilotageTurbineID.Turbine_soufflage, 1000);
            ResetSubState();
            isFinished = false;
        }

        public void Start(PositionTerrain cible)
        {
            Cible = cible;
            isFinished = false;
            state = MissionLCDSMState.GOTO_Point;
            ResetSubState();
        }

        MissionLCDSMState state = MissionLCDSMState.Idle;

        DateTime timestamp;
        double timoutDeplacement;
        public override void MissionStateMachine()
        {
            if (state != MissionLCDSMState.Idle)
                isRunning = true;
            else
                isRunning = false;

            switch (state)
            {
                case MissionLCDSMState.Idle: //On fait une tempo de 2 secondes
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            timestamp = DateTime.Now;

                            break;
                        case SubTaskState.EnCours:
                            // ExitState();                                /// A appeler quand on souhaite passer à Exit                                    
                            break;
                        case SubTaskState.Exit:                             /// L'état suivant ne doit être défini que dans le substate Exit
                            //On ne doit pas passer ici
                            break;
                    }
                    break;
                case MissionLCDSMState.GOTO_Point:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            //On décale le point robot de la longueur du bras orienté selon l'angle
                            if (Cible.AnglePrise != null)
                            {
                                timoutDeplacement = parentStrategie.SetRobotDestination(Cible.Pos, -(double)Cible.AnglePrise);
                            }
                            else
                            {
                                timoutDeplacement = parentStrategie.SetRobotDestination(Cible.Pos, Math.PI + Math.Atan2(Cible.Pos.Y - parentStrategie.robotCurrentLocation.Y, Cible.Pos.X - parentStrategie.robotCurrentLocation.X));
                            }
                            timestamp = DateTime.Now;
                            break;
                        case SubTaskState.EnCours:
                            if(Math.Abs(parentStrategie.robotCurrentLocation.X)<0.4)
                            {
                                ///On fait le grand méchant loup :
                                parent.OnPilotageTurbine((byte)PilotageTurbineID.Turbine_soufflage, 1300);
                            }
                            else
                            {
                                parent.OnPilotageTurbine((byte)PilotageTurbineID.Turbine_soufflage, 1000);
                            }
                            if (parentStrategie.isDeplacementFinished || DateTime.Now.Subtract(timestamp).TotalMilliseconds > timoutDeplacement)
                            {
                                ExitState();                                /// A appeler quand on souhaite passer à Exit
                            }
                            break;
                        case SubTaskState.Exit: 
                            Console.WriteLine("Déplacement vers la LCDSM terminé");
                            parent.OnPilotageTurbine((byte)PilotageTurbineID.Turbine_soufflage, 1000);
                            Cible.isAvailable = false;
                            state = MissionLCDSMState.Idle;                     /// L'état suivant ne doit être défini que dans le substate Exit
                            break;
                    }
                    break;
                
                //case MissionTestRamassageState.GOTO_Depose:
                //    switch (subState)
                //    {
                //        case SubTaskState.Entry:
                //            if (parentStrategie.playingColor == StrategyEurobot2021.SideColor.Blue)
                //            {
                //                parentStrategie.SetRobotDestination(new PointD(-1.25, 0.35), Math.PI / 4);
                //            }
                //            else if (parentStrategie.playingColor == StrategyEurobot2021.SideColor.Yellow)
                //            {
                //                parentStrategie.SetRobotDestination(new PointD(1.25, 0.35), Math.PI / 4);
                //            }
                //            isFinished = false;
                //            break;
                //        case SubTaskState.EnCours:
                //            if (Toolbox.Distance(new PointD(parent.robotCurrentLocation.X, parent.robotCurrentLocation.Y), parent.robotDestination) < 0.05)
                //            {

                //                    parentStrategie.taskBras_dict["Bras_225"].StartDepose();
                //                    parentStrategie.taskBras_dict["Bras_180"].StartDepose();
                //                    parentStrategie.taskBras_dict["Bras_45"].StartDepose();

                //                ExitState();                                /// A appeler quand on souhaite passer à Exit
                //            }
                //            break;
                //        case SubTaskState.Exit:
                //            Console.WriteLine("Déplacement MissionTest1 terminé");
                //            isFinished = true;
                //            state = MissionTestRamassageState.Idle;                     /// L'état suivant ne doit être défini que dans le substate Exit
                //            break;
                //    }
                //    break;

                default:
                    break;
            }
        }
    }
}
