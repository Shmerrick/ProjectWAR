-- 87_restore_items_confirmed_by_live_captures.sql
--
-- Adds 95 items that exist in the client's art, in Londo's database, AND in the live server's own
-- packets -- and in neither of our item tables.
--
-- WHY THESE 99 AND NOT THE OTHER 1,952.
--
-- `objects.csv` holds 9,905 art rows and 4,949 have no item pointing at them. Most are not missing
-- items: 1,734 are WORLD OBJ scenery, 514 NPC-only art, 332 Test/CCTest/placeholder/not-used, 319
-- unnamed. That leaves 2,051 pieces of plausible item art with no item.
--
-- Art existing is not evidence that an item existed. WAR shipped art for cut content, per-career
-- armour variants that were never itemised, and dev scratch. Inventing 2,051 items to match 2,051
-- models would manufacture exactly the fiction this project exists to remove. So the gate is that
-- a source has to name the thing:
--
--   199 of the 2,051 are named by a Londo row carrying name, type, slot and the same ModelID,
--       where neither that id nor that name appears anywhere in our table.
--    99 of those 199 are ALSO in the 1,027 live packet captures -- the server itself sent the item
--       to a player, at that entry id.
--
-- On those 99 the sources are independent and they agree: 99 of 99 ModelIds match and 97 of 99
-- names match. Where a name differs the capture wins, because a live packet outranks another
-- emulator's reconstruction (docs/CROSS_REPO.md, order of authority).
--
-- The other 100 have Londo only. They are probably real, but "probably" is how this database got
-- into its current state, so they are left out.
--
-- 95 of the 99 are inserted. Two were dropped as obvious dev rows -- `NPC_SK_Catcher_01**TEST**`
-- and `Dagger 1` -- and two more were duplicate ids that only appeared because the capture index
-- holds several sightings of the same item. The temporary table's PRIMARY KEY caught those rather
-- than letting them through as silently doubled inserts.
--
-- ONE MAPPING THAT IS NOT OBVIOUS. Londo's `DPS` column is overloaded: armour rows carry a value
-- there with Speed 0, weapons carry both. That is the shape of the wire field, which our own
-- `Item.BuildItem` writes as `info.Dps > 0 ? info.Dps : info.Armor`. So the value goes to `Dps`
-- when Speed > 0 and to `Armor` otherwise -- without that split a chest piece would have been
-- imported with 1188 DPS.
--
-- LEFT AT DEFAULTS ON PURPOSE: Stats, Effects, Skills, SpellId, StartQuest and the TokUnlock
-- columns. Londo does not carry them in a form worth trusting, and a wrong stat block is worse
-- than an absent one.
--
-- Both tables are written; `UseMythicActionCoverageTables` ships true, so writing only `item_infos`
-- would be invisible to the running server. Item data is cached at boot, so restart.

CREATE TEMPORARY TABLE tmp_new_items (
    Entry INT UNSIGNED NOT NULL PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    Description VARCHAR(255) NOT NULL,
    Type TINYINT UNSIGNED NOT NULL,
    Race TINYINT UNSIGNED NOT NULL,
    ModelId INT UNSIGNED NOT NULL,
    SlotId SMALLINT UNSIGNED NOT NULL,
    Rarity TINYINT UNSIGNED NOT NULL,
    Career INT UNSIGNED NOT NULL,
    Bind TINYINT UNSIGNED NOT NULL,
    Dps SMALLINT UNSIGNED NOT NULL,
    Speed SMALLINT UNSIGNED NOT NULL,
    Armor SMALLINT UNSIGNED NOT NULL,
    MinRank TINYINT UNSIGNED NOT NULL,
    MinRenown TINYINT UNSIGNED NOT NULL,
    SellPrice INT UNSIGNED NOT NULL,
    TalismanSlots TINYINT UNSIGNED NOT NULL,
    MaxStack SMALLINT UNSIGNED NOT NULL,
    ObjectLevel TINYINT UNSIGNED NOT NULL,
    UniqueEquiped TINYINT UNSIGNED NOT NULL,
    TwoHanded TINYINT NOT NULL,
    DyeAble TINYINT UNSIGNED NOT NULL,
    ItemSet INT UNSIGNED NULL
) ENGINE=InnoDB;

