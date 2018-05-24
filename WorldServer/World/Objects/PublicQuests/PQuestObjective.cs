using Common;
using FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorldServer.Services.World;

namespace WorldServer.World.Objects.PublicQuests
{
    public class PQuestObjective
    {
        public PublicQuest Quest;
        public PQuest_Objective Objective;
        public List<PQuestCreature> ActiveCreatures = new List<PQuestCreature>();
        public List<PQuestGameObject> ActiveGameObjects = new List<PQuestGameObject>();
        public List<uint> ActiveInteractableGameObjects = new List<uint>();
        public uint ObjectiveID;
        public int _Count;

        public int Count
        {
            get
            {
                return _Count;
            }
            set
            {
                _Count = value;
            }
        }

        public bool IsDone()
        {
            if (Objective == null)
                return false;
            else
                return Count >= Objective.Count;
        }

        public void Reset()
        {

            if (Objective == null)
            {
                Log.Error("PQuestObjective.Reset", string.Concat(Quest.Name, " has no objective!"));
                return;
            }   

            if (Objective.Spawns == null)
            {
                Log.Error("PQuestObjective.Reset", string.Concat(Quest.Name, " - ", Objective.StageName, " has no spawn set"));
                return;
            }

            foreach (PQuest_Spawn spawn in Objective.Spawns)
            {
                if (spawn.Type == 1)
                {
                    Creature_proto proto = CreatureService.GetCreatureProto(spawn.Entry);
                    if (proto == null)
                    {
                        Log.Error("PQCreature", "No Proto");
                        continue;
                    }

                    if (Quest == null)
                    {
                        Log.Error("PQuestObjective", "Missing quest for: " + Objective.Objective);
                        continue;
                    }

                    Creature_spawn S = new Creature_spawn
                    {
                        Guid = (uint)CreatureService.GenerateCreatureSpawnGUID(),
                        WorldO = spawn.WorldO,
                        WorldY = spawn.WorldY,
                        WorldZ = spawn.WorldZ,
                        WorldX = spawn.WorldX,
                        ZoneId = spawn.ZoneId
                    };
                    S.BuildFromProto(proto);
                    //This sets the emotes for NPCs in PQ
                    S.Emote = spawn.Emote;
                    //This sets NPC level from puest_spawns table, we are not using creature_protos here
                    S.Level = spawn.Level;
                    
                    PQuestCreature newCreature = new PQuestCreature(S, this, Quest);
                    newCreature.PQSpawnId = spawn.pquest_spawns_ID.Replace(" ", "");

                    if (newCreature != null)
                    {
                        lock (ActiveCreatures)
                        {
                            ActiveCreatures.Add(newCreature);
                        }

                        Quest.Region.AddObject(newCreature, spawn.ZoneId);
                    }
                }
                if (spawn.Type == 2)
                {
                    GameObject_proto Proto = GameObjectService.GetGameObjectProto(spawn.Entry);
                    if (Proto == null)
                    {
                        Log.Error("PQGO", "No Proto");
                        return;
                    }

                    GameObject_spawn S = new GameObject_spawn();
                    S.Guid = (uint)GameObjectService.GenerateGameObjectSpawnGUID();
                    S.BuildFromProto(Proto);
                    S.WorldO = spawn.WorldO;
                    S.WorldY = spawn.WorldY;
                    S.WorldZ = spawn.WorldZ;
                    S.WorldX = spawn.WorldX;
                    S.ZoneId = spawn.ZoneId;
                    S.SoundId = spawn.SoundId;
                    S.VfxState = spawn.VfxState;
                    S.AllowVfxUpdate = spawn.AllowVfxUpdate;
                    S.Unks = spawn.Unks;
                    S.Unk3 = spawn.Unk3;


                    PQuestGameObject NewGo = new PQuestGameObject(S, this);
                    if (NewGo != null)
                    { 
                        lock (ActiveGameObjects)
                        { 
                            ActiveGameObjects.Add(NewGo);
                        }
                        Quest.Region.AddObject(NewGo, spawn.ZoneId);
                    }

                    // PQ Sound player - this will play sounds on PQ Stage, need to be setup in DB
                    if (spawn.Entry == 2000489 && Objective.SoundId != 0)
                    {
                        string text = "";

                        var prms = new List<object>() { NewGo, (ushort)Objective.SoundId, text };

                        for (int i = 0; i < Objective.SoundIteration; i++)
                        {
                            NewGo.EvtInterface.AddEvent(PlayPQSound, i * (int)Objective.SoundDelay * 1000 + 500, 1, prms);
                        }
                    }

                }
                if (spawn.Type == 3)
                {

                    return;
#warning this sucks cant get gos by spawn id

                    ActiveInteractableGameObjects.Add(spawn.Entry);



                }
            }
        }

        public void Cleanup()
        {
            lock (ActiveCreatures)
            {
                foreach (PQuestCreature Creature in ActiveCreatures)
                {
                    Creature.Spawn.NoRespawn = 1;
                    if (Creature.IsDead) // We are destroying only the living - dead will stay around for 2 minutes to allow players stuff looting
                        Creature.EvtInterface.AddEvent(DelayedCorpseRemoval, 59999, 1, Creature);
                    else
                        Creature.Destroy();
                }
                ActiveCreatures.Clear();
            }

            foreach (PQuestGameObject go in ActiveGameObjects)
            {
                go.Destroy();
            }

            ActiveGameObjects = new List<PQuestGameObject>();
        }

        public void DelayedCorpseRemoval(object creature)
        {
            Creature Creature = creature as Creature;
            if (Creature != null)
                Creature.Destroy();
        }

        // This plays sound in PQ
        public void PlayPQSound(object go)
        {
            var Params = (List<object>)go;
            PQuestGameObject GO = Params[0] as PQuestGameObject;

            if (go != null && GO != null)
                GO.PlaySound((ushort)Params[1]);
        }
    }
}
