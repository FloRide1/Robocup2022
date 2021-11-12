using EventArgsLibrary;
using HerkulexManagerNS;
using StrategyManagerEurobotNS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static HerkulexManagerNS.HerkulexEventArgs;

namespace StrategyManagerNS
{
    public class Eurobot2022TaskBrasTurbine : TaskBase
    {
        DateTime timestamp;
        TaskBrasState state = TaskBrasState.Idle;
        public ServoId _servoID;
        Eurobot2022PilotageTurbineID _turbineID;

        ushort intensiteTurbineDeplacement = 1450;
        ushort intensiteTurbineDeplacementDistributeur = 1450;



        public Eurobot2022TaskBrasTurbine() : base()
        {

        }

        public Eurobot2022TaskBrasTurbine(StrategyGenerique sg, ServoId servoID, Eurobot2022PilotageTurbineID turbineID) : base(sg)
        {
            parent = sg;
            _turbineID = turbineID;
            _servoID = servoID;
            taskPeriod = 100;
        }

        enum TaskBrasState
        {
            Init,
            Idle,
            PrehensionGobeletLibre,
            PrehensionGobeletCouche,
            PrehensionGobeletDistributeurPreparation,
            PrehensionGobeletDistributeur,
            PrehensionGobeletDistributeurRelevage,
            StockageEnHauteur,
            Depose,
            BaisseBras,
        }

        enum TaskBrasServoPositions
        {
            Init = 512,
            GobeletLibre = 773,
            GobeletCouche = 850,
            GobeletDistributeurPreparation = 680,
            GobeletDistributeur = 730
        }

        Dictionary<ServoId, int> servoPositionsRequested = new Dictionary<ServoId, int>();

        public override void Init()
        {
            if (state != TaskBrasState.Init)
            {
                state = TaskBrasState.Init;
                ResetSubState();
                isFinished = false;
            }
        }

        public void StartPrehensionGobeletLibre()
        {
            state = TaskBrasState.PrehensionGobeletLibre;
            ResetSubState();
            isFinished = false;
        }

        public void StartPrehensionGobeletCouche()
        {
            state = TaskBrasState.PrehensionGobeletCouche;
            ResetSubState();
            isFinished = false;
        }

        public void StartPrehensionGobeletDistributeur()
        {
            state = TaskBrasState.PrehensionGobeletDistributeur;
            ResetSubState();
            isFinished = false;
        }
        public void StartPreparePrehensionGobeletDistributeur()
        {
            state = TaskBrasState.PrehensionGobeletDistributeurPreparation;
            ResetSubState();
            isFinished = false;
        }

        public void StartRemonteeBras()
        {
            state = TaskBrasState.StockageEnHauteur;
            ResetSubState();
            isFinished = false;
        }

        public void StartDepose()
        {
            state = TaskBrasState.Depose;
            ResetSubState();
            isFinished = false;
        }

        public void StartBaisseBras()
        {
            state = TaskBrasState.BaisseBras;
            ResetSubState();
            isFinished = false;
        }

        public override void TaskStateMachine()
        {
            switch (state)
            {
                case TaskBrasState.Init:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            //Console.WriteLine("Init Task Bras Turbine");
                            timestamp = DateTime.Now;
                            servoPositionsRequested = new Dictionary<ServoId, int>();
                            servoPositionsRequested.Add((ServoId)_servoID, (int)TaskBrasServoPositions.Init);
                            parent.OnHerkulexSetPosition(servoPositionsRequested);
                            parent.OnPilotageTurbine((byte)_turbineID, 1000);   //On eteint la turbine
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
                            state = TaskBrasState.Idle;
                            break;
                    }
                    break;
                case TaskBrasState.Idle:
                    {
                        /// On ne sort pas de cet état sans un forçage extérieur vers un autre état
                        /// On maintient l'état du bras
                        isFinished = true; /// Pas d'action en cours
                    }
                    break;
                case TaskBrasState.PrehensionGobeletLibre:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            isRunning = true;           //On a une action de task en cours
                            timestamp = DateTime.Now;
                            servoPositionsRequested = new Dictionary<ServoId, int>();
                            /// On allume la turbine a 50%
                            parent.OnPilotageTurbine((byte)_turbineID, 1570);
                            /// On descend le servo
                            servoPositionsRequested.Add((ServoId)_servoID, (int)TaskBrasServoPositions.GobeletLibre);
                            parent.OnHerkulexSetPosition(servoPositionsRequested);
                            break;
                        case SubTaskState.EnCours:
                            if (DateTime.Now.Subtract(timestamp).TotalMilliseconds > 400.0)
                            {
                                ExitState();/// A appeler quand on souhaite passer à Exit       
                            }
                            break;
                        case SubTaskState.Exit:
                            /// L'état suivant ne doit être défini que dans le substate Exit
                            /// On part en Idle après avoir demandé l'attrapage d'un gobelet
                            /// La turbine est active à ce moment là à puissance de prise normale                            
                            parent.OnPilotageTurbine((byte)_turbineID, intensiteTurbineDeplacement);
                            state = TaskBrasState.Idle;
                            break;
                    }

