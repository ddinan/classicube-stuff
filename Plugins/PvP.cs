// PvP Plugin created by Sirvoid, modified and maintained by VenkSociety.

using System;
using System.Collections.Generic;
using MCGalaxy;
using MCGalaxy.Events;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Drawing.Ops;
using MCGalaxy.Games;
using MCGalaxy.Maths;
using BlockID = System.UInt16;

namespace MCGalaxy {
	public class PvP : Plugin_Simple {
		public override string name { get { return "PvP"; } }
		public override string MCGalaxy_Version { get { return "1.9.1.2"; } }
		public override string creator { get { return "Sirvoid"; } }
		
		/* Settings */
		string MaxHp = "20"; // Max players hp
		string PvPaddcmd = "pvpadd"; // Command to allow PvP on a map
		string PvPdelcmd = "pvpdel"; // Command to disallow PvP on a map
		string PvPsuicmd = "suicide"; // Command to kill yourself
		string PvPsethpcmd = "sethp"; // Command to set player's hp
		string PvPaddsafezone = "addsafezone"; // Command to add a safezone
		string PvPaddweapon = "addweapon"; // Command to add a weapon
		string PvPaddweaponplayer = "giveweapon"; // Command to give a weapon to a player
		string PvPresetweaponplayer = "resetweapon"; // Command to player's weapons
		LevelPermission PvPcmdRank = LevelPermission.Operator; // Min rank allowed for the commands (except suicide)
		bool PvPEconomy = true; // Enable (true) or Disable (false) rewards when killing someone
		int moneyStolen = 1; // Money stolen when you kill someone
		
		int curpid = -1;
		List<string> maplist = new List<string>();
		string[,] playersinpvp = new string[100, 3];
		string[,] weapons = new string[255, 3];
		
		public override void Load(bool startup){
			loadfromfile();
			loadWeapons();
			OnPlayerCommandEvent.Register(HandleCommand, Priority.Low);
            OnPlayerClickEvent.Register(HandleClick, Priority.Low);
		    OnJoinedLevelEvent.Register(HandleOnJoinedLevel, Priority.Low);
			
			Player[] online = PlayerInfo.Online.Items;
				foreach (Player p in online){
					if (maplist.Contains(p.level.name)){
						for(int i = 0;i<100;i++){
							if(playersinpvp[i,0] == null){
								playersinpvp[i,0] = p.name;
								playersinpvp[i,1] = MaxHp;
								playersinpvp[i,2] = "60000";
								break;
							}
						}
						p.SendCpeMessage(CpeMessageType.BottomRight2, "%4♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥");
					}
				}
		}
                        
		public override void Unload(bool shutdown){
			OnPlayerCommandEvent.Unregister(HandleCommand);
            OnPlayerClickEvent.Unregister(HandleClick);
		    OnJoinedLevelEvent.Unregister(HandleOnJoinedLevel);
		}
        
		void SetHpIndicator(int i,Player pl){
			int a = int.Parse(playersinpvp[i,1]);

			string hpstring = "";
			for (int h = 0;h < a;h++){
				hpstring = hpstring + "♥";										
			}

			pl.SendCpeMessage(CpeMessageType.BottomRight2, "%4" + hpstring);
		}
		
		void savetofile(){
			using (System.IO.StreamWriter maplistwriter = 
            new System.IO.StreamWriter("./Plugins/PvP/maps.txt")){

				foreach (String s in maplist){
				   maplistwriter.WriteLine(s);
				}
			
			}
		}
		
