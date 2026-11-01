# Playtest checklist

Every tracker row still marked "client retest pending", turned into five play sessions that one
person can run. `tools/validation/Get-BugTrackerSummary.ps1 -Status Retest` is the live list. On
2026-10-30 it had 55 rows, and every one of them is placed below exactly once.

The ordering is deliberate: sessions 1 and 2 cover the newest, least-played changes and should go
first; sessions 4 and 5 cover dungeon work that the 2026-09-05 retest already partly exercised.

## How the list was cut from 70 to 55

Fifteen economy and network rows were moved to `Fixed; verified by automated check` on 2026-10-30:
BUG-178, 180, 185, 188, 189, 195, 199, 201, 204, 207, 208, 209, 211, 212 and 215. Each one met two
conditions:

- A check in `Invoke-AllValidation.ps1` drives the code that changed, and its tracker row names that
  check.
- The defect needed input an unmodified client never sends: a forged packet, a hard crash, the
  32-bit gold ceiling, or a TCP frame split at a chosen byte.

Playing cannot reproduce any of them, so a playtest would only show that ordinary use still works,
and the sessions below already cover that.

Some rows below have the same kind of exploit half, with no automated check yet: BUG-177, 182, 186,
187, 190, 192 and 194. For these the session tests the ordinary path. Once that passes, mark the
row Fixed and note that the exploit was reviewed in code but cannot be reproduced in the client.

## Before you start

- Use a GM-level account. `.gmcommands <command>` prints the usage of any command below.
- Useful commands:
  - `.modify level <n>` sets your level.
  - `.add item <id> [count]` gives you an item, and `.search item <name>` finds its id.
  - `.add money <copper>` gives you gold.
  - `.teleport entry <zone> [order|destruction]` moves you to a zone's portal arrival point.
  - `.invincible` toggles your own invulnerability.
  - `.kill` kills your target.
  - `.modify morale <0-3600>` sets your morale.
- When a step fails, note the clock time and the step number and carry on. The WorldServer log
  under `bin/Release/logs/` has the rest, so a time and a step are enough to report it.
- Afterwards, report back with something like "session 1 passed, except 1.3". Each step names the
  rows it settles.

## Session 1: combat, one character, about 20 minutes

1. [ ] Set yourself to morale 3600 and use a rank-4 morale ability on a target dummy or a mob. The
   damage should match the number in the tooltip, not 7.5 times it. **BUG-152**
2. [ ] Start a channelled ability, move to break it, and recast it at once. The new channel should
   run its full length and tick normally, with no early stop and no stale cast bar. Repeat with a
   cancel through Escape. **BUG-171**
   - Rank-10 tanks all have *Hold The Line!* (12 s, ticks every 1 s).
   - Other examples: Shaman *Bunch o' Waaagh* (rank 6), Archmage *Searing Touch* (6), Disciple
     *Rend Soul* (6), Warrior Priest *Divine Assault* (6), Marauder *Convulsive Slashing* (6),
     Witch Hunter *Trial By Pain* (12), Witch Elf *Ruthless Assault* (12), Squig Herder *Lots o'
     Arrers* (16), Shadow Warrior *Rapid Fire* (16).
3. [ ] Use an ability with a fractional cooldown. Knight auras (*Press The Attack!*, rank 2) and
   Chosen auras (*Dreadful Fear*, rank 2) are 4.5 s. Spam the key: the ability should not fire
   before the cooldown sweep ends, and should be usable the moment it does. Check a few other
   cooldowns against their tooltips as well. **BUG-174**
4. [ ] Use a potion or another item with a cooldown, then log out and back in before the
   cooldown ends. The remaining time should still show, and the item should stay unusable until it
   ends. Then let a cooldown run out while you are logged out and check that it is gone after
   login. **BUG-175**

## Session 2: shops, mail, auction and vault, one person with two characters, about 40 minutes

Make a second character, the "alt", in the same realm. Put both in one guild with `.guildadd`, and
give each some gold.

1. [ ] Buy three of a non-stacking item from a vendor in one purchase. You should be charged three
   times the price and get three items. **BUG-183**
2. [ ] Buy a large stack of something cheap, 200 or more. The charge should equal the price times
   the count. **BUG-187**
3. [ ] Sell an item and buy it back. It should come back once, the buyback list should drop it,
   and your gold should be correct. Repeat with part of a stack. **BUG-184**
4. [ ] Leave the guild and open a guild-rank vendor. The rank-gated items should not be listed.
   Rejoin the guild. **BUG-210**
5. [ ] Mail the alt an item and some gold. On the alt, take the item and the gold separately, then
   use Take All on a second mail. Everything should arrive exactly once. **BUG-200**
6. [ ] Send the alt a COD mail with an item attached. Pay it on the alt; the sender should then
   receive the gold. **BUG-180** is already closed; this step checks the ordinary path.
7. [ ] Mail the alt an item and gold, then press Return on the alt. The sender should get back the
   title, the item and the gold, and the message should say "returned", not "expired". **BUG-181**
8. [ ] Post an auction from a bag slot on one character and buy it on the other. The item and the
   gold should each arrive once. If the auction house refuses a purchase from the same account,
   move this step to session 3. **BUG-194**
9. [ ] Put an item and some gold into the guild vault, then take them out on the alt. Then pick up
   a vault item on one character and log out without dropping it; the alt should still be able to
   withdraw that item. **BUG-190, BUG-191**
10. [ ] Exchange one Warlord item at the Black Market vendor (find one with `.search item
    Warlord`). It should cost one item and give one reward. Then send one through Black Market mail,
    which should pay one reward. **BUG-186, BUG-182**
11. [ ] Fill your backpack, then:
    - Craft an apothecary or talisman item. Nothing should be consumed, and you should get a "no
      room" message.
    - Open a currency box. The box should not be used up.
    - Salvage an item. The result should arrive by mail.

    **BUG-205**
