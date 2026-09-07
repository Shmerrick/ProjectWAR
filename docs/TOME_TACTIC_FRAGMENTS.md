# Tome tactic fragments: the complete Tome of Knowledge unlock list

> **Each threshold unlocks a separately named tactic, not a rank of one tactic.** The Giant line's
> 5 / 10 / 15 give **Sky Titan's Bulwark**, **Sky Titan's Favor** and **Sky Titan's Strength** -
> three distinct abilities, each bought from a Librarian and slotted on its own. There are 27
> named tactics across the nine lines. The client's own fragment tooltip states it: "Collect more
> Giant Tactic Fragments to unlock higher and more powerful ranks of the Tome Tactic... the Tactic
> name will turn white in the list below and will be available for purchase from a Librarian in
> the Capital City."
>
> An unreachable threshold therefore costs a **specific named ability**, which is how this document
> and `Test-TomeTactics.ps1` now report it.

Every Tome of Knowledge entry that grants a bestiary tome tactic fragment, which line it
feeds, and whether anything in the world can actually award it.

## Where this comes from

The 1.4.8 client, first, exactly as `CLAUDE.md` requires:

* `interface/interfacecore/tome/tactics/acid_entries.csv` - the nine lines with their three
  thresholds and reward ids. Header: `ACID ID, String ID, Thresh. 1, Reward 1, Thresh. 2,
  Reward 2, Thresh. 3, Reward 3`.
* `interface/interfacecore/tome/tactics/tactic_entries.csv` - the tactic each reward id names.
* `interface/interfacecore/tome/bestiary/species.csv` - which species carries which line's
  counter, via `TOME_REWARD_ABILITY_COUNTER`, whose reward id is the line's ACID.

Names and unlock text below are the server's `tok_infos` rows, which carry the client's own
strings. Award paths were resolved against `creature_protos.TokUnlock`, `item_infos.TokUnlock`
1-3 and the `tok_bestiary` kill milestones. **No Return of Reckoning data was used**, and no
external source of any kind was needed - the client and the local database answered it in full.

Regenerate with `tools/validation/Test-TomeTactics.ps1`, which pins these counts.

## How a Tome entry actually unlocks, and why only one kind can break

Two separate mechanisms award Tome of Knowledge entries, and the distinction is the whole of
BUG-117.

**1. Direct link.** `creature_protos.TokUnlock`, or `item_infos.TokUnlock`/`TokUnlock2`/
`TokUnlock3`, names a Tome entry id outright. Kill that creature or acquire that item and exactly
that id unlocks. There is no ordering and no positions, so **this cannot become misaligned**. It
either points at something or it does not. All 20 remaining orphans are this kind: one-off
completion tasks that nothing references, so no player action can fire them.

**2. Positional array.** `tok_bestiary` holds one row per creature species with six columns --
`Kill1`, `Kill25`, `Kill100`, `Kill1000`, `Kill10000`, `Kill100000` -- each holding the Tome entry
id to award at that kill count. `TokInterface` counts kills per species and awards the id sitting
in the matching column.

Only the second kind can be *malformed*, because the mapping is by column position rather than by
identity, and the rows were populated by taking six **consecutive** entry ids from the species'
block. That is correct whenever a species' Tome entries are purely kill milestones -- which is 130
of the 131 counted species.

Metal Construct is the exception. Its block interleaves a completion task:

```
4350  encountered a Living Armor      -> Kill1
4351  killed 25 Living Armors         -> Kill25
4352  "You have completed: Anyone Have a Can Opener?"   <- a TASK, not a milestone
4353  killed 100 Living Armors        -> Kill100
4354  killed 1,000 Living Armors      -> Kill1000
4355  killed 10,000 Living Armors     -> Kill10000
```

Taking six consecutive ids swallowed 4352 and shifted the rest down, so `Kill100` awarded the task,
`Kill1000` awarded the 100-kill entry, and 4354 was awarded by nothing. Migration 71 corrects it.

The shifted row is present in the pristine `Database/war_world.7z` **and** in the pre-deletion dump
at `a4995e92`, so it is inherited data rather than anything this project introduced. The generator
that produced it was never in this repository.

## Summary

| ACID | Line | Fragments bound | Awardable | Thresholds | Tactics earnable |
|---:|:---|---:|---:|:---|:---|
| 330 | Daemonic | 24 | 23 | 7 / 15 / 22 | tier 3 |
| 331 | Beastial | 32 | 23 | 10 / 20 / 30 | tier 2 **<-** |
| 332 | Giant | 16 | 13 | 5 / 10 / 15 | tier 2 **<-** |
| 333 | Greenskin | 5 | 5 | 2 / 3 / 5 | tier 3 |
| 334 | Chaos | 18 | 16 | 5 / 10 / 15 | tier 3 |
| 335 | Mythical | 12 | 11 | 3 / 7 / 10 | tier 3 |
| 336 | Man | 6 | 3 | 2 / 4 / 6 | tier 1 **<-** |
| 337 | Skaven | 3 | 2 | 1 / 2 / 3 | tier 2 **<-** |
| 338 | Undead | 22 | 21 | 4 / 8 / 12 | tier 3 |

**138 fragments bound, 118 awardable, 20 with no award path at all** (BUG-117).

**Four of the 27 named tactics cannot be earned at all:**

| Tactic | Line | Fragments needed | Earnable |
|:---|:---|---:|---:|
| Harrier's Ken | Beastial | 30 | 23 |
| Sky Titan's Strength | Giant | 15 | 13 |
| Boon of Persistence | Man | 6 | 4 |
| Cunning Stratagem | Skaven | 3 | 2 |

### Two different failures, not one

The Tome entries are **not missing**. Every orphaned fragment has a `tok_infos` row with a proper
name and unlock text. The problem is that nothing in the world awards them:

* **20 are one-off completion tasks** - "You have completed: Indigestion" (Stone Troll, Giant
  line), "Avoid a Gory End" (Boar, Beastial), "What a Rat's Nest" (Skaven). No creature
  `TokUnlock`, no item `TokUnlock`, and no bestiary kill milestone references them, so no player
  action can trigger one. These need their task data restored.
