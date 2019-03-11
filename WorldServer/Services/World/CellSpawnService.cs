using Common;
using Common.Database.World.Maps;
using FrameWork;
using System;
using System.Collections.Generic;
using WorldServer.World.Map;

namespace WorldServer.Services.World
{
    [Service(typeof(CreatureService), typeof(GameObjectService), typeof(ZoneService))]
    class CellSpawnService : ServiceBase
    {
        public static Dictionary<ushort, CellSpawns[,]> _RegionCells = new Dictionary<ushort, CellSpawns[,]>();

        public static CellSpawns GetRegionCell(ushort RegionId, ushort X, ushort Y)
        {
            X = (ushort)Math.Min(RegionMgr.MaxCellID - 1, X);
            Y = (ushort)Math.Min(RegionMgr.MaxCellID - 1, Y);

            if (!_RegionCells.ContainsKey(RegionId))
                _RegionCells.Add(RegionId, new CellSpawns[RegionMgr.MaxCellID, RegionMgr.MaxCellID]);

            if (_RegionCells[RegionId][X, Y] == null)
            {
                CellSpawns Sp = new CellSpawns(RegionId, X, Y);
                _RegionCells[RegionId][X, Y] = Sp;
            }

            return _RegionCells[RegionId][X, Y];
        }
        public static CellSpawns[,] GetCells(ushort RegionId)
        {
            if (!_RegionCells.ContainsKey(RegionId))
                _RegionCells.Add(RegionId, new CellSpawns[RegionMgr.MaxCellID, RegionMgr.MaxCellID]);

            return _RegionCells[RegionId];
        }

        [LoadingFunction(true)]
        public static void LoadCellSpawns()
        {
            LoadRegionSpawns();

            if (Program.Config.CleanSpawns)
                RemoveDoubleSpawns();
        }
        
        public static void LoadRegionSpawns()
        {
            long InvalidSpawns = 0;
            Zone_Info Info = null;
            ushort X, Y = 0;
            Dictionary<string, int> RegionCount = new Dictionary<string, int>();

            {
                Creature_spawn Spawn;
                foreach (KeyValuePair<uint, Creature_spawn> Kp in CreatureService.CreatureSpawns)
                {
                    Spawn = Kp.Value;
                    Spawn.Proto = CreatureService.GetCreatureProto(Spawn.Entry);
                    if (Spawn.Proto == null)
                    {
                        Log.Debug("LoadRegionSpawns", "Invalid Creature Proto (" + Spawn.Entry + "), spawn Guid(" + Spawn.Guid + ")");
                        ++InvalidSpawns;
                        continue;
                    }

                    Info = ZoneService.GetZone_Info(Spawn.ZoneId);
                    if (Info != null)
                    {
                        X = (ushort)(Spawn.WorldX >> 12);
                        Y = (ushort)(Spawn.WorldY >> 12);

                        GetRegionCell(Info.Region, X, Y).AddSpawn(Spawn);
                        
                        if (!RegionCount.ContainsKey(Info.Name))
                            RegionCount.Add(Info.Name, 0);

                        ++RegionCount[Info.Name];
                    }
                    else
                    {
                        Log.Debug("LoadRegionSpawns", "ZoneId (" + Spawn.ZoneId + ") invalid, Spawn Guid(" + Spawn.Guid + ")");
                        ++InvalidSpawns;
                    }
                    
                }
            }

            {
                GameObject_spawn Spawn;
                foreach (KeyValuePair<uint, GameObject_spawn> Kp in GameObjectService.GameObjectSpawns)
                {
                    Spawn = Kp.Value;
                    Spawn.Proto = GameObjectService.GetGameObjectProto(Spawn.Entry);
                    if (Spawn.Proto == null)
                    {
                        Log.Debug("LoadRegionSpawns", "Invalid GameObject Proto (" + Spawn.Entry + "), spawn Guid(" + Spawn.Guid + ")");
                        ++InvalidSpawns;
                        continue;
                    }

                    Info = ZoneService.GetZone_Info(Spawn.ZoneId);
                    if (Info != null)
                    {
                        X = (ushort)(Spawn.WorldX >> 12);
                        Y = (ushort)(Spawn.WorldY >> 12);

                        GetRegionCell(Info.Region, X, Y).AddSpawn(Spawn);

                        if (!RegionCount.ContainsKey(Info.Name))
                            RegionCount.Add(Info.Name, 0);

                        ++RegionCount[Info.Name];
                    }
                    else
                    {
                        Log.Debug("LoadRegionSpawns", "ZoneId (" + Spawn.ZoneId + ") invalid, Spawn Guid(" + Spawn.Guid + ")");
                        ++InvalidSpawns;
                    }
                }
            }

            if (InvalidSpawns > 0)
                Log.Error("LoadRegionSpawns", "[" + InvalidSpawns + "] Invalid Spawns");

            foreach (KeyValuePair<string, int> Counts in RegionCount)
                Log.Debug("Region", "[" + Counts.Key + "] : " + Counts.Value);
        }

        public static void RemoveDoubleSpawns()
        {
            const uint Space = 400;
            int[] Removed = new int[255];
            List<uint> Guids = new List<uint>();

            Zone_Info Info = null;
            int i, y, Px, Py, SPx, Spy;
            CellSpawns[,] Cell;

            foreach (KeyValuePair<ushort, CellSpawns[,]> Kp in _RegionCells)
            {
                Cell = Kp.Value;
                for (i = 0; i < Cell.GetLength(0); ++i)
                {
                    for (y = 0; y < Cell.GetLength(1); ++y)
                    {
                        if (Cell[i, y] == null)
                            continue;

                        foreach (Creature_spawn Sp in Cell[i, y].CreatureSpawns.ToArray())
                        {
                            if (Sp != null || Sp.Proto == null || QuestService.GetStartQuests(Sp.Proto.Entry) != null)
                                continue;

                            if (Info == null || Info.ZoneId != Sp.ZoneId)
                                Info = ZoneService.GetZone_Info(Sp.ZoneId);

                            Px = ZoneService.CalculPin(Info, Sp.WorldX, true);
                            Py = ZoneService.CalculPin(Info, Sp.WorldY, false);

                            foreach (Creature_spawn SubSp in Cell[i, y].CreatureSpawns.ToArray())
                            {
                                if (SubSp.Proto == null || Sp.Entry != SubSp.Entry || Sp == SubSp)
                                    continue;

                                SPx = ZoneService.CalculPin(Info, SubSp.WorldX, true);

                                if (Px > SPx + Space || Px < SPx - Space)
                                    continue;

                                Spy = ZoneService.CalculPin(Info, SubSp.WorldY, true);

                                if (Py > Spy + Space || Py < Spy - Space)
                                    continue;

                                Removed[SubSp.ZoneId]++;
                                Guids.Add(SubSp.Guid);
                                Cell[i, y].CreatureSpawns.Remove(SubSp);
                                SubSp.Proto = null;
                            }
                        }
                    }
                }
            }

            if (Guids.Count > 0)
            {
                string L = "(";
                foreach (uint Guid in Guids)
                {
                    L += Guid + ",";
                }

                L = L.Remove(L.Length - 1, 1);
                L += ")";

                Log.Info("Spawns", "DELETE FROM creature_spawns WHERE Guid in " + L + ";");
                Database.ExecuteNonQuery("DELETE FROM creature_spawns WHERE Guid in " + L + ";");
            }


            for (i = 0; i < 255; ++i)
            {
                if (Removed[i] != 0)
                    Log.Info("Removed", "Zone : " + i + " : " + Removed[i]);
            }
        }
    }
}
