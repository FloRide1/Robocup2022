using System.Threading;

namespace StrategyManagerNS
{
    public abstract class TaskBase
    {
        Thread TaskThread;       

        public bool isRunning = false;
        public int taskPeriod = 20;
        bool exitRequested = false;
        public StrategyGenerique parent;

        ///Définition du subState de la tâche permettant d'avoir trois états 
        private SubTaskState _subState;
        public SubTaskState subState
        {
            get { return _subState; }
            private set { _subState = value; }
        }

        public TaskBase()
        {
            InitTaskThread();
        }
        public TaskBase(StrategyGenerique p)
        {
            parent = p;
            InitTaskThread();
        }

        public abstract void Init();

        /// <summary>
        /// Fonction permettant de passer dans le subState Exit de sortie d'une 
        /// </summary>
        public void ExitState()
        {
            exitRequested = true;
        }
        public bool isFinished = false;

        private void InitTaskThread()
        {
            TaskThread = new Thread(TaskThreadProcess);
            TaskThread.IsBackground = true;
            TaskThread.Start();
        }

        public void TaskThreadProcess()
        {
            while (true)
            {
                TaskStateMachine();
                TaskSubStateManager();
                Thread.Sleep(taskPeriod);
            }
        }

        public abstract void TaskStateMachine();

        private void TaskSubStateManager()
        {
            /***************** NE PAS MODIFIER *********************/

            if (subState == SubTaskState.Entry) //Assure qu'on ne reste qu'une itération en Entry
                subState = SubTaskState.EnCours;
            else if (subState == SubTaskState.Exit) //Assure qu'on ne reste qu'une itération en Exit
                subState = SubTaskState.Entry;
            if (exitRequested) //A faire absolument après les lignes précédentes
            {
                exitRequested = false;
                subState = SubTaskState.Exit;
            }
            /***************** FIN DU NE PAS MODIFIER ***************/
        }


        public void ResetSubState()
        {
            subState = SubTaskState.Entry;
        }
    }


    public enum SubTaskState
    {
        Entry,
        EnCours,
        Exit,
    }
}
