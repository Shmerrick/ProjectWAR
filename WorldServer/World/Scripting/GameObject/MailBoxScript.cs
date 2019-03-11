using FrameWork;
using WorldServer.NetWork.Handler;
using WorldServer.World.Objects;
using Object = WorldServer.World.Objects.Object;

namespace WorldServer.World.Scripting.GameObject
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
