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

**138 fragments bound, 117 awardable, 21 with no award path at all** (BUG-117).

**Five of the 27 named tactics cannot be earned at all:**

| Tactic | Line | Fragments needed | Earnable |
|:---|:---|---:|---:|
| Harrier's Ken | Beastial | 30 | 23 |
| Sky Titan's Strength | Giant | 15 | 13 |
| Boon of Tenacity | Man | 4 | 3 |
| Boon of Persistence | Man | 6 | 3 |
| Cunning Stratagem | Skaven | 3 | 2 |

The Man line is the worst case: only **Boon of the Impalpable** can be earned, and both
**Boon of Tenacity** and **Boon of Persistence** are out of reach - 3 earnable fragments against
thresholds of 4 and 6. It is short two fragments before that as well, since the client grants 8
across species that include two with no `tok_bestiary` row here.

## Fragments by line

### Daemonic - ACID 330

Tier 1 at 7 fragments, tier 2 at 15, tier 3 at 22. 24 bound, 23 awardable.

| ToK entry | Name | Unlock condition | Bestiary id | Award path |
|---:|:---|:---|---:|:---|
| 3075 | Bloodletters of Khorne | You have killed 1,000 Bloodletters of Khorne | 8 | bestiary kill |
| 3084 | Bloodthirsters of Khorne | You have killed 1,000 Bloodthirsters of Khorne | 9 | bestiary kill |
| 3175 | Chaos Furies | You have killed 1,000 Chaos Furies | 18 | bestiary kill |
| 3205 | Chaos Spawn | You have killed 1,000 Chaos Spawn | 21 | bestiary kill |
| 3215 | Bloodbeasts of Khorne | You have killed 1,000 Bloodbeasts of Khorne | 22 | bestiary kill |
| 3234 | Firewyrms of Tzeentch | You have killed 1,000 Firewyrms of Tzeentch | 24 | bestiary kill |
| 3244 | Plaguebeasts of Nurgle | You have killed 1,000 Plaguebeasts of Nurgle | 25 | bestiary kill |
| 3285 | Daemonettes of Slaanesh | You have killed 1,000 Daemonettes of Slaanesh | 29 | bestiary kill |
| 3514 | Flamers of Tzeentch | You have killed 1,000 Flamers of Tzeentch | 52 | bestiary kill |
| 3535 | Fleshhounds of Khorne | You have killed 1,000 Fleshhounds of Khorne | 54 | bestiary kill |
| 3704 | Great Unclean One | You have killed 1,000 Great Unclean Ones | 71 | bestiary kill |
| 3786 | Horrors of Tzeentch | You have completed: Pack of Three | 79 | creature TokUnlock |
| 3788 | Horrors of Tzeentch | You have completed: What Horrifies Horrors? | 79 | item TokUnlock |
| 3804 | Juggernauts of Khorne | You have killed 1,000 Juggernauts of Khorne | 81 | bestiary kill |
| 3814 | Keepers of Secrets | You have killed 1,000 Keepers of Secrets | 82 | bestiary kill |
| 3844 | Lords of Change | You have killed 1,000 Lords of Change | 85 | bestiary kill |
| 3876 | Nurgling | You have completed: No End to the Oozing | 88 | item TokUnlock |
| 3878 | Nurgling | You have completed: Say Hello to My Little Friends | 88 | creature TokUnlock |
| 3974 | Plaguebearers of Nurgle | You have completed: Finding Foul Flesh | 98 | **none** |
| 3976 | Plaguebearers of Nurgle | You have completed: Let It Fly or Let It Die | 98 | creature TokUnlock |
| 4035 | Screamers of Tzeentch | You have killed 1,000 Screamers of Tzeentch | 104 | bestiary kill |
| 4295 | Daemonvine | You have killed 1,000 Daemonvines | 130 | bestiary kill |
| 4314 | Watcher | You have killed 1,000 Watchers | 132 | bestiary kill |
| 4334 | Slimehound | You have killed 1,000 Slimehounds | 134 | bestiary kill |

### Beastial - ACID 331

Tier 1 at 10 fragments, tier 2 at 20, tier 3 at 30. 32 bound, 23 awardable.