* **1 was a wiring bug, now fixed.** Entry 4354, "You have killed 1,000 Living Armors", is a kill
  milestone that the Metal Construct bestiary row never pointed at: a completion task sits at 4352
  in the middle of that species' kill sequence, and the row had absorbed it, shifting `Kill100` and
  `Kill1000` down one slot. Migration 71 corrects it, which also stops 100 and 1,000 kills awarding
  the wrong entries. A check across all 131 counted species found this was the only one affected.

The Man line is the worst case: only **Boon of the Impalpable** can be earned, and both
**Boon of Tenacity** and **Boon of Persistence** are out of reach - 3 earnable fragments against
thresholds of 4 and 6. It is short two fragments before that as well, since the client grants 8
across species that include two with no `tok_bestiary` row here.

## Fragments by line, with where to earn them

`Award` is how the entry actually fires. **NO TRIGGER** means nothing in the world
references it — those are the BUG-117 orphans and cannot be earned at present.

`Creatures` lists the most-spawned prototypes carrying that bestiary species, with entry
ids, and `Where` the zones holding the most of them. Any creature of the species counts
toward its kill milestones, so these are the efficient places to farm, not the only ones.

### Daemonic — ACID 330

Thresholds 7 / 15 / 22. 24 fragments bound, **23 earnable**.

| ToK | Entry name | Unlock | Award | Creatures (entry id) | Where |
|---:|:---|:---|:---|:---|:---|
| 3075 | Bloodletters of Khorne | You have killed 1,000 Bloodletters of Khorne | kill 1000 | Brass Bloodfiend^f (11366); Bloodletter Effigy^n (39760); Brass Warfiend^f (11368) | Bastion Stair (160); High Pass (102); Chaos Wastes (103) |
| 3084 | Bloodthirsters of Khorne | You have killed 1,000 Bloodthirsters of Khorne | kill 1000 | Skull Lord Var'Ithrok^M (64106) | Thunder Mountain (5) |
| 3175 | Chaos Furies | You have killed 1,000 Chaos Furies | kill 1000 | Brass Fury^f (11362); Vicious Fury (5981); Skreeching Fury (6046) | Isle of the Dead (220); Chaos Wastes (103); Lost Vale (260) |
| 3205 | Chaos Spawn | You have killed 1,000 Chaos Spawn | kill 1000 | Chaos Spawn (1536); Forsaken Minion (40843); Unbound Ravager (6458) | Chaos Wastes (103); Kadrin Valley (9); Black Crag (3) |
| 3215 | Bloodbeasts of Khorne | You have killed 1,000 Bloodbeasts of Khorne | kill 1000 | Blood Spawn (10679) | Chaos Wastes (103) |
| 3234 | Firewyrms of Tzeentch | You have killed 1,000 Firewyrms of Tzeentch | kill 1000 | Corrupt Spawn (40938) | Chaos Wastes (103) |
| 3244 | Plaguebeasts of Nurgle | You have killed 1,000 Plaguebeasts of Nurgle | kill 1000 | Plague Spawn (10671); Blighted Plaguebeast (34046); Pestilent Soul (18419) | Chaos Wastes (103); Troll Country (101); Talabecland (108) |
| 3285 | Daemonettes of Slaanesh | You have killed 1,000 Daemonettes of Slaanesh | kill 1000 | Writhing Daemonette (7028); Wasteland Daemonette (6911); Flailing Daemonette (1460) | Chaos Wastes (103); Inevitable City (161); Lost Vale (260) |
| 3514 | Flamers of Tzeentch | You have killed 1,000 Flamers of Tzeentch | kill 1000 | Mysterious Flamer (38955); Unbound Flamer (6931); Shifting Flamer (35175) | Chaos Wastes (103); Black Crag (3); Thunder Mountain (5) |
| 3535 | Fleshhounds of Khorne | You have killed 1,000 Fleshhounds of Khorne | kill 1000 | Brass Bloodseeker^m (7625); Raging Flesh Hound (36621); Flesh Hound (1082) | Bastion Stair (160); Badlands (2); Chaos Wastes (103) |
| 3704 | Great Unclean One | You have killed 1,000 Great Unclean Ones | kill 1000 | NULL | NULL |
| 3786 | Horrors of Tzeentch | You have completed: Pack of Three | creature TokUnlock | Pained Horror (35171); Maddening Horror (38652); Mysterious Horror (38976) | Chaos Wastes (103); High Pass (102); Ostland (107) |
| 3788 | Horrors of Tzeentch | You have completed: What Horrifies Horrors? | item TokUnlock | Pained Horror (35171); Maddening Horror (38652); Mysterious Horror (38976) | Chaos Wastes (103); High Pass (102); Ostland (107) |
| 3804 | Juggernauts of Khorne | You have killed 1,000 Juggernauts of Khorne | kill 1000 | Juggernaut^n (8530); Unbound Mastodon^n (42516) | Bastion Stair (160); Chaos Wastes (103) |
| 3814 | Keepers of Secrets | You have killed 1,000 Keepers of Secrets | kill 1000 | N'Kari, Keeper of Secrets^F (62147) | Lost Vale (260) |
| 3844 | Lords of Change | You have killed 1,000 Lords of Change | kill 1000 | The Inevitable Lord of Change (1000455) | Barony of Nordland (106) |
| 3876 | Nurgling | You have completed: No End to the Oozing | item TokUnlock | Nurgling (1424); Bilerot Nurgling^m (118); Foul Nurgling (98312) | Bilerot Burrow (196); Chaos Wastes (103); Black Fire Pass (8) |
| 3878 | Nurgling | You have completed: Say Hello to My Little Friends | creature TokUnlock | Nurgling (1424); Bilerot Nurgling^m (118); Foul Nurgling (98312) | Bilerot Burrow (196); Chaos Wastes (103); Black Fire Pass (8) |
| 3974 | Plaguebearers of Nurgle | You have completed: Finding Foul Flesh | **NO TRIGGER** | Corrupted Plaguebearer (1685); Bilerot Moldbearer^m (1000717); Grotesque Plaguebearer (40253) | Chaos Wastes (103); Bilerot Burrow (196); Kadrin Valley (9) |
| 3976 | Plaguebearers of Nurgle | You have completed: Let It Fly or Let It Die | creature TokUnlock | Corrupted Plaguebearer (1685); Bilerot Moldbearer^m (1000717); Grotesque Plaguebearer (40253) | Chaos Wastes (103); Bilerot Burrow (196); Kadrin Valley (9) |
| 4035 | Screamers of Tzeentch | You have killed 1,000 Screamers of Tzeentch | kill 1000 | Screamer (1455); Screamer of Tzeentch^m (476); Fate's Edge Screamer^m (1916) | Inevitable City (161); Chaos Wastes (103); Isle of the Dead (220) |
| 4295 | Daemonvine | You have killed 1,000 Daemonvines | kill 1000 | Pestilent Tentacle (42814); Fiendish Barb (7835); Fiendish Talon (20793) | Black Crag (3); Avelorn (202); Lost Vale (260) |
| 4314 | Watcher | You have killed 1,000 Watchers | kill 1000 | Ominous Watcher^m (16231); Baleful Watcher^m (455); Menacing Watcher (731) | Inevitable City (161) |
| 4334 | Slimehound | You have killed 1,000 Slimehounds | kill 1000 | Tezakk's Slimehound^BeastlordNPC (2000492) | Chaos Wastes (103) |

