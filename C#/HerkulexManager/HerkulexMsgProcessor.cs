using EventArgsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HerkulexManagerNS.HerkulexDescription;

namespace HerkulexManagerNS
{
    public class HerkulexMsgProcessor
    {
        public HerkulexMsgProcessor()
        {

        }

        //Input CallBack        
        public void ProcessHerkulexDecodedMessage(object sender, HerkulexEventArgs.Hklx_RAM_READ_Ack_Args e)
        {
            ProcessDecodedMessage(e);
        }

        //Processeur de message en provenance du robot...
        //Une fois processé, le message sera transformé en event sortant
        public void ProcessDecodedMessage(HerkulexEventArgs.Hklx_RAM_READ_Ack_Args decodedMsg)
        {
            byte[] tab;
            uint timeStamp;
            switch (decodedMsg.Address)
            {
                //Torque RAW Data
                case (byte)RAM_ADDR.PWM:
                    {
                        ushort torque = (ushort)((ushort)(decodedMsg.ReceivedData[1] << 8) | decodedMsg.ReceivedData[0]);
                        OnTorqueInfoFromHerkulex((byte)decodedMsg.PID, torque);
                    }
                    break;
            }
        }


        //Output events
        public event EventHandler<TorqueEventArgs> OnTorqueFromHerkulexGeneratedEvent;
        public virtual void OnTorqueInfoFromHerkulex(byte servoID, ushort torque)
        {
            var handler = OnTorqueFromHerkulexGeneratedEvent;
            if (handler != null)
            {
                handler(this, new TorqueEventArgs
                {
                    servoID=servoID, Value=torque
                });
            }
        }
    }
}
