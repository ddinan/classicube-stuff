// PvP Plugin created by Venk and Sirvoid
 
using System;
using System.Collections.Generic;
using MCGalaxy;
using MCGalaxy.Commands;
using MCGalaxy.Events;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Drawing.Ops;
using MCGalaxy.Games;
using MCGalaxy.Maths;
using MCGalaxy.Network;
using BlockID = System.UInt16;

namespace MCGalaxy {
    public class PvP : Plugin_Simple {
		public override string name { get { return "PvP"; } }
		public override string MCGalaxy_Version { get { return "1.9.1.5"; } }
		public override string creator { get { return "Venk and Sirvoid"; } }
		
		/* Settings */
		public static string MaxHp = "20"; // Max players HP (10*2)
		bool economy = false; // Enable (true) or disable (false) rewards when killing someone
		int moneyStolen = 0; // If economy = true, money stolen when you kill someone
		string path = "./plugins/PvP/";
		
		public static int curpid = -1;
		public static List<string> maplist = new List<string>();
		public static string[,] players = new string[100, 3];
		public static string[,] weapons = new string[255, 3];
		
		public override void Load(bool startup) {
		  	// Load items
			loadMaps();
			loadWeapons();
		  
			OnPlayerClickEvent.Register(HandleClick, Priority.Low);
		    OnJoinedLevelEvent.Register(HandleOnJoinedLevel, Priority.Low);
		    
		  	Command.Register(new CmdPvP());
		  	Command.Register(new CmdSafeZone());
		  	Command.Register(new CmdWeapon());
			
			Player[] online = PlayerInfo.Online.Items;
			foreach (Player p in online) {
				for (int i = 0; i < 100; i++) {
					if (players[i,0] == null) {
						players[i,0] = p.name;
						players[i,1] = MaxHp;
						players[i,2] = "30000";
						break;
					}
				}
				
			  	p.SendCpeMessage(CpeMessageType.BottomRight2, "%4♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥");
			}
		}
                        
		public override void Unload(bool shutdown) {
            OnPlayerClickEvent.Unregister(HandleClick);
		    OnJoinedLevelEvent.Unregister(HandleOnJoinedLevel);

			Command.Unregister(Command.Find("PvP"));
			Command.Unregister(Command.Find("SafeZone"));
			Command.Unregister(Command.Find("Weapon"));
		}
        
		public static void SetHpIndicator(int i, Player pl) {
			int a = int.Parse(players[i,1]);

			string hpstring = "";
			for (int h = 0; h < a; h++){
				hpstring = hpstring + "♥";										
			}

			pl.SendCpeMessage(CpeMessageType.BottomRight2, "%4" + hpstring);
		}
		
		void loadMaps() {
			if (System.IO.File.Exists(path + "maps.txt")) {
				using (var maplistreader = new System.IO.StreamReader(path + "maps.txt")) {
					string line;
					while ((line = maplistreader.ReadLine()) != null) {
					   maplist.Add(line);
					}
				}
			}
		}
		
		bool hasWeapon(string world, Player p, string item) {
		    string filepath = path + "weapons/" + world + "/" + p.truename + ".txt";
			if (System.IO.File.Exists(filepath)) {
				using (var r = new System.IO.StreamReader(filepath)) {
					string line;
					while ((line = r.ReadLine()) != null) {
						if (line == item) return true;
					}
				}
			}
			return false;
		}
		
		string getWeaponStats(string item) {
			for (int i = 0; i < 255; i++) {
				if (weapons[i,0] == item) {
					return weapons[i,0] + " " + weapons[i,1] + " " + weapons[i,2];
				}
			}
			return "0 1 0";
		}
		
		void loadWeapons() {
			if (System.IO.File.Exists(path + "weapons.txt")) {
				using (var r = new System.IO.StreamReader(path + "weapons.txt")) {
					string line;
					while ((line = r.ReadLine()) != null) {
						string[] weaponstats = line.Split(';');
						for (int i = 0; i < 255; i++) {
							if (weapons[i,0] == null){
								weapons[i,0] = weaponstats[0];
								weapons[i,1] = weaponstats[1];
								weapons[i,2] = weaponstats[2];								
								break;
							}
						}
					}
				}
			}
		}
		