### Beastial — ACID 331

Thresholds 10 / 20 / 30. 32 fragments bound, **23 earnable**.

| ToK | Entry name | Unlock | Award | Creatures (entry id) | Where |
|---:|:---|:---|:---|:---|:---|
| 3004 | Basilisk | You have completed: It's in the Bag | item TokUnlock | Jetscale Basilisk (6535); Sulfurous Basilisk (5584); Young Basilisk (5827) | Black Crag (3); Mt Bloodhorn (11) |
| 3006 | Basilisk | You have completed: Scales of Mourning | creature TokUnlock | Jetscale Basilisk (6535); Sulfurous Basilisk (5584); Young Basilisk (5827) | Black Crag (3); Mt Bloodhorn (11) |
| 3014 | Giant Bat | You have completed: Things with Wings... | creature TokUnlock | Neborhest Bat (7417); Kadrin Bat (6694); Gloomcry Bat (5900) | Marshes of Madness (1); Ostland (107); Shadowlands (201) |
| 3016 | Giant Bat | You have completed: In the Dark of Day | creature TokUnlock | Neborhest Bat (7417); Kadrin Bat (6694); Gloomcry Bat (5900) | Marshes of Madness (1); Ostland (107); Shadowlands (201) |
| 3026 | Bear | You have completed: Heading for Profit | item TokUnlock | Ursa Prowler (72606); Blighthide Cub (2079); Forest Stalker Bear (4049) | Praag (105); Eataine (209); Avelorn (202) |
| 3028 | Bear | You have completed: Sharpen Your Knives | creature TokUnlock | Ursa Prowler (72606); Blighthide Cub (2079); Forest Stalker Bear (4049) | Praag (105); Eataine (209); Avelorn (202) |
| 3094 | Boar | You have completed: Avoid a Gory End | **NO TRIGGER** | Bloodtusk Warsnout (4602); Brownback Boar (4293); Shadebristle Boar (2264) | Barak Varr (7); Mt Bloodhorn (11); Dragonwake (205) |
| 3096 | Boar | You have completed: Great Tusk | creature TokUnlock | Bloodtusk Warsnout (4602); Brownback Boar (4293); Shadebristle Boar (2264) | Barak Varr (7); Mt Bloodhorn (11); Dragonwake (205) |
| 3185 | Chaos Hound | You have killed 1,000 Chaos Hounds | kill 1000 | Moltenback Snarler (6090); Chaos Hound (155); Blighted Hound (35920) | Thunder Mountain (5); Black Crag (3); TM East - Death Peak (27) |
| 3265 | Cold One | You have killed 1,000 Cold Ones | kill 1000 | Cold One (237); Uthorin Cold-Blood (6735); Snarling Cold One (4514) | Shadowlands (201); The Blighted Isle (200); Saphery (208) |
| 3574 | Ghoul | You have completed: Foul Amongst the Fowl | creature TokUnlock | Carrion Scavenger (8847); Restless Dead (199); Seared Ghoul (1640) | Norsca (100); Marshes of Madness (1); Thunder Mountain (5) |
| 3576 | Ghoul | You have completed: Figner Licking Good | **NO TRIGGER** | Carrion Scavenger (8847); Restless Dead (199); Seared Ghoul (1640) | Norsca (100); Marshes of Madness (1); Thunder Mountain (5) |
| 3674 | Great Cat | You have completed: Silencing Shadows | creature TokUnlock | Lionmarch Hunter (2485); Zandri Lion (93630); Frost Sabretusk^m (6349) | Chrace (206); Troll Country (101); High Pass (102) |
| 3676 | Great Cat | You have completed: Not So Great Any More | creature TokUnlock | Lionmarch Hunter (2485); Zandri Lion (93630); Frost Sabretusk^m (6349) | Chrace (206); Troll Country (101); High Pass (102) |
| 3685 | Great Eagle | You have killed 1,000 Great Eagles | kill 1000 | Snowperch Eagle (7065); Eagle^m (2836); Icewing Eagle (4201) | Troll Country (101); Chrace (206); Praag (105) |
| 3796 | Hound | You have completed: Dog Gone Crazy | creature TokUnlock | Krueger Mastiff (34375); Attack Hound (10932); Nordland Hunthound (3552) | Talabecland (108); Reikland (109); Barony of Nordland (106) |
| 3798 | Hound | You have completed: Picky Eater | item TokUnlock | Krueger Mastiff (34375); Attack Hound (10932); Nordland Hunthound (3552) | Talabecland (108); Reikland (109); Barony of Nordland (106) |
| 3834 | Giant Lizard | You have completed: Poached | **NO TRIGGER** | Mortis River Drake (93651); Zandri Sand Drake (93631); Zandri Lizard^f (93624) | Necropolis of Zandri - RvR Terrain - Tomb Kings (191); Shadowlands (201); Troll Country (101) |
| 3836 | Giant Lizard | You have completed: Hunger Pangs | **NO TRIGGER** | Mortis River Drake (93651); Zandri Sand Drake (93631); Zandri Lizard^f (93624) | Necropolis of Zandri - RvR Terrain - Tomb Kings (191); Shadowlands (201); Troll Country (101) |
| 4014 | Rhinoxen | You have killed 1,000 Rhinoxen | kill 1000 | Stonehorn Rhinox (8119); Blackhorn Rhinox (34651); Bristlehide Rhinox (7867) | TM West - Cinderfall (26); Thunder Mountain (5); Lost Vale (260) |
| 4024 | Giant Scorpion | You have completed: A Pinch to Grow an Inch | **NO TRIGGER** | Scorpion^m (6817); Deadly Chaerilida^m (93702); Zandri Scorpion (93575) | Necropolis of Zandri - RvR Terrain - Tomb Kings (191); Shadowlands (201); Caledor (203) |
| 4026 | Giant Scorpion | You have completed: Deadly When Dead | **NO TRIGGER** | Scorpion^m (6817); Deadly Chaerilida^m (93702); Zandri Scorpion (93575) | Necropolis of Zandri - RvR Terrain - Tomb Kings (191); Shadowlands (201); Caledor (203) |
| 4074 | Giant Spider | You have completed: Fast Finds | **NO TRIGGER** | Ash Crawler^m (6110); Whitefire Broodling (6824); Moss Crawler (5930) | Thunder Mountain (5); Ostland (107); Lost Vale (260) |
| 4076 | Giant Spider | You have completed: Big Identity Crisis | creature TokUnlock | Ash Crawler^m (6110); Whitefire Broodling (6824); Moss Crawler (5930) | Thunder Mountain (5); Ostland (107); Lost Vale (260) |
| 4096 | Squig | You have completed: Forgotten and Famished | creature TokUnlock | Eatin' Squig (4708); Mush 'unta Squig (20842); Mountain Squig (2102) | Black Crag (3); Badlands (2); Mt Bloodhorn (11) |
| 4097 | Squig | You have killed 10,000 Squigs | kill 10000 | Eatin' Squig (4708); Mush 'unta Squig (20842); Mountain Squig (2102) | Black Crag (3); Badlands (2); Mt Bloodhorn (11) |
| 4206 | Vulture | You have completed: Slop-of-the-Day | **NO TRIGGER** | Zandri Vulture (93616); Ashbeak Vulture (6953); Vulture^m (2557) | Necropolis of Zandri - RvR Terrain - Tomb Kings (191); Badlands (2); TM West - Cinderfall (26) |
| 4208 | Vulture | You have completed: More Than Just a Foul Feather | creature TokUnlock | Zandri Vulture (93616); Ashbeak Vulture (6953); Vulture^m (2557) | Necropolis of Zandri - RvR Terrain - Tomb Kings (191); Badlands (2); TM West - Cinderfall (26) |
| 4214 | Warhawk | You have completed: Bird Watching &  Up Close, in Person | **NO TRIGGER** | Zandri Hawk^m (93639); Hawk (30839); Wrathglade Hawk (34771) | Necropolis of Zandri - RvR Terrain - Tomb Kings (191); Shadowlands (201); Avelorn (202) |
| 4218 | Warhawk | You have completed: Well Watched Weaklings | creature TokUnlock | Zandri Hawk^m (93639); Hawk (30839); Wrathglade Hawk (34771) | Necropolis of Zandri - RvR Terrain - Tomb Kings (191); Shadowlands (201); Avelorn (202) |
| 4246 | Wolves | You have completed: Alpha in Bits | creature TokUnlock | Sandhowl Wolf (8190); Hunting Wolf (5264); Forest Wolf (4606) | Kadrin Valley (9); Barak Varr (7); Ostland (107) |
| 4248 | Wolves | You have completed: Fall Fur Fashions | item TokUnlock | Sandhowl Wolf (8190); Hunting Wolf (5264); Forest Wolf (4606) | Kadrin Valley (9); Barak Varr (7); Ostland (107) |

