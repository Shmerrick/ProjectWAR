# README #

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