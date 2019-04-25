using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common;
using FrameWork;
using WorldServer.Services.World;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using WorldServer.World.Scripting;
using Object = WorldServer.World.Objects.Object;

namespace WorldServer
{
    // This assigns script AltdorfSewersWing3Boss to creature with ID 33401
    [GeneralScript(false, "", 33401, 0)]
    public class AltdorfSewersWing3Boss : AGeneralScript
    {
        private Object Obj; // This is creature 33401
        private int Stage = -1; // This is variable that controls combat Stage
        private int AddsSpawnTimer = 3000; // When `The Creator` summons his adds this variable is used - after this many milliseconds adds will be spawned
        List<Object> stuffInRange = new List<Object>(); // This list keeps all objects in range
        List<GameObject> magicWalls = new List<GameObject>(); // this list keeps all magic walls in range
        List<Creature> addList = new List<Creature>(); // this list keeps all adds spawned by `The Creator`

        // With this we can do some stuff when creature 33401 spawns
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
                this.Obj.Say("Hahaha " + pkilled.Name + " I killed you!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY); // Boss will say this
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
                    Stage = -1; // Stage -1 means `The Creator` is ready to roll again
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

        // When something gets in range (I think it is 350 or 400) we want 
        // to add it to correct lists and set some events
        public override void OnEnterRange(Object Obj, Object DistObj)
        {
            if (DistObj.IsPlayer())
            {
                //Obj.Say("YOU HAVE COME IN RANGE " + DistObj.Name);
                stuffInRange.Add(DistObj);

                // Just some small talk every 10s
                //Obj.EvtInterface.AddEvent(SayStuff, 10000, 0);

                // Checks HP of boss when receiving damage
                Obj.EvtInterface.AddEventNotify(EventName.OnReceiveDamage, CheckHP);

                // Bragging about killing player when players die
                DistObj.EvtInterface.AddEventNotify(EventName.OnDie, OnKilledPlayer);

                // Spawns magic wall to make everybody fight in his labolatory

                // Despawns adds and maigc walls on boss death
                Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveAllAdds);
                Obj.EvtInterface.AddEventNotify(EventName.OnDie, RemoveWall);
            }

            // We are adding the magic walls we are spawning below to the list to quickly dispose them when needed
            else if (DistObj.IsGameObject())
            {
                GameObject GO = DistObj as GameObject;
                if (GO.Entry == 2000441)
                {
                    // Despawns wall when boss dies
                    magicWalls.Add(GO);
                }
            }
        }
        // Words for final stage
        public void FinalWords()
        {
            Obj.Say("Noo! You should work!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
            Obj.Say("Nevermind, I will destroy you myself!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
        }

        // Possible banter here
        public void SayStuff()
        {
            Obj.Say("Noo! You should work!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
            Obj.Say("Nevermind, I will destroy you myself!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
        }

        //This checks the current HP of boss
        public bool CheckHP(Object Obj, object instigator)
        {
            Creature c = this.Obj as Creature; // We are casting the script initiator as a Creature

            if (Stage < 0 && !c.IsDead)
            {
                AddWall(); // First time he is damaged he spawns a wall to block exit
                Stage = 0; // Setting control value to 0
                c.Say("Fools! How dare you disturb my experiments!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY); // Banter
            }

            if (c.Health < c.TotalHealth * 0.2 && Stage < 4 && !c.IsDead) // At 20% HP he fails to summon anything
            {
                c.IsImmovable = true; // Boss immovable when walking to table
                c.IsInvulnerable = true; // Boss invulnerable when walking to table
                c.Say("Watch this!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);

                c.MvtInterface.Move(38761, 33364, 11103); // Move to some coordinates

                c.EvtInterface.AddEvent(FinalWords, 5000, 1); // Banter
                c.EvtInterface.AddEvent(RemoveBuffs, 5000, 1); // We are removing is immovability and invulnerability

                Stage = 4;
            }
            else if (c.Health < c.TotalHealth * 0.4 && Stage < 3 && !c.IsDead)
            {
                c.IsImmovable = true;
                c.IsInvulnerable = true;
                c.Say("This isn't even my final form yet!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);

                c.MvtInterface.Move(38761, 33364, 11103);

                c.EvtInterface.AddEvent(SpawnMaggots, AddsSpawnTimer, 1); // We are spawnning some adds
                c.EvtInterface.AddEvent(RemoveBuffs, 5000, 1);

                Stage = 3;
            }
            else if (c.Health < c.TotalHealth * 0.6 && Stage < 2 && !c.IsDead)
            {
                c.IsImmovable = true;
                c.IsInvulnerable = true;
                c.Say("Tremble at my illogical glory!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);

                c.MvtInterface.Move(38761, 33364, 11103);

                c.EvtInterface.AddEvent(SpawnSpiders, AddsSpawnTimer, 1);
                c.EvtInterface.AddEvent(RemoveBuffs, 5000, 1);

                Stage = 2;
            }
            else if (c.Health < c.TotalHealth * 0.8 && Stage < 1 && !c.IsDead)
            {
                c.IsImmovable = true;
                c.IsInvulnerable = true;
                c.Say("You are just a nurgling bait!", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);

                c.MvtInterface.Move(38761, 33364, 11103);

                c.EvtInterface.AddEvent(SpawnNurglings, AddsSpawnTimer, 1);
                c.EvtInterface.AddEvent(RemoveBuffs, 5000, 1);

                Stage = 1;
            }

            return false;
        }

        // Removal of immovability and invulnerability
        public void RemoveBuffs()
        {
            Creature c = this.Obj as Creature;
            //c.IsImmovable = false;
            c.IsInvulnerable = false;
        }

        // Removal of magic wall
        public bool RemoveWall(Object GO = null, object instigator = null)
        {
            //2000441;

            foreach (GameObject wall in magicWalls)
            {
                wall.Destroy();
            }

            return false;
        }

        // Make RoR Great Again - adding wall
        public void AddWall()
        {
            GameObject_proto proto = GameObjectService.GetGameObjectProto(2000441);

            GameObject_spawn spawn = new GameObject_spawn
            {
                Guid = (uint)GameObjectService.GenerateGameObjectSpawnGUID(),
                WorldX = 137348,
                WorldY = 524446,
                WorldZ = 11128,
                WorldO = 2008,
                ZoneId = 169
            };

            spawn.BuildFromProto(proto);
            GameObject GO = Obj.Region.CreateGameObject(spawn);
            GO.Say("**Wall of magical force is now blocking the exit**", SystemData.ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
            magicWalls.Add(GO);
        }

        public void SpawnNurglings() // Spawning nurgling adds
        {
            //Creature_proto Proto = CreatureService.GetCreatureProto((uint)6409);
            Creature_proto Proto = CreatureService.GetCreatureProto((uint)1002073);

            Random rand = new Random();

            for (int i = 0; i < 6; i++) // We want 6 nurglings
            {
                Creature_spawn Spawn = new Creature_spawn();
                Spawn.Guid = (uint)CreatureService.GenerateCreatureSpawnGUID();
                Spawn.BuildFromProto(Proto);
                Spawn.WorldO = Obj.Heading;
                Spawn.WorldX = (int)(137065 + 150 - 300 * rand.NextDouble());
                Spawn.WorldY = (int)(524884 + 150 - 300 * rand.NextDouble());
                Spawn.WorldZ = 11103;
                Spawn.ZoneId = 169;
                Spawn.Level = 1;
                Creature c = Obj.Region.CreateCreature(Spawn);
                c.EvtInterface.AddEventNotify(EventName.OnDie, RemoveAdds); // We are removing spawns from server when adds die
                addList.Add(c); // Adding adds to the list for easy removal
            }
        }

        public void SpawnSpiders()
        {
            //Creature_proto Proto = CreatureService.GetCreatureProto((uint)3903);
            Creature_proto Proto = CreatureService.GetCreatureProto((uint)1002072);

            Random rand = new Random();

            for (int i = 0; i < 6; i++)
            {
                Creature_spawn Spawn = new Creature_spawn();
                Spawn.Guid = (uint)CreatureService.GenerateCreatureSpawnGUID();
                Spawn.BuildFromProto(Proto);
                Spawn.WorldO = Obj.Heading;
                Spawn.WorldX = (int)(137065 + 150 - 300 * rand.NextDouble());
                Spawn.WorldY = (int)(524884 + 150 - 300 * rand.NextDouble());
                Spawn.WorldZ = 11103;
                Spawn.ZoneId = 169;
                Spawn.Level = 1;
                Creature c = Obj.Region.CreateCreature(Spawn);
                c.EvtInterface.AddEventNotify(EventName.OnDie, RemoveAdds);
                addList.Add(c);
            }
        }

        public void SpawnMaggots()
        {
            //Creature_proto Proto = CreatureService.GetCreatureProto((uint)38236);
            Creature_proto Proto = CreatureService.GetCreatureProto((uint)1002074);

            Random rand = new Random();

            for (int i = 0; i < 6; i++)
            {
                Creature_spawn Spawn = new Creature_spawn();
                Spawn.Guid = (uint)CreatureService.GenerateCreatureSpawnGUID();
                Spawn.BuildFromProto(Proto);
                Spawn.WorldO = Obj.Heading;
                Spawn.WorldX = (int)(137065 + 150 - 300 * rand.NextDouble());
                Spawn.WorldY = (int)(524884 + 150 - 300 * rand.NextDouble());
                Spawn.WorldZ = 11103;
                Spawn.ZoneId = 169;
                Spawn.Level = 1;
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