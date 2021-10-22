using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using static StrategyManagerNS.StrategyEurobot2022;

namespace StrategyManagerNS
{
    public class Eurobot2022MissionPriseGobelet : MissionBase
    {
        double rayonRobot= 0.15;
        double longueurBrasRobot = 0.07;
        Eurobot2022Gobelet GobeletCourant;
        //PointD GobeletPosition = new PointD(0, 0);
        string BrasUtilise = "";
        double AnglePrise;
        double AngleRobot;

        private enum MissionPriseGobeletState
        {
            Idle,
            GOTO_gobelet,
            PRISE_Gobelet,
        }

        public Eurobot2022MissionPriseGobelet() : base()
        { }

        StrategyEurobot2022 parentStrategie;
        public Eurobot2022MissionPriseGobelet(StrategyGenerique sg) : base(sg)
        {
            parentStrategie = sg as StrategyEurobot2022;
        }

        public override void Init()
        {
            state = MissionPriseGobeletState.Idle;
            ResetSubState();
            isFinished = false;
        }

        public void Start(Eurobot2022Gobelet gob, double anglePrise, string bras)
        {
            GobeletCourant = gob;
            AnglePrise = anglePrise;
            Console.WriteLine("Demande de prise : AnglePrise =" + Toolbox.RadToDeg(anglePrise).ToString("N3"));
            Console.WriteLine("Demande de prise : Bras robot =" + bras);
            AngleRobot = Toolbox.Modulo2PiAngleRad(anglePrise - parentStrategie.matchDescriptor.DictionaryBrasTurbine[bras].AngleBras);
            Console.WriteLine("Demande de prise : AngleRobot =" + Toolbox.RadToDeg(AngleRobot).ToString("N3"));
            BrasUtilise = bras;
            isFinished = false;
            state = MissionPriseGobeletState.GOTO_gobelet;
            ResetSubState();
        }

        MissionPriseGobeletState state = MissionPriseGobeletState.Idle;