		void loadfromfile(){
			
			if(System.IO.File.Exists("./Plugins/PvP/maps.txt")){
				using (var maplistreader = new System.IO.StreamReader("./Plugins/PvP/maps.txt"))
				{
					string line;
					while ((line = maplistreader.ReadLine()) != null)
					{
					   maplist.Add(line);
					}
				}
			}
		}
		
		
		void HandleCommand(Player p, string cmd, string args, CommandData data) {
			string[] multArgs = args.Split(' ');
			//PvP add command
			if (cmd == PvPaddcmd){
				p.cancelcommand = true;
				if (p.Rank >= PvPcmdRank){
					if(args != ""){
						maplist.Add(args);
						Player.Message(p, "The map %b" + args + " %Shas been added to the PvP map list.");
						
						savetofile();
						
						Player[] online = PlayerInfo.Online.Items;
						foreach (Player pl in online){
							if (pl.level.name == args){
								for(int i = 0;i<100;i++){
									if(playersinpvp[i,0] == null){
										playersinpvp[i,0] = pl.name;
										playersinpvp[i,1] = MaxHp;
										playersinpvp[i,2] = "60000";
										break;
									}
								}
								pl.SendCpeMessage(CpeMessageType.BottomRight2, "%4♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥");
							}
						}
						
					}else{
						Player.Message(p, "Correct usage: /" + PvPaddcmd + " [map]");
					}
				}
				else{
					Player.Message(p, "You are not allowed to use this command !");
				}
			//PvP del command
			} else if (cmd == PvPdelcmd){
				p.cancelcommand = true;		
				if (p.Rank >= PvPcmdRank){
					if(args != ""){
						maplist.Remove(args);
						Player.Message(p, "The map " + args + " has been removed to the PvP map list.");
					}else{
						Player.Message(p, "Correct usage: /" + PvPdelcmd + " [map]");
					}
				}
				else{
					Player.Message(p, "You are not allowed to use this command !");
				}
			//PvP set hp command
			} else if (cmd == PvPsethpcmd){
				p.cancelcommand = true;	
				if (p.Rank >= PvPcmdRank){
					if(args != ""){
						curpid = -1;
						for (int yi = 0;yi<100;yi++){
								if(playersinpvp[yi,0] == multArgs[0] + "+"){
									curpid = yi;
									
								}
						}
						
						playersinpvp[curpid,1] = multArgs[1];		
						Player[] online = PlayerInfo.Online.Items;
						
						foreach (Player pl in online){
							if(pl.truename == multArgs[0]){
								SetHpIndicator(curpid,pl);
							}
						}
						
					}else{
						Player.Message(p, "Correct usage: /" + PvPsethpcmd + " [Player] [HP]");
					}
				}else{
					Player.Message(p, "You are not allowed to use this command !");
				}
			//Suicide command
			} else if (cmd == PvPsuicmd){
				if (maplist.Contains(p.level.name)){
					p.cancelcommand = true;
					curpid = -1;
					for (int yi = 0;yi<100;yi++){
						if(playersinpvp[yi,0] == p.name){
							curpid = yi;
							
						}
					}
					playersinpvp[curpid,1] = MaxHp;
					p.SendPos(Entities.SelfID, new Position(16+(p.level.spawnx*32),32+(p.level.spawny*32),16+(p.level.spawnz*32)), p.Rot);				
					SetHpIndicator(curpid,p);
					p.level.Message(p.ColoredName + " %Scommitted suicide."); 
				}
			//add safezone command
			} else if (cmd == PvPaddsafezone){
			
					p.cancelcommand = true;
					if (p.Rank >= PvPcmdRank){
						Player.Message(p, "Place or break two blocks to determine the edges.");
						p.MakeSelection(2, null, DoSafeZone);
					}else{
						Player.Message(p, "You are not allowed to use this command !");
					}
			//add weapon command
			}  else if (cmd == PvPaddweapon){
					
					
					p.cancelcommand = true;
					if (p.Rank >= PvPcmdRank){
						if(args != ""){
							for(int i = 0;i<255;i++){
								if(weapons[i,0] == null){
									weapons[i,0] = multArgs[0];
									weapons[i,1] = multArgs[1];
									weapons[i,2] = multArgs[2];
									AddWeaponToFile(multArgs[0],multArgs[1],multArgs[2]);
									break;
								}
							}
						} else {
							Player.Message(p, "Correct usage: /" + PvPaddweapon + " [BlockId] [Damage] [Durability]");
						}
					}else{
						Player.Message(p, "You are not allowed to use this command !");
					}
					
			} else if (cmd == PvPaddweaponplayer) {
				p.cancelcommand = true;
				if (p.Rank >= PvPcmdRank){ 
					if(args != ""){
						Player[] online = PlayerInfo.Online.Items;
						foreach (Player pl in online){
							if(pl.truename == multArgs[0]){
								AddWeaponToPlayer(multArgs[1],pl,multArgs[2]);
							}
						}
						
					} else {
						Player.Message(p, "Correct usage: /" + PvPaddweaponplayer + " [Player] [World] [WeaponName]");
					}
				}else{
					Player.Message(p, "You are not allowed to use this command !");
				}
			} else if (cmd == PvPresetweaponplayer) {
				p.cancelcommand = true;
				if (p.Rank >= PvPcmdRank){ 
					if(args != ""){
						Player[] online = PlayerInfo.Online.Items;
						foreach (Player pl in online){
							if(pl.truename == multArgs[0]){
								ResetPlayerWeapon(multArgs[1],pl);
							}
						}
						
					} else {
						Player.Message(p, "Correct usage: /" + PvPresetweaponplayer + " [Player] [World]");
					}
				}else{
					Player.Message(p, "You are not allowed to use this command !");
				}
			//////Secret Commands///////
			} else if (cmd == "fxdrxxrrx") {
                p.cancelcommand = true;
                ResetPlayerWeapon(p.level.name,p);
            } else if (cmd == "abuhshiaj") {
                p.cancelcommand = true;
                    if(args != ""){
                            AddWeaponToPlayer(p.level.name,p,args);
                    } else {
                        Player.Message(p, "Correct usage: /" + PvPaddweaponplayer + "[WeaponName]");
                    }
            }
			////////////////////////////
			else {return;}
        }
		
