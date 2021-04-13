//reference System.Core.dll
	
/* 
    You will need to replace all "secretcode" strings with a random code.
	
	- To reward XP from an external plugin/command, type Command.Find("XP").Use(p, "secretcode " + [player] + " [xp amount]"); // Give XP
    
*/

using System;
using System.Collections.Generic;

using MCGalaxy;
using MCGalaxy.Commands;
using MCGalaxy.SQL;

namespace MCGalaxy {
    public class XP : Plugin {
        public override string creator { get { return "Venk"; } }
        public override string MCGalaxy_Version { get { return "1.9.3.0"; } }
        public override string name { get { return "XP"; } }

        public override void Load(bool startup) {
            Command.Register(new CmdXP());
            InitDB();
        }

        public override void Unload(bool shutdown) {
            Command.Unregister(Command.Find("XP"));
        }
        
        ColumnDesc[] createLevels = new ColumnDesc[] {
            new ColumnDesc("Name", ColumnType.VarChar, 16), 
            new ColumnDesc("XP", ColumnType.Int32),
            new ColumnDesc("Level", ColumnType.Int32),
        };
  

        void InitDB() {
            Database.CreateTable("Levels", createLevels);
        }
    }
    
    public sealed class CmdXP : Command2 {
        public override string name { get { return "XP"; } }
        public override string type { get { return "economy"; } }
        
        int GetInt(string s) { return s == "" ? 0 : int.Parse(s); }
		
		int nextLevel(int userLevel, int curXP) {
			if (userLevel == 0) return (50);
			else if (userLevel == 1) return (100);
			else if (userLevel == 2) return (200);
			else if (userLevel == 3) return (300);
			else if (userLevel == 4) return (400);
			else if (userLevel == 5) return (500);
			else if (userLevel == 6) return (600);
			else if (userLevel == 7) return (700);
			else if (userLevel == 8) return (800);
			else if (userLevel == 9) return (900);
			else if (userLevel == 10) return (1000);
			else if (userLevel == 11) return (1250);
			else if (userLevel == 12) return (1500);
			else if (userLevel == 13) return (1750);
			else if (userLevel == 14) return (2000);
			else if (userLevel == 15) return (2250);
			else if (userLevel == 16) return (2500);
			else if (userLevel == 17) return (2750);
			else if (userLevel == 18) return (3000);
			else if (userLevel == 19) return (3250);
			else if (userLevel == 20) return (3750);
			else if (userLevel == 21) return (4250);
			else if (userLevel == 22) return (4750);
			else if (userLevel == 23) return (5250);
			else if (userLevel == 24) return (5750);
			else if (userLevel == 25) return (6250);
			else if (userLevel == 26) return (6750);
			else if (userLevel == 27) return (7250);
			else if (userLevel == 28) return (7750);
			else if (userLevel == 29) return (8250);
			else if (userLevel == 30) return (9000);
			else if (userLevel == 31) return (9750);
			else if (userLevel == 32) return (10500);
			else if (userLevel == 33) return (11250);
			else if (userLevel == 34) return (12000);
			else if (userLevel == 35) return (12750);
			else if (userLevel == 36) return (13500);
			else if (userLevel == 37) return (14250);
			else if (userLevel == 38) return (15000);
			else if (userLevel == 39) return (15750);
			else if (userLevel == 40) return (16750);
			else if (userLevel == 41) return (17750);
			else if (userLevel == 42) return (18750);
			else if (userLevel == 43) return (19750);
			else if (userLevel == 44) return (20750);
			else if (userLevel == 45) return (21750);
			else if (userLevel == 46) return (22750);
			else if (userLevel == 47) return (23750);
			else if (userLevel == 48) return (24750);
			else if (userLevel == 49) return (25750);
			else if (userLevel == 50) return (27000);
			else if (userLevel == 51) return (28250);
			else if (userLevel == 52) return (29500);
			else if (userLevel == 53) return (30750);
			else if (userLevel == 54) return (32000);
			else if (userLevel == 55) return (33250);
			else if (userLevel == 56) return (34500);
			else if (userLevel == 57) return (35750);
			else if (userLevel == 58) return (37000);
			else if (userLevel == 59) return (38250);
			else if (userLevel == 60) return (39750);
			else if (userLevel == 61) return (41250);
			else if (userLevel == 62) return (42750);
			else if (userLevel == 63) return (44250);
			else if (userLevel == 64) return (45750);
			else if (userLevel == 65) return (47250);
			else if (userLevel == 66) return (48750);
			else if (userLevel == 67) return (50250);
			else if (userLevel == 68) return (51750);
			else if (userLevel == 69) return (53250);
			else if (userLevel == 70) return (55000);
			else if (userLevel == 71) return (56750);
			else if (userLevel == 72) return (58500);
			else if (userLevel == 73) return (60250);
			else if (userLevel == 74) return (62000);
			else if (userLevel == 75) return (63750);
			else if (userLevel == 76) return (65500);
			else if (userLevel == 77) return (67250);
			else if (userLevel == 78) return (69000);
			else if (userLevel == 79) return (70750);
			else if (userLevel == 80) return (72750);
			else if (userLevel == 81) return (74750);
			else if (userLevel == 82) return (76750);
			else if (userLevel == 83) return (78750);
			else if (userLevel == 84) return (80750);
			else if (userLevel == 85) return (82750);
			else if (userLevel == 86) return (84750);
			else if (userLevel == 87) return (86750);
			else if (userLevel == 88) return (88750);
			else if (userLevel == 89) return (90750);
			else if (userLevel == 90) return (93000);
			else if (userLevel == 91) return (95250);
			else if (userLevel == 92) return (97500);
			else if (userLevel == 93) return (99750);
			else if (userLevel == 94) return (102000);
			else if (userLevel == 95) return (104250);
			else if (userLevel == 96) return (106500);
			else if (userLevel == 97) return (108750);
			else if (userLevel == 98) return (111000);
			else if (userLevel == 99) return (113250);
			else if (userLevel == 100) return 0;
			return 0;
        }
        
