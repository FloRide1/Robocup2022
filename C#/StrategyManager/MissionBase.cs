using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyManagerNS
{
    public abstract class MissionBase : TaskBase
    {
        int Timeout;

        public abstract override void Init();
        public abstract void MissionStateMachine();
        public override void TaskStateMachine()
        {
            MissionStateMachine();
        }

        //public abstract void Start();

        public MissionBase() : base()
        {
        }
        public MissionBase(StrategyGenerique p) : base(p)
        { }
    }
}
