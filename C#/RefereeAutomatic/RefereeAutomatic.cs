using Constants;
using MessagesNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RefereeAutomaticNS
{
    public class RefereeAutomatic
    {
        Thread RefereeThread;
        public bool isRunning = false;
        public int taskPeriod = 20;
        bool exitRequested = false;

        RefBoxMessage lastSimulatorEvent;
        bool IsLastSimulatorEventProcessed;
        DateTime lastSimulatorEventTimeStamp;

        Queue<RefereeAction> refereeActionList = new Queue<RefereeAction>();


        public RefereeAutomatic()
        {
            InitThread();
            lastSimulatorEvent = null;
            lastSimulatorEventTimeStamp = DateTime.Now;
        }
        private void InitThread()
        {
            RefereeThread = new Thread(RefereeThreadProcess);
            RefereeThread.IsBackground = true;
            RefereeThread.Start();
        }
        public void RefereeThreadProcess()
        {
            while (true)
            {
                if(refereeActionList.Count>0)
                {
                    var nextAction = refereeActionList.Peek();
                    if (nextAction.time < DateTime.Now)
                    {
                        var nextEvent = refereeActionList.Dequeue();
                        ///On lance l'event referee
                        OnRefBoxCommand(nextEvent.msg);
                    }
                }
                Thread.Sleep(taskPeriod);
            }
        }


        /// Input events
        public void OnSimulatorEventReceived(object sender, RefBoxMessageArgs e)
        {         
            ///La réception d'un nouvel event clear la totalité des action en attente dans la liste des refereeActions
            refereeActionList.Clear();
            var lastSimulatorEvent = e.refBoxMsg;

            if ((lastSimulatorEvent.command == RefBoxCommand.CORNER) || (lastSimulatorEvent.command == RefBoxCommand.GOAL) ||
                (lastSimulatorEvent.command == RefBoxCommand.GOALKICK) || (lastSimulatorEvent.command == RefBoxCommand.THROWIN))
            {
                //switch (lastSimulatorEvent.command)
                //{
                //    case RefBoxCommand.THROWIN:
                ///On commence par un STOP
                var refAction = new RefereeAction();
                refAction.msg = new RefBoxMessage() { command = RefBoxCommand.STOP };
                refAction.time = DateTime.Now;
                refereeActionList.Enqueue(refAction);

                ///On continue avec l'action
                refAction = new RefereeAction();
                refAction.msg = e.refBoxMsg;
                refAction.time = DateTime.Now.AddSeconds(1);
                refereeActionList.Enqueue(refAction);

                ///On termine par un start
                refAction = new RefereeAction();
                refAction.msg = new RefBoxMessage() { command = RefBoxCommand.START };
                refAction.time = DateTime.Now.AddSeconds(5);
                refereeActionList.Enqueue(refAction);
                //        break;
                //}         
            }
        }

        /// Output events
        public event EventHandler<RefBoxMessageArgs> OnRefBoxCommandEvent;
        public virtual void OnRefBoxCommand(RefBoxMessage msg)
        {
            var handler = OnRefBoxCommandEvent;
            if (handler != null)
            {
                handler(this, new RefBoxMessageArgs { refBoxMsg = msg });
            }
        }
    }

    public class RefereeAction
    {
        public RefBoxMessage msg;
        public DateTime time;
    }
}