INSERT INTO tmp_new_items
(Entry,Name,Description,Type,Race,ModelId,SlotId,Rarity,Career,Bind,Dps,Speed,Armor,MinRank,MinRenown,SellPrice,TalismanSlots,MaxStack,ObjectLevel,UniqueEquiped,TwoHanded,DyeAble,ItemSet)
VALUES
(8316,'Tempest Horn T-Shirt!','',0,0,636,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,NULL),
(36921,'Blood Skull','',21,0,479,0,0,0,0,0,0,334,0,0,0,0,1,0,0,0,0,NULL),
(36922,'Blood Skull','',21,0,479,0,0,0,0,0,0,334,0,0,0,0,1,0,0,0,0,NULL),
(36923,'Blood Skull','',21,0,479,0,0,0,0,0,0,334,0,0,0,0,1,0,0,0,0,NULL),
(36925,'Blood Skull','',21,0,479,0,0,0,0,0,0,334,0,0,0,0,1,0,0,0,0,NULL),
(36926,'Blood Skull','',21,0,479,0,0,0,0,0,0,334,0,0,0,0,1,0,0,0,0,NULL),
(36927,'Blood Skull','',21,0,479,0,0,0,0,0,0,334,0,0,0,0,1,0,0,0,0,NULL),
(65680,'Case of Captain\'s Elixir of Rejuvenation','Convert this case to unpack 10 Elixirs.',31,0,3959,0,3,0,0,0,0,0,0,81,1137,0,1,70,0,0,0,NULL),
(65681,'Case of Captain\'s Alacritious Elixir','Convert this case to unpack 10 Elixirs.',31,0,3960,0,3,0,0,0,0,0,0,81,1137,0,1,70,0,0,0,NULL),
(204114,'Baker\'s Mistake','A wonderful pie for throwing at enemies. Right-click on item to activate.',0,0,1306,0,3,0,0,0,0,0,0,0,0,0,1,0,0,0,0,NULL),
(208049,'Silver Battlepak','This item will summon your mighty steed. To dismount, activate the item.',0,1,9326,0,3,0,0,0,0,0,40,60,800000,0,1,0,0,0,0,NULL),
(208062,'Cream Battle Boar','This item will summon your mighty steed. To dismount, activate the item.',0,2,9332,0,3,0,0,0,0,0,40,60,800000,0,1,0,0,0,0,NULL),
(208461,'Doomflayer Insignia','A seal of the Doomflayer. These can be used to trade for incredible equipment and supplies at a quartermaster.',36,0,8871,0,4,0,0,0,0,0,0,0,0,0,1,0,0,0,0,NULL),
(208473,'War Chest','This was looted off the body of a fallen foe. The previous owner appears to have been storing some War Crests in this!\n\nConvert this item to claim 2500 War Crests.',36,0,9502,0,5,0,0,0,0,0,0,0,0,0,1,0,0,0,0,NULL),
(436480,'Sovereign Kladgird of the Unstoppable','',0,0,2228,28,5,1,0,0,0,0,40,77,3412,1,1,65,0,0,0,4672),
(436586,'Sovereign Bladebelt of the Soulstealer','',0,0,4933,28,5,4194304,0,0,0,0,40,77,3412,1,1,65,0,0,0,4694),
(436610,'Sovereign Paincloak of the Soulstealer','',0,0,4938,27,5,4194304,0,0,0,0,40,78,3412,0,1,65,0,0,0,4694),
(436622,'Sovereign Breastscale of the Soulstealer','',22,0,4934,20,5,4194304,0,0,0,650,40,80,4875,1,1,65,0,0,0,4694),
(520040,'Mythic Elixir','Drinking this liquid will immediately infuse you with the knowledge and wisdom equivalent to someone of Career Rank 40 and Renown Rank 100. There\'s also 100 gold and 2,500 War Crests inside. Don\'t drink that.',0,0,4681,0,5,0,0,0,0,0,0,0,0,0,1,0,0,0,0,NULL),
(520041,'Mostly-Mythic Elixir','Drinking this liquid will immediately infuse you with the knowledge and wisdom equivalent to someone of Career Rank 40 and Renown Rank 80 because progression packs are a thing. There\'s also 100 gold and 2,500 War Crests inside. We didn\'t forget about you.',0,0,4683,0,5,0,0,0,0,0,0,0,0,0,1,0,0,0,0,NULL),
(652203,'Trespasser\'s Darkstaff','',11,16,5508,10,4,0,0,348,340,0,14,0,1330,2,1,19,0,1,0,NULL),
(1000027,'Dune Stalker\'s Chitin','This very potent ingredient can be used to create armor potions.',34,0,387,0,2,0,0,0,0,0,0,0,600,0,1,40,0,0,0,NULL),
(1000035,'Supple Scale','This very potent ingredient can be used to create armor potions.',34,0,387,0,1,0,0,0,0,0,0,0,487,0,1,39,0,0,0,NULL),
(2015182,'Duelist\'s Mediocre Silveraxe','',2,0,3484,10,2,262144,0,116,340,0,2,0,180,0,1,3,0,1,0,NULL),
(2015434,'Notched Mailcoat of Fortification','',19,0,3589,20,1,262144,0,0,0,30,0,0,150,0,1,3,0,0,0,NULL),
(2015610,'Ragged Band of the Spirit','',0,0,965,31,1,262144,0,0,0,0,0,0,150,0,1,5,0,0,0,NULL),
(2015658,'Worked Shinsteels of Brawn','',19,0,3590,22,2,262144,0,0,0,48,0,0,288,0,1,6,0,0,0,NULL),
(2015730,'Ragged Greataxe of Triumph','',2,0,3485,10,1,262144,0,145,340,0,0,0,250,0,1,5,0,0,0,NULL),
(2015910,'Feeble Band of the Flesh','',0,0,965,31,1,262144,0,0,0,0,0,0,210,0,1,7,0,0,0,NULL),
(5000001,'Templar\'s Cracked Demi-Grevieres','',19,0,8611,22,1,0,0,0,0,24,3,0,120,0,1,3,0,0,0,NULL),
(5000006,'Gladiator\'s Mediocre Armbands','',19,0,8616,21,2,0,0,0,0,32,3,0,192,0,1,4,0,0,0,NULL),
(5000016,'Martyr\'s Standard Band','',0,0,963,31,2,0,0,0,0,0,5,0,216,0,1,6,0,0,0,NULL),
(5000019,'Defender\'s Standard Relic','',0,0,960,31,2,0,0,0,0,0,5,0,216,0,1,6,0,0,0,NULL),
(5000028,'Defender\'s Standard Armbands','',19,0,8617,21,2,0,0,0,0,48,5,0,288,1,1,6,0,0,0,NULL),
(5000262,'Cracked Brogans of Fortification','',18,0,8611,22,1,0,0,0,0,8,2,0,80,0,1,2,0,0,0,NULL),
(5000266,'Cracked Baldric of the Elements','',19,0,8612,28,1,0,0,0,0,0,2,0,70,0,1,2,0,0,0,NULL),
(5000270,'Cracked Mark of the Elements','',0,0,459,31,1,0,0,0,0,0,2,0,60,0,1,2,0,0,0,NULL),
(5000274,'Cracked Relic of the Elements','',0,0,960,31,1,0,0,0,0,0,2,0,60,0,1,2,0,0,0,NULL),
(5002512,'Ragged Brogans of Fortification','',18,0,8611,22,1,0,0,0,0,24,6,0,240,1,1,6,0,0,0,NULL),
(5002523,'Ragged Necklace of the Flesh','',0,0,954,31,1,0,0,0,0,0,6,0,180,0,1,6,0,0,0,NULL),
(5004752,'Fitted Besagews of Brawn','',18,0,8631,24,2,0,0,0,0,59,12,0,702,1,1,13,0,0,0,NULL),
(5004758,'Fitted Garment of Insight','',6,0,8625,20,2,0,0,0,0,65,12,0,780,1,1,13,0,0,0,NULL),
(5004825,'Fitted Armbands of Volition','',19,0,8628,21,2,0,0,0,0,104,12,0,624,1,1,13,0,0,0,NULL),
(5004866,'Fitted Plackart of Dexterity','',20,0,8624,28,2,0,0,0,0,0,12,0,546,1,1,13,0,0,0,NULL),
(5004870,'Fitted Waistband of Volition','',22,0,8624,28,2,0,0,0,0,0,12,0,546,1,1,13,0,0,0,NULL),
(5004878,'Fitted Ailettes of Volition','',20,0,8629,24,2,0,0,0,0,176,12,0,702,1,1,13,0,0,0,NULL),
(5005028,'Primal Cinch of Volition','',6,0,8623,28,3,0,0,0,0,0,14,0,773,1,1,17,0,0,0,NULL),
(5007031,'Decent Ornament of Brawn','',0,0,8896,31,2,0,0,0,0,0,16,0,612,0,1,17,0,0,0,NULL),
(5007042,'Decent Ailettes of Brawn','',20,0,8631,24,2,0,0,0,0,231,16,0,918,1,1,17,0,0,0,NULL),
(5007180,'Decent Pallium of Dexterity','',0,0,8899,31,2,0,0,0,0,0,16,0,612,0,1,17,0,0,0,NULL),
(5009261,'Grim Mantellum of Discipline','',22,0,8630,24,2,0,0,0,0,198,20,0,1188,1,1,22,0,0,0,NULL),
(5009264,'Grim Breastplate of Skill','',20,0,8626,20,2,0,0,0,0,331,20,0,1320,1,1,22,0,0,0,NULL),
(5009424,'Grim Hood of Dexterity','',6,0,8632,23,2,0,0,0,0,99,20,0,1188,1,1,22,0,0,0,NULL),
(5009444,'Grim Mitts of Perseverance','',18,0,8628,21,2,0,0,0,0,88,20,0,1056,1,1,22,0,0,0,NULL),
(5009456,'Grim Cinch of Volition','',6,0,8623,28,2,0,0,0,0,0,20,0,924,1,1,22,0,0,0,NULL),
(5011323,'Scout\'s Nefarious Garment','',6,0,8638,20,4,0,0,0,0,170,25,0,2380,1,1,34,0,0,0,NULL),
(5011344,'Sage\'s Nefarious Hood','',6,0,8645,23,4,0,0,0,0,153,25,0,2142,1,1,34,0,0,0,NULL),
(5011554,'Capable Bangle of Skill','',0,0,8905,31,2,0,0,0,0,0,24,0,936,0,1,26,0,0,0,NULL),
(5011740,'Capable Haqueton of Dexterity','',18,0,8638,20,2,0,0,0,0,130,24,0,1560,1,1,26,0,0,0,NULL),
(5013773,'Forceful Mitts of Brawn','',18,0,8639,21,2,0,0,0,0,120,28,0,1440,1,1,30,0,0,0,NULL),
(5013857,'Forceful Locket of Skill','',0,0,8906,31,2,0,0,0,0,0,28,0,1080,0,1,30,0,0,0,NULL),
(5013898,'Forceful Baldric of Volition','',19,0,8636,28,2,0,0,0,0,0,28,0,1260,1,1,30,0,0,0,NULL),
(5016331,'Martyr\'s Fierce Frock','',22,0,8653,20,2,0,0,0,0,330,31,0,1980,1,1,33,0,0,0,NULL),
(5018967,'Suitable Badge of Dexterity','',0,0,8914,31,2,0,0,0,0,0,34,0,1296,0,1,36,0,0,0,NULL),
(5019024,'Suitable Jewelry of Dexterity','',0,0,8910,31,2,0,0,0,0,0,34,0,1296,0,1,36,0,0,0,NULL),
(5023254,'Staunch Coiffure of Discipline','',22,0,8660,23,2,0,0,0,0,351,37,0,2106,1,1,39,0,0,0,NULL),
(5023256,'Staunch Hood of Discipline','',6,0,8660,23,2,0,0,0,0,176,37,0,2106,1,1,39,0,0,0,NULL),
(5023257,'Staunch Ailettes of Skill','',20,0,8656,24,2,0,0,0,0,528,37,0,2106,1,1,39,0,0,0,NULL),
(5023259,'Staunch Besagews of Brawn','',18,0,8656,24,2,0,0,0,0,176,37,0,2106,1,1,39,0,0,0,NULL),
(5023283,'Staunch Brogans of Skill','',18,0,8648,22,2,0,0,0,0,156,37,0,1872,1,1,39,0,0,0,NULL),
(5023288,'Staunch Cinch of Insight','',6,0,8650,28,2,0,0,0,0,0,37,0,1638,1,1,39,0,0,0,NULL),
(5023290,'Staunch Girdle of Skill','',18,0,8650,28,2,0,0,0,0,0,37,0,1638,1,1,39,0,0,0,NULL),
(5023315,'Staunch Besagews of Skill','',18,0,8657,24,2,0,0,0,0,176,37,0,2106,1,1,39,0,0,0,NULL),
(5023364,'Staunch Cap of Perseverance','',18,0,8662,23,2,0,0,0,0,176,37,0,2106,1,1,39,0,0,0,NULL),
(5023368,'Staunch Hood of Volition','',6,0,8662,23,2,0,0,0,0,176,37,0,2106,1,1,39,0,0,0,NULL),
(5023436,'Staunch Frock of Volition','',22,0,8652,20,2,0,0,0,0,390,37,0,2340,1,1,39,0,0,0,NULL),
(5023446,'Staunch Cuissarts of Dexterity','',20,0,8649,22,2,0,0,0,0,470,37,0,1872,1,1,39,0,0,0,NULL),
(5023450,'Staunch Slipshoes of Volition','',22,0,8649,22,2,0,0,0,0,312,37,0,1872,1,1,39,0,0,0,NULL),
(5023452,'Staunch Thinsoles of Dexterity','',6,0,8649,22,2,0,0,0,0,156,37,0,1872,1,1,39,0,0,0,NULL),
(5023494,'Staunch Garment of Perseverance','',6,0,8652,20,2,0,0,0,0,195,37,0,2340,1,1,39,0,0,0,NULL),
(5023514,'Staunch Girdle of Volition','',18,0,8650,28,2,0,0,0,0,0,37,0,1638,1,1,39,0,0,0,NULL),
(5023528,'Staunch Buckle of Volition','',0,0,8912,31,2,0,0,0,0,0,37,0,1404,0,1,39,0,0,0,NULL),
(5750014,'Respectable Breastplate of Volition','',20,0,8653,20,2,0,0,0,0,1068,40,80,4800,1,1,80,0,0,0,NULL),
(5751083,'Imposing Thinsoles of Volition','',6,0,8664,22,3,0,0,0,0,296,40,87,4524,1,1,87,0,0,0,NULL),
(5751585,'Stately Baldric of Skill','',0,0,8667,28,3,0,0,0,0,0,40,95,4322,1,1,95,0,0,0,NULL),
(5757070,'Titan\'s Exalted Chestplate','',20,0,9729,20,4,0,0,0,0,1170,40,97,6790,1,1,97,0,0,0,NULL),
(5757164,'Fleet Stag Mantle','',0,0,7142,27,3,0,0,0,0,0,40,86,3913,0,1,86,0,0,0,NULL),
(5757588,'Warpforged Klad of the Unbreakable','',20,0,9710,20,5,1,0,0,0,1188,40,100,7500,1,1,100,0,0,0,4744),
(5757684,'Warpforged Ironmantle of the Unstoppable','',20,0,9706,24,5,1,0,0,0,1070,40,100,6750,1,1,100,0,0,0,4768),
(5757696,'Warpforged Greathelm of the Unstoppable','',20,0,9707,23,5,1,0,0,0,1070,40,100,6750,1,1,100,0,0,0,4768),
(5758890,'Grund\'Dammaz the Avenger','',2,1,8802,13,5,0,0,840,240,0,40,100,7500,1,1,100,0,0,0,NULL),
(5759208,'War Cluster','A cluster of Fused War Crests. You can probably pry a few apart intact, but you had better hurry - it wont be long before these Crests fuse totally into an unusable hunk of metal.',0,0,7635,0,4,0,0,0,0,0,0,0,0,0,1,0,0,0,0,NULL),
(5900010,'Mailcoat of Brawn','',19,0,3589,20,2,262144,0,0,0,10,1,0,120,0,1,2,0,0,0,NULL),
(5900034,'Wristguards of Brawn','',19,0,3591,21,2,262144,0,0,0,8,1,0,96,0,1,2,0,0,0,NULL),
(5900058,'Shinsteels of Brawn','',19,0,3590,22,2,262144,0,0,0,8,1,0,96,0,1,2,0,0,0,NULL);