### Giant — ACID 332

Thresholds 5 / 10 / 15. 16 fragments bound, **13 earnable**.

| ToK | Entry name | Unlock | Award | Creatures (entry id) | Where |
|---:|:---|:---|:---|:---|:---|
| 3586 | Giant | You have completed: Brutal Nose Ring | item TokUnlock | Bubor^M (1703); Ufgor^M (6419); Thrags^M (14814) | Thunder Mountain (5); Black Crag (3); Reikland (109) |
| 3588 | Giant | You have completed: Giant Amongst Giants | creature TokUnlock | Bubor^M (1703); Ufgor^M (6419); Thrags^M (14814) | Thunder Mountain (5); Black Crag (3); Reikland (109) |
| 3594 | Chaos Giant | You have killed 1,000 Chaos Giants | kill 1000 | Flesheater Giant^m (1964); Friderax^M (6987); Geral the Dim^M (41844) | Black Crag (3); Inevitable City (161); Praag (105) |
| 3655 | Gorger | You have killed 1,000 Gorgers | kill 1000 | Bonegash Gorger^m (16728); Maddened Gorger^m (35362); Crazed Gorger^m (34382) | Troll Country (101); High Pass (102); Bloodwrought Enclave (195) |
| 3714 | Griffon | You have killed 1,000 Griffons | kill 1000 | Skyrage Griffon (20835); Skyrage Sharptalon (36183); Warcry Griffon (436) | Saphery (208); Praag (105); Reikland (109) |
| 3884 | Ogre Bull | You have completed: It's Safer Outside | **NO TRIGGER** | Brokenmaw Tracker^m (5193); Bonestomper Irongut^m (37245); Bloodmaw Irongut (7874) | High Pass (102); Troll Country (101); Badlands (2) |
| 3886 | Ogre Bull | You have completed: Bigger They Are& | creature TokUnlock | Brokenmaw Tracker^m (5193); Bonestomper Irongut^m (37245); Bloodmaw Irongut (7874) | High Pass (102); Troll Country (101); Badlands (2) |
| 3894 | Ogre Tyrant | You have killed 1,000 Ogre Tyrants | kill 1000 | Graw Leadgut (19915); Grug Boneshaker^M (37356); Karerg the Reaver^M (99884) | Badlands (2); Kadrin Valley (9); Barak Varr (7) |
| 4116 | Troll | You have completed: Rock in the Maw of Darkness | creature TokUnlock | Felmaw Troll^m (4036); Rock-Eater Troll^m (3286); Blackhide Troll^m (35523) | Troll Country (101); Avelorn (202); Badlands (2) |
| 4118 | Troll | You have completed: Prescribed Flesh | item TokUnlock | Felmaw Troll^m (4036); Rock-Eater Troll^m (3286); Blackhide Troll^m (35523) | Troll Country (101); Avelorn (202); Badlands (2) |
| 4125 | Chaos Troll | You have killed 1,000 Chaos Trolls | kill 1000 | Wart Troll (12393); Vragi the Brute^M (15347); Goremaw^M (35876) | Troll Country (101); Avelorn (202); Ekrund (6) |
| 4136 | River Troll | You have completed: Gotta Go | **NO TRIGGER** | Feral Sludge Troll^m (42643); Bogbile Troll^m (11435); Bogwallow Troll^m (4397) | Troll Country (101); TM East - Death Peak (27); Badlands (2) |
| 4138 | River Troll | You have completed: O'de Toilet | creature TokUnlock | Feral Sludge Troll^m (42643); Bogbile Troll^m (11435); Bogwallow Troll^m (4397) | Troll Country (101); TM East - Death Peak (27); Badlands (2) |
| 4144 | Stone Troll | You have completed: It's a Snap | creature TokUnlock | Stonegullet Troll^m (1975); Stone Troll^m (81781); Feltongue Troll^m (7421) | Troll Country (101); Black Crag (3); Barak Varr (7) |
| 4146 | Stone Troll | You have completed: Indigestion | **NO TRIGGER** | Stonegullet Troll^m (1975); Stone Troll^m (81781); Feltongue Troll^m (7421) | Troll Country (101); Black Crag (3); Barak Varr (7) |
| 4265 | Yhetee | You have killed 1,000 Yhetee | kill 1000 | Glacier Yeti^m (5545); Yhetee Frozenmaw^m (35032); Yhetee Icetooth^m (34452) | High Pass (102); Troll Country (101); Lost Vale (260) |

