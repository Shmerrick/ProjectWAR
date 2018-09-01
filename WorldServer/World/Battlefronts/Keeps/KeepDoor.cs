using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using FrameWork;
using GameData;
using WorldServer.Services.World;
using WorldServer.World.Map;

namespace WorldServer.World.BattleFronts.Keeps
{
    public enum KeepDoorType
    {
        None,
        InnerMain,
        OuterMain,
        InnerPostern,
        OuterPostern
    }
    public class KeepDoor
    {
        public RegionMgr Region;
        public KeepGameObject GameObject;
        public Keep_Door Info;
        public Keep Keep;

        public KeepDoor(RegionMgr region, Keep_Door info, Keep keep)
        {
            Region = region;
            Info = info;
            Keep = keep;
        }

        public class KeepGameObject : GameObject
        {
            KeepDoor _keepDoor;
            Keep _keep;

            public uint DoorId
            {
                get
                {
                    if(_keepDoor != null && _keepDoor.Info != null)
                        return _keepDoor.Info.DoorId;
                    return 0;
                }
            }

            private readonly Point3D[] _enterExitPoints = new Point3D[2];

            public KeepGameObject(GameObject_spawn spawn, KeepDoor keepDoor, Keep keep)
            {
                _keep = keep;
                Spawn = spawn;
                Name = spawn.Proto.Name;
                _keepDoor = keepDoor;

                if (Constants.DoomsdaySwitch == 0)
                {
                    if (keepDoor.Info.Number == (int)KeepDoorType.InnerMain || keepDoor.Info.Number == (int)KeepDoorType.OuterMain)
                    {
                        Realm = keep.Realm;
                        Spawn.Proto.HealthPoints = (uint)_keep.Tier * 100000;
                    }
                }
                else
                {
                    if (keepDoor.Info.Number == (int)KeepDoorType.InnerMain || keepDoor.Info.Number == (int)KeepDoorType.OuterMain)
                    {
                        Realm = keep.Realm;
                        Spawn.Proto.HealthPoints = 4 * 500000;
                    }
                }

                _enterExitPoints[0] = new Point3D(_keepDoor.Info.TeleportX1, _keepDoor.Info.TeleportY1, _keepDoor.Info.TeleportZ1);
                _enterExitPoints[1] = new Point3D(_keepDoor.Info.TeleportX2, _keepDoor.Info.TeleportY2, _keepDoor.Info.TeleportZ2);

                EvtInterface.AddEventNotify(EventName.OnReceiveDamage, OnReceiveDamage);
            }

            // RB   5/20/2016   Keep doors should not regenerate naturally.
            public override void UpdateHealth(long tick)
            {
                return;
            }

            protected override void SetDeath(Unit killer)
            {
                _keep.ResetSafeTimer();
                base.SetDeath(killer);
                OpenDoor(false);
                EvtInterface.RemoveEventNotify(EventName.OnReceiveDamage, OnReceiveDamage);
                _keep.OnDoorDestroyed(_keepDoor.Info.Number, killer.Realm);
                Occlusion.SetFixtureVisible(_keepDoor.Info.DoorId, false);
            }

            /// <summary>Inflicts damage upon this unit and returns whether lethal damage was dealt.</summary>
            public override bool ReceiveDamage(Unit caster, uint damage, float hatredScale = 1f, uint mitigation = 0)
            {
                // RB   6/8/2016    Posterns shouldn't be dealt damage.
                if (_keepDoor != null && _keepDoor.Info.Number != (int)KeepDoorType.InnerMain && _keepDoor.Info.Number != (int)KeepDoorType.OuterMain)
                    return false;

                bool wasKilled = false;

                if (IsDead || PendingDisposal || IsInvulnerable)
                    return false;

                lock (DamageApplicationLock)
                {
                    if (IsDead)
                        return false;
                    if (damage >= Health)
                    {
                        wasKilled = true;

                        damage = Health;
                        _health = 0;

                        TotalDamageTaken = PendingTotalDamageTaken;
                        PendingTotalDamageTaken = 0;
                    }
                    else
                        _health = Math.Max(0, _health - damage);
                }

                _keep.SendKeepStatus(null);

                CbtInterface.OnTakeDamage(caster, damage, 1f);

                Siege siege = caster as Siege;
                if (siege != null)
                {
                    foreach (KeyValuePair<Player, byte> p in siege.SiegeInterface.Players)
                    {
                        p.Key.CbtInterface.OnDealDamage(this, damage);
                        Region.Campaign.AddContribution(p.Key, 10);
                    }
                }
                else
                    caster.CbtInterface.OnDealDamage(this, damage);

                LastHitOid = caster.Oid;
                
                if (wasKilled)
                    SetDeath(caster);

                return wasKilled;
            }