| ToK entry | Name | Unlock condition | Bestiary id | Award path |
|---:|:---|:---|---:|:---|
| 3004 | Basilisk | You have completed: It's in the Bag | 1 | item TokUnlock |
| 3006 | Basilisk | You have completed: Scales of Mourning | 1 | creature TokUnlock |
| 3014 | Giant Bat | You have completed: Things with Wings... | 2 | creature TokUnlock |
| 3016 | Giant Bat | You have completed: In the Dark of Day | 2 | creature TokUnlock |
| 3026 | Bear | You have completed: Heading for Profit | 3 | item TokUnlock |
| 3028 | Bear | You have completed: Sharpen Your Knives | 3 | creature TokUnlock |
| 3094 | Boar | You have completed: Avoid a Gory End | 10 | **none** |
| 3096 | Boar | You have completed: Great Tusk | 10 | creature TokUnlock |
| 3185 | Chaos Hound | You have killed 1,000 Chaos Hounds | 19 | bestiary kill |
| 3265 | Cold One | You have killed 1,000 Cold Ones | 27 | bestiary kill |
| 3574 | Ghoul | You have completed: Foul Amongst the Fowl | 58 | creature TokUnlock |
| 3576 | Ghoul | You have completed: Figner Licking Good | 58 | **none** |
| 3674 | Great Cat | You have completed: Silencing Shadows | 68 | creature TokUnlock |
| 3676 | Great Cat | You have completed: Not So Great Any More | 68 | creature TokUnlock |
| 3685 | Great Eagle | You have killed 1,000 Great Eagles | 69 | bestiary kill |
| 3796 | Hound | You have completed: Dog Gone Crazy | 80 | creature TokUnlock |
| 3798 | Hound | You have completed: Picky Eater | 80 | item TokUnlock |
| 3834 | Giant Lizard | You have completed: Poached | 84 | **none** |
| 3836 | Giant Lizard | You have completed: Hunger Pangs | 84 | **none** |
| 4014 | Rhinoxen | You have killed 1,000 Rhinoxen | 102 | bestiary kill |
| 4024 | Giant Scorpion | You have completed: A Pinch to Grow an Inch | 103 | **none** |
| 4026 | Giant Scorpion | You have completed: Deadly When Dead | 103 | **none** |
| 4074 | Giant Spider | You have completed: Fast Finds | 108 | **none** |
| 4076 | Giant Spider | You have completed: Big Identity Crisis | 108 | creature TokUnlock |
| 4096 | Squig | You have completed: Forgotten and Famished | 110 | creature TokUnlock |
| 4097 | Squig | You have killed 10,000 Squigs | 110 | bestiary kill |
| 4206 | Vulture | You have completed: Slop-of-the-Day | 121 | **none** |
| 4208 | Vulture | You have completed: More Than Just a Foul Feather | 121 | creature TokUnlock |
| 4214 | Warhawk | You have completed: Bird Watching &  Up Close, in Person | 122 | **none** |
| 4218 | Warhawk | You have completed: Well Watched Weaklings | 122 | creature TokUnlock |
| 4246 | Wolves | You have completed: Alpha in Bits | 125 | creature TokUnlock |
| 4248 | Wolves | You have completed: Fall Fur Fashions | 125 | item TokUnlock |

### Giant - ACID 332

Tier 1 at 5 fragments, tier 2 at 10, tier 3 at 15. 16 bound, 13 awardable.

| ToK entry | Name | Unlock condition | Bestiary id | Award path |
|---:|:---|:---|---:|:---|
| 3586 | Giant | You have completed: Brutal Nose Ring | 59 | item TokUnlock |
| 3588 | Giant | You have completed: Giant Amongst Giants | 59 | creature TokUnlock |
| 3594 | Chaos Giant | You have killed 1,000 Chaos Giants | 60 | bestiary kill |
| 3655 | Gorger | You have killed 1,000 Gorgers | 66 | bestiary kill |
| 3714 | Griffon | You have killed 1,000 Griffons | 72 | bestiary kill |
| 3884 | Ogre Bull | You have completed: It's Safer Outside | 89 | **none** |
| 3886 | Ogre Bull | You have completed: Bigger They Are& | 89 | creature TokUnlock |
| 3894 | Ogre Tyrant | You have killed 1,000 Ogre Tyrants | 90 | bestiary kill |
| 4116 | Troll | You have completed: Rock in the Maw of Darkness | 112 | creature TokUnlock |
| 4118 | Troll | You have completed: Prescribed Flesh | 112 | item TokUnlock |
| 4125 | Chaos Troll | You have killed 1,000 Chaos Trolls | 113 | bestiary kill |
| 4136 | River Troll | You have completed: Gotta Go | 114 | **none** |
| 4138 | River Troll | You have completed: O'de Toilet | 114 | creature TokUnlock |
| 4144 | Stone Troll | You have completed: It's a Snap | 115 | creature TokUnlock |
| 4146 | Stone Troll | You have completed: Indigestion | 115 | **none** |
| 4265 | Yhetee | You have killed 1,000 Yhetee | 127 | bestiary kill |

