using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Common;
using FrameWork;

namespace WorldServer
{
    [GeneralScript(true,"MailBoxScript", 0 , 0)]
    public class MailBoxScript : AGeneralScript
    {
        public override void OnInteract(Object Obj, Player Target, InteractMenu Menu)
        {
            Log.Debug("MailBox", "OnInteract " + Target);

            Target.MlInterface.SendMailBox();
        }
    }
}