INSERT INTO mythic_src_item_infos
    (Entry, Name, Description, Type, Race, ModelId, SlotId, Rarity, Career, Skills, Bind, Armor,
     SpellId, Dps, Speed, MinRank, MinRenown, StartQuest, Stats, SellPrice, TalismanSlots, MaxStack,
     ScriptName, ObjectLevel, UniqueEquiped, TwoHanded, ItemSet, DyeAble, TokUnlock2, IsSiege, Source, TokUnlock3)
SELECT t.Entry, t.Name, t.Description, t.Type, t.Race, t.ModelId, t.SlotId, t.Rarity, t.Career, 0, t.Bind, t.Armor,
       0, t.Dps, t.Speed, t.MinRank, t.MinRenown, 0, '', t.SellPrice, t.TalismanSlots, t.MaxStack,
       '', t.ObjectLevel, t.UniqueEquiped, t.TwoHanded, t.ItemSet, t.DyeAble, 0, 0, 0, 0
FROM tmp_new_items t
WHERE NOT EXISTS (SELECT 1 FROM mythic_src_item_infos x WHERE x.Entry = t.Entry);

INSERT INTO item_infos
    (Entry, Name, Description, Type, Race, ModelId, SlotId, Rarity, Career, Skills, Bind, Armor,
     SpellId, Dps, Speed, MinRank, MinRenown, StartQuest, Stats, SellPrice, TalismanSlots, MaxStack,
     ScriptName, ObjectLevel, UniqueEquiped, TwoHanded, ItemSet, DyeAble, TokUnlock2, IsSiege, Source, TokUnlock3)
SELECT t.Entry, t.Name, t.Description, t.Type, t.Race, t.ModelId, t.SlotId, t.Rarity, t.Career, 0, t.Bind, t.Armor,
       0, t.Dps, t.Speed, t.MinRank, t.MinRenown, 0, '', t.SellPrice, t.TalismanSlots, t.MaxStack,
       '', t.ObjectLevel, t.UniqueEquiped, t.TwoHanded, t.ItemSet, t.DyeAble, 0, 0, 0, 0
FROM tmp_new_items t
WHERE NOT EXISTS (SELECT 1 FROM item_infos x WHERE x.Entry = t.Entry);

DROP TEMPORARY TABLE tmp_new_items;