### Greenskin — ACID 333

Thresholds 2 / 3 / 5. 5 fragments bound, **5 earnable**.

| ToK | Entry name | Unlock | Award | Creatures (entry id) | Where |
|---:|:---|:---|:---|:---|:---|
| 3604 | Gnoblar | You have completed: Tiny Tyrant | creature TokUnlock | Gnoblar Impaler (5012); Ashreaver Tooth Gnoblar (8115); Blood Boglar (14818) | Marshes of Madness (1); TM West - Cinderfall (26); Troll Country (101) |
| 3606 | Gnoblar | You have completed: Cull the Weak | item TokUnlock | Gnoblar Impaler (5012); Ashreaver Tooth Gnoblar (8115); Blood Boglar (14818) | Marshes of Madness (1); TM West - Cinderfall (26); Troll Country (101) |
| 3935 | Savage Orc | You have killed 1,000 Savage Orcs | kill 1000 | Blue Face Eata (4699); Blue Face Git (1109); Blue Face Roamer (19919) | Badlands (2); Black Crag (3); Black Fire Pass (8) |
| 4066 | Snotling | You have completed: Green Lightning | creature TokUnlock | Bloody Sun Snotling (37714); Skaven Dungeon Invis (99620); Snotling (3163) | Thunder Mountain (5); Ekrund (6); Thanquuol's Incursion (410) |
| 4068 | Snotling | You have completed: Relish the Relics | item TokUnlock | Bloody Sun Snotling (37714); Skaven Dungeon Invis (99620); Snotling (3163) | Thunder Mountain (5); Ekrund (6); Thanquuol's Incursion (410) |

### Chaos — ACID 334

Thresholds 5 / 10 / 15. 18 fragments bound, **16 earnable**.

