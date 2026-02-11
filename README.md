# ![logo](https://user-images.githubusercontent.com/812439/170816868-98fbcf66-8f8a-4d30-989f-4f97bb1c0254.png)

**Discord:** [https://discordapp.com/invite/MBxsJBQ](https://discordapp.com/invite/MBxsJBQ)

Welcome! This guide will walk you through setting up the project, from installing the required software to compiling and running the server. We've written this for absolute beginners, so don't worry if you're new to this!

---

## 1. Prerequisites: What You Need to Install

Before you can work on this project, you'll need a few tools. Think of these as the ingredients for our recipe. We'll explain what each one is for.

### a) Visual Studio 2022: Your Digital Workshop

**What is this?** Visual Studio is an **Integrated Development Environment (IDE)**. The simplest way to think about it is as a digital workshop for programmers. It has a text editor for writing code, tools for finding and fixing errors, and buttons to "build" or "compile" the code into a program that can actually run.

1.  **Download:** Go to the [Visual Studio download page](https://visualstudio.microsoft.com/downloads/). You will see several options. Download the **Community** edition. This version is completely free for individual developers and students.

2.  **Install the Correct "Workload":** When you run the installer, you'll be shown a screen with many different options called "Workloads." A workload is a pre-packaged set of tools for a specific type of programming.
    -   **What to choose:** It is **very important** that you select the **".NET desktop development"** workload. This project is written in C#, a language that is part of the .NET framework, and this workload includes all the essential tools we need to work with it.
    -   ![VS Installer](https://i.imgur.com/V3t3B7G.png)

3.  **Finish Installation:** After selecting the workload, click the "Install" button. The installer will download and set up everything for you. This might take a little while, so feel free to grab a cup of coffee!

### b) MySQL Server & Workbench: The Game's Memory

**What is this?** A **database** is like a massive, organized collection of filing cabinets for data. Our game server needs one to store all the important information: player accounts, character details, items, monster locations, and so on. MySQL is a popular and free database system.

1.  **Download:** Get the **MySQL Community Server** from the [official MySQL website](https://dev.mysql.com/downloads/mysql/). The standard installer will also include **MySQL Workbench**, which is a separate program that gives you a user-friendly, visual way to look at and manage your database without having to write complex commands.

2.  **Install and Set Your Password:**
    -   Run the installer and follow the on-screen instructions. Choose the "Developer Default" setup type if you are unsure.
    -   During the installation, you will reach a step called "Accounts and Roles." Here, you will be asked to create a password for the MySQL **`root` user**.
    -   **What is a `root` user?** The `root` user is the super-administrator for your database. It has full permission to do anything.
    -   **IMPORTANT:** You **must** write down and remember this password. You will need it in a later step to allow the game server to connect to the database. For a private development machine, using a simple password like `root` or `password` is perfectly fine and can make things easier.

### c) A File Archiver: The Unpacker

**What is this?** The database files we provide are compressed into a single file with a `.rar` extension to save space. You need a special program to "un-pack" or "extract" the files from this archive.

*   **7-Zip:** [Download here (free)](https://www.7-zip.org/download.html)
*   **WinRAR:** [Download here (free trial)](https://www.win-rar.com/download.html)

Install one of these programs. After it's installed, you will be able to right-click on `.rar` files and see an option to extract them.

### d) The WAR Game Client

**What is this?** You need the actual game client to connect to your server and play the game.

- **Download Full WAR Client:**
  - [Mirror 1 (Mega.nz)](https://mega.nz/file/3AVRCRSZ#GTvN2wphlqjOflUe1OdAcT1oyjSw0y0Aqq5U4x2o3F4)
  - [Mirror 2 (Mega.nz)](https://mega.nz/file/8OYXgQIZ#Pshha5mzTifZbxQzSwekI8DX5V_dsssARXJ51JDlLyk)
  - [Mirror 3 (Google Drive)](https://drive.google.com/file/d/1QODsa-Bp5ON_2FTz8Nqca9c-MwM5_Lyl/view?usp=sharing)

#### Optional: Language Packs

If you wish to play in a language other than English, you can download one of the following language packs.

- [French Language Pack](https://drive.google.com/file/d/18DMb2OjRVjtIBi1Oj462_mAcKugC8463/view)
- [Spanish Language Pack](https://drive.google.com/file/d/1PMcimU7vfN5DiaW4I0VLIR3va54ngQho/view)
- [Russian Language Pack](https://drive.google.com/file/d/1hdh7-tYh_NkLQt4kzy12zYrN9kOk8ylb/view)
- [German Language Pack](https://drive.google.com/file/d/1FzQvQf52o-oneNlnSEBiy8VakWbhmL_c/view)

---

## 2. Setting Up Your Database

Now that you have the tools, it's time to set up the database. This is where all the game's data will live.

### a) Extract the Database Files

**What's happening here?** We are un-packing the compressed database blueprints.

1.  In the project folder, find the `SQL DATABASE INSTALL` directory.
2.  Inside, you'll see a file named `SQL DATABASE INSTALL fix.rar`.
3.  Right-click this file. If you installed 7-Zip or WinRAR, you should see an option in the context menu like "7-Zip > Extract Here" or "WinRAR > Extract Here".
4.  Choose to extract the files. A new folder will be created containing several files that end in `.sql`.
    -   **What is a `.sql` file?** It's a script, or a set of instructions, that tells the database system what tables to create and what initial data to put into them. Think of it as the assembly manual for our data filing cabinets.

### b) Create the Databases

**What's happening here?** We are creating the empty "filing cabinets" that our `.sql` scripts will fill with data.

1.  Open **MySQL Workbench**. You should see a connection to your local MySQL server on the main screen.
2.  Click on it to connect. It will ask for the `root` password you created during the MySQL installation. Enter it to continue.
3.  Once you are connected, you will see a main screen with a "Query 1" tab. This is where you can enter commands.
4.  We need to create three new, empty databases (often called "schemas" in Workbench). Copy and paste the following commands into the query editor:
    ```sql
    CREATE DATABASE war_account;
    CREATE DATABASE war_characters;
    CREATE DATABASE war_world;
    ```
5.  To run these commands, you can either click the **yellow lightning bolt icon** at the top of the query editor (which runs all commands) or select each line and click the **lightning bolt icon with a cursor** next to it (which runs only the selected command).
6.  After running the commands, you can verify that the databases were created by looking at the "SCHEMAS" panel on the left. You may need to right-click and choose "Refresh All" to see them appear.

### c) Import the Database Data

**What's happening here?** Now we run the `.sql` scripts to fill our empty databases with all the tables and data the server needs to function.

1.  In MySQL Workbench, look at the top menu bar and click on **Server > Data Import**.
2.  This will open the "Data Import" screen. Select the option that says **"Import from Dump Project Folder"**.
3.  Click the "..." button and navigate to the folder where you extracted the `.sql` files in step 2a. Select that folder.
4.  Workbench will automatically inspect the folder and show the database schemas it found. You should see `war_account`, `war_characters`, and `war_world` listed.
5.  **Important:** Make sure each of these is mapped to the correct target database you created in the previous step. It should do this automatically, but it's good to double-check.
6.  At the bottom of the window, click the **Start Import** button. This process will take some time as it executes all the scripts. You can watch the progress in the import log.
7.  Once it's finished, you're all set! The databases are now ready.

---

## 3. Configuring the Project

**What's happening here?** The server programs need to know how to connect to the database you just set up. We'll edit some configuration files to tell them the server address, database names, and, most importantly, the password you created.

**What are `.xml` files?** XML (eXtensible Markup Language) is a format for storing data in a way that is readable by both humans and machines. In this project, they are used as configuration files.

1.  Navigate to the `src/WorldServer/Configs/LocalDevelopment/` folder in the project directory.
2.  You will see three important files: `Account.xml`, `Lobby.xml`, and `World.xml`. You need to edit all three.
3.  Open each of these files with a text editor. You can use a basic one like Notepad, or open them right inside Visual Studio by double-clicking on them in the Solution Explorer.
4.  Inside each file, you need to find the database connection settings and change the password.

**a) Editing `Account.xml`**

Look for this section. You only need to change the password.

```xml
<AccountDatabase>
  <Server>127.0.0.1</Server>
  <Port>3306</Port>
  <Database>war_account</Database>
  <Username>root</Username>
  <Password>password</Password> <!-- CHANGE THIS to your MySQL root password -->
  ...
</AccountDatabase>
```

**b) Editing `Lobby.xml`**

Look for this section. Again, just change the password.

```xml
<CharacterDatabase>
  <Server>127.0.0.1</Server>
  <Port>3306</Port>
  <Database>war_characters</Database>
  <Username>root</Username>
  <Password>password</Password> <!-- CHANGE THIS to your MySQL root password -->
  ...
</CharacterDatabase>
```

**c) Editing `World.xml`**

This file has two sections to edit. Change the password in both of them.

```xml
<CharacterDatabase>
  <Server>127.0.0.1</Server>
  <Port>3306</Port>
  <Database>war_characters</Database>
  <Username>root</Username>
  <Password>password</Password> <!-- CHANGE THIS to your MySQL root password -->
  ...
</CharacterDatabase>
<WorldDatabase>
  <Server>127.0.0.1</Server>
  <Port>3306</Port>
  <Database>war_world</Database>
  <Username>root</Username>
  <Password>password</Password> <!-- CHANGE THIS to your MySQL root password -->
  ...
</WorldDatabase>
```

**IMPORTANT:** The `<Password>` value in **all three files** must be the exact same `root` password you set up when you installed MySQL. If they don't match, the servers won't be able to start correctly.

---

## 4. Compiling the Project in Visual Studio

You're almost there! The final step on the computer-side of things is to "build" or "compile" the project.

**What is compiling?** Compiling is the process of turning the human-readable source code you see in the text files into machine-readable instructions (an executable program, like an `.exe` file) that your computer can understand and run.

### a) Open the Solution

1.  Find the file named `ProjectWAR.sln` in the root folder of the project. A `.sln` file is a "solution" file, which is a master file that organizes all the different parts of our project for Visual Studio.
2.  Double-click this file. It should open automatically in Visual Studio. You'll see a list of all the projects in the "Solution Explorer" panel, usually on the right.

### b) Build the Solution

1.  Once the project is fully loaded, go to the top menu in Visual Studio and click **Build > Build Solution**.
2.  Visual Studio will now do two things:
    -   **Restore NuGet Packages:** It will automatically download all the necessary third-party libraries that the project depends on. (Think of **NuGet packages** as pre-built toolkits or add-ons that save us from having to write everything from scratch).
    -   **Compile the Code:** It will go through all the project code and build it into the final server applications.
3.  You can watch the progress in the "Output" window at the bottom of Visual Studio.
4.  If everything is successful, you'll see a "Build succeeded" message at the end. Congratulations! You've just compiled the server.

### Troubleshooting Build Errors

Sometimes, the build process can fail. This is a normal part of programming! Here's a common issue and how to solve it.

*   **Error: Package restore fails with repository-signing or .NET tooling errors**
    -   **Symptom:** Build output shows errors similar to "Compilation couldn't be verified because the required .NET tools were missing and repository-signing problems blocked dependency installation."
    -   **Solution:** Ensure that Visual Studio 2022 with the ".NET desktop development" workload or the .NET Framework 4.8 Developer Pack is installed. If your NuGet feed includes unsigned packages, add a `NuGet.config` file with `<signatureValidationMode>accept</signatureValidationMode>` to the project root or add the feed's certificate to your `trustedSigners`, then retry the restore.

*   **Error: "The type or namespace name 'NLog' could not be found"** (or similar errors for `Evolve`, `MySql.Data`, etc.)
    -   **What it means:** This usually means the NuGet packages (the add-ons) didn't download or install correctly.
    -   **How to fix it:** You can force Visual Studio to reinstall them.
        1.  Go to **Tools > NuGet Package Manager > Package Manager Console**. This opens a command-line interface inside Visual Studio.
        2.  In the console window that appears at the bottom, type the following command and press Enter:
            ```
            Update-Package -reinstall
            ```
        3.  This command tells Visual Studio to go through every project in the solution and reinstall all of its dependencies. It may take a few minutes.
        4.  Once it's finished, try building the solution again by going to **Build > Build Solution**. The errors should now be gone.

---

## 5. Running the Server

With the project compiled, you are ready to launch the servers!

**What are all these servers?** This project runs as a set of separate but interconnected server applications. Here's what they do:

*   `LauncherServer`: Manages the game launcher, sending updates and news.
*   `AccountCacher`: Handles account-related data and authentication.
*   `LobbyServer`: Manages the character creation screen, world list, and logging into the game world.
*   `WorldServer`: This is the main server that runs the game world itself. All the in-game action happens here.

To run them all at once for development, we need to configure Visual Studio's startup settings.

1.  In Visual Studio, find the **"Solution Explorer"** panel (usually on the right).
2.  Right-click on the very top item, which should say **Solution 'ProjectWAR'**.
3.  In the context menu, click **Properties**.
4.  A new window will open. On the left, under **Common Properties > Startup Project**, select the **Multiple startup projects** option.
5.  You will see a list of all the projects. For each of the following four projects, change the **Action** in the dropdown menu to **Start**:
    -   `AccountCacher`
    -   `LauncherServer`
    -   `LobbyServer`
    -   `WorldServer`
6.  Click **Apply**, then **OK**.
7.  Now, when you press the big **Start** button (the green play icon ▶️) in the top toolbar of Visual Studio, it will launch all four server applications simultaneously. You will see several black console windows appear. These are your running servers!

You now have a locally running server! The next step would be to connect to it with the game client.

---

## 6. General Troubleshooting

If you've followed all the steps and something still isn't working, here are a few extra things to check.

*   **Incorrect Password in Config Files:**
    -   **Symptom:** The server console windows open and then immediately close, or you see "Access denied for user 'root'@" in the logs.
    -   **Solution:** This is the most common problem. Double-check that the password you entered in the three `.xml` config files (`Account.xml`, `Lobby.xml`, `World.xml`) is **exactly** the same as the `root` password you created when installing MySQL.

*   **Windows Firewall:**
    -   **Symptom:** The servers run, but you can't connect with the game client, or the launcher can't patch.
    -   **Solution:** Your Windows Firewall might be blocking the connection. When you first run the servers, Windows may ask you for permission to allow them through the firewall. Make sure you grant access for them on Private networks.

*   **Database Not Running:**
    -   **Symptom:** The servers fail to start and the logs mention "Unable to connect to any of the specified MySQL hosts."
    -   **Solution:** Make sure your MySQL database server is actually running. You can check this by opening the Services app in Windows and looking for a service named "MySQL" (the exact name might vary slightly). It should have a status of "Running".

*   **Build Failed:**
    -   **Symptom:** You can't press the "Start" button because it is greyed out, or you get a message saying "The project needs to be built."
    -   **Solution:** This means the project didn't compile correctly. Look in the "Error List" window in Visual Studio to see what went wrong and refer back to the "Troubleshooting Build Errors" section above.
