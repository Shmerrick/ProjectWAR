# ![logo](https://user-images.githubusercontent.com/812439/170816868-98fbcf66-8f8a-4d30-989f-4f97bb1c0254.png)

Discord: https://discordapp.com/invite/MBxsJBQ

# README #

#### Download los and zones files, put it in your server folder ####

https://drive.google.com/file/d/1TgHnAsn-reUbLyGavd9hLUVuXR5dEbf2/view?usp=sharing

#### Download MySQL Community ####

https://dev.mysql.com/downloads/installer/

#### Download SQLyog Community ####

https://github.com/webyog/sqlyog-community/wiki/Downloads

#### Download Full WAR Client ####

https://mega.nz/file/3AVRCRSZ#GTvN2wphlqjOflUe1OdAcT1oyjSw0y0Aqq5U4x2o3F4

Mirror 2: https://mega.nz/file/8OYXgQIZ#Pshha5mzTifZbxQzSwekI8DX5V_dsssARXJ51JDlLyk

Mirror 3: https://drive.google.com/file/d/1QODsa-Bp5ON_2FTz8Nqca9c-MwM5_Lyl/view?usp=sharing 

#### Language Packs ####
French Language Pack: https://drive.google.com/file/d/18DMb2OjRVjtIBi1Oj462_mAcKugC8463/view

Spanish Language Pack: https://drive.google.com/file/d/1PMcimU7vfN5DiaW4I0VLIR3va54ngQho/view

Russian Language Pack: https://drive.google.com/file/d/1hdh7-tYh_NkLQt4kzy12zYrN9kOk8ylb/view

German Language Pack: https://drive.google.com/file/d/1FzQvQf52o-oneNlnSEBiy8VakWbhmL_c/view

#### Development Tools for Server ####

https://gitlab.com/Shmerrick/projectwar2-tools

#### YouTube channel ####

https://www.youtube.com/channel/UCZVtxRU0aEML7qRcIuukZtQ

#### nuget packages ####
* If you get an error compiling, complaining about NLog, you may need to force the VS package manager to get NLog. Do the following : 
	* install-package NLog  
		* Make sure you select the Default Project = Launcher and WarConsoleLauncher
		* If that fails, uninstall-package Nlog, and the install-package NLog
	* install-package Evolve
* You can also look to use Update-Package -reinstall (which should update all the packages for you)

#### Building and running ####

* There are really a couple of 'modes' of developing / debugging the solution. 
	* Debug the servers
		* Set the solution to start multiple projects (AccountCacher, LauncherServer, LobbyServer, WorldServer). This will start these 4 projects in debug mode. You can then connect the client to these. 
	* Debug DB changes
		* You can do this in combination with 'Debug the Servers', but might be better(?) to just run the Servers up without the debugger attached and connect with a Client to see if the changes made work as expected.
	* Debug the Launcher
		* Set the solution to start a single project, Launcher.

### Implementing new Battlefield Objectives

Current system uses database file and commands under the name of "Battlefront". The battlefront incorporates all things involving oRvR.
The previous system was instanced PQs that were then used for a larger calculation. It looks as if the new system generates some type of 'resources'.

The following two tables work in tandem. If you delete an entry for one, or change something out of bounds for the array, the world server will crash.
BOs are currently only spawning in game when a zone is declared a battlefront.

battlefront_objects
battlefront_objectives

The .GO SPAWN object 'Battlefield Objective' should not be confused with code/DB driven battlefronts. This object is only a nuetral flag that has been named  'Battlefield Objective'.

### T1 and T4 Progression - July 2018 ###

The T1 and T4 Battlefront progressions have been re-written.

T1 is designed to rotate from Emp -> Dwarf -> Elf. This should look and act in a similar fashion to the current T1 flow.

T4 is based upon starting at Praag. Locking that zone will take you to CW or Reik, depending upon the winner of Praag. If that same side locks CW or Reik, the pairing itself will lock and the campaign will move to Dwarf (TM). If the other realm locks, they will return to Praag. 

Simple rewards have been added (mail items), this section is not complete.

The process will repeat through Dwarf and into Elf, until one side locks all 9 zones and then the system will reset and you will be back at Praag. 

Commands of interest : 
.campaign status (gives you the status of the T4 region). 
.campaign setvictorypoints <#> <#> (Number of victory points, and the realm to assign)
.campaign lockpairing <#> <#> (locks the current pairing. # - [1 Order, 2 Dest], # - [0 - no reward, 1 - reward])
.campaign advancepairing <#> <#> (moves the pairing # - [1 Order, 2 Dest], # - tier (1 or 4))
.campaign UpdateRegionCaptureStatus (forces a refresh of the progression [ie the client's map])
.campaign SetRegionCaptureStatus <#> <#> (this one is complex # - forces the region progression to change to the first parameter. The first parameter is a list of 9 digits, the first three representing Dwarf, the next three Empire and the final three Elf. Within each of these pairings is the region, eg <BC><TM><KV> where 1 is lock to Order, 2 is lock to Dest, 0 is unlocked. So 111111110 is lock all to order except Caledor. # - Open and make active the region Id defined by the number). An example is .campaign SetRegionCaptureStatus 111111110 7

Note : Lockpairing and Advancepairing are only two steps in the 'flip' process. They should only be used by developers (ie dont use them in Production). For testing, it is better to use .campaign setvictorypoints <#> <#>, as this emulates holding BOs/keep takes equivalent to the number of VP.

If it all goes haywire, reset by .campaign SetRegionCaptureStatus 102102102 2  

### Database - Updated May 2021 ###

The ability system controls everything in the game, from potions, to skills, to hidden buffs, etc, etc.

Potion System
Item Level = MinLevel + Rarity

The item level will cast the potion ability at the given level.
	
### creature_smart_ability - Updated May 2022 ###
	
We updated system for use one of more production creature system in the game. Anyway we can script creatures by the types.