		bool inSafeZone(Player p, string map) {
			if (System.IO.File.Exists(path + "safezones" + map + ".txt")) {
				using (var r = new System.IO.StreamReader(path + "safezones" + map + ".txt")) {
					string line;
					while ((line = r.ReadLine()) != null) {
						string[] temp = line.Split(';');
						string[] coord1 = temp[0].Split(',');
						string[] coord2 = temp[1].Split(',');
						
						if ((p.Pos.BlockX <= int.Parse(coord1[0]) && p.Pos.BlockX >= int.Parse(coord2[0])) || (p.Pos.BlockX >= int.Parse(coord1[0]) && p.Pos.BlockX <= int.Parse(coord2[0]))) {
							if ((p.Pos.BlockZ <= int.Parse(coord1[2]) && p.Pos.BlockZ >= int.Parse(coord2[2])) || (p.Pos.BlockZ >= int.Parse(coord1[2]) && p.Pos.BlockZ <= int.Parse(coord2[2]))) {
								if ((p.Pos.BlockY <= int.Parse(coord1[1]) && p.Pos.BlockY >= int.Parse(coord2[1])) || (p.Pos.BlockY >= int.Parse(coord1[1]) && p.Pos.BlockY <= int.Parse(coord2[1]))) {
									return true;
								}
							}
						}
					} return false;
				}
			} return false;
		}
		
		void HandleClick(Player p, MouseButton button, MouseAction action, ushort yaw, ushort pitch, byte entity, ushort x, ushort y, ushort z, TargetBlockFace face) {
			if (button == MouseButton.Left) {
				if (maplist.Contains(p.level.name)) {
					curpid = -1;
					for (int yi = 0; yi < 100; yi++) {
						if (players[yi,0] == p.name) {
							curpid = yi;
						}
					}
				  
				  	int s = DateTime.Now.Second;
				  	int ms = DateTime.Now.Second;
					if (int.Parse(s + "" + ms) - int.Parse(players[curpid,2]) > 300 || int.Parse(s + "" + ms) - int.Parse(players[curpid,2]) < -300) {
						Player[] online = PlayerInfo.Online.Items;
						foreach (Player pl in online) {
							if (pl.EntityID == entity) {  
								for (int i = 0; i < 100; i++) {
									if (players[i,0] == pl.name) {
									  
										if (i < 100) {
											if (pl.invincible) return;
											if (!inSafeZone(p, p.level.name) && !inSafeZone(pl, pl.level.name)) { // If both not in a safezone
												if (p.Game.Referee) return;
												if (pl.Game.Referee) return;
												int a = int.Parse(players[i,1]);
														
												BlockID b = p.GetHeldBlock();
												string[] weaponstats = getWeaponStats((byte)b + "").Split(' ');
												// p.Message("dmg: " + weaponstats[1] + " id: " +  b.ExtID);
														
												if (hasWeapon(p.level.name, p, Block.GetName(p, b)) || weaponstats[0] == "0") {
													players[i,1] = (a - int.Parse(weaponstats[1])) + "";	
													p.Message("%cHit, they have {0} HP left", players[i, 1]);
													PushPlayer(p, pl);
													SetHpIndicator(i, pl);
															
													if (a <= 1) { // If player killed them
														string stringweaponused = weaponstats[0] == "0" ? "." : " %Susing " + Block.GetName(p, b) + ".";
														pl.level.Message(pl.ColoredName + " %Swas killed by " +  p.truename + stringweaponused);
														pl.Message("You were killed by " +  p.ColoredName + stringweaponused); 
														pl.HandleDeath(Block.Stone);
														pl.Game.Referee = true;
														
														players[i,1] = MaxHp;
														pl.SendPos(Entities.SelfID, new Position(16 + (p.level.spawnx*32),32+(p.level.spawny *32),16+(p.level.spawnz*32)), pl.Rot);
																
														pl.SendCpeMessage(CpeMessageType.BottomRight2, "%4♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥");
																
														if (economy == true && (p.ip != pl.ip || p.ip == "127.0.0.1")) {
															if (pl.money > moneyStolen - 1) {
																p.Message("You stole " + moneyStolen + " " + Server.Config.Currency + " %Sfrom " + pl.ColoredName + "%S.");
																		Player.Message(pl, p.ColoredName + " %Sstole " + moneyStolen + " " + Server.Config.Currency + " from you.");
																		p.SetMoney(p.money+moneyStolen);
																		pl.SetMoney(pl.money-moneyStolen);
																		
																		MCGalaxy.Games.BountyData bounty = ZSGame.Instance.FindBounty(pl.name);
																		if (bounty != null){
																			ZSGame.Instance.Bounties.Remove(bounty);
																			
																			Player setter = PlayerInfo.FindExact(bounty.Origin);
																			
																			if (setter == null) {
																				p.Message("Cannot collect the bounty, as the player who set it is offline.");
																			} else {
																				p.level.Message("&c" + p.DisplayName + " %Scollected the bounty of &a" +
																							    bounty.Amount + " %S" + Server.Config.Currency + " on " + pl.ColoredName + "%S.");
																				p.SetMoney(p.money + bounty.Amount);
																			}
																		}
																	}
																}
															}
														} else { p.Message("You don't own this weapon."); } 
													} else { p.Message("You can't hurt people in a safe zone."); }
												}
												players[curpid,2] =  DateTime.Now.Second + "" + DateTime.Now.Millisecond + "";
											}
										
										}
									}
								}
					}
				}
			}
		}
		