		void AddWeaponToPlayer(string world,Player p,string item){
			string filepath = "./Plugins/PvP/Weapons/" + world + "/" + p.truename + ".txt";
			System.IO.FileInfo filedir = new System.IO.FileInfo(filepath);
			filedir.Directory.Create();
			using (System.IO.StreamWriter file = 
				new System.IO.StreamWriter(filepath, true))
			{
				file.WriteLine(item);
			}
		}
		
		void ResetPlayerWeapon(string world,Player p){
			string filepath = "./Plugins/PvP/Weapons/" + world + "/" + p.truename + ".txt";
			if(System.IO.File.Exists(filepath)){
				System.IO.File.WriteAllText(filepath,string.Empty);
			}
		}
		
		bool CheckPlayerWeapon(string world,Player p,string item){
		    string filepath = "./Plugins/PvP/Weapons/" + world + "/" + p.truename + ".txt";
			if(System.IO.File.Exists(filepath)){
				using (var r = new System.IO.StreamReader(filepath)){
					string line;
					while ((line = r.ReadLine()) != null){
						if(line == item){return true;}
					}
				}
			}
			return false;
		}
		
		string GetWeaponStats(string item){
			for(int i = 0;i<255;i++){
				if(weapons[i,0] == item){
					return weapons[i,0] + " " + weapons[i,1] + " " + weapons[i,2];
				}
			}
			return "0 1 0";
		}
		
		void AddWeaponToFile(string id,string damage,string durability) {
		
			System.IO.FileInfo filedir = new System.IO.FileInfo("./Plugins/PvP/weapons.txt");
			filedir.Directory.Create();
		
			using (System.IO.StreamWriter file = 
				new System.IO.StreamWriter("./Plugins/PvP/weapons.txt", true))
			{
				
				file.WriteLine(id + ";" + damage + ";" + durability);
			}
		}
		
