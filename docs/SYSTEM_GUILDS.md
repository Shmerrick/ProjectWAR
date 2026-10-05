# System Guilds

## Overview
ProjectWAR includes a built-in "System Guild" feature designed to provide all new players with an immediate sense of community and a base for operations. These guilds are automatically maintained by the server and serve as the default home for every character entering the world.

## The System Guilds
There are two primary system guilds, one for each realm:
- **Forces of Order**: For all Order characters (Dwarf, High Elf, Empire).
- **Forces of Destruction**: For all Destruction characters (Chaos, Dark Elf, Greenskin).

## Mechanics

### 1. Automatic Joining
Every new character created on the server will automatically join their respective realm's system guild the first time they log into the game world. This ensures that even on a low-population local server, players have access to guild chat and social structures immediately.

### 2. Level and Progression
System guilds are automatically set to **Level 40** (the maximum guild level) by the server on startup. This gives all members immediate access to any guild-level-dependent features or bonuses available in the emulator.

### 3. Departure and Restrictions
Players are free to leave the system guilds at any time to join player-created guilds. However, to maintain the "starter" nature of these guilds:
- **Permanent Flag**: Once a player leaves a system guild, a `LeftSystemGuild` flag is set on their character in the database (`characters_value` table).
- **Invite Restriction**: Regular players cannot invite a character back into a system guild if they have already left one. This prevents these guilds from being used as permanent high-level hubs for veteran players who wish to alternate between guilds.

### 4. GM Accounts Administer the System Guilds
System guilds have no character leader (`LeaderId = 0`), so they are administered by GMs. Any character on a GM account (GM level above 1) holds **rank 9 ("System")** in its system guild, which carries every permission bit (`FF FF FF FF FF FF FF 3F`, covering all 56 `GuildPermissions` values): editing ranks and permissions, the MOTD and recruitment page, promoting, demoting and kicking, the guild vault and its money, and guild tactics.

- **Joining**: a GM character that joins a system guild -- automatically on first login, or through `.guildadd` -- starts at rank 9 instead of rank 1 (`Guild.JoinGuild`).
- **Existing members**: on every login, a GM character already in a system guild is raised to rank 9, and a character whose account is no longer GM drops from rank 9 back to rank 1 (`Guild.ApplySystemGuildRank`, from `Player.StartInit`). Officers a GM promoted keep their rank.
- **Boot**: the duplicate-leader check in `CharMgr.LoadGuilds`, which demotes every rank-9 member when there is more than one, skips leaderless system guilds -- otherwise several GM characters at rank 9 would all be demoted on each restart.
- **Handing over leadership**: rank 9 can assign a guild leader. Doing so gives the system guild a real `LeaderId`, after which it is treated like a player guild for leadership and the former-GM demotion no longer applies. Avoid it unless that is the intent.

## GM Management
GMs have specialized commands to manage guild memberships, which can bypass the standard restrictions:

### Commands
- `.guildadd <playerName> <guildName>`
  - Forces a player into a specific guild.
  - If adding to a System Guild, this command **resets** the `LeftSystemGuild` flag, allowing the player to be a member again.
- `.guildremove <playerName>`
  - Forces a player out of their current guild.
  - If removing from a System Guild, this will set the `LeftSystemGuild` flag.
- `.getguildid <guildName>`
  - Returns the internal database ID for a given guild name.

## Database Integration
The system uses the following database structures:
- **Table**: `characters_value`
- **Column**: `LeftSystemGuild` (TINYINT)
- **Update Script**: `Database/update_008_add_leftsystemguild_to_characters_value.sql`

The server automatically ensures these guilds exist in the `guild_info` table on startup. If they are deleted or missing, they will be recreated with default ranks and settings.