| ToK | Entry name | Unlock | Award | Creatures (entry id) | Where |
|---:|:---|:---|:---|:---|:---|
| 3036 | Bestigor | You have completed: Appalling Fashion | item TokUnlock | Blacktalon Bestigor (39215); Darkhorn Bestigor (2286); Ravaging Bestigor (7423) | Avelorn (202); Ostland (107); West Praag (120) |
| 3038 | Bestigor | You have completed: Nacht to Dread | **NO TRIGGER** | Blacktalon Bestigor (39215); Darkhorn Bestigor (2286); Ravaging Bestigor (7423) | Avelorn (202); Ostland (107); West Praag (120) |
| 3045 | Bray Shaman | You have killed 1,000 Bray Shaman | kill 1000 | Writhing Shaman (6861); Dreadhorn Shaman (3695); Black Mire Bray (18008) | Ostland (107); Chaos Wastes (103); Avelorn (202) |
| 3054 | Gor | You have completed: Thin the Herd | **NO TRIGGER** | Blighted Gorhorn (35904); Mottled Gor (1921); Krul'Gor Ambusher (593) | Ostland (107); Kadrin Valley (9); Black Fire Pass (8) |
| 3056 | Gor | You have completed: For the Greater Gor | creature TokUnlock | Blighted Gorhorn (35904); Mottled Gor (1921); Krul'Gor Ambusher (593) | Ostland (107); Kadrin Valley (9); Black Fire Pass (8) |
| 3064 | Ungor | You have completed: Thorn in My Side | creature TokUnlock | Mottled Ungor (6101); Blackhorn Ungor (35609); Bloodherd Ungor^m (2000671) | Barak Varr (7); Kadrin Valley (9); Talabecland (108) |
| 3066 | Ungor | You have completed: Trinkets for Treasure | item TokUnlock | Mottled Ungor (6101); Blackhorn Ungor (35609); Bloodherd Ungor^m (2000671) | Barak Varr (7); Kadrin Valley (9); Talabecland (108) |
| 3195 | Chaos Mutant | You have killed 1,000 Chaos Mutants | kill 1000 | Mutated Peasant (1134); Plagued Miner (72634); Mutated Cultist (35101) | High Pass (102); Praag (105); Sigmar Crypts (176) |
| 3344 | Doombull | You have killed 1,000 Doombulls | kill 1000 | NULL | NULL |
| 3374 | Dragon Ogre | You have killed 1,000 Dragon Ogres | kill 1000 | Dragon Ogre (3352); Azorgari Marauder Spirit (7857); Nightfall Dragon Ogre (3705) | TM West - Cinderfall (26); Thunder Mountain (5); Chaos Wastes (103) |
| 3524 | Flayerkin | You have killed 1,000 Flayerkin | kill 1000 | Bloodfiend Flayerkin (4913); Sacellum Thrallkeeper (25726); Sacellum Willtamer (17458) | Inevitable City (161); Lost Vale (260); Sacellum Dungeons: South Wing - Sacellum (173) |
| 3724 | Harpies | You have completed: Get Down Here! | creature TokUnlock | Arkaneth Harpy (1563); Rageclaw Harpy (6991); Harpy (1407) | The Blighted Isle (200); Eataine (209); Chrace (206) |
| 3726 | Harpies | You have completed: Silent Star | creature TokUnlock | Arkaneth Harpy (1563); Rageclaw Harpy (6991); Harpy (1407) | The Blighted Isle (200); Eataine (209); Chrace (206) |
| 3964 | Plague Victim | You have completed: Epic Epidemic | creature TokUnlock | Forsaken Infantry (40443); Blighted Griffon (34047); Forsaken Archer (40442) | Troll Country (101); Black Crag (3); Ostland (107) |
| 3966 | Plague Victim | You have completed: Wash Your Hands | creature TokUnlock | Forsaken Infantry (40443); Blighted Griffon (34047); Forsaken Archer (40442) | Troll Country (101); Black Crag (3); Ostland (107) |
| 4166 | Tuskgor | You have completed: Saving Face | item TokUnlock | Tuskgor (2821); Riphorn Tuskgor (575); Hrim Tuskgor (5754) | Ostland (107); Norsca (100); Troll Country (101) |
| 4168 | Tuskgor | You have completed: Tusk for Tusk | creature TokUnlock | Tuskgor (2821); Riphorn Tuskgor (575); Hrim Tuskgor (5754) | Ostland (107); Norsca (100); Troll Country (101) |
| 4344 | Maggot | You have killed 1,000 Maggots | kill 1000 | Maggot^f (6838); Painling (6831) | Black Crag (3); Lost Vale (260) |

### Mythical — ACID 335

Thresholds 3 / 7 / 10. 12 fragments bound, **11 earnable**.

| ToK | Entry name | Unlock | Award | Creatures (entry id) | Where |
|---:|:---|:---|:---|:---|:---|
| 3104 | Hydra | You have killed 1,000 Hydra | kill 1000 | Blackheart Hydra^f (12016); Deathclaw Hydra^f (40391) | Shadowlands (201); Caledor (203) |
| 3255 | Cockatrice | You have killed 1,000 Cockatrice | kill 1000 | Felgaze Cockatrice (20838); Stonewood Cockatrice^f (4757); Stonegaze Cockatrice (2734) | Saphery (208); Shadowlands (201); Reikland (109) |
| 3396 | Dryad | You have completed: Chip Off the Old& | creature TokUnlock | Wrathborn Dryad^f (7837); Emberwood Whisperer (15682); Shadowdell Dryad (2260) | Avelorn (202); TM East - Death Peak (27); Saphery (208) |
| 3398 | Dryad | You have completed: Fire Seldom Dies Alone | **NO TRIGGER** | Wrathborn Dryad^f (7837); Emberwood Whisperer (15682); Shadowdell Dryad (2260) | Avelorn (202); TM East - Death Peak (27); Saphery (208) |
| 3855 | Manticore | You have killed 1,000 Manticores | kill 1000 | Deathclaw Manticore (40364); Skyprowler Bloodclaw (2935); Skyprowler Manticore (9755) | Chrace (206); Caledor (203); Saphery (208) |
| 4086 | Spite | You have completed: Lost and Found | creature TokUnlock | Wrathborn Terror (4627); Thanalorn Spite (2375); Gloomridge Terror (4328) | Avelorn (202); The Blighted Isle (200); Chrace (206) |
| 4088 | Spite | You have completed: A Defiled Life-Force | item TokUnlock | Wrathborn Terror (4627); Thanalorn Spite (2375); Gloomridge Terror (4328) | Avelorn (202); The Blighted Isle (200); Chrace (206) |
| 4105 | Treekin | You have killed 1,000 Treekins | kill 1000 | Emberwood Defender (11223); Bleakwind Colossus (8663); Avelorn Treekin^m (4027) | TM East - Death Peak (27); Avelorn (202); Lost Vale (260) |
| 4184 | Unicorn | You have killed 1,000 Unicorns | kill 1000 | Majestic Unicorn (2171); Unicorn^n (2169); Windhorn Unicorn (36298) | The Blighted Isle (200); Saphery (208); Shadowlands (201) |
| 4255 | Wyvern | You have killed 1,000 Wyvern | kill 1000 | Riding Wyvern^m (728); Vile Wyvern^m (39631); Moltenwing Broodling^n (6167) | Thunder Mountain (5); Black Crag (3); TM East - Death Peak (27) |
| 4304 | Imp | You have killed 1,000 Imps | kill 1000 | Dark Sprite (57); Pain-Sprite (4286); Sootwing Woodspirit (15688) | Lost Vale (260); The Blighted Isle (200); TM East - Death Peak (27) |
| 4364 | Treemen | You have killed 1,000 Treemen | kill 1000 | Sarthain the Worldbearer (6842) | Lost Vale (260) |

### Man — ACID 336

Thresholds 2 / 4 / 6. 6 fragments bound, **4 earnable**.

