using WorldServer.World.Interfaces;
using WorldServer.World.Positions;
using WorldServer.World.Scripting;

namespace WorldServer.World.Objects.Instances.Gunbad
{
    [GeneralScript(false, "", 38623, 0)]
    class FoulMoufdaUngry : BasicGunbad 
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;
            spawnWorldX = (int)Obj.WorldPosition.X;
            spawnWorldY = (int)Obj.WorldPosition.Y;
            spawnWorldZ = (int)Obj.WorldPosition.Z;
            spawnWorldO = (int)Obj.Heading;

            Obj.EvtInterface.AddEventNotify(EventName.OnEnterCombat, OnEnterCombat);
            Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, OnLeaveCombat);

            Obj.EvtInterface.AddEvent(ClearImmunities, 900, 0);

            Creature c = Obj as Creature;
            c.AddCrowdControlImmunity((int)GameData.CrowdControlTypes.All);
        }
    }

    [GeneralScript(false, "", 38633, 0)]
    class SlimespawnSquiglingsFoulMouf : BasicGunbad
    {
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            Obj.EvtInterface.AddEvent(SetRandomTarget, 200, 1);
        }
    }
}