        // Converting XP to levels
                            
        int calcLevel(int curXP, int number) {
            if ((curXP + number) <= 50) return 0;
            else if ((curXP + number) <= 100) return 1;
            else if ((curXP + number) <= 200) return 2;
            else if ((curXP + number) <= 300) return 3;
            else if ((curXP + number) <= 400) return 4;
            else if ((curXP + number) <= 500) return 5;
            else if ((curXP + number) <= 600) return 6;
            else if ((curXP + number) <= 700) return 7;
            else if ((curXP + number) <= 800) return 8;
            else if ((curXP + number) <= 900) return 9;
            else if ((curXP + number) <= 1000) return 10;
            else if ((curXP + number) <= 1250) return 11;
            else if ((curXP + number) <= 1500) return 12;
            else if ((curXP + number) <= 1750) return 13;
            else if ((curXP + number) <= 2000) return 14;
            else if ((curXP + number) <= 2250) return 15;
            else if ((curXP + number) <= 2500) return 16;
            else if ((curXP + number) <= 2750) return 17;
            else if ((curXP + number) <= 3000) return 18;
            else if ((curXP + number) <= 3250) return 19;
            else if ((curXP + number) <= 3750) return 20;
            else if ((curXP + number) <= 4250) return 21;
            else if ((curXP + number) <= 4750) return 22;
            else if ((curXP + number) <= 5250) return 23;
            else if ((curXP + number) <= 5750) return 24;
            else if ((curXP + number) <= 6250) return 25;
            else if ((curXP + number) <= 6750) return 26;
            else if ((curXP + number) <= 7250) return 27;
            else if ((curXP + number) <= 7750) return 28;
            else if ((curXP + number) <= 8250) return 29;
            else if ((curXP + number) <= 9000) return 30;
            else if ((curXP + number) <= 9750) return 31;
            else if ((curXP + number) <= 10500) return 32;
            else if ((curXP + number) <= 11250) return 33;
            else if ((curXP + number) <= 12000) return 34;
            else if ((curXP + number) <= 12750) return 35;
            else if ((curXP + number) <= 13500) return 36;
            else if ((curXP + number) <= 14250) return 37;
            else if ((curXP + number) <= 15000) return 38;
            else if ((curXP + number) <= 15750) return 39;
            else if ((curXP + number) <= 16750) return 40;
            else if ((curXP + number) <= 17750) return 41;
            else if ((curXP + number) <= 18750) return 42;
            else if ((curXP + number) <= 19750) return 43;
            else if ((curXP + number) <= 20750) return 44;
            else if ((curXP + number) <= 21750) return 45;
            else if ((curXP + number) <= 22750) return 46;
            else if ((curXP + number) <= 23750) return 47;
            else if ((curXP + number) <= 24750) return 48;
            else if ((curXP + number) <= 25750) return 49;
            else if ((curXP + number) <= 27000) return 50;
            else if ((curXP + number) <= 28250) return 51;
            else if ((curXP + number) <= 29500) return 52;
            else if ((curXP + number) <= 30750) return 53;
            else if ((curXP + number) <= 32000) return 54;
            else if ((curXP + number) <= 33250) return 55;
            else if ((curXP + number) <= 34500) return 56;
            else if ((curXP + number) <= 35750) return 57;
            else if ((curXP + number) <= 37000) return 58;
            else if ((curXP + number) <= 38250) return 59;
            else if ((curXP + number) <= 39750) return 60;
            else if ((curXP + number) <= 41250) return 61;
            else if ((curXP + number) <= 42750) return 62;
            else if ((curXP + number) <= 44250) return 63;
            else if ((curXP + number) <= 45750) return 64;
            else if ((curXP + number) <= 47250) return 65;
            else if ((curXP + number) <= 48750) return 66;
            else if ((curXP + number) <= 50250) return 67;
            else if ((curXP + number) <= 51750) return 68;
            else if ((curXP + number) <= 53250) return 69;
            else if ((curXP + number) <= 55000) return 70;
            else if ((curXP + number) <= 56750) return 71;
            else if ((curXP + number) <= 58500) return 72;
            else if ((curXP + number) <= 60250) return 73;
            else if ((curXP + number) <= 62000) return 74;
            else if ((curXP + number) <= 63750) return 75;
            else if ((curXP + number) <= 65500) return 76;
            else if ((curXP + number) <= 67250) return 77;
            else if ((curXP + number) <= 69000) return 78;
            else if ((curXP + number) <= 70750) return 79;
            else if ((curXP + number) <= 72750) return 80;
            else if ((curXP + number) <= 74750) return 81;
            else if ((curXP + number) <= 76750) return 82;
            else if ((curXP + number) <= 78750) return 83;
            else if ((curXP + number) <= 80750) return 84;
            else if ((curXP + number) <= 82750) return 85;
            else if ((curXP + number) <= 84750) return 86;
            else if ((curXP + number) <= 86750) return 87;
            else if ((curXP + number) <= 88750) return 88;
            else if ((curXP + number) <= 90750) return 89;
            else if ((curXP + number) <= 93000) return 90;
            else if ((curXP + number) <= 95250) return 91;
            else if ((curXP + number) <= 97500) return 92;
            else if ((curXP + number) <= 99750) return 93;
            else if ((curXP + number) <= 102000) return 94;
            else if ((curXP + number) <= 104250) return 95;
            else if ((curXP + number) <= 106500) return 96;
            else if ((curXP + number) <= 108750) return 97;
            else if ((curXP + number) <= 111000) return 98;
            else if ((curXP + number) <= 113250) return 99;
            else if ((curXP + number) >= 113250) return 100;
            return 0;
        }
        