        DateTime timestamp;
        double timeoutindicatif;
        public override void MissionStateMachine()
        {
            if (state != MissionPriseGobeletState.Idle)
                isRunning = true;
            else
                isRunning = false;

            switch (state)
            {
                case MissionPriseGobeletState.Idle: //On fait une tempo de 2 secondes
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
                case MissionPriseGobeletState.GOTO_gobelet:
                    switch (subState)
                    {
                        case SubTaskState.Entry:                            
                            //On décale le point robot de la longueur du bras orienté selon l'angle
                            PointD emplacementPrise = new PointD(GobeletCourant.Pos.X- (rayonRobot + longueurBrasRobot)*Math.Cos(AnglePrise),
                                GobeletCourant.Pos.Y - (rayonRobot + longueurBrasRobot) * Math.Sin(AnglePrise));
                                timeoutindicatif = parentStrategie.SetRobotDestination(emplacementPrise, AngleRobot);

                            Console.WriteLine("GOTO gobelet : X=" + emplacementPrise.X.ToString("N3") + " - Y=" + emplacementPrise.Y.ToString("N3"));
                            Console.WriteLine("GOTO gobelet : Angle Prise = " + Toolbox.RadToDeg(AnglePrise).ToString("N3"));
                            if (GobeletCourant.Type == Eurobot2022TypeGobelet.Libre)
                                ; /// On ne fait rien avant d'arriver sur zone en libre
                            else if (GobeletCourant.Type == Eurobot2022TypeGobelet.Distributeur)
                                parentStrategie.taskBras_dict[BrasUtilise].StartPreparePrehensionGobeletDistributeur();
                            timestamp = DateTime.Now;
                            break;
                        case SubTaskState.EnCours:
                            if ((parentStrategie.isDeplacementFinished && parentStrategie.taskBras_dict[BrasUtilise].isFinished) 
                                || DateTime.Now.Subtract(timestamp).TotalMilliseconds > timeoutindicatif)
                            {
                                ExitState();                                /// A appeler quand on souhaite passer à Exit
                            }
                            break;
                        case SubTaskState.Exit:

                            if (DateTime.Now.Subtract(timestamp).TotalMilliseconds > timeoutindicatif)
                            {
                                /// On a timouté                             
                                /// Il faut savoir si on est juste à côté de notre position ou pas
                                PointD emplacementPriseCopy = new PointD(GobeletCourant.Pos.X - (rayonRobot + longueurBrasRobot) * Math.Cos(AnglePrise),
                                GobeletCourant.Pos.Y - (rayonRobot + longueurBrasRobot) * Math.Sin(AnglePrise));
                                double seuilPrisePossible = 0.05;
                                if (Toolbox.Distance(emplacementPriseCopy, new PointD(parent.robotCurrentLocation.X, parent.robotCurrentLocation.Y)) < seuilPrisePossible)
                                {
                                    Console.WriteLine("Déplacement vers gobelet terminé mais imprécis");
                                    state = MissionPriseGobeletState.PRISE_Gobelet;                     /// L'état suivant ne doit être défini que dans le substate Exit
                                }
                                else
                                {
                                    /// Failed : on revient à Idle sans cocher la case
                                    /// On remet en jeu les gobelets considérés avec un priorité de 1
                                    /// Ils seront gérés par l'algo ensuite
                                    Console.WriteLine("Déplacement vers gobelet FAILED");
                                    if (parentStrategie.playingColor == Eurobot2022SideColor.Blue)
                                        GobeletCourant.PriorityBlue = 1;
                                    if (parentStrategie.playingColor == Eurobot2022SideColor.Yellow)
                                        GobeletCourant.PriorityYellow = 1;
                                    state = MissionPriseGobeletState.Idle;                     /// L'état suivant ne doit être défini que dans le substate Exit
                                }
                            }
                            else
                            {
                                /// On est sortis sans timout
                                Console.WriteLine("Déplacement vers gobelet terminé");
                                state = MissionPriseGobeletState.PRISE_Gobelet;                     /// L'état suivant ne doit être défini que dans le substate Exit
                            }
                            break;
                    }
                    break;

                case MissionPriseGobeletState.PRISE_Gobelet:
                    switch (subState)
                    {
                        case SubTaskState.Entry:                            
                            timestamp = DateTime.Now;

                            //Si on est sur un gobelet standard, la préhension est en cours
                            if (GobeletCourant.Type == Eurobot2022TypeGobelet.Libre)
                            {
                                /// Si on est sur un gobelet standard, on démarre la préhension classique
                                parentStrategie.taskBras_dict[BrasUtilise].StartPrehensionGobeletLibre();
                            }
                            else if (GobeletCourant.Type == Eurobot2022TypeGobelet.Distributeur)
                            {
                                /// Si on est sur un gobelet distributeur
                                /// On lance la préhnesion distributeur
                                parentStrategie.taskBras_dict[BrasUtilise].StartPrehensionGobeletDistributeur();
                            }

                            break;
                        case SubTaskState.EnCours:
                            if (parentStrategie.taskBras_dict[BrasUtilise].isFinished)
                            {
                                ExitState();                                /// A appeler quand on souhaite passer à Exit
                            }
                            break;
                        case SubTaskState.Exit:
                            Console.WriteLine("Prise terminée");
                            parentStrategie.taskBras_dict[BrasUtilise].StartRemonteeBras();
                            isFinished = true;
                            GobeletCourant.isAvailable = false;
                            parentStrategie.matchDescriptor.DictionaryBrasTurbine[BrasUtilise].GobletCapturedColor = GobeletCourant.Color;
                            parentStrategie.matchDescriptor.DictionaryBrasTurbine[BrasUtilise].HasGobelet = true;
                            // On valide qu'une préhension a été effectuée
                            /// On regarde si le gobelet pris doit déclencher une prise immédiate
                            if (GobeletCourant.NbDeposeToTrigger != null)
                            {
                                /// Si le gobelet pris impose un nb  de dépose fixé
                                /// On incrémente le nombre de gobelets sur bras, sans déclencher une prise de 5 gobelets si les bras sont full
                                parentStrategie.matchDescriptor.IncrementeGobeletSurBras(false);
                                parentStrategie.matchDescriptor.ForçageCompteurDeposeAEffectuer((int)GobeletCourant.NbDeposeToTrigger);
                            }
                            else
                            {
                                /// Si le gobelet pris n'impose pas de nb de dépose fixé
                                /// On incrémente le nombre de gobelets sur bras, et on déclenche une prise de 5 gobelets si les bras sont full
                                parentStrategie.matchDescriptor.IncrementeGobeletSurBras();
                            }
                            state = MissionPriseGobeletState.Idle;                 /// L'état suivant ne doit être défini que dans le substate Exit
                            break;
                    }
                    break;
                
                //case MissionTestRamassageState.GOTO_Depose:
                //    switch (subState)
                //    {
                //        case SubTaskState.Entry:
                //            if (parentStrategie.playingColor == StrategyEurobot2022.SideColor.Blue)
                //            {
                //                parentStrategie.SetRobotDestination(new PointD(-1.25, 0.35), Math.PI / 4);
                //            }
                //            else if (parentStrategie.playingColor == StrategyEurobot2022.SideColor.Yellow)
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
