-- Authority: WAR-RE-Toolkit/libs/protocolservices/Packet Logs/bastion_stairs.txt.gz.
-- Packet ordinals are 1-based across both directions (Read-OfficialPackets.ps1).
USE war_world;

-- F_CREATE_MONSTER #71889/#72001: Kaarn, model 1251, scale byte payload +20 = 0x37.
-- Prototype 2000751 is used only by the zone-165 boss placement in the local data.
UPDATE creature_protos SET MinScale = 55, MaxScale = 55
WHERE Entry = 2000751 AND Model1 = 1251 AND MinScale = 36 AND MaxScale = 36;

-- S_PLAYER_INITTED #18276 sets Bastion's atlas shift (1,25).
-- F_CREATE_STATIC #46264: chest at client (51888,217783,13992), immediately before
-- Path of Fury completion #46268. World = client + (240 << 12) - (shift << 13).
-- Independently repeated in PATH OF FURY.log.txt.gz #25878/#25882.
UPDATE pquest_info
SET GoldChestWorldX = 1026736, GoldChestWorldY = 996023, GoldChestWorldZ = 13992
WHERE Entry = 333 AND ZoneId = 160
  AND GoldChestWorldX = 1027022 AND GoldChestWorldY = 997128 AND GoldChestWorldZ = 14006;