                    break;
                case TaskBrasState.PrehensionGobeletCouche:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            isRunning = true;           //On a une action de task en cours
                            timestamp = DateTime.Now;
                            servoPositionsRequested = new Dictionary<ServoId, int>();
                            /// On allume la turbine a 40%
                            parent.OnPilotageTurbine((byte)_turbineID, 1600);
                            servoPositionsRequested.Add((ServoId)_servoID, (int)TaskBrasServoPositions.GobeletCouche);
                            parent.OnHerkulexSetPosition(servoPositionsRequested);
                            break;
                        case SubTaskState.EnCours:
                            if (DateTime.Now.Subtract(timestamp).TotalMilliseconds > 800.0)
                            {
                                ExitState();/// A appeler quand on souhaite passer à Exit       
                            }
                            break;
                        case SubTaskState.Exit:
                            /// L'état suivant ne doit être défini que dans le substate Exit
                            /// On part en Idle après avoir demandé l'attrapage d'un gobelet
                            /// La turbine est active à ce moment là à puissance de prise normale                            
                            parent.OnPilotageTurbine((byte)_turbineID, intensiteTurbineDeplacement);
                            state = TaskBrasState.Idle;
                            break;
                    }

                    break;
                case TaskBrasState.PrehensionGobeletDistributeurPreparation:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            isRunning = true;           //On a une action de task en cours
                            timestamp = DateTime.Now;
                            servoPositionsRequested = new Dictionary<ServoId, int>();
                            parent.OnPilotageTurbine((byte)_turbineID, 1200); //On allume la turbine a 40%
                            servoPositionsRequested.Add((ServoId)_servoID, (int)TaskBrasServoPositions.GobeletDistributeurPreparation);
                            parent.OnHerkulexSetPosition(servoPositionsRequested);
                            break;
                        case SubTaskState.EnCours:
                            //if (DateTime.Now.Subtract(timestamp).TotalMilliseconds > 200.0)
                            {
                                ExitState();/// A appeler quand on souhaite passer à Exit       
                            }
                            break;
                        case SubTaskState.Exit:                             /// L'état suivant ne doit être défini que dans le substate Exit
                            state = TaskBrasState.Idle;
                            break;
                    }
                    break;
                case TaskBrasState.PrehensionGobeletDistributeur:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            isRunning = true;           //On a une action de task en cours
                            timestamp = DateTime.Now;
                            servoPositionsRequested = new Dictionary<ServoId, int>();
                            parent.OnPilotageTurbine((byte)_turbineID, 1500); //On allume la turbine a 50%
                            servoPositionsRequested.Add((ServoId)_servoID, (int)TaskBrasServoPositions.GobeletDistributeur);
                            parent.OnHerkulexSetPosition(servoPositionsRequested);
                            break;
                        case SubTaskState.EnCours:
                            if (DateTime.Now.Subtract(timestamp).TotalMilliseconds > 400.0)
                            {
                                ExitState();/// A appeler quand on souhaite passer à Exit       
                            }
                            break;
                        case SubTaskState.Exit:                             /// L'état suivant ne doit être défini que dans le substate Exit
                            state = TaskBrasState.PrehensionGobeletDistributeurRelevage;
                            break;
                    }
                    break;
                case TaskBrasState.PrehensionGobeletDistributeurRelevage:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            isRunning = true;           //On a une action de task en cours
                            timestamp = DateTime.Now;
                            servoPositionsRequested = new Dictionary<ServoId, int>();
                            parent.OnPilotageTurbine((byte)_turbineID, 1500); //On allume la turbine a 50%
                            servoPositionsRequested.Add((ServoId)_servoID, (int)TaskBrasServoPositions.Init);
                            parent.OnHerkulexSetPosition(servoPositionsRequested);
                            break;
                        case SubTaskState.EnCours:
                            if (DateTime.Now.Subtract(timestamp).TotalMilliseconds > 200.0)
                            {
                                ExitState();/// A appeler quand on souhaite passer à Exit       
                            }
                            break;
                        case SubTaskState.Exit:                             /// L'état suivant ne doit être défini que dans le substate Exit
                            state = TaskBrasState.Idle;
                            break;
                    }
                    break;
                case TaskBrasState.StockageEnHauteur:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            timestamp = DateTime.Now;
                            servoPositionsRequested = new Dictionary<ServoId, int>();
                            servoPositionsRequested.Add((ServoId)_servoID, (int)TaskBrasServoPositions.Init);
                            parent.OnHerkulexSetPosition(servoPositionsRequested);
                            break;
                        case SubTaskState.EnCours:
                            if (DateTime.Now.Subtract(timestamp).TotalMilliseconds > 400.0)
                            {
                                ExitState();                                /// A appeler quand on souhaite passer à Exit         
                            }
                            break;
                        case SubTaskState.Exit:                             /// L'état suivant ne doit être défini que dans le substate Exit
                            parent.OnPilotageTurbine((byte)_turbineID, (ushort)(intensiteTurbineDeplacementDistributeur)); //On baisse la turbine a 35%
                            state = TaskBrasState.Idle;
                            //On a terminé l'action en cours, mais la task est toujours running tant que l'on a pas deposé le gobelet
                            break;
                    }
                    break;

                case TaskBrasState.BaisseBras:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            timestamp = DateTime.Now;
                            servoPositionsRequested = new Dictionary<ServoId, int>();
                            servoPositionsRequested.Add((ServoId)_servoID, (int)TaskBrasServoPositions.GobeletLibre);
                            parent.OnHerkulexSetPosition(servoPositionsRequested);
                            break;
                        case SubTaskState.EnCours:
                            if (DateTime.Now.Subtract(timestamp).TotalMilliseconds > 500.0)
                            {
                                //parent.OnPilotageTurbine((byte)_turbineID, 1000);   //On eteint la turbine
                                ExitState();                                /// A appeler quand on souhaite passer à Exit         
                            }
                            break;
                        case SubTaskState.Exit:                             /// L'état suivant ne doit être défini que dans le substate Exit
                            isFinished = true;//On a terminé l'action en cours,
                                              //On vient d'effectuer une depose, on repasse donc a l'init afin de remettre le bras en position initiale
                            state = TaskBrasState.Idle;
                            break;
                    }
                    break;


                case TaskBrasState.Depose:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            timestamp = DateTime.Now;
                            servoPositionsRequested = new Dictionary<ServoId, int>();
                            servoPositionsRequested.Add((ServoId)_servoID, (int)TaskBrasServoPositions.GobeletLibre);
                            parent.OnHerkulexSetPosition(servoPositionsRequested);
                            break;
                        case SubTaskState.EnCours:
                            if (DateTime.Now.Subtract(timestamp).TotalMilliseconds > 500.0)
                            {
                                parent.OnPilotageTurbine((byte)_turbineID, 1000);   //On eteint la turbine
                                ExitState();                                /// A appeler quand on souhaite passer à Exit         
                            }
                            break;
                        case SubTaskState.Exit:                             /// L'état suivant ne doit être défini que dans le substate Exit
                            servoPositionsRequested = new Dictionary<ServoId, int>();
                            servoPositionsRequested.Add((ServoId)_servoID, (int)TaskBrasServoPositions.Init);
                            parent.OnHerkulexSetPosition(servoPositionsRequested);
                            isFinished = true;//On a terminé l'action en cours,
                                              //On vient d'effectuer une depose, on repasse donc a l'init afin de remettre le bras en position initiale
                            state = TaskBrasState.Idle;
                            break;
                    }
                    break;
                default:
                    break;
            }

        }

    }
}
