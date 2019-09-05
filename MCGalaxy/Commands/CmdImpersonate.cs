// Evil command.
namespace MCGalaxy.Commands {
    
    public sealed class CmdImpersonate : Command {

        public override bool museumUsable { get { return true; } }
        public override string name { get { return "Impersonate"; } }
        public override string shortcut { get { return "imp"; } }
        public override string type { get { return CommandTypes.Other; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        
        public override void Use(Player p, string message) {
            if (!message.Contains(" ")) { Help(p); return; }
            string[] args = message.SplitSpaces(2);
            Player who = PlayerInfo.FindMatches(p, args[0]);
            if (who == null || message == "") {Help(p); return; }
            if (who.muted) { Player.Message(p, "Cannot impersonate a muted player"); return; }
            
            if (p == null || p == who || p.Rank > who.Rank) {
                Player.SendChatFrom(who, args[1]);
            } else {
                MessageTooHighRank(p, "Impersonate", false); return;
            }
        }
        
        public override void Help(Player p) {
            Player.Message(p, "%T/Impersonate [player] [message]");
            Player.Message(p, "%HSends a message as if it came from [player]");
        }
    }
}
