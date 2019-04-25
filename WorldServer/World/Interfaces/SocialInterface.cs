using System;
using System.Collections.Generic;
using System.Linq;
using SystemData;
using Common;
using FrameWork;
using GameData;
using WorldServer.Managers;
using WorldServer.World.Objects;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Interfaces
{
    public class SocialInterface : BaseInterface
    {
        private Player _player;

        private readonly Dictionary<uint, Character_social> _friendCharacterIds = new Dictionary<uint, Character_social>();
        private readonly Dictionary<uint, Character_social> _ignoreCharacterIds = new Dictionary<uint, Character_social>();  

        public List<Character_social> GetFriendList()
        {
            lock (_friendCharacterIds)
                return _friendCharacterIds.Values.ToList();
        }

        public List<Character_social> GetIgnoreList()
        {
            lock (_ignoreCharacterIds)
                return _ignoreCharacterIds.Values.ToList();
        }

        public bool Anon
        {
            get
            {
                return _player != null && _player.Info.Anonymous;
            }
            set
            {
                if (_player == null)
                    return;
                _player.Info.Anonymous = value;
                CharMgr.Database.SaveObject(_player.Info);
                //CharMgr.Database.ForceSave();
            }
        }

        public bool Hide
        {
            get
            {
                if (_player != null)
                    return false; //  Disable hidden option.

                return false;
            }
            set
            {
                if (_player != null)
                {

                    _player.Info.Hidden = false;   // Force hidden disabled.
                    CharMgr.Database.SaveObject(_player.Info);
                    //CharMgr.Database.ForceSave();

                    if (value)
                        NotifyOffline(false);
                    else
                        NotifyOnline(false);
                }
            }
        }

        public override void SetOwner(Object obj)
        {
            _player = (Player) obj;
        }

        public override bool Load()
        {
            if (Loaded)
                return base.Load();

            if (_player.Info.Socials == null)
                _player.Info.Socials = new List<Character_social>();
            else
            {
                lock (_player.Info.Socials)
                {
                    int socialCount = _player.Info.Socials.Count;

                    for (int i = 0; i < socialCount; ++i)
                    {
                        Character_social social = _player.Info.Socials[i];

                        if (social.Ignore == 1)
                        {
                            lock (_ignoreCharacterIds)
                            {
                                if (_ignoreCharacterIds.ContainsKey(social.DistCharacterId))
                                {
                                    _player.Info.Socials.RemoveAt(i);
                                    CharMgr.Database.DeleteObject(social);
                                    --i;
                                    --socialCount;
                                    continue;
                                }

                                _ignoreCharacterIds.Add(social.DistCharacterId, social);
                            }
                        }


                        if (social.Friend == 1)
                        {
                            lock (_friendCharacterIds)
                                if (_friendCharacterIds.ContainsKey(social.DistCharacterId))
                                {
                                    _player.Info.Socials.RemoveAt(i);
                                    CharMgr.Database.DeleteObject(social);
                                    --i;
                                    --socialCount;
                                    continue;
                                }
                            _friendCharacterIds.Add(social.DistCharacterId, social);
                        }
                    }
                }
            }

            SendSocialWindowStatusFlags();
            NotifyOnline();
            SendSocialLists();

            return base.Load();
        }

        public override void Stop()
        {
            base.Stop();
            NotifyOffline();
        }

        private void NotifyOnline(bool sendOnlineText = true)
        {
            List<Player> players;

            lock (Player._Players)
            {
                players = Player._Players.ToList();
            }

            foreach (var friend in players)
            {
                if(friend != _player)
                    if (friend.SocInterface.HasFriend(_player.CharacterId) && (!HasIgnore(friend.CharacterId) || friend.GmLevel != 1)) //notify people who have this char as friend, except ones ignored.
                    {
                        if(sendOnlineText)
                            friend.SendLocalizeString(_player.Name, ChatLogFilters.CHATLOGFILTERS_SHOUT, Localized_text.TEXT_SN_FRIEND_LOGON);
                        friend.SocInterface.SendFriend(_player, true);
                    }
            }
        }

        private void NotifyOffline(bool sendOnlineText = true)
        {
            List<Player> players = new List<Player>();
            lock (Player._Players)
            {
                players = Player._Players.ToList();
            }

            foreach (var friend in players)
            {
                if (friend != _player)
                    if (friend.SocInterface.HasFriend(_player.CharacterId))
                    {
                        if (sendOnlineText)
                            friend.SendLocalizeString(_player.Name, ChatLogFilters.CHATLOGFILTERS_SHOUT, Localized_text.TEXT_SN_FRIEND_LOGOFF);
                        friend.SocInterface.SendFriend(_player, false);
                    }
            }
        }

        public bool HasFriend(uint characterId)
        {
            lock (_friendCharacterIds)
                return _friendCharacterIds.ContainsKey(characterId);
        }
        public bool HasIgnore(uint characterId)
        {
            lock (_ignoreCharacterIds)
                return _ignoreCharacterIds.ContainsKey(characterId);
        }

        public void AddFriend(string name)
        {
            if (name.Length <= 0 || name.Equals(_player.Name, StringComparison.OrdinalIgnoreCase))
            {
                _player.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_SN_FRIENDSLIST_ERR_ADD_SELF);
                return;
            }

            string charName = Player.AsCharacterName(name);
            uint characterId = CharMgr.GetCharacterId(charName);

            // Character didn't exist
            if (characterId == 0)
            {
                _player.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_SN_LISTS_ERR_PLAYER_NOT_FOUND);
                return;
            }

            // Lift any ignore when friending someone
            if (HasIgnore(characterId))
                RemoveIgnore(characterId, name);

            // Check for existing friend
            lock (_friendCharacterIds)
            {
                if (_friendCharacterIds.ContainsKey(characterId))
                {
                    _player.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_SN_FRIENDSLIST_ERR_EXISTS);
                    return;
                }
            }

            // Players may not add a GM as a friend unless the GM friended them first
            Character charInfo = CharMgr.GetCharacter(characterId, false);

            if (charInfo != null && _player.GmLevel == 1)
            {
                Account acct = Program.AcctMgr.GetAccountById(charInfo.AccountId);

                if (acct != null && acct.GmLevel > 1)
                {
                    lock (charInfo.Socials)
                    {
                        if (!charInfo.Socials.Any(soc => soc.DistCharacterId == _player.Info.CharacterId && soc.Friend == 1))
                        {
                            _player.SendClientMessage("To prevent abuse of the Friends list for staff harassment, you may not friend staff members unless they friend you first.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                            return;
                        }
                    }
                }
            }

            Character_social social = new Character_social
            {
                CharacterId = _player.Info.CharacterId,
                DistName = charName,
                DistCharacterId = characterId,
                Friend = 1,
                Ignore = 0
            };
            CharMgr.Database.AddObject(social);
            _player.Info.Socials.Add(social);

            lock (_friendCharacterIds)
                _friendCharacterIds.Add(characterId, social);

            //CharMgr.Database.ForceSave();

            SendSocialList(social, SocialListType.SOCIAL_FRIEND);
            _player.SendLocalizeString(charName, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_SN_FRIENDSLIST_ADD_SUCCESS);

            Player distPlayer = Player.GetPlayer(name);

            if (distPlayer != null)
            {
                distPlayer.SendLocalizeString(_player.Name, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_X_FRIENDED_YOU);
                SendFriend(distPlayer, true);
            }
        }

        public void RemoveFriend(string name)
        {
            uint characterId = CharMgr.GetCharacterId(Player.AsCharacterName(name));

            Character_social social;

            lock (_friendCharacterIds)
                _friendCharacterIds.TryGetValue(characterId, out social);

            if (social == null)
            {
                _player.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_SN_LISTS_ERR_PLAYER_NOT_FOUND);
                return;
            }

            PacketOut Out = new PacketOut((byte)Opcodes.F_SOCIAL_NETWORK);

            Out.WriteUInt16(0);
            Out.WriteByte((byte)SocialListType.SOCIAL_FRIEND);
            Out.WriteByte(1); // Count
            Out.WriteByte(1);
            Out.WriteUInt32(social.DistCharacterId);

            _player.SendPacket(Out);

            _player.SendLocalizeString(social.DistName, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_SN_FRIEND_REMOVE);

            if (social.Ignore > 0)
            {
                social.Friend = 0;
                CharMgr.Database.SaveObject(social);
            }
            else
            {
                lock (_player.Info.Socials)
                {
                    if (_player.Info.Socials.Contains(social))
                        _player.Info.Socials.Remove(social);
                }
                CharMgr.Database.DeleteObject(social);
            }

            lock(_friendCharacterIds)
                _friendCharacterIds.Remove(social.DistCharacterId);

            //CharMgr.Database.ForceSave();
        }

        public void Ignore(string name)
        {
            if (name.Length <= 0 || name.Equals(_player.Name, StringComparison.OrdinalIgnoreCase))
            {
                _player.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_SN_FRIENDSLIST_ERR_ADD_SELF);
                return;
            }

            string charName = Player.AsCharacterName(name);
            uint characterId = CharMgr.GetCharacterId(charName);

            // Character didn't exist
            if (characterId == 0)
            {
                _player.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_SN_LISTS_ERR_PLAYER_NOT_FOUND);
                return;
            }
            bool applyIgnore = true;

            lock(_ignoreCharacterIds)
                if (_ignoreCharacterIds.ContainsKey(characterId))
                    applyIgnore = false;

            if (applyIgnore)
                AddIgnore(characterId, name);
            else
                RemoveIgnore(characterId, name);
        }

        public void AddIgnore(uint characterId, string charName)
        {
            // Remove friend status when applying ignore
            if (HasFriend(characterId))
                RemoveFriend(charName);

            Character_social social = new Character_social
            {
                CharacterId = _player.Info.CharacterId,
                DistName = charName,
                DistCharacterId = characterId,
                Friend = 0,
                Ignore = 1
            };

            CharMgr.Database.AddObject(social);
            _player.Info.Socials.Add(social);

            lock (_ignoreCharacterIds)
                _ignoreCharacterIds.Add(characterId, social);

            //CharMgr.Database.ForceSave();

            SendSocialList(social, SocialListType.SOCIAL_IGNORE);
            _player.SendLocalizeString(charName, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_SN_IGNORELIST_ADD_SUCCESS);

            Player distPlayer = Player.GetPlayer(charName);

            if (distPlayer != null && distPlayer.SocInterface.HasFriend(_player.CharacterId))
            {
                distPlayer.SocInterface.SendFriend(_player, false);
            }
        }

        public void RemoveIgnore(uint characterId, string charName)
        {
            Character_social social;

            lock (_ignoreCharacterIds)
                social = _ignoreCharacterIds[characterId];

            PacketOut Out = new PacketOut((byte)Opcodes.F_SOCIAL_NETWORK);

            Out.WriteUInt16(0);
            Out.WriteByte((byte)SocialListType.SOCIAL_IGNORE);
            Out.WriteByte(1); // Count
            Out.WriteByte(1);
            Out.WriteUInt32(social.DistCharacterId);

            _player.SendPacket(Out);

            _player.SendLocalizeString(social.DistName, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_SN_IGNORE_REMOVE);

            if (social.Friend > 0)
            {
                social.Ignore = 0;
                CharMgr.Database.SaveObject(social);
            }
            else
            {
                lock (_player.Info.Socials)
                {
                    if (_player.Info.Socials.Contains(social))
                        _player.Info.Socials.Remove(social);
                }
                CharMgr.Database.DeleteObject(social);
            }

            lock (_ignoreCharacterIds)
                _ignoreCharacterIds.Remove(social.DistCharacterId);

            //CharMgr.Database.ForceSave();

            //if player who was taken off ignore had this character as a friend, tell them this character is online
            Player distPlayer = Player.GetPlayer(charName);

            if (distPlayer != null && distPlayer.SocInterface.HasFriend(_player.CharacterId))
            {
                distPlayer.SocInterface.SendFriend(_player, true);
            }
        }

        public static void BuildPlayerInfo(ref PacketOut Out, Player Plr, bool noHide = false)
        {
            BuildPlayerInfo(ref Out, (ushort)Plr._Value.CharacterId, Plr.Name, true, Plr.SocInterface.Anon, Plr.Level, Plr.Info.Career, Plr._Value.ZoneId, Plr.GldInterface.GetGuildName(), noHide);
        }

        public static void BuildPlayerInfo(ref PacketOut Out, uint CharId, string Name, bool Online, bool Anonymous, byte Level, ushort Career, ushort Zone, string GuildName, bool noHide = false)
        {
            Out.WriteUInt32(CharId);

            Out.WriteUInt16((ushort)(Name.Length + 1));
            Out.WriteStringBytes(Name);
            Out.WriteByte(0);

            Out.WriteByte(0);

            if (!Online || Anonymous && !noHide)
            {
                Out.WriteByte(0);
            }
            else
            {
                Out.WriteByte(1);
                Out.WriteByte(1);

                Out.WriteByte(Level);
                Out.WriteUInt16(0);
                Out.WriteUInt16(Career);
                Out.WriteUInt16(Zone);

                Out.WriteUInt16((ushort)(GuildName.Length));
                Out.WriteStringBytes(GuildName);
            }
        }

        public static void BuildPlayerInfo(ref PacketOut Out, Character_social Social, bool noHide = false)
        {
            var player = Player.GetPlayer(Social.DistName);

            BuildPlayerInfo(ref Out, Social.DistCharacterId,
                Social.DistName,
                player != null, //online
                (bool)(player != null ? player.SocInterface.Anon : true), //anon
                (byte)(player != null ? player.Level : 0), //level
                (ushort)(player != null ? player.Info.Career : 0), //career
                (ushort)(player != null ? player.Zone.ZoneId : 0), //zoneid
                player != null ? player.GldInterface.GetGuildName() : "", //guild
                noHide);
        }


        public void SendFriend(Player Plr, bool online)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_SOCIAL_NETWORK, 25 + (online ? 1 : Plr.Info.Name.Length + Plr.GldInterface.GetGuildName().Length));
            Out.WriteUInt16(0);
            Out.WriteByte((byte)SocialListType.SOCIAL_FRIEND);
            Out.WriteByte(1); // Count
            Out.WriteByte(0);
            BuildPlayerInfo(ref Out, Plr.Info.CharacterId, Plr.Info.Name, online, false, Plr._Value.Level, Plr.Info.Career, Plr._Value.ZoneId, Plr.GldInterface.GetGuildName());
            Out.WriteByte(0);
            _player.SendPacket(Out);
        }

        public void SendSocialList(List<Character_social> socials, SocialListType Type)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_SOCIAL_NETWORK);
            Out.WriteUInt16(0);
            Out.WriteByte((byte)Type);
            Out.WriteByte((byte)socials.Count);
            Out.WriteByte(0);

            foreach (Character_social social in socials)
            {
                BuildPlayerInfo(ref Out, social);
                Out.WriteByte(0);
            }

            _player.SendPacket(Out);
        }

        public void SendSocialWindowStatusFlags()
        {
            byte flags = 0;

            if (Anon)
                flags |= 1;
            if (Hide)
                flags |= 2;

            PacketOut Out = new PacketOut((byte)Opcodes.F_SOCIAL_NETWORK);
            Out.Fill(0, 10);
            Out.WriteByte(flags);
            Out.Fill(0, 10);

            _player.SendPacket(Out);
        }

        public void SendSocialList(Character_social Social, SocialListType Type)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_SOCIAL_NETWORK);
            Out.WriteUInt16(0);
            Out.WriteByte((byte)Type);
            Out.WriteByte(1);
            Out.WriteByte(0);
            BuildPlayerInfo(ref Out, Social);
            Out.WriteByte(0);
            _player.SendPacket(Out);
        }

        public void SendSocialLists()
        {
            SendSocialList(GetFriendList(), SocialListType.SOCIAL_FRIEND);
            SendSocialList(GetIgnoreList(), SocialListType.SOCIAL_IGNORE);
        }

        public static byte MAX_SEND = 255;
        public bool blocksTells;

        public void SendPlayers(List<Player> players, bool noHide = false)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_SOCIAL_NETWORK);
            Out.WriteUInt16(0);
            Out.WriteByte(4);

            Out.WriteByte((byte)(players.Count > MAX_SEND ? MAX_SEND : players.Count));
            foreach (Player Dist in players.Take(MAX_SEND))
            {
                BuildPlayerInfo(ref Out, Dist, noHide);
            }
            Out.WriteByte((byte)(players.Count > MAX_SEND ? 1 : 0));
            _player.SendPacket(Out);
        }

    }
}
