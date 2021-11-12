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
    public class Eurobot2022TaskAffichageLidar : TaskBase
    {
        DateTime timestamp;
        TaskAffichageLidarState state = TaskAffichageLidarState.Idle;

        string textPermanentLigne1 = "";
        string textPermanentLigne2 = "";
        string textTemporiseLigne1 = "";
        string textTemporiseLigne2 = "";

        int temporisationLigne1 = 1000;
        int temporisationLigne2 = 1000;

        public Eurobot2022TaskAffichageLidar() : base()
        {

        }

        public Eurobot2022TaskAffichageLidar(StrategyGenerique sg) : base(sg)
        {
            parent = sg;
        }

        enum TaskAffichageLidarState
        {
            Init,
            Idle,
            AffichageTemporiseLigne1,
            AffichageTemporiseLigne2,
            AffichagePermanentLigne1,
            AffichagePermanentLigne2,
        }

        public override void Init()
        {
            if (state != TaskAffichageLidarState.Init)
            {
                state = TaskAffichageLidarState.Init;
                ResetSubState();
                isFinished = false;
            }
        }

        public void StartAffichageTempoLigne1(string displayString, int duree)
        {
            textTemporiseLigne1 = displayString;
            state = TaskAffichageLidarState.AffichageTemporiseLigne1;
            ResetSubState();
            isFinished = false;
        }
        public void StartAffichageTempoLigne2(string displayString, int duree)
        {
            textTemporiseLigne2 = displayString;
            state = TaskAffichageLidarState.AffichageTemporiseLigne1;
            ResetSubState();
            isFinished = false;
        }

        public void StartAffichagePermanentLigne1(string displayString)
        {
            textPermanentLigne1 = displayString;
            parent.OnLidarMessage(textPermanentLigne1, 1);
        }

        public void StartAffichagePermanentLigne2(string displayString)
        {
            textPermanentLigne2 = displayString;
            parent.OnLidarMessage(textPermanentLigne2, 2);
        }

        public override void TaskStateMachine()
        {
            switch (state)
            {
                case TaskAffichageLidarState.Init:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            //Console.WriteLine("Init Task Affichage Lidar");
                            timestamp = DateTime.Now;
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
                            state = TaskAffichageLidarState.Idle;
                            break;
                    }
                    break;

                case TaskAffichageLidarState.Idle:
                    {
                        /// On ne sort pas de cet état sans un forçage extérieur vers un autre état
                        /// On maintient l'état du bras
                        isFinished = true; /// Pas d'action en cours
                    }
                    break;

                case TaskAffichageLidarState.AffichageTemporiseLigne1:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            isRunning = true;           //On a une action de task en cours
                            timestamp = DateTime.Now;
                            parent.OnLidarMessage(textTemporiseLigne1, 1);
                            break;
                        case SubTaskState.EnCours:
                            if (DateTime.Now.Subtract(timestamp).TotalMilliseconds > temporisationLigne1)
                            {
                                ExitState();/// A appeler quand on souhaite passer à Exit       
                            }
                            break;
                        case SubTaskState.Exit:                             /// L'état suivant ne doit être défini que dans le substate Exit      
                            parent.OnLidarMessage(textPermanentLigne1, 1);
                            state = TaskAffichageLidarState.Idle;
                            break;
                    }
                    break;

                case TaskAffichageLidarState.AffichageTemporiseLigne2:
                    switch (subState)
                    {
                        case SubTaskState.Entry:
                            isRunning = true;           //On a une action de task en cours
                            timestamp = DateTime.Now;
                            parent.OnLidarMessage(textTemporiseLigne2, 2);
                            break;
                        case SubTaskState.EnCours:
                            if (DateTime.Now.Subtract(timestamp).TotalMilliseconds > temporisationLigne1)
                            {
                                ExitState();/// A appeler quand on souhaite passer à Exit       
                            }
                            break;
                        case SubTaskState.Exit:                             /// L'état suivant ne doit être défini que dans le substate Exit      
                            parent.OnLidarMessage(textPermanentLigne2, 2);
                            state = TaskAffichageLidarState.Idle;
                            break;
                    }
                    break;

                default:
                    break;
            }
        }
    }
}
