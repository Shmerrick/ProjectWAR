using Common;
using FrameWork;
using GameData;
using WorldServer.Managers;
using WorldServer.NetWork.Handler;
using WorldServer.Services.World;
using WorldServer.World.Abilities.CareerInterfaces;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Objects
{


    public class Standard : Creature
    {
        public Player Owner { get; }
        byte Bannertyp = 0;
        Realms RealmStandard;

        public Standard(Creature_spawn spawn,Player owner, byte bannertyp)
        {
            Spawn = spawn;
            Name = owner.GldInterface.Guild.Info.Name;
            Owner = owner;
            RealmStandard = owner.Realm;
            Bannertyp = bannertyp;
            Faction = (byte)(owner.Realm == Realms.REALMS_REALM_DESTRUCTION ? 8 : 6);

            
            
       
        }

        public override void Destroy()
        {
            if (!PendingDisposal)
            {
                PendingDisposal = true;

                if (!IsDead)
                {
                    IPetCareerInterface petInterface = Owner.CrrInterface as IPetCareerInterface;

                    petInterface?.Notify_PetDown();
                }
            }
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;

            base.Dispose();
        }

        public override void SendInteract(Player player, InteractMenu menu)
        {
            uint itemid = 0;
            switch (Bannertyp)
            {
                case 0: itemid = (uint)(RealmStandard == Realms.REALMS_REALM_DESTRUCTION ? 187704 : 187701); break;
                case 1: itemid = (uint)(RealmStandard == Realms.REALMS_REALM_DESTRUCTION ? 187705 : 187702); break;
                case 2: itemid = (uint)(RealmStandard == Realms.REALMS_REALM_DESTRUCTION ? 187706 : 187703); break;
            }


            if (player == Owner)
            {
                if (player.ItmInterface.GetItemInSlot(14) == null)
                {
                    player.ItmInterface.CreateItem(ItemService.GetItem_Info(itemid), 1, 14);
                    player.ItmInterface.SendEquipped(player);
                    player.ItmInterface.SendEquipped(null);
                }
                else
                {
                    player.ItmInterface.CreateItem(ItemService.GetItem_Info(itemid), 1);
                }
            }
            else if(player.Realm == RealmStandard)
            {
                Character_mail Mail = new Character_mail();
                Mail.Guid = CharMgr.GenerateMailGuid();
                Mail.CharacterId = Owner.CharacterId;
                Mail.CharacterIdSender = player.CharacterId;
                Mail.SenderName = player.Name;
                Mail.ReceiverName = Owner.Name;
                Mail.SendDate = (uint)TCPManager.GetTimeStamp();
                Mail.Title = "Guild Standard";
                Mail.Content = "Found your Guild Standard";
                Mail.Money = 0;
                Mail.Opened = false;
                Mail.Items.Add(new MailItem(itemid,1));
                CharMgr.AddMail(Mail);
            }
            else
            {
                player.AddRenown(600,false);

            }

            
            player.PlantedStandard = null;
            Dispose();

        }

        protected override void SendCreateMonster(Player plr)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_CREATE_MONSTER);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(0);

            Out.WriteUInt16(Heading);
            Out.WriteUInt16((ushort)WorldPosition.Z);
            Out.WriteUInt32((uint)WorldPosition.X);
            Out.WriteUInt32((uint)WorldPosition.Y);
            Out.WriteUInt16(0); // Speed Z
            // 18
            Out.WriteUInt16(1578);   //1578    1583???

            Out.WriteByte(50);
            Out.WriteByte(Owner.GldInterface.Guild.Info.Level);
            if(RealmStandard == Realms.REALMS_REALM_DESTRUCTION)
                Out.WriteByte(128);
            else
                Out.WriteByte(64);
            Out.Fill(0, 6);
            Out.WriteUInt16(9);
            Out.Fill(0, 13);

            Out.WriteByte(0x02);
            Out.WriteByte(0x17);
            Out.WriteByte(0x19);
            Out.WriteByte(0x01);

            Owner.GldInterface.Guild.BuildHeraldry(Out);
            Log.Info("",""+ Owner.GldInterface.Guild.GetBannerPost(Bannertyp));
            Out.WriteByte(Owner.GldInterface.Guild.GetBannerPost(Bannertyp));
            //Out.WriteByte(0x01);
            Out.WriteByte(0x02);


            Out.WriteStringBytes(Name);

            Out.WriteHexStringBytes("000000100303010A0000001205002905CAA286BB2910640005040000100343002905000000");
            

            plr.SendPacket(Out);
        
        }
    }
}