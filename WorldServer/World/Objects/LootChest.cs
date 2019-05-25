using System;
using System.Collections.Generic;
using System.Linq;
using SystemData;
using Common;
using FrameWork;
using GameData;
using WorldServer.Managers;
using WorldServer.NetWork.Handler;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Apocalypse.Loot;
using WorldServer.World.Interfaces;
using WorldServer.World.Map;
using WorldServer.World.Objects.PublicQuests;
using WorldServer.World.Positions;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Objects
{

    public class LootChest : GameObject
    {
        // Dictionary of characterId, and Bag -* List of Bag Contents
        public Dictionary<uint, KeyValuePair<Item_Info, List<Talisman>>> LootBags { get; set; }
        public string Title { get; set; }
        public string SenderName { get; set; }
        public string Content { get; set; }

        
        public LootChest(GameObject_spawn spawn)
        {
            Spawn = spawn;
            if (!string.IsNullOrEmpty(Spawn.AlternativeName))
                Name = Spawn.AlternativeName;
            else
                Name = spawn.Proto.Name;
            VfxState = (byte)spawn.VfxState;

            this.Y = spawn.WorldY;
            this.Z = spawn.WorldZ;
            this.X = spawn.WorldX;
            

            EvtInterface.AddEvent(Destroy, 180 * 1000, 1);
        }

        public override string ToString()
        {
            if (LootBags != null)
            {
                return $"{Name}. {X}.{Y}.{Z}. Lootbags = {LootBags.Count}";
            }
            else
            {
                return $"{Name}. {X}.{Y}.{Z}";    
            }

            
        }

        public static LootChest Create(RegionMgr region, Point3D location, ushort zoneId, bool convertPin = true)
        {
            if (region == null)
            {
                Log.Error("LootChest", "Attempt to create for NULL region");
                return null;
            }
            
            GameObject_proto proto = GameObjectService.GetGameObjectProto(188);
            GameObject_spawn spawn = new GameObject_spawn();

            if (convertPin)  // Non-fort zone location points are PIN position system, forts are worldposition.
            {
                var targetPosition = ZoneService.GetWorldPosition(ZoneService.GetZone_Info(zoneId), (ushort) location.X,
                    (ushort) location.Y, (ushort) location.Z);

                spawn.Guid = (uint)GameObjectService.GenerateGameObjectSpawnGUID();
                spawn.WorldO = 0;
                spawn.WorldY = targetPosition.Y+StaticRandom.Instance.Next(50,100);
                spawn.WorldZ = targetPosition.Z;
                spawn.WorldX = targetPosition.X+StaticRandom.Instance.Next(50,100);
                spawn.ZoneId = zoneId;
            }
            else
            {
                spawn.Guid = (uint)GameObjectService.GenerateGameObjectSpawnGUID();
                spawn.WorldO = 0;
                spawn.WorldY = location.Y+StaticRandom.Instance.Next(50,100);
                spawn.WorldZ = location.Z;
                spawn.WorldX = location.X+StaticRandom.Instance.Next(50,100);
                spawn.ZoneId = zoneId;
            }


            spawn.BuildFromProto(proto);
            var chest = region.CreateLootChest(spawn);
            chest.LootBags = new Dictionary<uint, KeyValuePair<Item_Info, List<Talisman>>>();
            
            
            return chest;
        }

        

        // Called by Destroy of Object
        public override void Dispose()
        {
            if (IsDisposed)
                return;

            try
            {
                foreach (var lootBag in LootBags)
                {
                    var character = CharMgr.GetCharacter(lootBag.Key, false);
                    var characterName = character?.Name;

                    // Forced to have some value here.
                    if (Content == null)
                        Content = "mail";
                    if (String.IsNullOrEmpty(Content))
                        Content = "mail";

                    Character_mail mail = new Character_mail
                    {
                        Guid = CharMgr.GenerateMailGuid(),
                        CharacterId = lootBag.Key,  //CharacterId
                        SenderName = SenderName,
                        ReceiverName = characterName,
                        SendDate = (uint)TCPManager.GetTimeStamp(),
                        Title = Title,
                        Content = Content,
                        Money = 0,
                        Opened = false
                    };
                 
                    mail.CharacterIdSender = lootBag.Key;
                    MailItem item = new MailItem((uint)lootBag.Value.Key.Entry, lootBag.Value.Value, 0, 0,(ushort)lootBag.Value.Value.Count);
                    if (item != null)
                    {
                        mail.Items.Add(item);
                        CharMgr.AddMail(mail);
                    }

                

                }
            }
            catch (Exception ex)
            {
                Log.Error("LootChest", $"Failed to mail loot. {ex.Message} {ex.StackTrace}");
            }

            

            base.Dispose();
        }

        public override void SendInteract(Player player, InteractMenu menu)
        {
            switch (menu.Menu)
            {
                case 15: // Close the loot
                    return;

                case 13: // Retrieve all items
                    TakeAll(player);
                    break;

                case 12: // Retrieve an item
                    TakeLoot(player);
                    break;
            }

            // Unknown if needed TODO
            //GoldBag bag;

           
            if (this.LootBags.ContainsKey(player.CharacterId))
            {
                KeyValuePair<Item_Info, List<Talisman>> lootBag;
                this.LootBags.TryGetValue(player.CharacterId, out lootBag);

                PacketOut Out = new PacketOut((byte)Opcodes.F_INTERACT_RESPONSE, 32);
                Out.WriteByte(4);
                Out.WriteByte(1);
                Out.WriteByte(1);
                Item.BuildItem(ref Out, null,lootBag.Key, null, 0, 1);
                player.SendPacket(Out);
            }
            else
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_INTERACT_RESPONSE, 32);
                Out.WriteByte(4);
                Out.WriteByte(0);
                player.SendPacket(Out);
            }
        }

        public void TakeLoot(Player plr)
        {
            Character character = CharMgr.GetCharacter(plr.CharacterId, false);

            var bagExists = LootBags.ContainsKey(character.CharacterId);
            if (bagExists)
            {
                var bag = LootBags[character.CharacterId];

                foreach (var talisman in bag.Value)
                {
                    // Adding bag to the player
                    ItemResult result = plr.ItmInterface.CreateItem(bag.Key, 1, bag.Value,    0, 0, false);

                    if (result == ItemResult.RESULT_OK)
                        LootBags.Remove(plr.CharacterId);
                    else if (result == ItemResult.RESULT_MAX_BAG)
                        plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_LOOT,
                            Localized_text.TEXT_OVERAGE_CANT_LOOT);


                }

                //ItemResult result = plr.ItmInterface.CreateItem(bag.Key, 1, items.talisman,
                //    items.primary_dye, items.secondary_dye, false);


            }

        }

        public void TakeAll(Player player)
        {
            if (player.ItmInterface.GetTotalFreeInventorySlot() < 1)
                player.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_OVERAGE_CANT_LOOT);
            else
                TakeLoot(player);
        }

       
        public override void SendMeTo(Player plr)
        {
            // Log.Info("STATIC", "Creating static oid=" + Oid + " name=" + Name + " x=" + Spawn.WorldX + " y=" + Spawn.WorldY + " z=" + Spawn.WorldZ + " doorID=" + Spawn.DoorId);
            PacketOut Out = new PacketOut((byte)Opcodes.F_CREATE_STATIC);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(VfxState); //ie: red glow, open door, lever pushed, etc

            Out.WriteUInt16((ushort)Spawn.WorldO);
            Out.WriteUInt16((ushort)Spawn.WorldZ);
            Out.WriteUInt32((uint)Spawn.WorldX);
            Out.WriteUInt32((uint)Spawn.WorldY);
            Out.WriteUInt16((ushort)Spawn.DisplayID);

            Out.WriteByte((byte)(Spawn.GetUnk(0) >> 8));

            // Get the database if the value hasnt been changed (currently only used for keep doors)
            if (Realm == Realms.REALMS_REALM_NEUTRAL)
                Out.WriteByte((byte)(Spawn.GetUnk(0) & 0xFF));
            else
                Out.WriteByte((byte)Realm);

            Out.WriteUInt16(Spawn.GetUnk(1));
            Out.WriteUInt16(Spawn.GetUnk(2));
            Out.WriteByte(Spawn.Unk1);

            int flags = Spawn.GetUnk(3);

            if (Realm != Realms.REALMS_REALM_NEUTRAL && !IsInvulnerable)
                flags |= 8; // Attackable (stops invalid target errors)

            LootContainer loots = LootsMgr.GenerateLoot(this, plr, 1);
            if ((loots != null && loots.IsLootable()) || (plr.QtsInterface.PublicQuest != null) || plr.QtsInterface.GameObjectNeeded(Spawn.Entry) || Spawn.DoorId != 0)
            {
                flags |= 4; // Interactable
            }

            Out.WriteUInt16((ushort)flags);
            Out.WriteByte(Spawn.Unk2);
            Out.WriteUInt32(Spawn.Unk3);
            Out.WriteUInt16(Spawn.GetUnk(4));
            Out.WriteUInt16(Spawn.GetUnk(5));
            Out.WriteUInt32(Spawn.Unk4);

            Out.WritePascalString(Name);

            if (Spawn.DoorId != 0)
            {
                Out.WriteByte(0x04);

                Out.WriteUInt32(Spawn.DoorId);
            }
            else
                Out.WriteByte(0x00);

            plr.SendPacket(Out);

            base.SendMeTo(plr);
        }

        //public void Scoreboard(ContributionInfo playerRoll, int preIndex, int postIndex)
        //{

        //    Player targPlayer = Player.GetPlayer(playerRoll.PlayerCharId);

        //    if (targPlayer == null)
        //        return;

        //    PacketOut Out = new PacketOut((byte)Opcodes.F_PQLOOT_TRIGGER, 1723);
        //    Out.WriteStringBytes(_publicQuestInfo.Name);
        //    Out.Fill(0, 24 - _publicQuestInfo.Name.Length);
        //    Out.WriteByte(_bags[gold]);  // gold
        //    Out.WriteByte(_bags[purple]);
        //    Out.WriteByte(_bags[blue]);
        //    Out.WriteByte(_bags[green]);
        //    Out.WriteByte(_bags[white]); // white
        //    Out.Fill(0, 3);

        //    WritePreRolls(Out);

        //    Out.WriteStringBytes(playerRoll.PlayerName);
        //    Out.Fill(0, 24 - playerRoll.PlayerName.Length);
        //    Out.Fill(0, 2);
        //    Out.WriteUInt16R((ushort)playerRoll.RandomBonus);
        //    Out.WriteUInt16R((ushort)playerRoll.ContributionBonus);
        //    Out.WriteUInt16R((ushort)playerRoll.PersistenceBonus);
        //    Out.WriteUInt16((ushort)(preIndex + 1)); // place

        //    WritePostRolls(Out);

        //    Out.WriteUInt16((ushort)(postIndex + 1)); // place
        //    Out.WriteStringBytes(playerRoll.PlayerName);
        //    Out.Fill(0, 24 - playerRoll.PlayerName.Length);
        //    Out.Fill(0, 2);
        //    Out.WriteUInt16R((ushort)playerRoll.RandomBonus);
        //    Out.WriteUInt16R((ushort)playerRoll.ContributionBonus);
        //    Out.WriteUInt16R((ushort)playerRoll.PersistenceBonus);
        //    Out.WriteByte(1);  // ???
        //    Out.WriteByte(playerRoll.BagWon);  // bag won

        //    Out.Fill(0, 2);
        //    //Out.WriteUInt16(TIME_PQ_RESET);
        //    Out.WriteByte(0);
        //    Out.WriteByte(3);


        //    Out.WriteByte(0);
        //    Out.WriteByte(0);
        //    Out.WriteByte(1);
        //    Out.Fill(0, 27);
        //    //
        //    // no clue yet seams to be if you didnt won anything you get that item


        //    /*
        //    Out.WritePacketString(@"|d4 c0 01 |...d............|     
        //    |57 61 72 20 43 72 65 73 74 00 00 00 00 00 00 00 |War Crest.......|
        //    |00 00 00 00 00 00 00 00 00 00 00                |...........     |
        //    ");
        //    */

        //    targPlayer.SendPacket(Out);
        //    // Info.SendCurrentStage(plr);
        //}  //  d4 c0 01

        //public void PersonalScoreboard(ContributionInfo playerRoll, byte bagWon)
        //{
        //    Player targPlayer = Player.GetPlayer(playerRoll.PlayerCharId);

        //    if (targPlayer == null)
        //        return;

        //    PacketOut Out = new PacketOut((byte)Opcodes.F_PQLOOT_TRIGGER, 1723);
        //    Out.WriteStringBytes(_publicQuestInfo.Name);
        //    Out.Fill(0, 24 - _publicQuestInfo.Name.Length);
        //    Out.WriteByte(_bags[gold]);  // gold
        //    Out.WriteByte(_bags[purple]);
        //    Out.WriteByte(_bags[blue]);
        //    Out.WriteByte(_bags[green]);
        //    Out.WriteByte(_bags[white]); // white
        //    Out.Fill(0, 3);
        //    ContributionInfo curRoll = playerRoll;

        //    Out.WriteStringBytes(targPlayer.Name);
        //    Out.Fill(0, 24 - targPlayer.Name.Length);
        //    Out.Fill(0, 2);
        //    Out.WriteUInt16R((ushort)curRoll.RandomBonus);
        //    Out.WriteUInt16R((ushort)curRoll.ContributionBonus);
        //    Out.WriteUInt16R((ushort)curRoll.PersistenceBonus);
        //    for (int i = 1; i < 24; i++)
        //        Out.Fill(0, 32);

        //    Out.WriteStringBytes(targPlayer.Name);
        //    Out.Fill(0, 24 - targPlayer.Name.Length);
        //    Out.Fill(0, 2);
        //    Out.WriteUInt16R((ushort)playerRoll.RandomBonus);
        //    Out.WriteUInt16R((ushort)playerRoll.ContributionBonus);
        //    Out.WriteUInt16R((ushort)playerRoll.PersistenceBonus);
        //    Out.WriteUInt16(1); // place
        //    Out.WriteStringBytes(targPlayer.Name);
        //    Out.Fill(0, 24 - targPlayer.Name.Length);
        //    Out.Fill(0, 2);
        //    Out.WriteUInt16R((ushort)curRoll.RandomBonus);
        //    Out.WriteUInt16R((ushort)curRoll.ContributionBonus);
        //    Out.WriteUInt16R((ushort)curRoll.PersistenceBonus);
        //    Out.WriteByte(1);  // ???
        //    Out.WriteByte(curRoll.BagWon);  // bag won
        //    for (int i = 1; i < 24; i++)
        //        Out.Fill(0, 34);  // i just send empty once here

        //    Out.WriteUInt16(1); // place
        //    Out.WriteStringBytes(targPlayer.Name);
        //    Out.Fill(0, 24 - targPlayer.Name.Length);
        //    Out.Fill(0, 2);
        //    Out.WriteUInt16R((ushort)playerRoll.RandomBonus);
        //    Out.WriteUInt16R((ushort)playerRoll.ContributionBonus);
        //    Out.WriteUInt16R((ushort)playerRoll.PersistenceBonus);
        //    Out.WriteByte(1);  // ???
        //    Out.WriteByte(playerRoll.BagWon);  // bag won

        //    Out.Fill(0, 2);
        //    //Out.WriteUInt16(TIME_PQ_RESET);
        //    Out.WriteByte(0);
        //    Out.WriteByte(3);


        //    Out.WriteByte(0);
        //    Out.WriteByte(0);
        //    Out.WriteByte(1);
        //    Out.Fill(0, 27);
        //    //
        //    // no clue yet seams to be if you didnt won anything you get that item


        //    /*
        //    Out.WritePacketString(@"|d4 c0 01 |...d............|     
        //    |57 61 72 20 43 72 65 73 74 00 00 00 00 00 00 00 |War Crest.......|
        //    |00 00 00 00 00 00 00 00 00 00 00                |...........     |
        //    ");
        //    */
        //    targPlayer.SendPacket(Out);
        //    // Info.SendCurrentStage(plr);
        //}  //  d4 c0 01

        //private void WritePreRolls(PacketOut Out)
        //{
        //    int maxCount = Math.Min(24, _preRoll.Count);

        //    for (int i = 0; i < maxCount; i++)
        //    {
        //        ContributionInfo curRoll = _preRoll[i].Value;

        //        Out.WriteStringBytes(curRoll.PlayerName);
        //        Out.Fill(0, 24 - curRoll.PlayerName.Length);
        //        Out.Fill(0, 2);
        //        Out.WriteUInt16R((ushort)curRoll.RandomBonus);
        //        Out.WriteUInt16R((ushort)curRoll.ContributionBonus);
        //        Out.WriteUInt16R((ushort)curRoll.PersistenceBonus);
        //    }

        //    if (maxCount < 24)
        //        for (int i = maxCount; i < 24; i++)
        //            Out.Fill(0, 32);
        //}

        //private void WritePersonalPreRolls(PacketOut Out)
        //{
        //    int maxCount = Math.Min(24, 1);

        //    for (int i = 0; i < maxCount; i++)
        //    {
        //        ContributionInfo curRoll = _preRoll[i].Value;

        //        Out.WriteStringBytes(curRoll.PlayerName);
        //        Out.Fill(0, 24 - curRoll.PlayerName.Length);
        //        Out.Fill(0, 2);
        //        Out.WriteUInt16R((ushort)curRoll.RandomBonus);
        //        Out.WriteUInt16R((ushort)curRoll.ContributionBonus);
        //        Out.WriteUInt16R((ushort)curRoll.PersistenceBonus);
        //    }

        //    if (maxCount < 24)
        //        for (int i = maxCount; i < 24; i++)
        //            Out.Fill(0, 32);
        //}

        //private void WritePostRolls(PacketOut Out)
        //{
        //    int maxCount = Math.Min(24, _postRoll.Count);

        //    for (int i = 0; i < maxCount; i++)
        //    {
        //        ContributionInfo curRoll = _postRoll[i].Value;

        //        Out.WriteStringBytes(curRoll.PlayerName);
        //        Out.Fill(0, 24 - curRoll.PlayerName.Length);
        //        Out.Fill(0, 2);
        //        Out.WriteUInt16R((ushort)curRoll.RandomBonus);
        //        Out.WriteUInt16R((ushort)curRoll.ContributionBonus);
        //        Out.WriteUInt16R((ushort)curRoll.PersistenceBonus);
        //        Out.WriteByte(1);  // ???
        //        Out.WriteByte(curRoll.BagWon);  // bag won
        //    }

        //    if (maxCount < 24)
        //        for (int i = maxCount; i < 24; i++)
        //            Out.Fill(0, 34);  // i just send empty once here
        //}

        public void Add(uint characterId, KeyValuePair<Item_Info, List<Talisman>> generatedLootBag)
        {
            LootBags.Add(characterId, generatedLootBag);
        }
    }

  
}