| ToK | Entry name | Unlock | Award | Creatures (entry id) | Where |
|---:|:---|:---|:---|:---|:---|
| 3645 | Night Goblin | You have killed 1,000 Night Goblins | kill 1000 | Bloody Sun Detective (1000304); Uzfang (4696); Night Gobbo Boss (778100) | Inevitable City Contested (167); Inevitable City (161); Winds of Chaos (175) |
| 3946 | Bandit | You have completed: Wanted! | creature TokUnlock | Bandit Raider^m (93625); Bandit Scout^m (93605); Dog Ambusher (2096) | Necropolis of Zandri - RvR Terrain - Tomb Kings (191); Ostland (107); Mt Bloodhorn (11) |
| 3948 | Bandit | You have completed: Robbery | **NO TRIGGER** | Bandit Raider^m (93625); Bandit Scout^m (93605); Dog Ambusher (2096) | Necropolis of Zandri - RvR Terrain - Tomb Kings (191); Ostland (107); Mt Bloodhorn (11) |
| 4286 | Cultist | You have completed: Don't Die All At Once | **NO TRIGGER** | Drakk Convert (8029); Drakk Ritualist (39217); Drakk Cultist (38126) | TM West - Cinderfall (26); Thunder Mountain (5); Inevitable City Contested (167) |
| 4288 | Cultist | You have completed: Hard To Be a Leader | creature TokUnlock | Drakk Convert (8029); Drakk Ritualist (39217); Drakk Cultist (38126) | TM West - Cinderfall (26); Thunder Mountain (5); Inevitable City Contested (167) |
| 4354 | Metal Construct | You have killed 1,000 Living Armors | kill 1000 | Living Armor^m (40126); Armor of Damnation^Event-Halloween (2000815) | Isle of the Dead (220); Marshes of Madness (1) |

### Skaven — ACID 337

Thresholds 1 / 2 / 3. 3 fragments bound, **2 earnable**.

| ToK | Entry name | Unlock | Award | Creatures (entry id) | Where |
|---:|:---|:---|:---|:---|:---|
| 3994 | Rat Ogre | You have killed 1,000 Rat Ogres | kill 1000 | Warprot Brute^f (19412); Warpblade Brute (2501336); Blisterclaw Rat Ogre^m (10547) | Warpblade Tunnels 2 (154); The Sewers of Altdorf: Wing 3 - Sewers (169); TM East - Death Peak (27) |
| 4044 | Skaven | You have completed: What a Rat's Nest | **NO TRIGGER** | Slave Rat (1000802); Clanrat (1000811); Warpblade Clanrat (2501332) | Warpblade Tunnels 1 (177); Marshes of Madness (1); Talabecland (108) |
| 4048 | Skaven | You have completed: Septic Sanitation | creature TokUnlock | Slave Rat (1000802); Clanrat (1000811); Warpblade Clanrat (2501332) | Warpblade Tunnels 1 (177); Marshes of Madness (1); Talabecland (108) |

### Undead — ACID 338

Thresholds 4 / 8 / 12. 22 fragments bound, **21 earnable**.