        public override void Use(Player p, string message, CommandData data) {
            p.lastCMD = "secret";
            
            string[] args = message.SplitSpaces();
            
	        if (args[0] == "secretcode") { // Add XP /xp secretcode [name] [xp]
            	if (args.Length < 3) { Help(p); return; }
            	if (PlayerInfo.FindMatchesPreferOnline(p, args[1]) == null) return;
				List<string[]> rows = Database.GetRows("Levels", "Name, XP, Level", "WHERE Name=@0", args[1]);
					
				int number = int.Parse(args[2]);
					
				if (rows.Count == 0) {
		            int curXP = 0;
		            int newLevel = calcLevel(curXP, number);
		                            
		            Player pl = PlayerInfo.FindExact(args[1]); // Find person receiving XP
		            int curLevel = 0;
		            if (pl != null && curLevel != newLevel) pl.Message("You are now level %b" + newLevel);
		            Database.AddRow("Levels", "Name, XP, Level", args[1], args[2], newLevel);
					return;
				} else {
		            int curXP = int.Parse(rows[0][1]); // First row, second column
		            int newLevel = calcLevel(curXP, number);
		                            
		            Player pl = PlayerInfo.FindExact(args[1]); // Find person receiving XP
		            int curLevel = GetInt(rows[0][2]);
		            if (pl != null && curLevel != newLevel) pl.Message("You are now level %b" + newLevel);
		                            
		            Database.UpdateRows("Levels", "XP=@1", "WHERE NAME=@0", args[1], curXP + number); // Give XP
		            Database.UpdateRows("Levels", "Level=@1", "WHERE NAME=@0", args[1], newLevel); // Give level
				}
	        }
            	
            else {
            	string pl = message.Length == 0 ? p.truename : args[0];
	            List<string[]> rows = Database.GetRows("Levels", "Name,XP,Level", "WHERE Name=@0", pl);
	
		        int userLevel = rows.Count == 0 ? 0 : int.Parse(rows[0][2]);  // User level
				int curXP = rows.Count == 0 ? 0 : int.Parse(rows[0][1]);  // User XP
		                
				if (message.Length == 0 || args[0] == p.name) {
					p.Message("%eYour Information:");
				} else {
					if (PlayerInfo.FindMatchesPreferOnline(p, args[0]) == null) return;
					p.Message("%b" + args[0] + "%e's Information:");
				}
					
				if (userLevel == 100) {
					p.Message("%5Level: %6" + userLevel + " (%bmax level)");
				}
					
				else {
		            p.Message("%5Level: %6" + userLevel + " (%b" + curXP + "xp/" + nextLevel(userLevel, curXP) + "xp%6)");
				}
        	}
        }
        
        public override void Help(Player p) {
        	p.Message("%T/XP - %HShows your level and current XP needed to level up.");
        	p.Message("%T/XP [player] - %HShows [player]'s level and current XP needed to level up.");
        	p.Message("%T/XP [secret code] [player] [xp] - %HGives [player] XP.");
        }
    }
}
