# Welcome to my repository for MCGalaxy tools!
Here you will find the source of my public plugins and commands, free to use however you please. If a command/plugin isn't compiling or working as it should, make a new issue [here](https://github.com/ddinan/classicube-stuff/issues) and I'll get to it when possible. Command/plugin requests are **open** and you can suggest them [here](https://github.com/ddinan/classicube-stuff/issues).

# Installation

### How to install plugins:
1. Put the plugin's .cs file into the **./plugins/** folder. If it's not there, make a new folder and put it in there.
2. Either in-game or via the server console, type **/pcompile [plugin name]**. It should say **Plugin compiled successfully.**, if it doesn't, make an issue [here](https://github.com/ddinan/classicube-stuff/issues).
3. Now type **/pload [plugin name]**. It should say **Plugin loaded successfully.**
4. And you're done, enjoy your plugin.

### How to install commands:
1. Put the command's .cs file into the **./extra/commands/source/** folder. If it's not there, make a new folder and put it in there.
2. Either in-game or via the server console, type **/compile [command name]**. It should say **Command compiled successfully.**, if it doesn't, make an issue [here](https://github.com/ddinan/classicube-stuff/issues).
3. Now type **/cmdload [command name]**. It should say **Command loaded successfully.**
4. And you're done, enjoy your command.

`NOTE:`
All compiled plugins and commands are now loaded on startup in latest MCGalaxy versions.
# List

### Plugins
| Name | Description |
| ------------- | -----|
|  **AntiVPN** | Prevents people from joining your server with a VPN.
|  **BetterPing** | Makes your ping significantly lower.
|  **BugWebhook** | Send a message on Discord whenever there's an error on your server.
|  **CommandsInPluginExample** | Example source code for embedding commands into your plugin.
|  **Compass** | Adds a compass into your HUD.
|  **Crouching** | Adds an option to crouch in the game by pressing left shift (does not prevent falling off blocks).
|  **CustomChat*** | Allows changing the chat format.
|  **CustomEventExample** | Example source code for adding custom events for cross-plugin communication.
|  **CustomSoftware** | Allows changing the software name both in-game and in the launcher.
|  **CustomStats** | Adds custom /top stats.
|  **CustomTab*** | Allows changing the format of your tab list.
|  **CustomWorldGen** | Example source code for adding custom newlvl themes.
|  **DailyBonus** | Gives people money once per day when they login OR type /daily OR have been on the server for 30+ minutes.
|  **DayNightCycle** | Adds a day-night cycle into the game. Does NOT modify sun or shadow values due to chunk reloading.
|  **DiscordActionLog** | Relays a message to a Discord channel whenever an action has been performed (kick, ban, warn etc).
|  **DiscordChannelName** | Renames a Discord channel name to match server player count.
|  **DiscordVerify** | Bridge between Discord <-> server for linking Discord accounts.
|  **Example** | Example source code in case you want to make your own plugins.
|  **ExampleStoreItem** | Example source code for adding a custom item into /store.
|  **FavouriteMap** | Allows people to set their favourite map which is shown in /whois.
|  **GamemodeTemplate** | The template I use for gamemodes on my server.
|  **HoldBlocks** | Shows the block you're holding in your hand for everybody to see.
|  **IRCWebhook** | Relays a message from your server to Discord without a bot.
|  **LastLocation*** | Return to your last known location.
|  **LocationJoin** | Announces the country players connect to the server from in chat.
|  **Lottery** | Enter and win money.
|  **MobAI*** | Adds custom bot AI instructions. Warning: Experimental.
|  **Nametags*** | Changes your nametag.
|  **NickBlocker** | Prevents using /whonick in a level which has -nicks in its MOTD.
|  **Parties** | Create/join parties and talk privately with specific people (temporary /team).
|  **PlayerCount** | Changes the max player count of the server to be +1 of the player count.
|  **PreventOPBlocks** | Prevents being able to delete OP blocks.
|  **SessionPunishments** | Forces players to be online for the entire duration of their mute/freeze.
|  **SneakAI** | Instantly kills players who get too close to the bot.
|  **Stopwatch** | Adds a stopwatch into the game.
|  **TempFlood*** | Allows client-side water flooding.
|  **TimeAFK** | Shows the amount of time players have been AFK for.
|  **VenkLib** | Essential commands every server owner should have.
|  **VenkSurvival** | Adds survival options such as PvP, drowning, fall damage, hunger, mining and more. Requires VenkLib plugin.
|  **XP** | Adds an XP leveling system.

`*` means that the plugin is private or made exclusive for specific people and requires some modification before adding here.

### Commands
| Name | Description |
| ------------- | -----|
|  **/Adventure** | Easily toggle /map buildable and /map deletable in one command.
|  **/Announce** | Shows custom text in the middle of peoples' screen.
|  **/BestMaps** | Randomly teleport to one of the specified best maps on the server.
|  **/Bottom** | Opposite of /top in that it sorts in ascending order.
|  **/Chevify** | Turns your map into a funky ~~abomination~~ masterpiece.
|  **/FakeGive** | Tells people they've gotten money when they haven't.
|  **/FileManager** | Allows players to modify files without file access.
|  **/FixTP** | Replaces all map textures with a newly-specified one. Useful for mass-fixing levels with broken textures.
|  **/ImportSchematic** | Imports a .schematic file from Minecraft into CC's .lvl format.
|  **/ListLevels** | Shows a list of levels that have either build or visit permissions set to a specified rank.
|  **/MoveEverything** | Moves bots, message blocks and portals relatively. Useful for moving builds.
|  **/Preset** | Easier access for /os env preset that also allows people with build access to change.
|  **/Quote** | Add and view quotes from players.
|  **/Remove** | Removes a player from the database permanently.
|  **/Reward** | Used primarily in message blocks to give rewards to players.
