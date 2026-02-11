using WorldServer.World.Objects.PublicQuests;
using Object = WorldServer.World.Objects.Object;

namespace WorldServer.World.Scripting
{
    public abstract class APublicQuestScript : AGeneralScript
    {
        public PublicQuest publicQuest;

        public virtual void OnStart()
        {
        }

        public virtual void OnEnd()
        {
        }

        public virtual void OnReset()
        {
        }

        public virtual void OnFail()
        {
        }

        public virtual void OnDie(Object owner, Object victim)
        {
        }

        public virtual void OnUse(Object owner, Object go)
        {
        }

        public virtual void OnStageChange(PQuestStage Stage)
        {
        }
    }
}