### Greenskin - ACID 333

Tier 1 at 2 fragments, tier 2 at 3, tier 3 at 5. 5 bound, 5 awardable.

| ToK entry | Name | Unlock condition | Bestiary id | Award path |
|---:|:---|:---|---:|:---|
| 3604 | Gnoblar | You have completed: Tiny Tyrant | 61 | creature TokUnlock |
| 3606 | Gnoblar | You have completed: Cull the Weak | 61 | item TokUnlock |
| 3935 | Savage Orc | You have killed 1,000 Savage Orcs | 94 | bestiary kill |
| 4066 | Snotling | You have completed: Green Lightning | 107 | creature TokUnlock |
| 4068 | Snotling | You have completed: Relish the Relics | 107 | item TokUnlock |

### Chaos - ACID 334

Tier 1 at 5 fragments, tier 2 at 10, tier 3 at 15. 18 bound, 16 awardable.

| ToK entry | Name | Unlock condition | Bestiary id | Award path |
|---:|:---|:---|---:|:---|
| 3036 | Bestigor | You have completed: Appalling Fashion | 4 | item TokUnlock |
| 3038 | Bestigor | You have completed: Nacht to Dread | 4 | **none** |
| 3045 | Bray Shaman | You have killed 1,000 Bray Shaman | 5 | bestiary kill |
| 3054 | Gor | You have completed: Thin the Herd | 6 | **none** |
| 3056 | Gor | You have completed: For the Greater Gor | 6 | creature TokUnlock |
| 3064 | Ungor | You have completed: Thorn in My Side | 7 | creature TokUnlock |
| 3066 | Ungor | You have completed: Trinkets for Treasure | 7 | item TokUnlock |
| 3195 | Chaos Mutant | You have killed 1,000 Chaos Mutants | 20 | bestiary kill |
| 3344 | Doombull | You have killed 1,000 Doombulls | 35 | bestiary kill |
| 3374 | Dragon Ogre | You have killed 1,000 Dragon Ogres | 38 | bestiary kill |
| 3524 | Flayerkin | You have killed 1,000 Flayerkin | 53 | bestiary kill |
| 3724 | Harpies | You have completed: Get Down Here! | 73 | creature TokUnlock |
| 3726 | Harpies | You have completed: Silent Star | 73 | creature TokUnlock |
| 3964 | Plague Victim | You have completed: Epic Epidemic | 97 | creature TokUnlock |
| 3966 | Plague Victim | You have completed: Wash Your Hands | 97 | creature TokUnlock |
| 4166 | Tuskgor | You have completed: Saving Face | 117 | item TokUnlock |
| 4168 | Tuskgor | You have completed: Tusk for Tusk | 117 | creature TokUnlock |
| 4344 | Maggot | You have killed 1,000 Maggots | 135 | bestiary kill |

### Mythical - ACID 335

Tier 1 at 3 fragments, tier 2 at 7, tier 3 at 10. 12 bound, 11 awardable.

| ToK entry | Name | Unlock condition | Bestiary id | Award path |
|---:|:---|:---|---:|:---|
| 3104 | Hydra | You have killed 1,000 Hydra | 11 | bestiary kill |
| 3255 | Cockatrice | You have killed 1,000 Cockatrice | 26 | bestiary kill |
| 3396 | Dryad | You have completed: Chip Off the Old& | 40 | creature TokUnlock |
| 3398 | Dryad | You have completed: Fire Seldom Dies Alone | 40 | **none** |
| 3855 | Manticore | You have killed 1,000 Manticores | 86 | bestiary kill |
| 4086 | Spite | You have completed: Lost and Found | 109 | creature TokUnlock |
| 4088 | Spite | You have completed: A Defiled Life-Force | 109 | item TokUnlock |
| 4105 | Treekin | You have killed 1,000 Treekins | 111 | bestiary kill |
| 4184 | Unicorn | You have killed 1,000 Unicorns | 119 | bestiary kill |
| 4255 | Wyvern | You have killed 1,000 Wyvern | 126 | bestiary kill |
| 4304 | Imp | You have killed 1,000 Imps | 131 | bestiary kill |
| 4364 | Treemen | You have killed 1,000 Treemen | 137 | bestiary kill |