            public bool OnReceiveDamage(Object sender, object args)
            {
                // RB   6/13/2016   args is damage value 
                if (args != null && Convert.ToInt32(args) > 1)
                    _keep.ResetSafeTimer();

                _keep.OnKeepDoorAttacked(_keepDoor.Info.Number, PctHealth);

                return false;
            }
           
            /*
            // Keep doors block all hits that are not from ST siege
            public override void ModifyDamageIn(AbilityDamageInfo damageInfo)
            {
                // T4 doors have 4k hp.
                // T4 cannons hit for about 2.2k -> 50%
                switch (damageInfo.SubDamageType)
                {
                    case SubDamageTypes.Cannon:
                        switch (_keep.Rank)
                        {
                            case 0:
                                damageInfo.Mitigation = damageInfo.Damage*0.95f;
                                damageInfo.Damage *= 0.05f; // 0.25% per full power shot
                                break;
                            case 1:
                                damageInfo.Mitigation = damageInfo.Damage * 0.98f;
                                damageInfo.Damage *= 0.02f; // 0.1%
                                break;
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                                damageInfo.Mitigation = damageInfo.Damage * 0.99f;
                                damageInfo.Damage *= 0.01f; // 0.05%
                                break;
                        }
                        break;
                    // Rams always hit for full damage, knocking 2.5% out of a door
                    case SubDamageTypes.Ram:
                        break;
                    default:
                        damageInfo.DamageEvent = (byte) CombatEvent.COMBATEVENT_BLOCK;
                        damageInfo.Mitigation = damageInfo.Damage;
                        damageInfo.Damage = 0;
                        damageInfo.PrecalcMitigation = damageInfo.Damage;
                        damageInfo.PrecalcDamage = 0;
                        break;
                }
            }
            */

            public override bool AllowInteract(Player interactor)
            {
                if (PctHealth < 50 && _keep.KeepStatus != KeepStatus.KEEPSTATUS_SAFE)
                {
                    interactor.SendClientMessage("You can't open a damaged door when the keep is not safe!");
                    return false;
                }
                if (interactor.Get2DDistanceToWorldPoint(_enterExitPoints[0]) <= 15 || interactor.Get2DDistanceToWorldPoint(_enterExitPoints[1]) <= 15)
                    return true;
                interactor.SendClientMessage("Too far away from the door's entry point.");
                return false;
            }

