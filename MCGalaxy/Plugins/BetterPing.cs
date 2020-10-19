/* 
	Below utilizes Nagle's Algorithm to improve the ping of players upon server join.
	For more information on Nagle's Algorithm see: https://networkencyclopedia.com/nagles-algorithm/
 */

using System;
using MCGalaxy.Events.PlayerEvents;

namespace MCGalaxy {
	public class BetterPing : Plugin_Simple {
		public override string creator { get { return "Venk"; } }
		public override string MCGalaxy_Version { get { return "1.9.1.3"; } }
		public override string name { get { return "BetterPing"; } }
		
		public override void Load(bool startup) {
			OnPlayerConnectEvent.Register(DisableNagle, Priority.High);
		}
		
		static void DisableNagle(Player p) {
			try { p.Socket.LowLatency = true; } catch { }
		}
		
		public override void Unload(bool shutdown) {}
	}
}