### Man - ACID 336

Tier 1 at 2 fragments, tier 2 at 4, tier 3 at 6. 6 bound, 3 awardable.

| ToK entry | Name | Unlock condition | Bestiary id | Award path |
|---:|:---|:---|---:|:---|
| 3645 | Night Goblin | You have killed 1,000 Night Goblins | 65 | bestiary kill |
| 3946 | Bandit | You have completed: Wanted! | 95 | creature TokUnlock |
| 3948 | Bandit | You have completed: Robbery | 95 | **none** |
| 4286 | Cultist | You have completed: Don't Die All At Once | 129 | **none** |
| 4288 | Cultist | You have completed: Hard To Be a Leader | 129 | creature TokUnlock |
| 4354 | Metal Construct | You have killed 1,000 Living Armors | 136 | **none** |

### Skaven - ACID 337

Tier 1 at 1 fragments, tier 2 at 2, tier 3 at 3. 3 bound, 2 awardable.

| ToK entry | Name | Unlock condition | Bestiary id | Award path |
|---:|:---|:---|---:|:---|
| 3994 | Rat Ogre | You have killed 1,000 Rat Ogres | 100 | bestiary kill |
| 4044 | Skaven | You have completed: What a Rat's Nest | 105 | **none** |
| 4048 | Skaven | You have completed: Septic Sanitation | 105 | creature TokUnlock |

### Undead - ACID 338

Tier 1 at 4 fragments, tier 2 at 8, tier 3 at 12. 22 bound, 21 awardable.

| ToK entry | Name | Unlock condition | Bestiary id | Award path |
|---:|:---|:---|---:|:---|
| 3544 | Banshee | You have killed 1,000 Banshee | 55 | bestiary kill |
| 3556 | Spirit Host | You have completed: Higher Caliber of Soul | 56 | creature TokUnlock |
| 3558 | Spirit Host | You have completed: Worth the Waiting For | 56 | creature TokUnlock |
| 3565 | Wraith | You have killed 1,000 Wraiths | 57 | bestiary kill |
| 3825 | Liche | You have killed 1,000 Liches | 83 | bestiary kill |
| 4054 | Skeleton | You have completed: Bones Have Names Too | 106 | creature TokUnlock |
| 4056 | Skeleton | You have completed: When There's a Will | 106 | item TokUnlock |
| 4174 | Bone Giant | You have killed 1,000 Bone Giants | 118 | bestiary kill |
| 4194 | Vampire | You have killed 1,000 Vampires | 120 | bestiary kill |
| 4225 | Wight | You have killed 1,000 Wights | 123 | bestiary kill |
| 4234 | Winged Nightmare | You have killed 1,000 Winged Nightmares | 124 | bestiary kill |
| 4276 | Zombie | You have completed: The Family That Dies Together | 128 | **none** |
| 4278 | Zombie | You have completed: Wulgrig | 128 | creature TokUnlock |
| 4324 | Walker | You have killed 1,000 Walkers | 133 | bestiary kill |
| 4402 | Asp Bone Construct 1000 | You have killed 1,000 Asp bone Constructs | 620 | bestiary kill |
| 4407 | Scarab Bone Construct 1000 | You have killed 1,000 Scarab Bone Constructs | 621 | bestiary kill |
| 4412 | Tomb Scorpions 100 | You have killed 100 Tomb Scorpions | 622 | bestiary kill |
| 4417 | Ushabti 100 | You have killed 100 Ushabti | 623 | bestiary kill |
| 4422 | Preserved Dead 100 | You have killed 100 Preserved Dead | 624 | bestiary kill |
| 4427 | Giant Scarab 1000 | You have killed 1,000 Giant Scarabs | 625 | bestiary kill |
| 4432 | Tomb Swarm 1000 | You have killed 1,000 Tomb Swarm | 626 | bestiary kill |
| 4437 | Carrion 1000 | You have killed 1,000 Carrion | 627 | bestiary kill |

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
