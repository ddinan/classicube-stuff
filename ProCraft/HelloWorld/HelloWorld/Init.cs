// To use the plugin:
// - Add a reference to fCraft.dll
// - Press compile
// - Place the compiled [plugin].dll file from ./bin/ into your server's plugin folder
// - Restart server

using System;
using System.Timers;
using System.Collections.Generic;

using fCraft;
using fCraft.Events;

namespace HelloWorldPlugin {

	internal sealed class Init : Plugin {

		public const string PlainName = "HelloWorld";
		public const string PlainVersion = "0.1";
		public const string PlainAuthor = "Venk";
		public const string PlainDescription = "A template LegendCraft/ProCraft plugin for implementing custom commands.";

		public string Name {
			get {
				return PlainName;
			}

			set {
			}
		}

		public string Version {
			get {
				return PlainVersion;
			}

			set {
			}
		}

		public string Author {
			get {
				return PlainAuthor;
			}
		}

		public string Description {
			get {
				return PlainDescription;
			}
		}
		
		static readonly CommandDescriptor CmdHelloWorld = new CommandDescriptor
        {
            Name = "HelloWorld",
            Category = CommandCategory.Moderation,
            IsConsoleSafe = true,
            Permissions = new[] { Permission.Chat },
            Usage = "/HelloWorld",
            Help = "Sends you a message saying \"Hello world!\"",
            Handler = CmdHelloWorldHandler
        };

        static void CmdHelloWorldHandler(Player p, CommandReader cmd) {
        	p.Message("Hello world!");
        }

		public void Initialize() {
			Logger.Log(LogType.ConsoleOutput, PlainNameAndVersionWithBrackets + " Loading. . .");
			Server.ShutdownEnded += OnServerShutdownEnded;
			
			// Load commands
			CommandManager.RegisterCustomCommand(CmdHelloWorld);

			Logger.Log(LogType.ConsoleOutput, PlainNameAndVersionWithBrackets + " Loaded!");
		}

		private void OnServerShutdownEnded(object sender, ShutdownEventArgs shutdownEventArgs) {
			Server.ShutdownEnded -= OnServerShutdownEnded;
		}
	}
}
