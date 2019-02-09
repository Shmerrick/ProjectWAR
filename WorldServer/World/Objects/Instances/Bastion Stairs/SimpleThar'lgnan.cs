using System;
using System.Collections.Generic;
using Common;
using WorldServer.Services.World;

namespace WorldServer
{
    [GeneralScript(false, "", 45084, 0)]
    public class SimpleTharlgnan : AGeneralScript
    {
        private Object Obj; // This is creature 45084
        private int Stage = -1; // This is variable that controls combat Stage
        private int AddsSpawnTimer = 3000; // this variable is used - after this many milliseconds adds will be spawned
        List<Object> stuffInRange = new List<Object>(); // This list keeps all objects in range
        List<GameObject> magicWalls = new List<GameObject>(); // this list keeps all magic walls in range
        List<Creature> addList = new List<Creature>(); // this list keeps all adds spawned by `The Creator`

        // With this we can do some stuff when creature 45084 spawns
        public override void OnObjectLoad(Object Obj)
        {
            this.Obj = Obj;
            //Obj.EvtInterface.AddEvent(CheckHP, 1000, 0);
        }

        // When Boss kills player this event is run
        public bool OnKilledPlayer(Object pkilled, object instigator)
        {
            Player plr = pkilled as Player; // Casting object pkilled to Player type
            if (plr.IsPlayer())
            {
                this.Obj.Say("Filthy Scum " + pkilled.Name + " Blood for the Blood God!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY); // Boss will say this
            }

            // Below we check if the killed player was the last alive player in the instance - we check this for group and for individual players
            if (plr?.PriorityGroup != null)
            {
                Group currentGroup = plr.PriorityGroup;

                int groupCount = currentGroup.MemberCount;
                int deadMembers = 0;
                for (int i = 0; i < groupCount; i++)
                {
                    if (plr.IsDead) deadMembers++;
                }
                // If all party members died we are removing magic walls and all spawned adds
                if (deadMembers == groupCount)
                {
                    Stage = -1; // Stage -1 means `The Mob` is ready to roll again
                    RemoveWall();
                    RemoveAllAdds();
                }
            }
            else
            {
                Stage = -1;
                RemoveWall();
                RemoveAllAdds();
            }
            return false;
        }

        public bool OnDmg(Object pkilled, object instigator)
        {
            return false;
        }

        public override void OnEnterRange(Object Obj, Object DistObj)
        {
            if (DistObj.IsPlayer())
            {
                stuffInRange.Add(DistObj);
                Obj.EvtInterface.AddEventNotify(EventName.OnReceiveDamage, CheckHP);
                DistObj.EvtInterface.AddEventNotify(EventName.OnDie, OnKilledPlayer);
                Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveAllAdds);
                Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveWall);
            }
            else if (DistObj.IsGameObject())
            {
                GameObject GO = DistObj as GameObject;
                if (GO.Entry == 2000441)
                {
                    magicWalls.Add(GO);
                }
            }
        }
        public void FinalWords()
        {
            Obj.Say("Blood will Flow!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
            Obj.Say("Blood for the Blood God!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
        }
        public void SayStuff()
        {
            Obj.Say("Blood will Flow!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
            Obj.Say("Blood for the Blood God!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
        }
        public bool CheckHP(Object Obj, object instigator)
        {
            Creature c = this.Obj as Creature;
            if (Stage < 0 && !c.IsDead)
            {
                AddWall();
                Stage = 0;
                c.Say("Fools! How dare you disturb the Bloodherd!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY); // Banter
            }
            if (c.Health < c.TotalHealth * 0.2 && Stage < 4 && !c.IsDead)
            {
                c.IsImmovable = true;
                c.IsInvulnerable = true;
                c.Say("Watch this!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
                c.EvtInterface.AddEvent(FinalWords, 5000, 1);
                c.EvtInterface.AddEvent(RemoveBuffs, 5000, 1); // We are removing is immovability and invulnerability
                Stage = 4;
            }
            else if (c.Health < c.TotalHealth * 0.4 && Stage < 3 && !c.IsDead)
            {
                c.IsImmovable = true;
                c.IsInvulnerable = true;
                c.Say("You have not seen the last of the Bloodherd!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
                c.EvtInterface.AddEvent(SpawnTharlgnanGor, AddsSpawnTimer, 1); // We are spawnning some adds
                c.EvtInterface.AddEvent(RemoveBuffs, 5000, 1);
                Stage = 3;
            }
            else if (c.Health < c.TotalHealth * 0.6 && Stage < 2 && !c.IsDead)
            {
                c.IsImmovable = true;
                c.IsInvulnerable = true;
                c.Say("Tremble at the Bloodherd!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
                c.EvtInterface.AddEvent(SpawnTharlgnanGuardian, AddsSpawnTimer, 1);
                c.EvtInterface.AddEvent(RemoveBuffs, 5000, 1);
                Stage = 2;
            }
            else if (c.Health < c.TotalHealth * 0.8 && Stage < 1 && !c.IsDead)
            {
                c.IsImmovable = true;
                c.IsInvulnerable = true;
                c.Say("You are just a frenzied bait!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
                c.EvtInterface.AddEvent(SpawnTharlgnanbloodsnout, AddsSpawnTimer, 1);
                c.EvtInterface.AddEvent(RemoveBuffs, 5000, 1);
                Stage = 1;
            }
            return false;
        }

        public void RemoveBuffs()
        {
            Creature c = this.Obj as Creature;
            //c.IsImmovable = false;
            c.IsInvulnerable = false;
        }

        // Removal of magic wall
        public bool RemoveWall(Object GO = null, object instigator = null)
        {
            foreach (GameObject wall in magicWalls)
            {
                wall.Destroy();
            }
            return false;
        }

        public void AddWall()
        {
            GameObject_proto proto = GameObjectService.GetGameObjectProto(2000441);
            GameObject_spawn spawn = new GameObject_spawn
            {
                Guid = (uint)GameObjectService.GenerateGameObjectSpawnGUID(),
                WorldX = 998950,
                WorldY = 988894,
                WorldZ = 8894,
                WorldO = 2046,
                ZoneId = 163
            };

            spawn.BuildFromProto(proto);
            GameObject GO = Obj.Region.CreateGameObject(spawn);
            GO.Say("**Wall of magical force is now blocking the exit**", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
            magicWalls.Add(GO);
        }

        public void SpawnTharlgnanbloodsnout() // Spawning adds
        {
            Creature_proto Proto = CreatureService.GetCreatureProto((uint)1630999);
            Random rand = new Random();
            for (int i = 0; i < 6; i++)
            {
                Creature_spawn Spawn = new Creature_spawn();
                Spawn.Guid = (uint)CreatureService.GenerateCreatureSpawnGUID();
                Spawn.BuildFromProto(Proto);
                Spawn.WorldO = Obj.Heading;
                Spawn.WorldX = (int)(999112 + 150 - 300 * rand.NextDouble());
                Spawn.WorldY = (int)(984749 + 150 - 300 * rand.NextDouble());
                Spawn.WorldZ = 9022;
                Spawn.ZoneId = 163;
                Spawn.Level = 33;
                Creature c = Obj.Region.CreateCreature(Spawn);
                c.EvtInterface.AddEventNotify(EventName.OnDie, RemoveAdds); // We are removing spawns from server when adds die
                addList.Add(c); // Adding adds to the list for easy removal
            }
        }

        public void SpawnTharlgnanGuardian()
        {
            Creature_proto Proto = CreatureService.GetCreatureProto((uint)1631999);
            Random rand = new Random();
            for (int i = 0; i < 6; i++)
            {
                Creature_spawn Spawn = new Creature_spawn();
                Spawn.Guid = (uint)CreatureService.GenerateCreatureSpawnGUID();
                Spawn.BuildFromProto(Proto);
                Spawn.WorldO = Obj.Heading;
                Spawn.WorldX = (int)(999112 + 150 - 300 * rand.NextDouble());
                Spawn.WorldY = (int)(984749 + 150 - 300 * rand.NextDouble());
                Spawn.WorldZ = 8969;
                Spawn.ZoneId = 163;
                Spawn.Level = 33;
                Creature c = Obj.Region.CreateCreature(Spawn);
                c.EvtInterface.AddEventNotify(EventName.OnDie, RemoveAdds);
                addList.Add(c);
            }
        }
        
        public void SpawnTharlgnanGor()
        {
            Creature_proto Proto = CreatureService.GetCreatureProto((uint)1632999);
            Random rand = new Random();
            for (int i = 0; i < 6; i++)
            {
                Creature_spawn Spawn = new Creature_spawn();
                Spawn.Guid = (uint)CreatureService.GenerateCreatureSpawnGUID();
                Spawn.BuildFromProto(Proto);
                Spawn.WorldO = Obj.Heading;
                Spawn.WorldX = (int)(999112 + 150 - 300 * rand.NextDouble());
                Spawn.WorldY = (int)(984749 + 150 - 300 * rand.NextDouble());
                Spawn.WorldZ = 9023;
                Spawn.ZoneId = 163;
                Spawn.Level = 33;
                Creature c = Obj.Region.CreateCreature(Spawn);
                c.EvtInterface.AddEventNotify(EventName.OnDie, RemoveAdds);
                addList.Add(c);
            }
        }

        // This remove adds from game
        public bool RemoveAdds(Object npc = null, object instigator = null)
        {
            Creature c = npc as Creature;
            c.EvtInterface.AddEvent(c.Destroy, 20000, 1);
            return false;
        }

        // This remove all adds from game
        public bool RemoveAllAdds(Object npc = null, object instigator = null)
        {
            foreach (Creature add in addList)
            {
                add.Health = 0;
                add.EvtInterface.AddEvent(add.Destroy, 20000, 1);
            }
            return false;
        }
    }
}