| ToK | Entry name | Unlock | Award | Creatures (entry id) | Where |
|---:|:---|:---|:---|:---|:---|
| 3544 | Banshee | You have killed 1,000 Banshee | kill 1000 | Wailing Specter^m (4856); Deathwrought Screamer (3222); Thorshafn Ancestor (283) | Norsca (100); Sigmar Crypts (176); The Blighted Isle (200) |
| 3556 | Spirit Host | You have completed: Higher Caliber of Soul | creature TokUnlock | Spectral Ancestor (14816); Spectral Screamer (5300); Norse Specter^m (4853) | Marshes of Madness (1); Reikland (109); Norsca (100) |
| 3558 | Spirit Host | You have completed: Worth the Waiting For | creature TokUnlock | Spectral Ancestor (14816); Spectral Screamer (5300); Norse Specter^m (4853) | Marshes of Madness (1); Reikland (109); Norsca (100) |
| 3565 | Wraith | You have killed 1,000 Wraiths | kill 1000 | Awakened Spirit^m (289); Withered Hand^m (4695); Hallowed Spirit (1000581) | Reikland (109); Badlands (2); Norsca (100) |
| 3825 | Liche | You have killed 1,000 Liches | kill 1000 | Disciple of Ualatp (93592); Priest of Tahoth (93580); Nekh Akhet^M (93692) | Necropolis of Zandri - RvR Terrain - Tomb Kings (191); Tomb of the Vulture Lord - RvR Dungeon (179); Altdorf (162) |
| 4054 | Skeleton | You have completed: Bones Have Names Too | creature TokUnlock | Skeletal Warrior^m (33584); Desiccated Archer (93715); Riverwind Assassin^m (97487) | Necropolis of Zandri - RvR Terrain - Tomb Kings (191); Ostland (107); Tomb of the Vulture Lord - RvR Dungeon (179) |
| 4056 | Skeleton | You have completed: When There's a Will | item TokUnlock | Skeletal Warrior^m (33584); Desiccated Archer (93715); Riverwind Assassin^m (97487) | Necropolis of Zandri - RvR Terrain - Tomb Kings (191); Ostland (107); Tomb of the Vulture Lord - RvR Dungeon (179) |
| 4174 | Bone Giant | You have killed 1,000 Bone Giants | kill 1000 | Eternal Sentinel (93576); Leviathan Observer (94192); Undead Construct (36022) | Necropolis of Zandri - RvR Terrain - Tomb Kings (191); Tomb of the Vulture Lord - RvR Dungeon (179); Ellyrion (207) |
| 4194 | Vampire | You have killed 1,000 Vampires | kill 1000 | Kamilla the Decayed (28432); Krela Darkshroud (56580); Natalya Von Carstein (1000511) | Altdorf (162); Badlands (2); Norsca (100) |
| 4225 | Wight | You have killed 1,000 Wights | kill 1000 | Bleakwind Wight^m (1464); Forgotten Chaos Wight (1000493); Brimdall Yggdreidar (3349) | Chaos Wastes (103); TM East - Death Peak (27); Altdorf (162) |
| 4234 | Winged Nightmare | You have killed 1,000 Winged Nightmares | kill 1000 | Terrorwing (15675); Nightmare of the Abyss^Event-Halloween (2000818); Zorethex (16760) | TM East - Death Peak (27); Badlands (2); Shadowlands (201) |
| 4276 | Zombie | You have completed: The Family That Dies Together | **NO TRIGGER** | Unterbaum Zombie^m (94712); Unterbaum Zombie^m (94713); Diseased Forsaken (10647) | Talabecland (108); Marshes of Madness (1); Chaos Wastes (103) |
| 4278 | Zombie | You have completed: Wulgrig | creature TokUnlock | Unterbaum Zombie^m (94712); Unterbaum Zombie^m (94713); Diseased Forsaken (10647) | Talabecland (108); Marshes of Madness (1); Chaos Wastes (103) |
| 4324 | Walker | You have killed 1,000 Walkers | kill 1000 | Timeworn Soul^m (1187); Ancient Walker (734); Timeworn Assistant^m (1735) | Inevitable City (161); Altdorf (162); Chaos Wastes East - Outer Dark (117) |
| 4402 | Asp Bone Construct 1000 | You have killed 1,000 Asp bone Constructs | kill 1000 | Spitting Asp^f (93627); Sedjhet Twister (93732); Sedjhet Spewer^m (93729) | Necropolis of Zandri - RvR Terrain - Tomb Kings (191) |
| 4407 | Scarab Bone Construct 1000 | You have killed 1,000 Scarab Bone Constructs | kill 1000 | Scarab Construct (93604); Pit Dermestid (98820); Vault Scarab (93838) | Necropolis of Zandri - RvR Terrain - Tomb Kings (191); Tomb of the Vulture Lord - RvR Dungeon (179) |
| 4412 | Tomb Scorpions 100 | You have killed 100 Tomb Scorpions | kill 100 | Hieratic Scourge (94189); Stinger of Sokth (93782); Pyre Scorpion (93853) | Tomb of the Vulture Lord - RvR Dungeon (179); Necropolis of Zandri - RvR Terrain - Tomb Kings (191) |
| 4417 | Ushabti 100 | You have killed 100 Ushabti | kill 100 | Champion of Djaf (93697); Image of Djaf^n (93555); Champion of Sebok (93578) | Necropolis of Zandri - RvR Terrain - Tomb Kings (191); Tomb of the Vulture Lord - RvR Dungeon (179) |
| 4422 | Preserved Dead 100 | You have killed 100 Preserved Dead | kill 100 | Augurer of Khsar (93735); Physician of Khsar (93758); Sepulcher Guardian (93855) | Tomb of the Vulture Lord - RvR Dungeon (179); Necropolis of Zandri - RvR Terrain - Tomb Kings (191) |
| 4427 | Giant Scarab 1000 | You have killed 1,000 Giant Scarabs | kill 1000 | Zandri Scarab^m (93572); Bitter Wind Coleopter (93587); Forbidden Dorr (93637) | Necropolis of Zandri - RvR Terrain - Tomb Kings (191); Tomb of the Vulture Lord - RvR Dungeon (179) |
| 4432 | Tomb Swarm 1000 | You have killed 1,000 Tomb Swarm | kill 1000 | Forbidden Infestation (93626); Bitter Wind Swarm (93591); Tomb Swarm^m (93568) | Necropolis of Zandri - RvR Terrain - Tomb Kings (191); Tomb of the Vulture Lord - RvR Dungeon (179) |
| 4437 | Carrion 1000 | You have killed 1,000 Carrion | kill 1000 | Carrion^m (93620); Screeching Carrion (93706); Merciless Carrion (93705) | Necropolis of Zandri - RvR Terrain - Tomb Kings (191) |

## What each of the 27 tactics actually does

Read from `mythic_bin_ability.MythicComponentData` for entries 15100-15126 — the client's own
component list, not inferred. Component operations are named from `ComponentOperationType`, and
the `BONUS_TYPE_ADJUST` targets from the bonus-type enum in `Common/Database/GameData.cs`:
**41 = ActionPointCost, 42 = CriticalHitRate, 54 = AggroRadius, 58 = XpReceived**.

Effects are cumulative down a line: the second tactic carries the first's components plus its own,
and the third carries all of them. That is how the client data is built, so a player who owns all
three has three separately slottable abilities of increasing breadth, not three copies of one.

| Line | Tactic | Components |
|:---|:---|:---|
| Daemonic | Aethyric Pandemonium | +5% damage |
| | Aethyric Ward | +5% damage, −5% damage taken |
| | Aethyric Insight | + morale regen +25% |
| Beastial | Harrier's Savagery | **aggro radius −50%** |
| | Harrier's Pelt | + −5% damage taken |
| | Harrier's Ken | + XP received +50% |
| Giant | Sky Titan's Bulwark | +5 defensive stat |
| | Sky Titan's Favor | + crit rate +5 |
| | Sky Titan's Strength | + morale regen +25% |
| Greenskin | Outmaneuver the Dim | **aggro radius −50%** |
| | Outmaneuver the Cunning | + action point cost −10 |
| | Outmaneuver the Clever | + morale regen +25% |
| Chaos | Favour the Daemonic | +5 defensive stat |
| | Favour of the Gods | + −5% damage taken |
| | Favour of the Mad | + cooldowns −2000ms |
| Mythical | Apotheosis of Flesh | +5% damage |
| | Apotheosis of Spirit | + action point cost −10 |
| | Apotheosis of Mind | + cooldowns −2000ms |
| Man | Boon of the Impalpable | **aggro radius −50%** |
| | Boon of Tenacity | + action point cost −10 |
| | Boon of Persistence | + XP received +50% |
| Skaven | Cunning Evasion | +5 defensive stat |
| | Cunning Assault | + crit rate +5 |
| | Cunning Stratagem | + XP received +50% |
| Undead | Benediction of Morr | +5% damage |
| | Benediction of Winters | + −5% damage taken |
| | Benediction of the Dead | + cooldowns −2000ms |

The three aggro-radius tactics are the ones that change enemy AI behaviour: `BONUS_TYPE_ADJUST`
on bonus type 54 at −50, which is the −50% the server implements as a 0.5 multiplier in
`TomeTacticService`. Morale and cooldown components sit behind an `EVENT_LISTENER(3)` gate and
carry a 10-second duration, which is why they need sustained-engagement state (BUG-118).