		static bool ClickOnPlayer(Player p, byte entity, MouseButton button) {
		    Player[] players = PlayerInfo.Online.Items;
			for (int i = 0; i < players.Length; i++) {
		        if (players[i].EntityID != entity) continue;
				Player pl = players[i];
				Vec3F32 delta = p.Pos.ToVec3F32() - pl.Pos.ToVec3F32();
				float reachSq = p.ReachDistance * p.ReachDistance;
				// Don't allow clicking on players further away than their reach distance
				if (delta.LengthSquared > (reachSq + 1)) return false;
				
				//if (!p.Game.Referee) continue;
				//if (!pl.Game.Referee) continue;
								
				if (button == MouseButton.Left) {
                    PushPlayer(p, pl);
				}
				return true;
		    }
			return false;
		}
		
		static void PushPlayer(Player p, Player pl) {
            int srcHeight = ModelInfo.CalcEyeHeight(p.Model);
            int dstHeight = ModelInfo.CalcEyeHeight(pl.Model);
            int dx = p.Pos.X - pl.Pos.X, dy = (p.Pos.Y + srcHeight) - (pl.Pos.Y + dstHeight), dz = p.Pos.Z - pl.Pos.Z;
            Vec3F32 dir = new Vec3F32(dx, dy, dz);
            dir = Vec3F32.Normalise(dir);
			
			float mult = 1 / ModelInfo.GetRawScale(pl.Model);
			float plScale = ModelInfo.GetRawScale(pl.Model);
			
			if (pl.Supports(CpeExt.VelocityControl) && p.Supports(CpeExt.VelocityControl)) {
				// Intensity of force is in part determined by model scale
                pl.Send(Packet.VelocityControl(-dir.X*mult, 1.233f*mult, -dir.Z*mult, 0, 1, 0));
            } else {
				p.Message("You can left and right click people to push or pull them if you update to dev build with launcher options!");
			}
		}
		
