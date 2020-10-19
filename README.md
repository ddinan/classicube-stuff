# Welcome to my repository for MCGalaxy tools!
Here you will find the source of my public plugins and commands, free to use however you please. If a command/plugin isn't compiling or working as it should, make a new issue [here](https://github.com/VenkSociety/MCGalaxy-Tools/issues) and I'll get to it when possible. Command/plugin requests are **open** and you can suggest them [here](https://github.com/VenkSociety/MCGalaxy-Tools/issues).

# List

### Plugins
| Name | Description |
| ------------- | -----|
|  **AntiVPN** | Doesn't let people with a VPN join the server.
|  **BetterPing** | Makes your ping significantly lower.
|  **BugWebhook** | Send a message on Discord whenever there's an error on your server.
|  **Compass** | Adds a compass into your HUD.
|  **CustomChat*** | Allows changing the chat format.
|  **CustomSoftware** | Allows changing the software name both in-game and in the launcher.
|  **CustomTab*** | Allows changing the format of your tab list.
|  **DailyBonus** | Give people money once per day when they login.
|  **Example** | Example source code in case you want to make your own plugins.
|  **Flood*** | Allows client-side water flooding.
|  **GamemodeTemplate** | The template I use on my servers.
|  **LastLocation*** | Return to your last known location.
|  **Nametags*** | Changes your nametag.
|  **Parties** | Join and talk with specific people.
|  **PvP** | Click on players to do damage and knock them back.
|  **XP** | Adds an XP and leveling system.

`*` means that the plugin is private or made exclusive for specific people.

### Commands
| Name | Description |
| ------------- | -----|
|  **/Announce** | Show custom text in the middle of peoples' screen.
|  **/FakeGive** | Tell people they've gotten money but they haven't.
|  **/Preset** | Easier access for /os env preset that doesn't require realm ownership to use.
|  **/Remove** | Removes a player from the playerbase permanently.
|  **/ImportSchematic** | Imports a .schematic file from Minecraft into CC.

# Installation

### How to install plugins:
1. Put the plugin's .cs file into the **./plugins/** folder. If it's not there, make a new folder and put it in there.
2. Either in-game or via the server console, type **/pcompile [plugin name]**. It should say **Plugin compiled successfully.**, if it doesn't, make an issue [here](https://github.com/VenkSociety/MCGalaxy-Tools/issues).
3. Now type **/pload [plugin name]**. It should say **Plugin loaded successfully.**
4. And you're done, enjoy your plugin.

### How to install commands:
1. Put the command's .cs file into the **./extra/commands/source/** folder. If it's not there, make a new folder and put it in there.
2. Either in-game or via the server console, type **/compile [command name]**. It should say **Command compiled successfully.**, if it doesn't, make an issue [here](https://github.com/VenkSociety/MCGalaxy-Tools/issues).
3. Now type **/cmdload [command name]**. It should say **Command loaded successfully.**
4. And you're done, enjoy your command.
