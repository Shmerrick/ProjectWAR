using Common;
using FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemData;
using WorldServer.Services.World;

namespace WorldServer
{
    public class InstanceMgr
    {
        Dictionary<ushort, ushort> realminstancesorder = new Dictionary<ushort, ushort>();
        Dictionary<ushort, ushort> realminstancesdestro = new Dictionary<ushort, ushort>();
        private Dictionary<ushort, Instance> _instances = new Dictionary<ushort, Instance>();

        public InstanceMgr()
        {

        }



        public bool ZoneIn(Player player, byte instancetyp, Zone_jump Jump = null)
        {

            // jump type 4 = realm 5 = raid 6= group instances
            Instance ints = null;
            ushort zoneID;

            if (Jump == null)
                zoneID = player.Info.Value.ZoneId;
            else
                zoneID = Jump.ZoneID;

            Instance_Info II;
            InstanceService._InstanceInfo.TryGetValue(zoneID, out II);
            ushort InstanceMainID = II.Entry;
            ushort instanceid = 0;
            byte maxplayers = 0;

            if (instancetyp == 4) // Realm instance
            {

                if (Jump == null && (player._Value.DisconcetTime == 0 || (player._Value.DisconcetTime + 600) < TCPManager.GetTimeStamp()))
                    return false;

                if (player.Realm == GameData.Realms.REALMS_REALM_DESTRUCTION)
                {
                    realminstancesdestro.TryGetValue(zoneID, out instanceid);
                }
                else
                {
                    realminstancesorder.TryGetValue(zoneID, out instanceid);
                }
                if (instanceid == 0)
                {
                    lock (_instances)
                    {
                        for (ushort i = 1; i < ushort.MaxValue; i++)
                        {
                            if (!_instances.ContainsKey(i))
                            {
                                if (player.Realm == GameData.Realms.REALMS_REALM_DESTRUCTION)
                                {
                                    realminstancesdestro.Add(zoneID, i);
                                }
                                else
                                {
                                    realminstancesorder.Add(zoneID, i);
                                }
                                ints = new Instance(zoneID, i, (byte)player.Realm, null);
                                _instances.Add(i, ints);
                                instanceid = i;
                                break;
                            }
                        }
                    }
                }
            }
            else // Group Raid Instance
            {
                maxplayers = 6;
                if (instancetyp == 5)
                    maxplayers = 24;

                String plrlockout = player._Value.GetLockout(InstanceMainID);

                if (player.PriorityGroup != null && player.PriorityGroup.GetLeader() != player)
                {
                    instanceid = Find_OpenInstanceoftheplayer(player.PriorityGroup.GetLeader(), zoneID);
                }
                else
                    instanceid = Find_OpenInstanceoftheplayer(player, zoneID);
            }
            if (instanceid == 0 && Jump == null)
                return false;

                 
            if (instanceid == 0 && (player.PriorityGroup == null || player.PriorityGroup.GetLeader() == player))
                instanceid = Create_new_instance(player, Jump);

            if(instanceid == 0)
            {
                player.SendClientMessage("Your Groupleader needs to enter first", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return false;
            }


            if (!Join_Instance(player, instanceid, Jump, maxplayers, InstanceMainID))
                return false;

            return true;
        }

        private ushort Find_OpenInstanceoftheplayer(Player player,ushort ZoneID)
        {
            lock (_instances)
            {
                foreach(KeyValuePair<ushort,Instance> ii in _instances)
                {
                    if (ii.Value.ZoneID == ZoneID && ii.Value._Players.Contains(player.Name))
                    {
                        return ii.Key;
                    }
                }
            }
            return 0;
        }

        private ushort Create_new_instance(Player player, Zone_jump Jump)
        {
            lock (_instances)
            {
                for (ushort i = 1; i < ushort.MaxValue ; i++)
                {
                    if (!_instances.ContainsKey(i))
                    {
                        if (Jump.ZoneID == 179)
                        {
                            TOTVL ints = null;
                            Instance_Lockouts deadbosses = null;
                            if (player._Value.GetLockout(Jump.InstanceID) != null)
                                InstanceService._InstanceLockouts.TryGetValue(player._Value.GetLockout(Jump.InstanceID), out deadbosses);
                            ints = new TOTVL(Jump.ZoneID, i, 0, deadbosses);
                            _instances.Add(i, ints);
                            return i;
                        }
                        else
                        {
                            Instance ints = null;
                            Instance_Lockouts deadbosses = null;
                            if (player._Value.GetLockout(Jump.InstanceID) != null)
                                InstanceService._InstanceLockouts.TryGetValue(player._Value.GetLockout(Jump.InstanceID), out deadbosses);
                            ints = new Instance(Jump.ZoneID, i, 0, deadbosses);
                            _instances.Add(i, ints);
                            return i;
                        }
                    }
                }
            }
            return 0;
        }

        private bool Join_Instance(Player player,ushort Instanceid,Zone_jump Jump,int maxplayers,ushort InstancemainID)
        {
            lock (_instances)
            {
                Instance inst;
                _instances.TryGetValue(Instanceid,out inst);

                if (player._Value.GetLockout(InstancemainID) != null && inst.Lockout != null && inst.Lockout.InstanceID != player._Value.GetLockout(InstancemainID))
                {
                    player.SendClientMessage("Your Instance Lockout dont match the Instance you are trying to enter", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    return false;
                }

                if (inst.Encounterinprogress)
                {
                    player.SendClientMessage("There is an Encounter in progress you cannot enter now", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    return false;
                }

                if (maxplayers == 0 || inst.Region.Players.Count <= maxplayers)
                {
                    if (Jump != null && Jump.ZoneID == 179)
                        ((TOTVL)inst).AddPlayer(player, Jump);
                    else
                        inst.AddPlayer(player, Jump);
                }
                else
                {
                    player.SendClientMessage("Instance is full", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    return false;
                }
            }
            return true;
        }

        public void closeInstance(Instance inst,ushort ID)
        {
            _instances.TryGetValue(ID,out inst);
            _instances.Remove(ID);

            inst = null;

            for(int i = 0; i < realminstancesorder.Count; i++)
            {
                if (realminstancesorder.ElementAt(i).Value == ID)
                    realminstancesorder.Remove(realminstancesorder.ElementAt(i).Key);
            }
            for (int i = 0; i < realminstancesdestro.Count; i++)
            {
                if (realminstancesdestro.ElementAt(i).Value == ID)
                    realminstancesdestro.Remove(realminstancesdestro.ElementAt(i).Key);
            }
        }
        public void sendInstanceInfo(Player plr,ushort instanceid)
        {
            if (instanceid == 0)
            {
                plr.SendClientMessage("Total instances =" + _instances.Count(), ChatLogFilters.CHATLOGFILTERS_SAY);
                lock (_instances)
                {
                    foreach (KeyValuePair<ushort, Instance> i in _instances)
                    {
                        plr.SendClientMessage("Instance id = " + i.Key + "  map= " + i.Value.Info.Name + "  Players: " + i.Value.Region.Players.Count, ChatLogFilters.CHATLOGFILTERS_SAY);
                    }
                }
            }
            else
            {
                Instance i;
                _instances.TryGetValue(instanceid,out i);
                if (i == null)
                {
                    plr.SendClientMessage("Instance id = " + instanceid + "not found", ChatLogFilters.CHATLOGFILTERS_SAY);
                    return;
                }
                plr.SendClientMessage("Instance id = " + instanceid + "  Map= " + i.Info.Name + "  Players: " + i.Region.Players.Count, ChatLogFilters.CHATLOGFILTERS_SAY);
                String players="";
                foreach (Player pl in i.Region.Players)
                {
                    players += pl.Name + "  ,";
                }
                plr.SendClientMessage("Players: " + players, ChatLogFilters.CHATLOGFILTERS_SAY);

            }
        }
    }
}