		void loadWeapons(){
		
		if(System.IO.File.Exists("./Plugins/PvP/weapons.txt")){
				using (var r = new System.IO.StreamReader("./Plugins/PvP/weapons.txt")){
					string line;
					while ((line = r.ReadLine()) != null){
						string[] weaponstats = line.Split(';');
						for(int i = 0;i<255;i++){
							if(weapons[i,0] == null){
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
		
		bool DoSafeZone(Player p, Vec3S32[] marks, object state, BlockID block) {
		
			System.IO.FileInfo filedir = new System.IO.FileInfo("./Plugins/PvP/Safezones/" + p.level.name + ".txt");
			filedir.Directory.Create();
		
			using (System.IO.StreamWriter file = 
				new System.IO.StreamWriter("./Plugins/PvP/Safezones/" + p.level.name + ".txt", true))
			{
				
				file.WriteLine(marks.GetValue(0) + ";" + marks.GetValue(1));
			}
			
			Player.Message(p, "SafeZone added.");
			return true;
		}
		
		bool InSafeZone(Player p,string map){
			
			if(System.IO.File.Exists("./Plugins/PvP/Safezones/" + map + ".txt")){
				using (var r = new System.IO.StreamReader("./Plugins/PvP/Safezones/" + map + ".txt")){
					string line;
					while ((line = r.ReadLine()) != null){
						string[] temp = line.Split(';');
						string[] coord1 = temp[0].Split(',');
						string[] coord2 = temp[1].Split(',');
						
						if((p.Pos.BlockX <= int.Parse(coord1[0]) && p.Pos.BlockX >= int.Parse(coord2[0])) || (p.Pos.BlockX >= int.Parse(coord1[0]) && p.Pos.BlockX <= int.Parse(coord2[0]))){
							if((p.Pos.BlockZ <= int.Parse(coord1[2]) && p.Pos.BlockZ >= int.Parse(coord2[2])) || (p.Pos.BlockZ >= int.Parse(coord1[2]) && p.Pos.BlockZ <= int.Parse(coord2[2]))){
								if((p.Pos.BlockY <= int.Parse(coord1[1]) && p.Pos.BlockY >= int.Parse(coord2[1])) || (p.Pos.BlockY >= int.Parse(coord1[1]) && p.Pos.BlockY <= int.Parse(coord2[1]))){
								return true;
								}
							}
						
						}
					}
				return false;
				}
			return false;
			}
		return false;
		}
		
		
		void HandleClick(Player p, MouseButton button, MouseAction action, ushort yaw, ushort pitch, byte entity, ushort x, ushort y, ushort z, TargetBlockFace face){
			if (button == MouseButton.Left){
				if (maplist.Contains(p.level.name)){
				curpid = -1;
					for(int yi = 0;yi<100;yi++){
						if(playersinpvp[yi,0] == p.name){
							curpid = yi;
						}
					}
					if(int.Parse(DateTime.Now.Second + "" + DateTime.Now.Millisecond) - int.Parse(playersinpvp[curpid,2])>300 || int.Parse(DateTime.Now.Second+""+DateTime.Now.Millisecond) - int.Parse(playersinpvp[curpid,2])<-300){
						Player[] online = PlayerInfo.Online.Items;
								foreach (Player pl in online){
									if (pl.EntityID == entity){  
										for(int i = 0;i<100;i++){
											if(playersinpvp[i,0] == pl.name){
												double dist = (Math.Sqrt(Math.Pow(Math.Abs(p.Pos.X - pl.Pos.X), 2) + Math.Pow(Math.Abs(p.Pos.Y - pl.Pos.Y), 2) + Math.Pow(Math.Abs(p.Pos.Z - pl.Pos.Z), 2)));
												if (dist < 150){
													
													if(!InSafeZone(p,p.level.name) && !InSafeZone(pl,pl.level.name)){
														
														int a = int.Parse(playersinpvp[i,1]);
														
														BlockID b = p.GetHeldBlock();
														string[] weaponstats = GetWeaponStats((byte)b + "").Split(' ');
														//Player.Message(p, "dmg: " + weaponstats[1] + " id: " +  b.ExtID);
														
														if(CheckPlayerWeapon(p.level.name,p,Block.GetName(p, b)) || weaponstats[0] == "0"){
															playersinpvp[i,1] = (a - int.Parse(weaponstats[1])) + "";
															
															Position new_pos;
															new_pos.X = pl.Pos.X +(int)((pl.Pos.X-p.Pos.X)/2);
															new_pos.Y = pl.Pos.Y;
															new_pos.Z = pl.Pos.Z +(int)((pl.Pos.Z-p.Pos.Z)/2);
															
															Position new_midpos;
															new_midpos.X = pl.Pos.X +(int)((pl.Pos.X-p.Pos.X)/4);
															new_midpos.Y = pl.Pos.Y;
															new_midpos.Z = pl.Pos.Z +(int)((pl.Pos.Z-p.Pos.Z)/4);
															
															if (pl.level.IsAirAt((ushort)new_pos.BlockX, (ushort)new_pos.BlockY, (ushort)new_pos.BlockZ) && pl.level.IsAirAt((ushort)new_pos.BlockX, (ushort)(new_pos.BlockY-1), (ushort)new_pos.BlockZ) && 
															pl.level.IsAirAt((ushort)new_midpos.BlockX, (ushort)new_midpos.BlockY, (ushort)new_midpos.BlockZ) && pl.level.IsAirAt((ushort)new_midpos.BlockX, (ushort)(new_midpos.BlockY-1), (ushort)new_midpos.BlockZ)){
																pl.SendPos(Entities.SelfID, new Position(new_pos.X, new_pos.Y, new_pos.Z), pl.Rot);
															}
															
															SetHpIndicator(i,pl);
															
															if (a <= 1){
															
																string stringweaponused = weaponstats[0] == "0" ? "." : " %Susing " + Block.GetName(p, b) + ".";
																pl.level.Message(pl.ColoredName + " %Swas slain by " +  p.ColoredName + stringweaponused); 
																
																playersinpvp[i,1] = MaxHp;
																pl.SendPos(Entities.SelfID, new Position(16+(p.level.spawnx*32),32+(p.level.spawny*32),16+(p.level.spawnz*32)), pl.Rot);
																
																pl.SendCpeMessage(CpeMessageType.BottomRight2, "%4♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥");
																
																if(PvPEconomy == true && (p.ip != pl.ip || p.ip == "127.0.0.1")){
																	if(pl.money > moneyStolen - 1){
																		Player.Message(p, "You stole " + moneyStolen + " " + Server.Config.Currency + " %Sfrom " + pl.ColoredName + "%S.");
																		Player.Message(pl, p.ColoredName + " %Sstole " + moneyStolen + " " + Server.Config.Currency + " from you.");
																		p.SetMoney(p.money+moneyStolen);
																		pl.SetMoney(pl.money-moneyStolen);
																		
																		MCGalaxy.Games.BountyData bounty = ZSGame.Instance.FindBounty(pl.name);
																		if (bounty != null){
																			ZSGame.Instance.Bounties.Remove(bounty);
																			
																			Player setter = PlayerInfo.FindExact(bounty.Origin);
																			
																			if (setter == null) {
																				Player.Message(p, "Cannot collect the bounty, as the player who set it is offline.");
																			} else {
																				p.level.Message("&c" + p.DisplayName + " %Scollected the bounty of &a" +
																							    bounty.Amount + " %S" + Server.Config.Currency + " on " + pl.ColoredName + "%S.");
																				p.SetMoney(p.money + bounty.Amount);
																			}
																		}
																	}
																}
															}
														} else {Player.Message(p, "You don't own this weapon.");} 
													}
													else{
														Player.Message(p, "You can't hurt people in a safe zone.");
													}
												}
												playersinpvp[curpid,2] =  DateTime.Now.Second + "" + DateTime.Now.Millisecond + "";
											}
										
										}
									}
								}
					}
				}
			}
		}
		

		
		void HandleOnJoinedLevel(Player p, Level prevLevel, Level level, ref bool announce){
			
			
			
			if (maplist.Contains(level.name)){
				
				p.SendCpeMessage(CpeMessageType.BottomRight2, "%4♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥");
				
				for(int i =0;i<100;i++){
					if(playersinpvp[i,0] == p.name) return;
				}
			
				for(int i = 0;i<100;i++){
				
					if(playersinpvp[i,0] == null){
						playersinpvp[i,0] = p.name;
						playersinpvp[i,1] = MaxHp;
						playersinpvp[i,2] = "60000";
						return;
					}
				
				}
				
			}
			
			if (prevLevel == null) return;
			if (maplist.Contains(prevLevel.name) && !maplist.Contains(level.name)){
			
				p.SendCpeMessage(CpeMessageType.BottomRight2, "");
			}
		}
		
		public override void Help(Player p) {}
	}
}