            public override void SendInteract(Player player, InteractMenu menu)
            {
                if (VfxState == 1)
                    return;

                if (_keep.Realm != player.Realm)
                {
                    if (_keepDoor.Info.GameObjectId != 72)
                    {
                        player.SendClientMessage("This main door is barricaded and requires a ram to open.");
                        return;
                    }

                    if (player.BuffInterface.GetBuff(14423, null) == null && player.BuffInterface.GetBuff(14427, null) == null && !_keep.AttackerCanUsePostern(_keepDoor.Info.Number))
                    {
                        //player.SendClientMessage("This postern door is barred. To gain access, you must bypass it with an ability, or destroy the associated main door and lower the keep's rank to zero.");
                        player.SendClientMessage("This postern door is barred. To gain access, you must bypass it with an ability, or destroy the associated main door and lower the keep's rank to three.");
                        return;
                    }

                    if (player.CurrentSiege != null)
                    {
                        player.SendClientMessage("You can't take siege weapons through a postern door!");
                        return;
                    }
                }

                if (_keepDoor.Info.TeleportX1 != 0) //new coodinates defined
                {
                    var d1 = player.GetDistanceToWorldPoint(_enterExitPoints[0]);
                    var d2 = player.GetDistanceToWorldPoint(_enterExitPoints[1]);
                    if(d1 > d2)
                        player.IntraRegionTeleport((uint)_enterExitPoints[0].X, (uint)_enterExitPoints[0].Y, (ushort)_enterExitPoints[0].Z, (ushort)_keepDoor.Info.TeleportO1);
                    else
                        player.IntraRegionTeleport((uint)_enterExitPoints[1].X, (uint)_enterExitPoints[1].Y, (ushort)_enterExitPoints[1].Z, (ushort)_keepDoor.Info.TeleportO2);
                }
                else //use legacy jump formula
                {
                    var x = _keep.Doors[0].Info.TeleportX1;
                    float dx = Spawn.WorldX - player._Value.WorldX;
                    float dy = Spawn.WorldY - player._Value.WorldY;

                    double heading = Math.Atan2(-dx, dy);

                    if (heading < 0)
                        heading += 4096;

                    int distanceToMove = 15;

                    if (_keepDoor.Info.Number == (int)KeepDoorType.None)
                        distanceToMove = 25;

                    int distance = (int)(distanceToMove * 13.2f);
                    double angle = heading;
                    double targetX = Spawn.WorldX - (Math.Sin(angle) * distance);
                    double targetY = Spawn.WorldY + (Math.Cos(angle) * distance);

                    int newX;
                    int newY;

                    if (targetX > 0)
                        newX = (int)targetX;
                    else
                        newX = 0;

                    if (targetY > 0)
                        newY = (int)targetY;
                    else
                        newY = 0;

                    player.IntraRegionTeleport((uint)newX, (uint)newY, (ushort)Spawn.WorldZ, (ushort)player._Value.WorldO);
                }
            }

            public void SetAttackable(bool attackable)
            {
                if (IsInvulnerable == attackable)
                {
                    IsInvulnerable = !attackable;

                    foreach (Player plr in PlayersInRange)
                        SendMeTo(plr);
                }
            }

            public override void RezUnit()
            {
                _keepDoor.GameObject = new KeepGameObject(Spawn, _keepDoor, _keep);
                Region.AddObject(_keepDoor.GameObject, Spawn.ZoneId);

                Occlusion.SetFixtureVisible(_keepDoor.Info.DoorId, true);
                Destroy();

                if (_keepDoor.Info.Number == (int)KeepDoorType.OuterMain && _keep.LastMessage >= Keep.KeepMessage.Outer0)
                    _keep.LastMessage = Keep.KeepMessage.Safe;
                else if (_keepDoor.Info.Number == (int)KeepDoorType.InnerMain && _keep.LastMessage >= Keep.KeepMessage.Inner0)
                    _keep.LastMessage = Keep.KeepMessage.Outer0;
            }
        }

        public void Spawn()
        {
            GameObject?.Destroy();

            GameObject_proto proto = GameObjectService.GetGameObjectProto(Info.GameObjectId);
            if (proto == null)
            {
                Log.Error("KeepDoor", "No Door Proto");
                return;
            }

            // Log.Info("KeepDoor", "Spawning Keep Door = " + Info.DoorId + " Number = " + Info.Number);

            GameObject_spawn spawn = new GameObject_spawn
            {
                Guid = (uint)GameObjectService.GenerateGameObjectSpawnGUID(),
                WorldO = Info.O,
                WorldY = Info.Y,
                WorldZ = Info.Z,
                WorldX = Info.X,
                ZoneId = Info.ZoneId,
                DoorId = Info.DoorId,
            };

            spawn.BuildFromProto(proto);

            GameObject = new KeepGameObject(spawn, this, Keep);
            Region.AddObject(GameObject, spawn.ZoneId);

            GameObject.SetAttackable(Keep.KeepStatus != KeepStatus.KEEPSTATUS_LOCKED);

            Occlusion.SetFixtureVisible(Info.DoorId, true);
        }


       
    }
}