/* Londo
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WarGameServer.Protocol;
using WarServer.Game;
using WarServer.Game.Entities;
using WarServer.Services.Combat;
using WarServer.Services.Data;
using WarShared;
using WarShared.Data;
using WarShared.Net;
using WarShared.Protocol;
using static WarGameServer.Protocol.F_GROUP_COMMAND;

namespace WarServer.Services.PlayerSvc
{
    public class PlayerService : ServiceBase
    {
        public override string Name => "PlayerService";
        public InventoryService InventorySvc => GetService<InventoryService>();
        public CareerPackageService CareerPackageSvc => GetService<CareerPackageService>();

        public PlayerService(IWarServiceProvider provider, CancellationTokenSource token) : base(provider, token)
        {
        }

        public async Task InitPlayer(GameClient client)
        {
            if (client.DisableInit)
                return;

            bool newChar = client.PlayerData.UserData == null;

            await SendStats(client);
            await client.Send(F_CHARACTER_INFO.CreateCareerInfo(client.Player));
            await client.Send(S_PLAYER_INITTED.Create(client.Player));
            await client.Send(F_BAG_INFO.CreateBag(client.Player));
            await CareerPackageSvc.SendPackages(client);
            await client.Send(F_PLAYER_RANK_UPDATE.Create(client.Player));
            await client.Send(F_PLAYER_RENOWN.Create(client.Player));
            await SendStats(client);
            await InventorySvc.SendItemSetInfo(client);
            await client.Send(F_GET_ITEM.Create(client.Player.Inventory.Values.ToList()));
            await InventorySvc.ApplyItemSetAbilityBonuses(client);
            await InventorySvc.GrantItemAbilitiesAndBuffs(client);
            await client.Send(F_PLAYER_WEALTH.Create(client.Player));

            if (!newChar) //send abilities without updating ui (client data will do that
                await CareerPackageSvc.SendVerifyPackageAbilities(client, true);

            await client.Send(S_PLAYER_LOADED.Create());
            await client.Send(WarGameServer.Protocol.F_TACTICS.Create(client.Player.GetSlottedTactics().Select(e => (ushort)e.ID).ToList()));
            await client.Send(F_MORALE_LIST.Create(client.Player.GetSlottedMorales()));
            await client.Send(F_PLAYER_INIT_COMPLETE.Create(client.Player));

            if (newChar) //send abilities to update ui
            {
                client.Player.Data.UserData = new byte[1024];
                await client.Send(WarGameServer.Protocol.F_CLIENT_DATA.Create( client.Player));
                await CareerPackageSvc.SendVerifyPackageAbilities(client, true);
            }
            else
                await client.Send(WarGameServer.Protocol.F_CLIENT_DATA.Create(client.Player));

            await client.Player.InitVitals();
            await SendStats(client);

            //await client.Player.Buffs.SendBuffsTo(client);
            client.JumpToZone = null;

            await client.Player.UpdateVelocity(true);
            await client.Player.Controller.SendWorld(client.Player);
            await client.Player.Send(F_INTERACT_RESPONSE.CreateScenarioQueue(GetService<DataService>().Scenarios.Values.Where(e => e.Enabled).ToList()));
        }

        public async Task SendStats(GameClient client)
        {
            var skillStats = await client.Player.GetSkillStats();
            await client.Send(F_PLAYER_STATS.Create(client.Player, skillStats));
            await client.Send(F_BAG_INFO.CreateStats(skillStats));
        }

        public async Task SetRenown(Player player, byte level)
        {
            byte prevLevel = (byte)player.EffectiveLevel;

            player.RenownLevel = level;
            await player.InitVitals();
            await player.Client.Send(F_PLAYER_RENOWN.Create(player));
            await CareerPackageSvc.SendVerifyPackageAbilities(player.Client, false);
            await player.Client.Send(WarGameServer.Protocol.F_TACTICS.Create(player.GetSlottedTactics().Select(e => (ushort)e.ID).ToList()));
            await SendStats(player.Client);
            await player.Client.Send(F_MORALE_LIST.Create(player.GetSlottedMorales()));
            await player.AdjustHealth(await GetService<CombatService>().GetNullComponent(player), 0);
            await GetService<CareerPackageService>().SendPackages(player.Client);

            //recaculate ability data
            await player.Client.Send( F_INTERACT_RESPONSE.Create(0, 5,level ));
        }

        public async Task SetLevel(Player player, byte level, bool buyAll = false)
        {
            byte prevLevel = (byte)player.EffectiveLevel;

            player.Level = level;
            await player.InitVitals();
            if ((byte)level > prevLevel)
                await player.Client.Send(F_PLAYER_LEVEL_UP.Create(player, (byte)(level - prevLevel)));

            await player.Controller.Broadcast(player, F_PLAYER_RANK_UPDATE.Create(player));
            await GetService<CareerPackageService>().SendVerifyPackageAbilities(player.Client, false);
            await GetService<CareerPackageService>().SendPackages(player.Client);

            await player.Client.Send(F_MORALE_LIST.Create(player.GetSlottedMorales()));
            await player.Client.Send(new F_TACTICS() { Tactics = player.GetSlottedTactics().Select(e => (ushort)e.ID).ToList() });
            await GetService<PlayerService>().SendStats(player.Client);

            await player.AdjustHealth(await GetService<CombatService>().GetNullComponent(player), 0);
            //recaculate ability data
            await player.Client.Send(F_INTERACT_RESPONSE.Create(0, 5, level));
        }

        public async Task QuitPlayer(GameClient client)
        {
            if (client.Player != null)
            {
                await client.Player.Controller.RemovePlayer(client.Player);
            }
        }

        private async Task EquipMorale(GameClient client, byte slotIndex, ushort abilityID)
        {
            var list = new List<PlayerPackageData>();

            var slotted = client.Player.AbilityPackages.Values.Where(e => e.Active == slotIndex).FirstOrDefault();
            if (slotted != null)
            {
                slotted.Active = 0;
                list.Add(slotted);
            }

            var abilityPackage = client.Player.AbilityPackages.Values.Where(e => e.Package.AbilityID == abilityID).FirstOrDefault();
            if (abilityPackage != null)
            {
                var ability = GetService<DataService>().GetAbility(abilityID);
                abilityPackage.Active = ability.MoraleLevel;
                list.Add(abilityPackage);
            }
            else
                await client.Debug($"You do not have ability {abilityID}");
            if (list.Count > 0)
            {
                await GetService<DataService>().UpdatePackages(list);

                await client.Send( F_MORALE_LIST.Create(client.Player.GetSlottedMorales()));
            }
        }

        #region Events

        [EventHandler(EventType.PLAYER_DISCONNECTED_GAME)]
        public async Task PLAYER_DISCONNECTED_GAME(GameClient client)
        {
            await QuitPlayer(client);
        }

        #endregion Events

        #region Handlers

        [FrameRoute((int)GameOp.F_GROUP_COMMAND, FrameType.Game)]
        public async Task F_GROUP_COMMAND(GameClient client, F_GROUP_COMMAND frame)
        {
            switch (frame.Cmd)
            {
                case GroupCommand.ACCEPT_INVITATION:
                    if (client.Player.PendingGroup.Group != null)
                        await client.Player.PendingGroup.Group.AcceptInvite(client);
                    break;

                case GroupCommand.DECLINE_INVITATION:
                    if (client.Player.PendingGroup.Group != null)
                        await client.Player.PendingGroup.Group.DeclineInvite(client);
                    break;

                case GroupCommand.REMOVE_CHARACTER:
                    if (client.Player.Group != null)
                        await client.Player.Group.LeaveGroup(client.Player);
                    break;

                case GroupCommand.CLAIM_ASSIST:
                    if (client.Player.Group != null)
                        await client.Player.Group.ClaimMainAssist(client.Player);
                    break;
            }
            await Task.CompletedTask;
        }

        [FrameRoute((int)GameOp.F_TACTICS, FrameType.Game)]
        public async Task F_TACTICS(GameClient client, F_TACTICS frame)
        {
            var slottedTactics = client.Player.GetSlottedTactics();

            var list = new List<PlayerPackageData>();

            //purge unslotted tactics
            foreach (var ca in client.Player.ActiveAbilityPackages.Select(e => e.Value))
            {
                var ability = ca.Package.Ability;
                if (ability.AbilityType == AbilityType.TACTIC)
                {
                    if (!frame.Tactics.Contains((ushort)ability.ID))
                    {
                        ca.Active = 0;
                        var buffs = await client.Player.Buffs.GetBuffsByAbilityID(ability.ID);
                        await GetService<CombatService>().AbEngine.RemoveBuffs(buffs);
                    }
                    else
                    {
                        ca.Active = frame.Tactics.IndexOf((ushort)ca.Package.AbilityID) + 1;
                    }

                    list.Add(ca);
                }
            }

            //slot tactics that werent slotted before, and activate their buffs
            foreach (var ca in client.Player.AbilityPackages.Where(e => e.Value.Active == 0
                && e.Value.Package.Ability.AbilityType == AbilityType.TACTIC
                && frame.Tactics.Contains((ushort)e.Value.Package.Ability.ID)).Select(e => e.Value))
            {
                ca.Active = frame.Tactics.IndexOf((ushort)ca.Package.AbilityID) + 1;
                list.Add(ca);

                await GetService<CombatService>().ApplyPassiveBuff(client.Player, ca.Package.Ability, false, 0, 0);
            }

            await GetService<DataService>().UpdatePackages(list);

            await client.Send(new F_TACTICS() { Tactics = client.Player.GetSlottedTactics().Select(e => (ushort)e.ID).ToList() });
        }

        [FrameRoute((int)GameOp.F_CLIENT_DATA, FrameType.Game)]
        public async Task F_CLIENT_DATA(GameClient client, F_CLIENT_DATA frame)
        {
            if (client.PlayerData.UserData == null)
                client.PlayerData.UserData = new byte[1024];

            using (MemoryStream ms = new MemoryStream(client.PlayerData.UserData))
            {
                ms.Position = frame.ClientOffset;
                ms.Write(frame.ClientData, 0, frame.ClientData.Length);
            }

            client.PlayerData.Changed = true;

            await Task.CompletedTask;
        }

        #endregion Handlers
    }
}
*/