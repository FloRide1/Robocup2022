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
    public class Eurobot2021TaskBrasDeclencheur : TaskBase
    {
        DateTime timestamp;
        TaskBrasState state = TaskBrasState.Idle;
        ServoId _servoID;

        public Eurobot2021TaskBrasDeclencheur() : base()
        {

        }

        public Eurobot2021TaskBrasDeclencheur(StrategyGenerique sg, ServoId servoID) : base(sg)
        {
            parent = sg;
            _servoID = servoID;
        }

        enum TaskBrasState
        {
            Init,
            Idle,
            Releve,
            MancheAir,
            Phare,
        }

        enum TaskBrasServoPositions
        {
            Init = 512,
            MancheAir = 840,
            Phare = 720,
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

        public void StartPositionnementMancheAir()
        {
            state = TaskBrasState.MancheAir;
            ResetSubState();
            isFinished = false;
        }
        public void StartPositionnementPhare()
        {
            state = TaskBrasState.Phare;
            ResetSubState();
            isFinished = false;
        }
        public void StartRemonteeBras()
        {
            state = TaskBrasState.Releve;
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
                            //Console.WriteLine("Init Task Bras Declencheur");
                            timestamp = DateTime.Now;
                            servoPositionsRequested = new Dictionary<ServoId, int>();
                            servoPositionsRequested.Add((ServoId)_servoID, (int)TaskBrasServoPositions.Init);
                            parent.OnHerkulexSetPosition(servoPositionsRequested);
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
                case TaskBrasState.MancheAir:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            isRunning = true;           //On a une action de task en cours
                            timestamp = DateTime.Now;
                            /// On descend le servo
                            servoPositionsRequested = new Dictionary<ServoId, int>();
                            servoPositionsRequested.Add((ServoId)_servoID, (int)TaskBrasServoPositions.MancheAir);
                            parent.OnHerkulexSetPosition(servoPositionsRequested);
                            break;
                        case SubTaskState.EnCours:
                            if (DateTime.Now.Subtract(timestamp).TotalMilliseconds > 200.0)
                            {
                                ExitState();/// A appeler quand on souhaite passer à Exit       
                            }
                            break;
                        case SubTaskState.Exit:
                            /// L'état suivant ne doit être défini que dans le substate Exit
                            state = TaskBrasState.Idle;
                            break;
                    }
                    break;
                case TaskBrasState.Phare:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            isRunning = true;           //On a une action de task en cours
                            timestamp = DateTime.Now;
                            /// On descend le servo
                            servoPositionsRequested = new Dictionary<ServoId, int>();
                            servoPositionsRequested.Add((ServoId)_servoID, (int)TaskBrasServoPositions.Phare);
                            parent.OnHerkulexSetPosition(servoPositionsRequested);
                            break;
                        case SubTaskState.EnCours:
                            if (DateTime.Now.Subtract(timestamp).TotalMilliseconds > 200.0)
                            {
                                ExitState();/// A appeler quand on souhaite passer à Exit       
                            }
                            break;
                        case SubTaskState.Exit:
                            /// L'état suivant ne doit être défini que dans le substate Exit
                            state = TaskBrasState.Idle;
                            break;
                    }
                    break;
                case TaskBrasState.Releve:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            isRunning = true;           //On a une action de task en cours
                            timestamp = DateTime.Now;
                            /// On descend le servo
                            servoPositionsRequested = new Dictionary<ServoId, int>();
                            servoPositionsRequested.Add((ServoId)_servoID, (int)TaskBrasServoPositions.Init);
                            parent.OnHerkulexSetPosition(servoPositionsRequested);
                            break;
                        case SubTaskState.EnCours:
                            if (DateTime.Now.Subtract(timestamp).TotalMilliseconds > 200.0)
                            {
                                ExitState();/// A appeler quand on souhaite passer à Exit       
                            }
                            break;
                        case SubTaskState.Exit:
                            /// L'état suivant ne doit être défini que dans le substate Exit
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
