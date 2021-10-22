using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace StrategyManagerNS
{
    public class Eurobot2021MissionDeposeGobelet : MissionBase
    {
        double rayonRobot= 0.16;
        double longueurBrasRobot = 0.07;
        double distancePreDepose = 0.15;
        Eurobot2021EmplacementDepose DeposeCourante;
        //PointD GobeletPosition = new PointD(0, 0);
        string BrasUtilise = "";
        
        private enum MissionDeposeGobeletState
        {
            Idle,
            GOTO_PreDepose,
            GOTO_Depose,
            DEPOSE_Gobelet,
        }

        public Eurobot2021MissionDeposeGobelet() : base()
        { }

        StrategyEurobot2021 parentStrategie;
        public Eurobot2021MissionDeposeGobelet(StrategyGenerique sg) : base(sg)
        {
            parentStrategie = sg as StrategyEurobot2021;
        }

        public override void Init()
        {
            state = MissionDeposeGobeletState.Idle;
            ResetSubState();
            isFinished = false;
        }

        public void Start(Eurobot2021EmplacementDepose emplacement, string bras)
        {
            DeposeCourante = emplacement; 
            BrasUtilise = bras;
            isFinished = false;
            state = MissionDeposeGobeletState.GOTO_PreDepose;
            ResetSubState();
        }

        MissionDeposeGobeletState state = MissionDeposeGobeletState.Idle;

        DateTime timestamp;
        double timoutDeplacement;
        public override void MissionStateMachine()
        {
            if (state != MissionDeposeGobeletState.Idle)
                isRunning = true;
            else
                isRunning = false;

            switch (state)
            {
                case MissionDeposeGobeletState.Idle: //On fait une tempo de 2 secondes
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
                case MissionDeposeGobeletState.GOTO_PreDepose:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            //On décale le point robot de la longueur du bras orienté selon l'angle
                            PointD deposePosition = new PointD(DeposeCourante.Pos.X - (rayonRobot + longueurBrasRobot + distancePreDepose) * Math.Cos(DeposeCourante.AngleDepose),
                                DeposeCourante.Pos.Y - (rayonRobot + longueurBrasRobot + distancePreDepose) * Math.Sin(DeposeCourante.AngleDepose));
                            timoutDeplacement = parentStrategie.SetRobotDestination(deposePosition, DeposeCourante.AngleDepose - parentStrategie.matchDescriptor.DictionaryBrasTurbine[BrasUtilise].AngleBras);
                            timestamp = DateTime.Now;
                            break;
                        case SubTaskState.EnCours:
                            if (parentStrategie.isDeplacementFinished || DateTime.Now.Subtract(timestamp).TotalMilliseconds > timoutDeplacement)
                            {
                                ExitState();                                /// A appeler quand on souhaite passer à Exit
                            }
                            break;
                        case SubTaskState.Exit:
                            parentStrategie.taskBras_dict[BrasUtilise].StartBaisseBras(); //On baisse le bras
                            Console.WriteLine("Déplacement vers la pré dépose terminé");
                            state = MissionDeposeGobeletState.GOTO_Depose;                     /// L'état suivant ne doit être défini que dans le substate Exit
                            break;
                    }
                    break;
                case MissionDeposeGobeletState.GOTO_Depose:
                    switch (subState)
                    {
                        case SubTaskState.Entry:                            
                            //On décale le point robot de la longueur du bras orienté selon l'angle
                            PointD deposePosition = new PointD(DeposeCourante.Pos.X- (rayonRobot + longueurBrasRobot)*Math.Cos(DeposeCourante.AngleDepose),
                                DeposeCourante.Pos.Y - (rayonRobot + longueurBrasRobot) * Math.Sin(DeposeCourante.AngleDepose));
                                timoutDeplacement = parentStrategie.SetRobotDestination(deposePosition, DeposeCourante.AngleDepose- parentStrategie.matchDescriptor.DictionaryBrasTurbine[BrasUtilise].AngleBras);
                            timestamp = DateTime.Now;
                            break;
                        case SubTaskState.EnCours:
                            if (parentStrategie.isDeplacementFinished || DateTime.Now.Subtract(timestamp).TotalMilliseconds > timoutDeplacement)
                            {
                                ExitState();                                /// A appeler quand on souhaite passer à Exit
                            }
                            break;
                        case SubTaskState.Exit:
                            Console.WriteLine("Déplacement vers la dépose terminé");
                            state = MissionDeposeGobeletState.DEPOSE_Gobelet;                     /// L'état suivant ne doit être défini que dans le substate Exit
                            break;
                    }
                    break;

                case MissionDeposeGobeletState.DEPOSE_Gobelet:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            parentStrategie.taskBras_dict[BrasUtilise].StartDepose();
                            timestamp = DateTime.Now;
                            break;
                        case SubTaskState.EnCours:
                            if (parentStrategie.taskBras_dict[BrasUtilise].isFinished)
                            {
                                ExitState();                                /// A appeler quand on souhaite passer à Exit
                            }
                            break;
                        case SubTaskState.Exit:
                            Console.WriteLine("Deposee terminée");
                            isFinished = true;
                            /// A la fin de la dépose, on coche la case en tant que used,
                            /// On indique que le bras n'a plus de gobelet et pas de couleur particulière
                            DeposeCourante.IsAvailable = false;
                            parentStrategie.matchDescriptor.DictionaryBrasTurbine[BrasUtilise].GobletCapturedColor = Eurobot2021Color.Neutre;
                            parentStrategie.matchDescriptor.DictionaryBrasTurbine[BrasUtilise].HasGobelet = false;
                            state = MissionDeposeGobeletState.Idle;                 /// L'état suivant ne doit être défini que dans le substate Exit
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
