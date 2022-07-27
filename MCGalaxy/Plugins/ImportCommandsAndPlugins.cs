// Notice The commands were made by Venk and were merged into one plugin by Ninja

//reference System.dll
using System;
using System.IO;
using System.Net;
using MCGalaxy;
using MCGalaxy.Levels.IO;
using MCGalaxy.Network;
//This Command was made by venk i only combined it into one simple plugin
namespace Core {
    public class ImportCommandsAndPlugins : Plugin {  
        public override string creator { get { return "Ninja"; } }
        public override string MCGalaxy_Version { get { return "1.9.3.0"; } }
        public override string name { get { return "ImportCommandsAndPlugins"; } }

        public override void Load(bool startup) {
            Command.Register(new CmdImportCommand());
            Command.Register(new CmdImportPlugin());
        }
        
        public override void Unload(bool shutdown) {
        	Command.Unregister(Command.Find("ImportCommand"));
            Command.Unregister(Command.Find("ImportPlugin"));
        }
    }
    
       public sealed class CmdImportCommand : Command2
    {
        public override string name { get { return "ImportCommand"; } }
        public override string shortcut { get { return "importcmd"; } }
        public override string type { get { return CommandTypes.World; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }

        public override void Use(Player p, string message)
        {
            string[] args = message.SplitSpaces();

            if (args.Length == 0) { p.Message("You need to specify a URL to import."); return; }
            if (args.Length == 1) { p.Message("You need to specify what to call the new command."); return; }

            if (File.Exists("./extra/commands/source/" + args[1] + ".cs"))
            {
                p.Message("Command already exists.");
                return;
            }

            WebClient Client = new WebClient();
            Client.DownloadFile(args[0], "./extra/commands/source/" + args[1] + ".cs");

            p.Message("Finished importing command %b" + args[1] + "%S.");
        }

        public override void Help(Player p)
        {
            p.Message("%T/ImportCommand [url] [output name] %S- Imports a .cs command.");
        }
    }
    

    public sealed class CmdImportPlugin : Command2
    {
        public override string name { get { return "ImportPlugin"; } }
        public override string shortcut { get { return "importp"; } }
        public override string type { get { return CommandTypes.World; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }

        public override void Use(Player p, string message)
        {
            string[] args = message.SplitSpaces();

            if (args.Length == 0) { p.Message("You need to specify a URL to import."); return; }
            if (args.Length == 1) { p.Message("You need to specify what to call the new plugin."); return; }

            if (File.Exists("./plugins/" + args[1] + ".cs"))
            {
                p.Message("Plugin already exists.");
                return;
            }

            WebClient Client = new WebClient();
            Client.DownloadFile(args[0], "./plugins/" + args[1] + ".cs");

            p.Message("Finished importing plugin %b" + args[1] + "%S.");
        }

        public override void Help(Player p)
        {
            p.Message("%T/ImportPlugin [url] [output name] %S- Imports a .cs plugin.");
            
        	
        }
    }
}