		void HandleOnJoinedLevel(Player p, Level prevLevel, Level level, ref bool announce){
			if (maplist.Contains(level.name)) {
				p.SendCpeMessage(CpeMessageType.BottomRight2, "%4♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥");
			  
				for (int i = 0; i < 100; i++){
					if (players[i,0] == p.name) return;
				}
			
				for (int i = 0; i < 100; i++){
					if (players[i,0] == null){
						players[i,0] = p.name;
						players[i,1] = MaxHp;
						players[i,2] = "30000";
						return;
					}
				}
			}
			
			if (prevLevel == null) return;
			if (maplist.Contains(prevLevel.name) && !maplist.Contains(level.name)) {
				p.SendCpeMessage(CpeMessageType.BottomRight2, "");
			}
		}
	}
  
	public sealed class CmdPvP : Command2 {
		string path = "./plugins/PvP/";
		
        public override string name { get { return "PvP"; } }
        public override string type { get { return CommandTypes.Games; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public override bool SuperUseable { get { return false; } }
        public override CommandPerm[] ExtraPerms {
            get { return new[] { new CommandPerm(LevelPermission.Admin, "can manage PvP") }; }
        }
        
        public override void Use(Player p, string message, CommandData data) {
		  	if (message.Length == 0) { Help(p); return; }
            string[] args = message.SplitSpaces(2);
            
            switch (args[0].ToLower()) {
                case "add": HandleAdd(p, args, data); return;
                case "del": HandleDelete(p, args, data); return;
            }
		}
        
        void HandleAdd(Player p, string[] args, CommandData data) {
        	if (args.Length == 1) { p.Message("You need to specify a map to add."); return; }
            if (!HasExtraPerm(p, data.Rank, 1)) return;
            string pvpMap = args[1];
            
            PvP.maplist.Add(pvpMap);
            p.Message("The map %b" + pvpMap + " %Shas been added to the PvP map list.");
			
            // Add the map to the map list
			using (System.IO.StreamWriter maplistwriter = 
            new System.IO.StreamWriter(path + "maps.txt")) {
				foreach (String s in PvP.maplist){
				   maplistwriter.WriteLine(s);
				}
			}
						
			Player[] online = PlayerInfo.Online.Items;
			foreach (Player pl in online) {
				if (pl.level.name == args[1]) {
					for (int i = 0; i < 100; i++) {
						if (PvP.players[i,0] == null) {
							PvP.players[i,0] = pl.name;
							PvP.players[i,1] = PvP.MaxHp;
							PvP.players[i,2] = "30000";
							break;
						}
					}
					
					pl.SendCpeMessage(CpeMessageType.BottomRight2, "%4♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥");
				}
            }
        }
        
        void HandleDelete(Player p, string[] args, CommandData data) {
        	if (args.Length == 1) { p.Message("You need to specify a map to remove."); return; }
            if (!HasExtraPerm(p, data.Rank, 1)) return;
            string pvpMap = args[1];
            
            PvP.maplist.Remove(pvpMap);
            p.Message("The map %b" + pvpMap + " %Shas been removed from the PvP map list.");
        }
        
        public override void Help(Player p) {
            p.Message("%T/PvP add <map> %H- Adds a map to the PvP map list.");
            p.Message("%T/PvP del <map> %H- Deletes a map from the PvP map list.");
        }
    }
  
    public sealed class CmdSafeZone : Command2 {
		string path = "./plugins/PvP/";
		
        public override string name { get { return "SafeZone"; } }
        public override string type { get { return CommandTypes.Games; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public override bool SuperUseable { get { return false; } }
        public override CommandPerm[] ExtraPerms {
            get { return new[] { new CommandPerm(LevelPermission.Admin, "can manage safe zones") }; }
        }

        public override void Use(Player p, string message, CommandData data) {
		  if (message.Length == 0) { Help(p); return; }
            string[] args = message.SplitSpaces(2);
            
            switch (args[0].ToLower()) {
                case "add": HandleAdd(p, args, data); return;
            }
		}
        
        void HandleAdd(Player p, string[] args, CommandData data) {
        	p.Message("Place or break two blocks to determine the edges.");
			p.MakeSelection(2, null, addSafeZone);
        }
        
        bool addSafeZone(Player p, Vec3S32[] marks, object state, BlockID block) {
			System.IO.FileInfo filedir = new System.IO.FileInfo(path + "safezones" + p.level.name + ".txt");
			filedir.Directory.Create();
		
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(path + "safezones" + p.level.name + ".txt", true)) {
				file.WriteLine(marks.GetValue(0) + ";" + marks.GetValue(1));
			}
			
			p.Message("Successfully added a safezone.");
			return true;
		}
        
        public override void Help(Player p) {
            p.Message("%T/SafeZone [add] %H- Adds a safe zone to the current PvP map.");
            //p.Message("%T/SafeZone [del] %H- Removes a safe zone from the current PvP map.");
        }
	}
	
    public sealed class CmdWeapon : Command2 {
		string path = "./plugins/PvP/";
		
        public override string name { get { return "Weapon"; } }
        public override string type { get { return CommandTypes.Games; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public override bool SuperUseable { get { return false; } }
        public override CommandPerm[] ExtraPerms {
            get { return new[] { new CommandPerm(LevelPermission.Admin, "can manage weapons") }; }
        }

        public override void Use(Player p, string message, CommandData data) {
		  if (message.Length == 0) { Help(p); return; }
            string[] args = message.SplitSpaces(4);
            
            switch (args[0].ToLower()) {
                case "add": HandleAdd(p, args); return;
                case "give": HandleGive(p, args); return;
                case "take": HandleTake(p, args); return;
            }
		}
        
        void HandleAdd(Player p, string[] args) {
        	if (args.Length == 1) { p.Message("You need to specify an ID for the block. E.g, '1' for stone."); return; }
        	if (args.Length == 2) { p.Message("You need to specify the damage that the weapon does. E.g, '1' for half a heart."); return; }
        	if (args.Length == 3) { p.Message("You need to specify how many clicks before it breaks. 0 for infinite clicks."); return; }
			
        	for (int i = 0; i < 255; i++) {
				if (PvP.weapons[i,0] == null) {
					PvP.weapons[i,0] = args[1];
					PvP.weapons[i,1] = args[2];
					PvP.weapons[i,2] = args[3];
					createWeapon(args[1], args[2], args[3]);
					break;
				}
        	}
        }
        
        void createWeapon(string id, string damage, string durability) {
			System.IO.FileInfo filedir = new System.IO.FileInfo(path + "weapons.txt");
			filedir.Directory.Create();
		
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(path + "weapons.txt", true)) {
				file.WriteLine(id + ";" + damage + ";" + durability);
			}
		}
        	
        void HandleGive(Player p, string[] args) {
        	if (args.Length == 1) { p.Message("You need to specify a username to give the weapon to."); return; }
        	if (args.Length == 2) { p.Message("You need to specify the world to allow them to use the weapon on."); return; }
        	if (args.Length == 3) { p.Message("You need to specify the name of the weapon."); return; }
        	
        	string filepath = path + "weapons/" + args[2] + "/" + args[1] + ".txt";
			System.IO.FileInfo filedir = new System.IO.FileInfo(filepath);
			filedir.Directory.Create();
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(filepath, true)) {
				file.WriteLine(args[3]);
			}
		}
        	
        void HandleTake(Player p, string[] args) {
        	Player[] online = PlayerInfo.Online.Items;
			foreach (Player pl in online) {
				if (pl.truename == args[1]) {
					string filepath = path + "weapons/" + args[2] + "/" + args[1] + ".txt";
					
					if (System.IO.File.Exists(filepath)) {
						System.IO.File.WriteAllText(filepath, string.Empty);
					}
				}
        	}
		}	
        
        public override void Help(Player p) {
            p.Message("%T/Weapon add [id] [damage] [durability] %H- Adds a weapon to the current PvP map.");
            p.Message("%T/Weapon del %H- Removes a weapon current PvP map.");
            p.Message("%T/Weapon give [player] [world] [weapon] %H- Gives a player a weapon.");
            p.Message("%T/Weapon take [player] [world] %H- Takes all weapons from a player away.");
        }
	}
}
