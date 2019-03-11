namespace WorldServer.World.Scripting.PublicQuests.Destro
{
    class DeathstoneQuarry : BasicPublicQuest
    {

    }

    [GeneralScript(false, "", 1000069, 0)]
    class ArkusTheChanger : DeathstoneQuarry
    {
        /*public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            spawnPoint = Obj as Point3D;

            Obj.EvtInterface.AddEventNotify(EventName.OnEnterCombat, OnEnterCombat);
            Obj.EvtInterface.AddEventNotify(EventName.OnLeaveCombat, OnLeaveCombat);
            Obj.EvtInterface.AddEventNotify(EventName.OnReceiveDamage, CheckHP);
        }

        public bool CheckHP(Object Obj, object instigator)
        {
            Creature c = this.Obj as Creature; // We are casting the script initiator as a Creature

            if (Stage < 0 && !c.IsDead)
            {
                Stage = 0; // Setting control value to 0
            }
            else if (c.Health < c.TotalHealth * 0.06 && Stage < 1 && !c.IsDead)
            {
                var prms = new List<object>() { 2000561, 1109983, 1119476, 19138, (int)Obj.Heading }; // Deathshadow Drudge
                c.EvtInterface.AddEvent(SpawnAdds, 100, 1, prms);

                c.Say("Feel my wrath mortal!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);

                c.Destroy();
                //c.IsInvulnerable = true;

                //c.EvtInterface.AddEvent(RemoveBuffs, 30000, 1);

                Stage = 1;
            }

            return false;
        }

        public void SpawnLordOfChange()
        {
            int X = Obj.WorldPosition.X;
            int Y = Obj.WorldPosition.Y;
            int Z = Obj.WorldPosition.Z; ;
            ushort O = Obj.Heading;

            Creature_proto Proto = CreatureService.GetCreatureProto(14872);

            Creature_spawn Spawn = new Creature_spawn();
            Spawn.Guid = (uint)CreatureService.GenerateCreatureSpawnGUID();
            Spawn.BuildFromProto(Proto);
            Spawn.WorldO = (int)O;
            Spawn.WorldX = X;
            Spawn.WorldY = Y;
            Spawn.WorldZ = Z;
            Spawn.ZoneId = (ushort)Obj.ZoneId;
            Spawn.Faction = 135;
            Spawn.Level = 10;

            Creature c = Obj.Region.CreateCreature(Spawn);

            RegionMgr region = WorldMgr.GetRegion(Obj.Region.RegionId, false);

            //PublicQuest PQ =

            //PQuestCreature LordOfChange = new PQuestCreature(Spawn, region.PublicQuests[201]);

            c.EvtInterface.AddEventNotify(EventName.OnDie, RemoveAdds);
        }*/

    }
}
