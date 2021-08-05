using System;
using System.Collections.Generic;

namespace MCGalaxy.Commands.Info
{
    public sealed class CmdListLevels : Command2
    {
        public override string name { get { return "ListLevels"; } }
        public override string type { get { return CommandTypes.Information; } }
        public override bool UseableWhenFrozen { get { return true; } }

        public override void Use(Player p, string message, CommandData data)
        {
            string[] maps = LevelInfo.AllMapNames();

            List<string> levels = new List<string>(maps);

            string[] args = message.SplitSpaces();

            Group grp = null;

            if (args.Length > 0)
            {
                grp = Matcher.FindRanks(p, args[0]);
                if (grp == null) return;


                foreach (string map in maps)
                {
                    LevelConfig cfg = LevelInfo.GetConfig(map);
                    if (cfg == null) continue;

                    if (cfg.BuildMin == grp.Permission || cfg.VisitMin == grp.Permission)
                    {
                        levels.Add(map);
                    }
                }

                maps = levels.ToArray();
            }

            else Help(p);

            p.Message(string.Join("%S, " + grp.Color, maps));
        }

        public override void Help(Player p)
        {
            p.Message("%T/ListLevels [rank]");
            p.Message("%HLists loaded levels and their physics levels.");
        }
    }
}