12. [ ] Try to craft without enough power, then craft properly. Crafting should not lock up.
    Swap an additive out after adding it; the craft should abort without consuming anything.
    **BUG-196**
13. [ ] Harvest a cultivation plot with a nearly full backpack. You should get no extra rare seed,
    and the plot should not become harvestable again. **BUG-197**
14. [ ] In the talisman window:
    - Add a talisman, then remove it with a full backpack. It should not vanish.
    - Abort the window with unfused talismans in it. They should come back, by mail if needed.

    **BUG-198**
15. [ ] Socket three talismans into an item that has the slots, fuse them, restart the server and
    log in again. The item and all three talismans should still be there. **BUG-202**
16. [ ] Equip and remove an item that has a talisman, and check that your stats return to normal.
    Open a mystery bag or a reward choice. **BUG-213**

## Session 3: two characters online at once, about 15 minutes

This session needs a second account and a second client window. Whether `WAR.exe` will run twice
on one machine has not been checked, so try that first. If it won't, these rows wait until
someone can play the second character, or until a bot helper exists (see the end of this file).

1. [ ] Trade part of a stack and some gold. Both trade windows should show the other side's offer,
   it should stay visible after one side accepts, and the items and gold should arrive once.
   **BUG-206, BUG-177**
2. [ ] Trade with the receiver's backpack full. The trade should be refused, and nothing should be
   lost on either side. **BUG-179**
3. [ ] Group up and kill a mob that drops loot:
   - Roll Pass and then Need on the same item. The second vote should not count.
   - The winner of a roll with a full backpack should get the item by mail.

   **BUG-193**
4. [ ] Leave the group. The other character should not be able to loot your kill. **BUG-192**
5. [ ] With the two characters grouped, enter Mount Gunbad or Bastion Stair once. Then enter an
   ordinary group dungeon together: the second member should not get "This instance is already
   full". **BUG-080**

## Session 4: Mount Gunbad, about 45 minutes

Use `.modify level 30`, `.invincible` and `.teleport entry 60`.

1. [ ] Ordinary creatures in Gunbad should con at ranks 21–33, not 40–42. **BUG-064**
2. [ ] Run the Squig Nursery public quest (514) to the end:
   - The Nursery Slimes can be broken.
   - Foul Mouf da 'ungry appears.
   - Deathspewin' Squigs count toward "Monstrous Squigs".
   - The squigs respawn quickly enough to reach 50.

   **BUG-070, BUG-071, BUG-102**
3. [ ] While doing that, the public quest window should advance on every kill, also with a second
   quest attached. At completion it should clear and not leave a stale stage on screen.
   **BUG-056, BUG-106, BUG-109**
4. [ ] Enter each of the four boss wings, as each realm if you can. **BUG-087**
5. [ ] Kill a wing boss with `.kill`. The server should not throw an error, and an exit portal
   should appear and work. **BUG-093**
6. [ ] Leave and re-enter that wing. The killed boss should still be dead. **BUG-107**
7. [ ] Log out inside Gunbad and log back in. You should come back somewhere sensible: the dungeon
   exit or your capital, never Avelorn. **BUG-054**
8. [ ] Over this whole session, no portal, wing jump or flight should drop you to character
   select. **BUG-094**

## Session 5: Bastion Stair, Tomb of the Vulture Lord, Warpblade Tunnels and Land of the Dead, about 45 minutes

1. [ ] As Destruction, walk into the Bastion Stair entrance in the Chaos Wastes. You should land in
   the entrance hall, not the Inevitable City. **BUG-062**
2. [ ] In Bastion Stair, the Path of Fury gold chest should appear once and in the right place.
   The Khornite Altar and Bloodherd Champion objectives should be completable. **BUG-057, BUG-085**
3. [ ] The Tomb of the Vulture Lord portal should take you in. **BUG-088**
   - As Order and then as Destruction, each realm should arrive on its own side. **BUG-096**
   - Creatures should show the Greater Ward sigil on target. **BUG-098**
   - Kills of ordinary creatures should advance its public quests, including Anointing Embalmers,
     High Priest Herakh and Skeletal Soldiers. **BUG-100, BUG-101**
4. [ ] Warpblade Tunnels creatures should show the Lesser Ward sigil, the same as Sigmar Crypts.
   **BUG-092**
5. [ ] Land of the Dead:
   - Run `.lotd unlock 1` (or `2` for Destruction).
   - Open the world map from any zone. The Land of the Dead bars should show, and also on the HUD
     tracker. **BUG-066**
   - The flight master should list Land of the Dead, and the flight should land you in your own
     realm's warcamp. **BUG-067, BUG-077**
   - Public quest trackers in the zone should advance on kills. **BUG-103**
   - Restart the server. Access should still be open afterwards. **BUG-068**

## Not testable by playing

- **BUG-203**: expired mail being returned rather than destroyed at boot. Reproducing it needs mail
  to expire while its recipient stays offline across two restarts, which is not practical by hand.
  It needs an automated check.
- **BUG-217**: the bot editor API no longer sends CORS headers. The next time you use
  `bot-template-viewer`, edit and save one bot's gear. If that works, the row is done.

## If a second player is the blocker

Session 3 is the only part that needs two people at once. If a second client window isn't
possible, the server already runs bots as real `Player` characters, so two helpers could replace
the second person:

- A GM command that makes a nearby bot accept a trade.
- A command that makes a bot join your group and roll on loot.

The rows with no automated check (BUG-177, 182, 186, 187, 190, 192, 194 and 203) can also be moved
to "verified by automated check" by adding checks to `RuntimeRegressionChecks.cs`. That takes them
off this list entirely.
