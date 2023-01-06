using Common;
using FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorldServer.NetWork.Handler;
using WorldServer.Services.World;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using WorldServer.World.Scripting;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Scripting.Quests
{
    [GeneralScript(false, "", 32, 0)] // Grimmenhagen Farmer Male
    public class GrimmenhagenFarmerMale : AGeneralScript
    {
        /// <summary>
        /// On interract with Farmer
        /// </summary>
        /// <param name="Obj"></param>
        /// <param name="Target"></param>
        /// <param name="Menu"></param>
        public override void OnInteract(Object Creature, Player Target, InteractMenu Menu)
        {
            ushort questID = 30003;// Set Quest ID Here

            // Make sure the player has the quest and hasn't already finished the objectives.
            if (!Target.GetPlayer().QtsInterface.HasQuest(questID) || Target.GetPlayer().QtsInterface.HasFinishQuest(questID))
                return;

            // Spawn Marauder Sympathizer
            uint MarauderSympathizerMale = 31;
            Creature_proto MarauderSympathizer = CreatureService.GetCreatureProto(MarauderSympathizerMale);
            if (MarauderSympathizer == null)
                return;

            Creature.UpdateWorldPosition();
            Creature_spawn Spawn = new Creature_spawn(MarauderSympathizer,
                                                      (uint)CreatureService.GenerateCreatureSpawnGUID(),
                                                      Creature.Zone.ZoneId,
                                                      Creature.WorldPosition.X,
                                                      Creature.WorldPosition.Y,
                                                      Creature.WorldPosition.Z,
                                                      Creature.Heading);

            Creature cr = Creature.Region.CreateCreature(Spawn);
            cr.EvtInterface.AddEventNotify(EventName.OnDie, RemoveAdds);

            // Remove the old npc
            Creature.Destroy();
            return;
        }

        /// <summary>
        /// This remove New NPC's from game
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="instigator"></param>
        /// <returns></returns>
        public new bool RemoveAdds(Object npc = null, object instigator = null)
        {
            Creature creature = npc as Creature;
            creature.EvtInterface.AddEvent(creature.Destroy, 20000, 1);

            return false;
        }
    }

    [GeneralScript(false, "", 31, 0)] // Marauder Sympathizer Male
    public class MarauderSympathizerMale : AGeneralScript
    {
        public override void OnDie(Object Creature)
        {
            // Respawn the orginal npc
            uint GrimmenhagenFarmerMale = 32;
            Creature_proto GrimmenhagenFarmer = CreatureService.GetCreatureProto(GrimmenhagenFarmerMale);
            if (GrimmenhagenFarmer == null)
                return;

            Creature.UpdateWorldPosition();
            Creature_spawn Spawn = new Creature_spawn(GrimmenhagenFarmer,
                (uint)CreatureService.GenerateCreatureSpawnGUID(),
                Creature.Zone.ZoneId,
                Creature.WorldPosition.X, Creature.WorldPosition.Y,
                Creature.WorldPosition.Z, Creature.Heading);

            Creature cr = Creature.Region.CreateCreature(Spawn);

            //  Set the new NPC has Death State
            cr.Health = 0;
            cr.States.Add(3);
            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECT_DEATH);
            Out.WriteUInt16(cr.Oid);
            Out.WriteByte(1);
            Out.WriteByte(0);
            Out.WriteUInt16(0);
            Out.Fill(0, 6);
            cr.DispatchPacket(Out, true);
            //  Make it respawn
            cr.EvtInterface.AddEvent(cr.RezUnit, 30000 + cr.Level * 1000, 1); // Respawn in 30 seconds
            // Remove the old npc
            Creature.Destroy();
            return;
        }

        /// <summary>
        /// Remove spawned NPC's
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="instigator"></param>
        /// <returns></returns>
        public bool RemoveAdds(Object npc = null, object instigator = null)
        {
            Creature c = npc as Creature;
            c.EvtInterface.AddEvent(c.Destroy, 20000, 1);
            return false;
        }
    }

    [GeneralScript(false, "", 32002, 0)] // Grimmenhagen Farmer Female
    public class GrimmenhagenFarmerFemale : AGeneralScript
    {
        /// <summary>
        /// On interract with Farmer
        /// </summary>
        /// <param name="Obj"></param>
        /// <param name="Target"></param>
        /// <param name="Menu"></param>
        public override void OnInteract(Object Creature, Player Target, InteractMenu Menu)
        {
            ushort questID = 30003;// Set Quest ID Here

            // Make sure the player has the quest and hasn't already finished the objectives.
            if (!Target.GetPlayer().QtsInterface.HasQuest(questID) || Target.GetPlayer().QtsInterface.HasFinishQuest(questID))
                return;

            // Spawn Marauder Sympathizer
            uint MarauderSympathizerFemale = 31002;
            Creature_proto MarauderSympathizer = CreatureService.GetCreatureProto(MarauderSympathizerFemale);
            if (MarauderSympathizer == null)
                return;

            Creature.UpdateWorldPosition();
            Creature_spawn Spawn = new Creature_spawn(MarauderSympathizer,
                                                      (uint)CreatureService.GenerateCreatureSpawnGUID(),
                                                      Creature.Zone.ZoneId,
                                                      Creature.WorldPosition.X,
                                                      Creature.WorldPosition.Y,
                                                      Creature.WorldPosition.Z,
                                                      Creature.Heading);

            Creature cr = Creature.Region.CreateCreature(Spawn);
            cr.EvtInterface.AddEventNotify(EventName.OnDie, RemoveAdds);

            // Remove the old npc
            Creature.Destroy();
            return;
        }

        /// <summary>
        /// This remove New NPC's from game
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="instigator"></param>
        /// <returns></returns>
        public new bool RemoveAdds(Object npc = null, object instigator = null)
        {
            Creature creature = npc as Creature;
            creature.EvtInterface.AddEvent(creature.Destroy, 20000, 1);

            return false;
        }
    }

    [GeneralScript(false, "", 31002, 0)] // Marauder Sympathizer Female
    public class MarauderSympathizerFemale : AGeneralScript
    {
        public override void OnDie(Object Creature)
        {
            // Respawn the orginal npc
            uint GrimmenhagenFarmerFemale = 32002;
            Creature_proto GrimmenhagenFarmer = CreatureService.GetCreatureProto(GrimmenhagenFarmerFemale);
            if (GrimmenhagenFarmer == null)
                return;

            Creature.UpdateWorldPosition();
            Creature_spawn Spawn = new Creature_spawn(GrimmenhagenFarmer,
                (uint)CreatureService.GenerateCreatureSpawnGUID(),
                Creature.Zone.ZoneId,
                Creature.WorldPosition.X, Creature.WorldPosition.Y,
                Creature.WorldPosition.Z, Creature.Heading);

            Creature cr = Creature.Region.CreateCreature(Spawn);

            //  Set the new NPC has Death State
            cr.Health = 0;
            cr.States.Add(3);
            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECT_DEATH);
            Out.WriteUInt16(cr.Oid);
            Out.WriteByte(1);
            Out.WriteByte(0);
            Out.WriteUInt16(0);
            Out.Fill(0, 6);
            cr.DispatchPacket(Out, true);
            //  Make it respawn
            cr.EvtInterface.AddEvent(cr.RezUnit, 30000 + cr.Level * 1000, 1); // Respawn in 30 seconds
            // Remove the old npc
            Creature.Destroy();
            return;
        }

        /// <summary>
        /// Remove spawned NPC's
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="instigator"></param>
        /// <returns></returns>
        public bool RemoveAdds(Object npc = null, object instigator = null)
        {
            Creature c = npc as Creature;
            c.EvtInterface.AddEvent(c.Destroy, 20000, 1);
            return false;
        }
    }
}