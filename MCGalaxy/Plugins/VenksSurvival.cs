/* 
  PvP Plugin created by Venk and Sirvoid.
  
  PLEASE NOTE:

  This plugin requires my VenkLib plugin. Install it from here: https://github.com/derekdinan/ClassiCube-Stuff/blob/master/MCGalaxy/Plugins/VenkLib.cs.

  1. PING MAY AFFECT PVP.
  2. FALL DAMAGE IS A HUGE WIP, USE AT OWN RISK.
  3. THE CODE (╫) IS USED FOR THE HALF-HEART EMOJI, YOU MAY NEED TO CHANGE THIS.
  
  TO SET UP PVP:
  1. Put this file into your plugins folder.
  2. Type /pcompile PvP.
  3. Type /pload PvP.
  4. Type /pvp add [name of map].
  
  IF YOU WANT WEAPONS (DO THIS FOR EACH WEAPON):
  1. Type /weapon add [id] [damage] [durability].
  
  IF YOU WANT TOOLS (DO THIS FOR EACH TOOL):
  1. Type /tool add [id] [speed] [durability] [type].
  
  IF YOU WANT MINEABLE BLOCKS (DO THIS FOR EACH BLOCK):
  1. Type /block add [id] [type] [durability].
  2. Add "mining=true" to the map's MOTD. (/map motd mining=true)
  
  IF YOU WANT POTIONS:
  1. Type /potion [secret code] [potion type] [amount].
  2. To use the potion, type /potion [potion type]
  
  IF YOU WANT SPRINTING:
  1. Type /map motd -speed maxspeed=1.47.
  2. To sprint, hold shift while running.
  
  IF YOU WANT TO SHOW THE BLOCK YOU'RE HOLDING TO OTHER PLAYERS:
  1. Download my HoldBlocks plugin: https://github.com/derekdinan/ClassiCube-Stuff/blob/master/MCGalaxy/Plugins/HoldBlocks.cs.

  IF YOU WANT MOB INSTRUCTIONS:
  1. Download my HoldBlocks plugin: https://github.com/derekdinan/ClassiCube-Stuff/blob/master/MCGalaxy/Plugins/MobAI.cs.
  
  TODO:
  1. Can still hit twice occasionally... let's disguise that as a critical hit for now?
  2. Hunger?
  
 */

using System;
using System.Collections.Generic;
using System.IO;

using MCGalaxy;
using MCGalaxy.Blocks;
using MCGalaxy.Bots;
using MCGalaxy.Commands;
using MCGalaxy.Config;
using MCGalaxy.Events;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Drawing.Ops;
using MCGalaxy.Games;
using MCGalaxy.Maths;
using MCGalaxy.Network;
using MCGalaxy.Scripting;
using MCGalaxy.SQL;
using MCGalaxy.Tasks;
using BlockID = System.UInt16;

namespace MCGalaxy
{
    public class PvP : Plugin
    {
        public override string name { get { return "&aVenk's Survival%S"; } } // To unload /punload Survival
        public override string MCGalaxy_Version { get { return "1.9.3.8"; } }
        public override string creator { get { return "Venk and Sirvoid"; } }

        public class Config
        {
            [ConfigBool("gamemode-only", "Survival", true)]
            public static bool GamemodeOnly = true;

            [ConfigBool("survival-death", "Survival", true)]
            public static bool SurvivalDeath = true;

            [ConfigBool("regeneration", "Survival", true)]
            public static bool Regeneration = true;

            [ConfigBool("drowning", "Survival", true)]
            public static bool Drowning = true;

            [ConfigBool("mining", "Survival", true)]
            public static bool Mining = true;

            [ConfigBool("economy", "Survival", true)]
            public static bool Economy = true;

            [ConfigInt("bounty", "Survival", 1)]
            public static int Bounty = 1;

            [ConfigBool("mobs", "Survival", false)]
            public static bool Mobs = false;

            [ConfigBool("use-goodly-effects", "Extra", false)]
            public static bool UseGoodlyEffects = false;

            [ConfigString("max-health", "Survival", "20")]
            public static string MaxHealth = "20";

            [ConfigString("path", "Extra", "./plugins/VenksSurvival/")]
            public static string Path = "./plugins/VenksSurvival/";

            [ConfigString("secret-code", "Extra", "unused")]
            public static string SecretCode = "unused";


            static ConfigElement[] cfg;
            public void Load()
            {
                if (cfg == null) cfg = ConfigElement.GetAll(typeof(Config));
                ConfigElement.ParseFile(cfg, "./plugins/VenksSurvival/config.properties", this);
            }

            public void Save()
            {
                if (cfg == null) cfg = ConfigElement.GetAll(typeof(Config));
                ConfigElement.SerialiseSimple(cfg, "./plugins/VenksSurvival/config.properties", this);
            }
        }

        public static void MakeConfig()
        {
            using (StreamWriter w = new StreamWriter("./plugins/VenksSurvival/config.properties"))
            {
                w.WriteLine("# Edit the settings below to modify how the plugin operates.");
                w.WriteLine("# Whether or not this plugin should be controlled by other gamemode plugins. E.g, SkyWars.");
                w.WriteLine("gamemode-only = false");
                w.WriteLine("# Whether or not the player can die from natural causes. E.g, drowning.");
                w.WriteLine("survival-death = true");
                w.WriteLine("# Whether or not players can drown.");
                w.WriteLine("drowning = true");
                w.WriteLine("# Whether or not players regenerate health.");
                w.WriteLine("regeneration = true");
                w.WriteLine("# Whether or not mining is enabled.");
                w.WriteLine("mining = true");
                w.WriteLine("# Whether or not players gain money for killing other players.");
                w.WriteLine("economy = true");
                w.WriteLine("# If economy is enabled, the amount of money players get for killing other players.");
                w.WriteLine("bounty = 1");
                //w.WriteLine("mobs = false # Whether or not mobs are toggled.");
                w.WriteLine("# The amount of health players have.");
                w.WriteLine("max-health = 20");
                w.WriteLine("# Whether or not to use Goodly's effects plugin for particles. Note: Needs GoodlyEffects to work.");
                w.WriteLine("use-goodly-effects = false");
                w.WriteLine();
            }
        }

        public static Config cfg = new Config();

        public static int curpid = -1;
        public static List<string> maplist = new List<string>();
        public static string[,] players = new string[100, 3];
        public static string[,] weapons = new string[255, 3];
        public static string[,] tools = new string[255, 4];
        public static string[,] blocks = new string[255, 3];

        public override void Load(bool startup)
        {
            if (!File.Exists("./plugins/VenksSurvival/config.properties")) MakeConfig();

            // Initialize config
            cfg.Load();

            // Load files

            loadMaps();
            loadWeapons();
            loadTools();
            loadBlocks();
            initDB();

            OnPlayerClickEvent.Register(HandlePlayerClick, Priority.Low);
            OnPlayerClickEvent.Register(HandleBlockClick, Priority.Low);
            OnJoinedLevelEvent.Register(HandleOnJoinedLevel, Priority.Low);
            OnBlockChangingEvent.Register(HandleBlockChanged, Priority.Low);
            if (Config.Drowning) Server.MainScheduler.QueueRepeat(HandleDrown, null, TimeSpan.FromSeconds(1));
            if (Config.Regeneration) Server.MainScheduler.QueueRepeat(HandleRegeneration, null, TimeSpan.FromSeconds(4));
            //Server.MainScheduler.QueueRepeat(HandleFall, null, TimeSpan.FromMilliseconds(1)); 

            Command.Register(new CmdPvP());
            Command.Register(new CmdSafeZone());
            Command.Register(new CmdWeapon());
            Command.Register(new CmdTool());
            Command.Register(new CmdBlock());
            Command.Register(new CmdPotion());
            Command.Register(new CmdDropBlock());
            Command.Register(new CmdPickupBlock());
            Command.Register(new CmdInventory());

            Player[] online = PlayerInfo.Online.Items;
            foreach (Player p in online)
            {
                for (int i = 0; i < 100; i++)
                {
                    if (players[i, 0] == null)
                    {
                        players[i, 0] = p.name;
                        players[i, 1] = Config.MaxHealth;
                        players[i, 2] = "30000";
                        break;
                    }
                }
                p.SendCpeMessage(CpeMessageType.BottomRight2, "♥♥♥♥♥♥♥♥♥♥");
            }
        }

        public override void Unload(bool shutdown)
        {
            // Unload events
            OnPlayerClickEvent.Unregister(HandlePlayerClick);
            OnPlayerClickEvent.Unregister(HandleBlockClick);
            OnJoinedLevelEvent.Unregister(HandleOnJoinedLevel);
            OnBlockChangingEvent.Unregister(HandleBlockChanged);

            // Unload commands
            Command.Unregister(Command.Find("PvP"));
            Command.Unregister(Command.Find("SafeZone"));
            Command.Unregister(Command.Find("Weapon"));
            Command.Unregister(Command.Find("Tool"));
            Command.Unregister(Command.Find("Block"));
            Command.Unregister(Command.Find("Potion"));
            Command.Unregister(Command.Find("DropBlock"));
            Command.Unregister(Command.Find("PickupBlock"));
            Command.Unregister(Command.Find("Inventory"));
        }

        ColumnDesc[] createPotions = new ColumnDesc[] {
            new ColumnDesc("Name", ColumnType.VarChar, 16),
                new ColumnDesc("Health", ColumnType.Int32),
                new ColumnDesc("Speed", ColumnType.Int32),
                new ColumnDesc("Invisible", ColumnType.Int32),
                new ColumnDesc("Jump", ColumnType.Int32),
                new ColumnDesc("Waterbreathing", ColumnType.Int32),
                new ColumnDesc("Damage", ColumnType.Int32),
                new ColumnDesc("Strength", ColumnType.Int32),
                new ColumnDesc("Slowness", ColumnType.Int32),
                new ColumnDesc("Blindness", ColumnType.Int32),
        };

        ColumnDesc[] createInventories = new ColumnDesc[] {
            new ColumnDesc("Name", ColumnType.VarChar, 16),
                new ColumnDesc("Slot1", ColumnType.VarChar, 16),
                new ColumnDesc("Slot2", ColumnType.VarChar, 16),
                new ColumnDesc("Slot3", ColumnType.VarChar, 16),
                new ColumnDesc("Slot4", ColumnType.VarChar, 16),
                new ColumnDesc("Slot5", ColumnType.VarChar, 16),
                new ColumnDesc("Slot6", ColumnType.VarChar, 16),
                new ColumnDesc("Slot7", ColumnType.VarChar, 16),
                new ColumnDesc("Slot8", ColumnType.VarChar, 16),
                new ColumnDesc("Slot9", ColumnType.VarChar, 16),
                new ColumnDesc("Slot10", ColumnType.VarChar, 16),
                new ColumnDesc("Slot11", ColumnType.VarChar, 16),
                new ColumnDesc("Slot12", ColumnType.VarChar, 16),
                new ColumnDesc("Slot13", ColumnType.VarChar, 16),
                new ColumnDesc("Slot14", ColumnType.VarChar, 16),
                new ColumnDesc("Slot15", ColumnType.VarChar, 16),
                new ColumnDesc("Slot16", ColumnType.VarChar, 16),
                new ColumnDesc("Slot17", ColumnType.VarChar, 16),
                new ColumnDesc("Slot18", ColumnType.VarChar, 16),
                new ColumnDesc("Slot19", ColumnType.VarChar, 16),
                new ColumnDesc("Slot20", ColumnType.VarChar, 16),
                new ColumnDesc("Slot21", ColumnType.VarChar, 16),
                new ColumnDesc("Slot22", ColumnType.VarChar, 16),
                new ColumnDesc("Slot23", ColumnType.VarChar, 16),
                new ColumnDesc("Slot24", ColumnType.VarChar, 16),
                new ColumnDesc("Slot25", ColumnType.VarChar, 16),
                new ColumnDesc("Slot26", ColumnType.VarChar, 16),
                new ColumnDesc("Slot27", ColumnType.VarChar, 16),
                new ColumnDesc("Slot28", ColumnType.VarChar, 16),
                new ColumnDesc("Slot29", ColumnType.VarChar, 16),
                new ColumnDesc("Slot30", ColumnType.VarChar, 16),
                new ColumnDesc("Slot31", ColumnType.VarChar, 16),
                new ColumnDesc("Slot32", ColumnType.VarChar, 16),
                new ColumnDesc("Slot33", ColumnType.VarChar, 16),
                new ColumnDesc("Slot34", ColumnType.VarChar, 16),
                new ColumnDesc("Slot35", ColumnType.VarChar, 16),
                new ColumnDesc("Slot36", ColumnType.VarChar, 16),
        };

        void initDB()
        {
            Database.CreateTable("Potions", createPotions);
            Database.CreateTable("Inventories3", createInventories);
        }

        void loadMaps()
        {
            if (File.Exists(Config.Path + "maps.txt"))
            {
                using (var maplistreader = new StreamReader(Config.Path + "maps.txt"))
                {
                    string line;
                    while ((line = maplistreader.ReadLine()) != null)
                    {
                        maplist.Add(line);
                    }
                }
            }
            else File.Create(Config.Path + "maps.txt").Dispose();
        }

        #region Drowning

        void KillPlayer(Player p, int i)
        {
            p.HandleDeath(Block.Water);
            p.Extras.Remove("DROWNING");
            players[i, 1] = Config.MaxHealth;
            p.SendCpeMessage(CpeMessageType.BottomRight2, "♥♥♥♥♥♥♥♥♥♥");
        }

        void HandleDrown(SchedulerTask task)
        {
            if (Config.SurvivalDeath == false)
            {
                return;
            }
            Player[] online = PlayerInfo.Online.Items;
            foreach (Player p in online)
            {
                if (maplist.Contains(p.level.name))
                {
                    if (p.invincible)
                    {
                        continue;
                    }
                    ushort x = (ushort)(p.Pos.X / 32);
                    ushort y = (ushort)((p.Pos.Y - Entities.CharacterHeight) / 32);
                    ushort y2 = (ushort)(((p.Pos.Y - Entities.CharacterHeight) / 32) + 1);
                    ushort z = (ushort)(p.Pos.Z / 32);

                    BlockID block = p.level.GetBlock((ushort)x, ((ushort)y), (ushort)z);
                    BlockID block2 = p.level.GetBlock((ushort)x, ((ushort)y2), (ushort)z);

                    string body = Block.GetName(p, block);
                    string head = Block.GetName(p, block2);

                    if (body == "Water" && head == "Water")
                    {
                        int number = p.Extras.GetInt("DROWNING");
                        p.Extras["DROWNING"] = number + 1;
                        int air = p.Extras.GetInt("DROWNING");
                        // (10 - number) + 1)
                        if (air == 1)
                        {
                            p.SendCpeMessage(CpeMessageType.BottomRight1, "%boooooooooo");
                        }
                        if (air == 2)
                        {
                            p.SendCpeMessage(CpeMessageType.BottomRight1, "%booooooooo");
                        }
                        if (air == 3)
                        {
                            p.SendCpeMessage(CpeMessageType.BottomRight1, "%boooooooo");
                        }
                        if (air == 4)
                        {
                            p.SendCpeMessage(CpeMessageType.BottomRight1, "%booooooo");
                        }
                        if (air == 5)
                        {
                            p.SendCpeMessage(CpeMessageType.BottomRight1, "%boooooo");
                        }
                        if (air == 6)
                        {
                            p.SendCpeMessage(CpeMessageType.BottomRight1, "%booooo");
                        }
                        if (air == 7)
                        {
                            p.SendCpeMessage(CpeMessageType.BottomRight1, "%boooo");
                        }
                        if (air == 8)
                        {
                            p.SendCpeMessage(CpeMessageType.BottomRight1, "%booo");
                        }
                        if (air == 9)
                        {
                            p.SendCpeMessage(CpeMessageType.BottomRight1, "%boo");
                        }
                        if (air == 10)
                        {
                            p.SendCpeMessage(CpeMessageType.BottomRight1, "%bo");
                        }

                        if (air >= 10)
                        {
                            p.SendCpeMessage(CpeMessageType.BottomRight1, "");
                            for (int i = 0; i < 100; i++)
                            {
                                if (players[i, 0] == p.truename)
                                {
                                    int a = int.Parse(players[i, 1]);
                                    if (air >= 11)
                                    {
                                        players[i, 1] = (a - 1) + "";
                                        SetHpIndicator(i, p);

                                        if (a <= 1) KillPlayer(p, i);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        p.SendCpeMessage(CpeMessageType.BottomRight1, "");
                        p.Extras.Remove("DROWNING");
                    }
                }
            }
        }

        #endregion

        #region Regeneration

        void HandleRegeneration(SchedulerTask task)
        {
            Player[] online = PlayerInfo.Online.Items;
            foreach (Player p in online)
            {
                if (maplist.Contains(p.level.name))
                {
                    for (int i = 0; i < 100; i++)
                    {
                        if (players[i, 0] == p.truename)
                        {
                            int a = int.Parse(players[i, 1]);
                            if (a.ToString() == Config.MaxHealth)
                            {
                                continue;
                            }
                            players[i, 1] = (a + 1) + "";
                            SetHpIndicator(i, p);
                        }
                    }
                }
            }
        }

        #endregion

        #region Fall damage WIP
        // TODO: Calculations are inaccurate and server can affect measurements. Use at own risk!

        int fallBlocks(int fallTime)
        {
            int ft = fallTime;
            if (ft < 96) return 3;
            if (ft >= 96 && ft < 128) return 4;
            if (ft >= 128 && ft < 160) return 5;
            if (ft >= 160 && ft < 192) return 6;
            if (ft >= 192 && ft < 224) return 7;
            if (ft >= 224 && ft < 256) return 8;
            if (ft >= 256 && ft < 288) return 9;
            if (ft >= 288 && ft < 320) return 10;
            if (ft >= 320 && ft < 352) return 11;
            if (ft >= 352 && ft < 384) return 12;
            if (ft >= 384 && ft < 416) return 13;
            if (ft >= 416 && ft < 448) return 14;
            if (ft >= 448 && ft < 480) return 15;
            if (ft >= 480 && ft < 512) return 16;
            if (ft >= 512 && ft < 544) return 17;
            if (ft >= 544 && ft < 576) return 18;
            if (ft >= 576 && ft < 608) return 19;
            if (ft >= 608 && ft < 640) return 20;
            if (ft >= 640 && ft < 672) return 21;
            if (ft >= 672 && ft < 704) return 22;
            if (ft >= 704 && ft < 736) return 23;
            if (ft >= 736) return 24;
            return 0;
        }

        int fallDamage(int fallBlocks)
        {
            int fb = fallBlocks;
            if (fb < 4) return 0;
            if (fb == 4) return 1;
            if (fb == 5) return 2;
            if (fb == 6) return 3;
            if (fb == 7) return 4;
            if (fb == 8) return 5;
            if (fb == 9) return 6;
            if (fb == 10) return 7;
            if (fb == 11) return 8;
            if (fb == 12) return 9;
            if (fb == 13) return 10;
            if (fb == 14) return 11;
            if (fb == 15) return 12;
            if (fb == 16) return 13;
            if (fb == 17) return 14;
            if (fb == 18) return 15;
            if (fb == 19) return 16;
            if (fb == 20) return 17;
            if (fb == 21) return 18;
            if (fb == 22) return 19;
            if (fb >= 23) return 20;
            return 0;
        }

        void HandleFall(SchedulerTask task)
        {
            if (Config.SurvivalDeath == false)
            {
                return;
            }
            Player[] online = PlayerInfo.Online.Items;
            foreach (Player p in online)
            {
                if (maplist.Contains(p.level.name))
                {
                    if (p.invincible)
                    {
                        return;
                    }
                    if (Hacks.CanUseFly(p))
                    {
                        return;
                    }

                    ushort x = (ushort)(p.Pos.X / 32);
                    //ushort y = (ushort) ((p.Pos.Y - Entities.CharacterHeight) / 32);
                    ushort y2 = (ushort)(((p.Pos.Y - Entities.CharacterHeight) / 32) - 1);
                    ushort z = (ushort)(p.Pos.Z / 32);

                    BlockID block = p.level.GetBlock((ushort)x, ((ushort)y2), (ushort)z);

                    string feet = Block.GetName(p, block);

                    if (p.Extras.GetBoolean("FALLING"))
                    { // If falling
                        int fallTime = p.Extras.GetInt("FALLTIME");

                        if (feet == "Air")
                        {
                            p.Extras["FALLTIME"] = fallTime + 1;
                        }
                        else
                        {
                            int damage = fallDamage(fallBlocks(fallTime)) - 6;
                            p.Extras.Remove("FALLING");
                            p.Message("%eYou fell for %b{0} %ewhich is %b{1} blocks %eand %b{2} hearts", fallTime.ToString(),
                                fallBlocks(fallTime).ToString(), damage.ToString());
                            p.Extras.Remove("FALLTIME");

                        }

                        return; // Avoid loop
                    }

                    if (feet == "Air")
                    {
                        // Set falling to true so we can use it up further
                        p.Extras["FALLING"] = true;
                    }
                }
            }
        }

        #endregion

        #region Mining blocks

        string RemoveExcess(string text)
        {
            string stopAt = "(";
            if (!String.IsNullOrWhiteSpace(text))
            {
                int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

                if (charLocation > 0)
                {
                    return text.Substring(0, charLocation);
                }
            }

            return String.Empty;
        }

        int FindSlotFor(string[] row, string name)
        {
            for (int col = 1; col <= 36; col++)
            {
                string contents = row[col];
                if (contents == "0" || contents.StartsWith(name)) return col;
            }

            return 0;
        }

        int FindActiveSlot(string[] row, string name)
        {
            for (int col = 1; col <= 36; col++)
            {
                string contents = row[col];
                if (contents.StartsWith(name)) return col;
            }
            return 0;
        }

        string GetID(BlockID block)
        {
            string id = block.ToString();
            if (Convert.ToInt32(block) >= 66) id = (block - 256).ToString(); // Need to convert block if ID is over 66
            return "b" + id;
        }

        void UpdateBlockList(Player p, int column)
        {
            List<string[]> pRows = Database.GetRows("Inventories3", "*", "WHERE Name=@0", p.truename);

            if (pRows.Count == 0) return;
            else
            {
                if (pRows[0][column].ToString().StartsWith("0"))
                {
                    p.Send(Packet.SetInventoryOrder(Block.Air, (BlockID)column, p.hasExtBlocks));
                }

                else
                {
                    // Need to trim raw code to get the ID of the block. The example below is for the ID 75:
                    // ID of block = 75, amount of block = 22
                    // b75(22) -> 75
                    string raw = pRows[0][column].ToString();

                    int from = raw.IndexOf("b") + "b".Length;
                    int to = raw.LastIndexOf("(");

                    string id = raw.Substring(from, to - from);
                    p.Send(Packet.SetInventoryOrder((BlockID)Convert.ToUInt16(id), (BlockID)column, p.hasExtBlocks));
                }
            }
        }

        void SaveBlock(Player p, BlockID block)
        {
            string name = Block.GetName(p, block);

            if (name.ToLower().Contains("air") || name.ToLower().Contains("water") || name.ToLower().Contains("lava")) return;

            List<string[]> pRows = Database.GetRows("Inventories3", "*", "WHERE Name=@0", p.truename);

            if (pRows.Count == 0)
            {
                Database.AddRow("Inventories3", "Name, Slot1, Slot2, Slot3, Slot4, Slot5, Slot6, Slot7, Slot8, Slot9, Slot10, Slot11, Slot12, Slot13, Slot14," +
                "Slot15, Slot16, Slot17, Slot18, Slot19, Slot20, Slot21, Slot22, Slot23, Slot24, Slot25, Slot26, Slot27, Slot28, Slot29," +
                "Slot30, Slot31, Slot32, Slot33, Slot34, Slot35, Slot36", p.truename, GetID(block) + "(1)", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "04", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0");
                p.Message("%SNew count: %b1");

                UpdateBlockList(p, 1);
                return;
            }
            else
            {
                int column = FindSlotFor(pRows[0], GetID(block));

                if (column == 0)
                {
                    p.Message("Your inventory is full.");
                    return;
                }

                int newCount = pRows[0][column].ToString().StartsWith("0") ? 1 : Int32.Parse(pRows[0][column].ToString().Replace(GetID(block), "").Replace("(", "").Replace(")", "")) + 1;

                p.Message("%SNew count: %b" + newCount);

                Database.UpdateRows("Inventories3", "Slot" + column.ToString() + "=@1", "WHERE NAME=@0", p.truename, GetID(block) + "(" + newCount.ToString() + ")");

                UpdateBlockList(p, column);
                return;
            }
        }

        void HandleBlockChanged(Player p, ushort x, ushort y, ushort z, BlockID block, bool placing, ref bool cancel)
        {
            if (!maplist.Contains(p.level.name)) return;
            if (!p.level.Config.MOTD.ToLower().Contains("mining=true")) return;

            if (p.invincible || p.Game.Referee)
            {
                p.Message("%f╒ &c∩αΓ: &7You cannot modify blocks as a spectator.");
                p.RevertBlock(x, y, z);
                cancel = true;
                return;
            }

            if (placing)
            {
                List<string[]> pRows = Database.GetRows("Inventories3", "*", "WHERE Name=@0", p.truename);

                if (pRows.Count == 0)
                {
                    p.Message("You do not have any of this block.");
                    p.RevertBlock(x, y, z);
                    cancel = true;
                    return;
                }
                else
                {
                    string name = Block.GetName(p, block);
                    int column = FindActiveSlot(pRows[0], GetID(block));

                    if (column == 0)
                    {
                        p.Message("You do not have any of this block.");
                        p.RevertBlock(x, y, z);
                        cancel = true;
                        return;
                    }

                    int newCount = pRows[0][column].ToString().StartsWith("0") ? 1 : Int32.Parse(pRows[0][column].ToString().Replace(GetID(block), "").Replace("(", "").Replace(")", "")) - 1;

                    p.Message("%SNew count: %b" + newCount);

                    if (newCount == 0)
                    {
                        Database.UpdateRows("Inventories3", "Slot" + column.ToString() + "=@1", "WHERE NAME=@0", p.truename, "0");
                        p.Send(Packet.SetInventoryOrder(Block.Air, (BlockID)column, p.hasExtBlocks));
                    }
                    else
                    {
                        UpdateBlockList(p, column);
                        Database.UpdateRows("Inventories3", "Slot" + column.ToString() + "=@1", "WHERE NAME=@0", p.truename, GetID(block) + "(" + newCount.ToString() + ")");
                    }
                }
            }
        }

        void HandleBlockClick(Player p, MouseButton button, MouseAction action, ushort yaw, ushort pitch, byte entity, ushort x, ushort y, ushort z, TargetBlockFace face)
        {
            if (button == MouseButton.Left)
            {
                if (maplist.Contains(p.level.name))
                {
                    if (!Config.Mining) return;

                    if (!p.level.Config.MOTD.ToLower().Contains("mining=true")) return;

                    if (p.invincible || p.Game.Referee)
                    {
                        p.Message("%f╒ &c∩αΓ: &7You cannot modify blocks as a spectator.");
                        return;
                    }
                    //if (p.invincible) return;

                    if (p.Extras.GetInt("HOLDING_TIME") == 0)
                    {
                        p.Extras["MINING_COORDS"] = x + "_" + y + "_" + z;
                    }

                    float px = Convert.ToSingle(x), py = Convert.ToSingle(y), pz = Convert.ToSingle(z);

                    // Offset particle in the center of the block

                    px += 0.5f;
                    pz += 0.5f;

                    if (action == MouseAction.Pressed)
                    {
                        string coords = p.Extras.GetString("MINING_COORDS");
                        if (coords != (x + "_" + y + "_" + z))
                        {
                            p.Extras["HOLDING_TIME"] = 0;
                            p.Extras["MINING"] = false;
                            p.Extras["MINING_COORDS"] = 0;

                            if (Config.UseGoodlyEffects)
                            {
                                // Despawn break particle
                                p.Send(Packet.DefineEffect(16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1000, 0, 0, false, false, false, false, false));
                            }

                            return;
                        }

                        int heldFor = p.Extras.GetInt("HOLDING_TIME");

                        // The client's click speed is ~4 times/second
                        TimeSpan duration = TimeSpan.FromSeconds(heldFor / 4.0);

                        BlockID clickedBlock = p.level.GetBlock(x, y, z);
                        if (clickedBlock == 255) return;
                        BlockID b = p.GetHeldBlock();

                        string[] blockstats = getBlockStats((byte)clickedBlock + "").Split(' ');
                        //p.Message(blockstats[2]);
                        string[] toolstats = getToolStats((byte)b + "").Split(' ');

                        //p.Message("block type: " + blockstats[1] + ", hard: " + blockstats[2] + ", id: " +  clickedBlock);
                        //p.Message("tool type: " + toolstats[3] + ", hard: " + toolstats[2] + ", speed %b" + toolstats[1] + "%S, id: " +  toolstats[0]);

                        // Check if block type and tool type go together
                        // Assign speed of tool based on tool type
                        // Get block durability, times by 125 then divide by the speed of the tool

                        float miningSpeed = 0f;
                        float baseLifetime = 1f;

                        // Position particle towards respective block face

                        if (face == TargetBlockFace.AwayX) px += 0.5625f;
                        if (face == TargetBlockFace.AwayY) py += 0.5f;
                        if (face == TargetBlockFace.AwayZ) pz += 0.5625f;
                        if (face == TargetBlockFace.TowardsX) px -= 0.5625f;
                        if (face == TargetBlockFace.TowardsY) py -= 0.5f;
                        if (face == TargetBlockFace.TowardsZ) pz -= 0.5625f;

                        if (blockstats[2] == "0")
                        {
                            SaveBlock(p, clickedBlock);
                            p.level.UpdateBlock(p, x, y, z, Block.Air);
                            return;
                        }

                        else
                        {
                            miningSpeed = Convert.ToSingle((int.Parse(blockstats[2]) - 4) * 0.1);
                            baseLifetime = miningSpeed + 0.75f;
                        }

                        if (toolstats[2] == "0")
                        {
                            int toolSpeed = 1;
                            int blockSpeed = Int32.Parse(blockstats[2]);
                            int speed = (blockSpeed * 140) / toolSpeed;
                            //p.Message("Speed: " + speed + " bs: " + blockstats[2] + /*" mult:" + multiplier +*/ " toolsp: " + toolSpeed + " blocksp:" + blockSpeed);

                            // 140ms per hit. E.g, leaves takes 2 hits so 180ms to break

                            if (duration > TimeSpan.FromMilliseconds(speed))
                            {

                                SaveBlock(p, clickedBlock);
                                p.level.UpdateBlock(p, x, y, z, Block.Air);
                                p.Extras["HOLDING_TIME"] = 0;
                                p.Extras["MINING"] = false;
                                p.Extras["MINING_COORDS"] = 0;
                                return;
                            }
                            p.Extras["HOLDING_TIME"] = heldFor + 1;

                            if (Config.UseGoodlyEffects)
                            {
                                if (!p.Extras.GetBoolean("MINING"))
                                {
                                    // Spawn break particle
                                    p.Send(Packet.DefineEffect(200, 0, 105, 15, 120, 255, 255, 255, 10, 1, 28, 0, 0, 0, 0, baseLifetime, 0, true, true, true, true, true));
                                    p.Send(Packet.SpawnEffect(200, px, py, pz, 0, 0, 0));
                                    p.Extras["MINING"] = true;
                                }
                            }
                        }
                        else
                        {
                            int toolSpeed = Int32.Parse(toolstats[2]);
                            int blockSpeed = Int32.Parse(blockstats[2]);
                            int speed = (blockSpeed * 140) / toolSpeed;
                            //p.Message("Speed: " + speed + " bs: " + blockstats[2] + /*" mult:" + multiplier +*/ " toolsp: " + toolSpeed + " blocksp:" + blockSpeed);

                            if (duration > TimeSpan.FromMilliseconds(speed))
                            {
                                SaveBlock(p, clickedBlock);
                                p.level.UpdateBlock(p, x, y, z, Block.Air);
                                p.Extras["HOLDING_TIME"] = 0;
                                p.Extras["MINING_COORDS"] = 0;

                                if (Config.UseGoodlyEffects)
                                {
                                    // Despawn break particle
                                    p.Send(Packet.DefineEffect(200, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1000, 0, 0, false, false, false, false, false));
                                }
                                return;
                            }
                            p.Extras["HOLDING_TIME"] = heldFor + 1;

                            if (Config.UseGoodlyEffects)
                            {
                                if (!p.Extras.GetBoolean("MINING"))
                                {
                                    // Spawn break particle
                                    p.Send(Packet.DefineEffect(200, 0, 105, 15, 120, 255, 255, 255, 10, 1, 28, 0, 0, 0, 0, baseLifetime, 0, true, true, true, true, true));
                                    p.Send(Packet.SpawnEffect(200, px, py, pz, 0, 0, 0));
                                    p.Extras["MINING"] = true;
                                }
                            }
                        }
                    }
                    else if (action == MouseAction.Released)
                    {
                        p.Extras["HOLDING_TIME"] = 0;
                        p.Extras["MINING"] = false;
                        p.Extras["MINING_COORDS"] = 0;

                        if (Config.UseGoodlyEffects)
                        {
                            // Despawn break particle
                            p.Send(Packet.DefineEffect(200, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1000, 0, 0, false, false, false, false, false));
                        }
                    }
                }
            }
        }

        #endregion

        #region Inventories

        //string directory = "text/inventory/" + p.name + "/";
        //if (!Directory.Exists(directory)) { Directory.CreateDirectory(directory); }

        #endregion

        #region PvP

        public static bool inSafeZone(Player p, string map)
        {
            if (File.Exists(Config.Path + "safezones" + map + ".txt"))
            {
                using (var r = new StreamReader(Config.Path + "safezones" + map + ".txt"))
                {
                    string line;
                    while ((line = r.ReadLine()) != null)
                    {
                        string[] temp = line.Split(';');
                        string[] coord1 = temp[0].Split(',');
                        string[] coord2 = temp[1].Split(',');

                        if ((p.Pos.BlockX <= int.Parse(coord1[0]) && p.Pos.BlockX >= int.Parse(coord2[0])) || (p.Pos.BlockX >= int.Parse(coord1[0]) && p.Pos.BlockX <= int.Parse(coord2[0])))
                        {
                            if ((p.Pos.BlockZ <= int.Parse(coord1[2]) && p.Pos.BlockZ >= int.Parse(coord2[2])) || (p.Pos.BlockZ >= int.Parse(coord1[2]) && p.Pos.BlockZ <= int.Parse(coord2[2])))
                            {
                                if ((p.Pos.BlockY <= int.Parse(coord1[1]) && p.Pos.BlockY >= int.Parse(coord2[1])) || (p.Pos.BlockY >= int.Parse(coord1[1]) && p.Pos.BlockY <= int.Parse(coord2[1])))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    return false;
                }
            }
            return false;
        }

        void HandlePlayerClick(Player p, MouseButton button, MouseAction action, ushort yaw, ushort pitch, byte entity, ushort x, ushort y, ushort z, TargetBlockFace face)
        {
            if (button == MouseButton.Left)
            {
                if (maplist.Contains(p.level.name))
                {
                    if (action != MouseAction.Pressed) return;
                    if (entity != Entities.SelfID && !ClickOnPlayer(p, entity, button)) return;

                    int placeholder = 1;
                    if (placeholder == 1)
                    {
                        #region PvP code
                        curpid = -1;
                        for (int yi = 0; yi < 100; yi++)
                        {
                            if (players[yi, 0] == p.name)
                            {
                                curpid = yi;
                            }
                        }

                        int s = DateTime.Now.Second;
                        int ms = DateTime.Now.Millisecond;
                        if (int.Parse(s + "" + ms) - int.Parse(players[curpid, 2]) > 1350 || int.Parse(s + "" + ms) - int.Parse(players[curpid, 2]) < -1350)
                        {
                            Player[] online = PlayerInfo.Online.Items;
                            foreach (Player pl in online)
                            {
                                if (pl.EntityID == entity)
                                {
                                    for (int i = 0; i < 100; i++)
                                    {
                                        if (players[i, 0] == pl.name)
                                        {
                                            if (pl.invincible) return;
                                            // Check if they can kill players, as determined by gamemode plugins
                                            bool canKill = Config.GamemodeOnly == false ? true : p.Extras.GetBoolean("PVP_CAN_KILL");
                                            if (!canKill)
                                            {
                                                p.Message("You cannot kill people.");
                                                return;
                                            }

                                            // If both players are not in safezones
                                            if (!inSafeZone(p, p.level.name) && !inSafeZone(pl, pl.level.name))
                                            {
                                                if (p.Game.Referee) return;
                                                if (pl.Game.Referee) return;
                                                if (p.level.Config.MOTD.ToLower().Contains("-health")) return;

                                                int a = int.Parse(players[i, 1]);

                                                BlockID b = p.GetHeldBlock();
                                                string[] weaponstats = getWeaponStats((byte)b + "").Split(' ');
                                                //p.Message("dmg: " + weaponstats[1] + " id: " +  b.ExtID);

                                                if (p.Extras.GetBoolean("PVP_UNLOCKED_" + b) || weaponstats[0] == "0")
                                                {
                                                    // Calculate damage from weapon
                                                    int damage = 1;
                                                    if (weaponstats[0] != "0")
                                                    {
                                                        damage = Int32.Parse(weaponstats[1]);
                                                        players[i, 1] = (a - damage) + "";
                                                    }
                                                    else players[i, 1] = (a - 1) + "";

                                                    if (a > 0)
                                                    {
                                                        p.Message("%c-" + damage + " %7(%b{0} %f♥ %bleft%7)", players[i, 1]);
                                                    }
                                                    SetHpIndicator(i, pl);

                                                    if (a <= 0)
                                                    { // If player killed them
                                                        string stringweaponused = weaponstats[0] == "0" ? "." : " %Susing " + Block.GetName(p, b) + ".";
                                                        pl.level.Message(pl.ColoredName + " %Swas killed by " + p.truename + stringweaponused);
                                                        pl.Extras["KILLEDBY"] = p.truename; // Support for custom gamemodes
                                                        pl.Extras["KILLER"] = p.truename; // Support for custom gamemodes
                                                        pl.Extras["PVP_DEAD"] = true; // Support for custom gamemodes
                                                                                      // Use string killedBy = p.Extras.GetInt("KILLEDBY") to get the player who killed them
                                                                                      // Use string killer = p.Extras.GetInt("KILLER") to get the killer

                                                        pl.HandleDeath(Block.Stone);
                                                        p.Extras.Remove("DROWNING");
                                                        //pl.Game.Referee = true;

                                                        players[i, 1] = Config.MaxHealth;

                                                        pl.SendCpeMessage(CpeMessageType.BottomRight2, "♥♥♥♥♥♥♥♥♥♥");

                                                        if (Config.Economy == true && (p.ip != pl.ip || p.ip == "127.0.0.1"))
                                                        {
                                                            if (pl.money > Config.Bounty - 1)
                                                            {
                                                                p.Message("You stole " + Config.Bounty + " " + Server.Config.Currency + " %Sfrom " + pl.ColoredName + "%S.");
                                                                pl.Message(p.ColoredName + " %Sstole " + Config.Bounty + " " + Server.Config.Currency + " from you.");
                                                                p.SetMoney(p.money + Config.Bounty);
                                                                pl.SetMoney(pl.money - Config.Bounty);

                                                                MCGalaxy.Games.BountyData bounty = ZSGame.Instance.FindBounty(pl.name);
                                                                if (bounty != null)
                                                                {
                                                                    ZSGame.Instance.Bounties.Remove(bounty);
                                                                    Player setter = PlayerInfo.FindExact(bounty.Origin);

                                                                    if (setter == null)
                                                                    {
                                                                        p.Message("Cannot collect the bounty, as the player who set it is offline.");
                                                                    }
                                                                    else
                                                                    {
                                                                        p.level.Message("&c" + p.DisplayName + " %Scollected the bounty of &a" +
                                                                            bounty.Amount + " %S" + Server.Config.Currency + " on " + pl.ColoredName + "%S.");
                                                                        p.SetMoney(p.money + bounty.Amount);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    p.Message("You do not own this weapon.");
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                p.Message("You cannot hurt people in a safe zone.");
                                                return;
                                            }
                                            players[curpid, 2] = DateTime.Now.Second + "" + DateTime.Now.Millisecond + "";
                                        }
                                    }
                                }
                            }
                        }
                    }

                    #endregion
                }
            }
        }

        static bool ClickOnPlayer(Player p, byte entity, MouseButton button)
        {
            Player[] ponline = PlayerInfo.Online.Items;
            for (int i = 0; i < ponline.Length; i++)
            {
                if (ponline[i].EntityID != entity) continue;
                Player pl = ponline[i];
                Vec3F32 delta = p.Pos.ToVec3F32() - pl.Pos.ToVec3F32();
                float reachSq = p.ReachDistance * p.ReachDistance;
                // Don't allow clicking on players further away than their reach distance
                if (delta.LengthSquared > (reachSq + 1)) return false;
                curpid = -1;
                for (int yi = 0; yi < 100; yi++)
                {
                    if (players[yi, 0] == p.name)
                    {
                        curpid = yi;
                    }
                }

                int s = DateTime.Now.Second;
                int ms = DateTime.Now.Millisecond;
                if (int.Parse(s + "" + ms) - int.Parse(players[curpid, 2]) > 1350 || int.Parse(s + "" + ms) - int.Parse(players[curpid, 2]) < -1350)
                {
                    if (button == MouseButton.Left)
                    {
                        // Check if they can kill players, as determined by gamemode plugins
                        bool canKill = PvP.Config.GamemodeOnly == false ? true : p.Extras.GetBoolean("PVP_CAN_KILL");
                        if (!canKill) return false;
                        if (p.Game.Referee) return false;
                        if (pl.Game.Referee) return false;
                        if (p.invincible) return false;
                        if (pl.invincible) return false;
                        if (inSafeZone(p, p.level.name) || inSafeZone(pl, pl.level.name)) return false;
                        BlockID b = p.GetHeldBlock();
                        string[] weaponstats = getWeaponStats((byte)b + "").Split(' ');
                        if (!hasWeapon(p.level.name, p, Block.GetName(p, b)) && weaponstats[0] != "0") return false;
                        PushPlayer(p, pl);
                    }
                    return true;
                }
            }
            return false;
        }

        static void PushPlayer(Player p, Player pl)
        {
            if (p.level.Config.MOTD.ToLower().Contains("-damage")) return;

            int srcHeight = ModelInfo.CalcEyeHeight(p);
            int dstHeight = ModelInfo.CalcEyeHeight(pl);
            int dx = p.Pos.X - pl.Pos.X, dy = (p.Pos.Y + srcHeight) - (pl.Pos.Y + dstHeight), dz = p.Pos.Z - pl.Pos.Z;
            Vec3F32 dir = new Vec3F32(dx, dy, dz);
            if (dir.Length > 0) dir = Vec3F32.Normalise(dir);

            float mult = 1 / ModelInfo.GetRawScale(pl.Model);
            float plScale = ModelInfo.GetRawScale(pl.Model);

            if (pl.Supports(CpeExt.VelocityControl) && p.Supports(CpeExt.VelocityControl))
            {
                // Intensity of force is in part determined by model scale
                pl.Send(Packet.VelocityControl((-dir.X * mult) * 0.57f, 1.0117f * mult, (-dir.Z * mult) * 0.57f, 0, 1, 0));
            }
            else
            {
                p.Message("You can left and right click people to hit them if you update your client!");
            }
        }

        void HandleOnJoinedLevel(Player p, Level prevLevel, Level level, ref bool announce)
        {
            if (Config.Mining)
            {
                if (Config.GamemodeOnly)
                {
                    List<string[]> rows = Database.GetRows("Inventories3", "*", "WHERE Name=@0", p.truename);
                    if (rows.Count > 0) Database.Execute("DELETE FROM Inventories3 WHERE Name=@0", p.truename);
                }

                if (p.level.Config.MOTD.ToLower().Contains("mining=true"))
                {
                    List<string[]> rows = Database.GetRows("Inventories3", "*", "WHERE Name=@0", p.truename);

                    if (rows.Count == 0)
                    {
                        for (int i = 0; i < 767; i++)
                        {
                            p.Send(Packet.SetInventoryOrder(Block.Air, (BlockID)i, p.hasExtBlocks));
                        }
                    }

                    else
                    {
                        for (int i = 1; i <= 767; i++)
                        {
                            if (i <= 36)
                            {
                                if (rows[0][i].ToString().StartsWith("0"))
                                {
                                    p.Send(Packet.SetInventoryOrder(Block.Air, (BlockID)i, p.hasExtBlocks));
                                    continue;
                                }

                                else
                                {
                                    // Need to trim raw code to get the ID of the block. The example below is for the ID 75:
                                    // ID of block = 75, amount of block = 22
                                    // b75(22) -> 75
                                    string raw = rows[0][i].ToString();

                                    int from = raw.IndexOf("b") + "b".Length;
                                    int to = raw.LastIndexOf("(");

                                    string id = raw.Substring(from, to - from);
                                    p.Send(Packet.SetInventoryOrder((BlockID)Convert.ToUInt16(id), (BlockID)i, p.hasExtBlocks));
                                    continue;
                                }
                            }

                            else
                            {
                                p.Send(Packet.SetInventoryOrder(Block.Air, (BlockID)i, p.hasExtBlocks));
                            }
                        }
                    }
                }
            }

            if (p.Supports(CpeExt.TextHotkey))
            {
                // Drop blocks hotkeys (del and backspace)
                p.Send(Packet.TextHotKey("DropBlocks", "/DropBlock◙", 211, 0, true));
                p.Send(Packet.TextHotKey("DropBlocks", "/DropBlock◙", 14, 0, true));
            }

            Command.Find("PvP").Use(p, "sethp " + p.truename + " " + Config.MaxHealth);

            if (maplist.Contains(level.name))
            {
                p.SendCpeMessage(CpeMessageType.BottomRight2, "♥♥♥♥♥♥♥♥♥♥");

                for (int i = 0; i < 100; i++)
                {
                    if (players[i, 0] == p.name) return;
                }

                for (int i = 0; i < 100; i++)
                {
                    if (players[i, 0] == null)
                    {
                        players[i, 0] = p.name;
                        players[i, 1] = Config.MaxHealth;
                        players[i, 2] = "30000";
                        return;
                    }
                }
            }

            if (prevLevel == null) return;
            if (!maplist.Contains(level.name))
            {
                p.SendCpeMessage(CpeMessageType.BottomRight2, "");
            }
        }

        public static void SetHpIndicator(int i, Player pl)
        {
            int a = int.Parse(players[i, 1]);

            if (a == 20)
            {
                pl.SendCpeMessage(CpeMessageType.BottomRight2, "%f♥♥♥♥♥♥♥♥♥♥");
            }
            if (a == 19)
            {
                pl.SendCpeMessage(CpeMessageType.BottomRight2, "%f♥♥♥♥♥♥♥♥♥╫");
            }
            if (a == 18)
            {
                pl.SendCpeMessage(CpeMessageType.BottomRight2, "%f♥♥♥♥♥♥♥♥♥%0♥");
            }
            if (a == 17)
            {
                pl.SendCpeMessage(CpeMessageType.BottomRight2, "%f♥♥♥♥♥♥♥♥╫%0♥");
            }
            if (a == 16)
            {
                pl.SendCpeMessage(CpeMessageType.BottomRight2, "%f♥♥♥♥♥♥♥♥%0♥♥");
            }
            if (a == 15)
            {
                pl.SendCpeMessage(CpeMessageType.BottomRight2, "%f♥♥♥♥♥♥♥╫%0♥♥");
            }
            if (a == 14)
            {
                pl.SendCpeMessage(CpeMessageType.BottomRight2, "%f♥♥♥♥♥♥♥%0♥♥♥");
            }
            if (a == 13)
            {
                pl.SendCpeMessage(CpeMessageType.BottomRight2, "%f♥♥♥♥♥♥╫%0♥♥♥");
            }
            if (a == 12)
            {
                pl.SendCpeMessage(CpeMessageType.BottomRight2, "%f♥♥♥♥♥♥%0♥♥♥♥");
            }
            if (a == 11)
            {
                pl.SendCpeMessage(CpeMessageType.BottomRight2, "%f♥♥♥♥♥╫%0♥♥♥♥");
            }
            if (a == 10)
            {
                pl.SendCpeMessage(CpeMessageType.BottomRight2, "%f♥♥♥♥♥%0♥♥♥♥♥");
            }
            if (a == 9)
            {
                pl.SendCpeMessage(CpeMessageType.BottomRight2, "%f♥♥♥♥╫%0♥♥♥♥♥");
            }
            if (a == 8)
            {
                pl.SendCpeMessage(CpeMessageType.BottomRight2, "%f♥♥♥♥%0♥♥♥♥♥♥");
            }
            if (a == 7)
            {
                pl.SendCpeMessage(CpeMessageType.BottomRight2, "%f♥♥♥╫%0♥♥♥♥♥♥");
            }
            if (a == 6)
            {
                pl.SendCpeMessage(CpeMessageType.BottomRight2, "%f♥♥♥%0♥♥♥♥♥♥♥");
            }
            if (a == 5)
            {
                pl.SendCpeMessage(CpeMessageType.BottomRight2, "%f♥♥╫%0♥♥♥♥♥♥♥");
            }
            if (a == 4)
            {
                pl.SendCpeMessage(CpeMessageType.BottomRight2, "%f♥♥%0♥♥♥♥♥♥♥♥");
            }
            if (a == 3)
            {
                pl.SendCpeMessage(CpeMessageType.BottomRight2, "%f♥╫%0♥♥♥♥♥♥♥♥");
            }
            if (a == 2)
            {
                pl.SendCpeMessage(CpeMessageType.BottomRight2, "%f♥%0♥♥♥♥♥♥♥♥♥");
            }
            if (a == 1)
            {
                pl.SendCpeMessage(CpeMessageType.BottomRight2, "%f╫%0♥♥♥♥♥♥♥♥♥");
            }
            if (a == 0)
            {
                pl.SendCpeMessage(CpeMessageType.BottomRight2, "%0♥♥♥♥♥♥♥♥♥♥");
            }
        }

        #endregion

        #region Weapons

        public static bool hasWeapon(string world, Player p, string weapon)
        {
            string filepath = Config.Path + "weapons/" + world + "/" + p.truename + ".txt";
            if (File.Exists(filepath))
            {
                using (var r = new StreamReader(filepath))
                {
                    string line;
                    while ((line = r.ReadLine()) != null)
                    {
                        if (line == weapon) return true;
                    }
                }
            }
            return false;
        }

        public static string getWeaponStats(string weapon)
        {
            for (int i = 0; i < 255; i++)
            {
                if (weapons[i, 0] == weapon)
                {
                    return weapons[i, 0] + " " + weapons[i, 1] + " " + weapons[i, 2];
                }
            }
            return "0 1 0";
        }

        void loadWeapons()
        {
            if (File.Exists(Config.Path + "weapons.txt"))
            {
                using (var r = new StreamReader(Config.Path + "weapons.txt"))
                {
                    string line;
                    while ((line = r.ReadLine()) != null)
                    {
                        string[] weaponStats = line.Split(';');
                        for (int i = 0; i < 255; i++)
                        {
                            if (weapons[i, 0] == null)
                            {
                                weapons[i, 0] = weaponStats[0];
                                weapons[i, 1] = weaponStats[1];
                                weapons[i, 2] = weaponStats[2];
                                break;
                            }
                        }
                    }
                }
            }
            else File.Create(Config.Path + "weapons.txt").Dispose();
        }

        #endregion

        #region Tools

        bool hasTool(string world, Player p, string tool)
        {
            string filepath = Config.Path + "tools/" + world + "/" + p.truename + ".txt";
            if (File.Exists(filepath))
            {
                using (var r = new StreamReader(filepath))
                {
                    string line;
                    while ((line = r.ReadLine()) != null)
                    {
                        if (line == tool) return true;
                    }
                }
            }
            return false;
        }

        string getToolStats(string tool)
        {
            for (int i = 0; i < 255; i++)
            {
                if (tools[i, 0] == tool)
                {
                    return tools[i, 0] + " " + tools[i, 1] + " " + tools[i, 2] + " " + tools[i, 3];
                }
            }
            return "0 1 0 0";
        }

        void loadTools()
        {
            if (File.Exists(Config.Path + "tools.txt"))
            {
                using (var r = new StreamReader(Config.Path + "tools.txt"))
                {
                    string line;
                    while ((line = r.ReadLine()) != null)
                    {
                        string[] toolStats = line.Split(';');
                        for (int i = 0; i < 255; i++)
                        {
                            if (tools[i, 0] == null)
                            {
                                tools[i, 0] = toolStats[0];
                                tools[i, 1] = toolStats[1];
                                tools[i, 2] = toolStats[2];
                                tools[i, 3] = toolStats[3];
                                break;
                            }
                        }
                    }
                }
            }
            else File.Create(Config.Path + "tools.txt").Dispose();
        }

        #endregion

        #region Blocks

        bool hasBlock(string world, Player p, string block)
        {
            string filepath = Config.Path + "blocks/" + world + "/" + p.truename + ".txt";
            if (File.Exists(filepath))
            {
                using (var r = new StreamReader(filepath))
                {
                    string line;
                    while ((line = r.ReadLine()) != null)
                    {
                        if (line == block) return true;
                    }
                }
            }
            return false;
        }

        string getBlockStats(string block)
        {
            for (int i = 0; i < 255; i++)
            {
                if (blocks[i, 0] == block)
                {
                    return blocks[i, 0] + " " + blocks[i, 1] + " " + blocks[i, 2];
                }
            }
            return "0 1 0";
        }

        void loadBlocks()
        {
            if (File.Exists(Config.Path + "blocks.txt"))
            {
                using (var r = new StreamReader(Config.Path + "blocks.txt"))
                {
                    string line;
                    while ((line = r.ReadLine()) != null)
                    {
                        string[] blockStats = line.Split(';');
                        for (int i = 0; i < 255; i++)
                        {
                            if (blocks[i, 0] == null)
                            {
                                blocks[i, 0] = blockStats[0];
                                blocks[i, 1] = blockStats[1];
                                blocks[i, 2] = blockStats[2];
                                break;
                            }
                        }
                    }
                }
            }
            else File.Create(Config.Path + "blocks.txt").Dispose();
        }

        #endregion

        #region Potions

        public static void CheckInvisible(SchedulerTask task)
        {
            Player[] players = PlayerInfo.Online.Items;
            foreach (Player p in players)
            {
                if (!p.Extras.GetBoolean("POTION_IS_INVISIBLE")) continue;
                // Timer

                string time = p.Extras.GetString("POTION_INV_TIMER");
                DateTime date1 = DateTime.Parse(time);

                DateTime date2 = date1.AddSeconds(10);

                if (DateTime.UtcNow > date2)
                {
                    p.Extras["POTION_IS_INVISIBLE"] = false;

                    Entities.GlobalSpawn(p, true);
                    Server.hidden.Remove(p.name);
                    p.Message("The invisibility potion has worn off, you are now visible again.");
                    Server.MainScheduler.Cancel(task);
                }
            }
        }

        public static void CheckSpeed(SchedulerTask task)
        {
            Player[] players = PlayerInfo.Online.Items;
            foreach (Player p in players)
            {
                if (!p.Extras.GetBoolean("POTION_IS_FAST")) continue;
                if (p.Extras.GetBoolean("POTION_IS_JUMP")) p.Send(Packet.Motd(p, p.level.Config.MOTD + " jumpheight=4 horspeed=3.75"));
                else p.Send(Packet.Motd(p, p.level.Config.MOTD + " horspeed=3.75"));
                // Timer

                string time = p.Extras.GetString("POTION_SPEED_TIMER");
                DateTime date1 = DateTime.Parse(time);
                DateTime date2 = date1.AddSeconds(7);

                if (DateTime.UtcNow > date2)
                {
                    p.Extras["POTION_IS_FAST"] = false;
                    p.SendMapMotd();
                    p.Message("The speed potion has worn off, you are now at normal speed again.");
                    Server.MainScheduler.Cancel(task);
                }
            }
        }

        public static void CheckJump(SchedulerTask task)
        {
            Player[] players = PlayerInfo.Online.Items;
            foreach (Player p in players)
            {
                if (!p.Extras.GetBoolean("POTION_IS_JUMP")) continue;
                if (p.Extras.GetBoolean("POTION_IS_FAST")) p.Send(Packet.Motd(p, p.level.Config.MOTD + " jumpheight=4 horspeed=3.75"));
                else p.Send(Packet.Motd(p, p.level.Config.MOTD + " jumpheight=4"));
                if (p.Supports(CpeExt.HackControl)) p.Send(Packet.HackControl(true, true, true, true, true, 128)); // Fly, noclip, speed, respawn, 3rd, jumpheight            	
                // Timer

                string time = p.Extras.GetString("POTION_JUMP_TIMER");
                DateTime date1 = DateTime.Parse(time);
                DateTime date2 = date1.AddSeconds(7);

                if (DateTime.UtcNow > date2)
                {
                    p.Extras["POTION_IS_JUMP"] = false;
                    p.SendMapMotd();
                    p.Message("The jump potion has worn off, you are now at normal jump height again.");
                    Server.MainScheduler.Cancel(task);
                }
            }
        }
        #endregion
    }

    #region Commands

    public sealed class CmdInventory : Command2
    {
        public override string name { get { return "Inventory"; } }
        public override string shortcut { get { return "backpack"; } }
        public override string type { get { return "Other"; } }

        public override void Use(Player p, string message, CommandData data)
        {

            List<string[]> pRows = Database.GetRows("Inventories3", "*", "WHERE Name=@0", p.truename);

            if (pRows.Count == 0)
            {
                p.Message("Your inventory is empty.");
                return;
            }
            else
            {
                List<string> slots = new List<string>();

                for (int i = 1; i < 37; i++)
                {
                    if (pRows[0][i].ToString().StartsWith("0")) continue; // Don't bother with air
                    slots.Add("%2[" + i + "%2] " + pRows[0][i].ToString());
                }

                string inventory = String.Join(" %8| ", slots.ToArray());

                p.Message(inventory);
            }
        }

        public override void Help(Player p)
        {

        }
    }

    public sealed class CmdPvP : Command2
    {
        public override string name { get { return "PvP"; } }
        public override string type { get { return CommandTypes.Games; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public override bool SuperUseable { get { return false; } }
        public override CommandPerm[] ExtraPerms { get { return new[] { new CommandPerm(LevelPermission.Admin, "can manage PvP") }; } }

        public override void Use(Player p, string message, CommandData data)
        {
            if (message.Length == 0)
            {
                Help(p);
                return;
            }
            string[] args = message.SplitSpaces();

            switch (args[0].ToLower())
            {
                case "add":
                    HandleAdd(p, args, data);
                    return;
                case "del":
                    HandleDelete(p, args, data);
                    return;
                case "sethp":
                    HandleSetHP(p, args, data);
                    return;
            }
        }

        void HandleAdd(Player p, string[] args, CommandData data)
        {
            if (args.Length == 1)
            {
                p.Message("You need to specify a map to add.");
                return;
            }
            if (!HasExtraPerm(p, data.Rank, 1)) return;
            string pvpMap = args[1];

            PvP.maplist.Add(pvpMap);
            p.Message("The map %b" + pvpMap + " %Shas been added to the PvP map list.");

            // Add the map to the map list
            using (StreamWriter maplistwriter =
                new StreamWriter(PvP.Config.Path + "maps.txt"))
            {
                foreach (String s in PvP.maplist)
                {
                    maplistwriter.WriteLine(s);
                }
            }

            Player[] online = PlayerInfo.Online.Items;
            foreach (Player pl in online)
            {
                if (pl.level.name == args[1])
                {
                    for (int i = 0; i < 100; i++)
                    {
                        if (PvP.players[i, 0] == null)
                        {
                            PvP.players[i, 0] = pl.name;
                            PvP.players[i, 1] = PvP.Config.MaxHealth;
                            PvP.players[i, 2] = "30000";
                            break;
                        }
                    }

                    pl.SendCpeMessage(CpeMessageType.BottomRight2, "♥♥♥♥♥♥♥♥♥♥");
                }
            }
        }

        void HandleDelete(Player p, string[] args, CommandData data)
        {
            if (args.Length == 1)
            {
                p.Message("You need to specify a map to remove.");
                return;
            }
            if (!HasExtraPerm(p, data.Rank, 1)) return;
            string pvpMap = args[1];

            PvP.maplist.Remove(pvpMap);
            p.Message("The map %b" + pvpMap + " %Shas been removed from the PvP map list.");
        }

        // Sekrit cmd

        void HandleSetHP(Player p, string[] args, CommandData data)
        {
            if (args.Length == 1)
            {
                p.Message("You need to specify a player to set their health.");
                return;
            }
            if (args.Length == 2)
            {
                p.Message("You need to specify an amount of health to set.");
                return;
            }

            Player[] online = PlayerInfo.Online.Items;
            Player target = PlayerInfo.FindMatches(p, args[1]);
            if (target == null) return;
            foreach (Player pl in online)
            {
                if (PvP.maplist.Contains(p.level.name))
                {
                    if (pl.invincible)
                    {
                        continue;
                    }

                    for (int i = 0; i < 100; i++)
                    {
                        if (PvP.players[i, 0] == target.name)
                        {

                            PvP.players[i, 1] = args[2];
                            PvP.SetHpIndicator(i, target);
                        }
                    }
                }
            }
        }

        public override void Help(Player p)
        {
            p.Message("%T/PvP add <map> %H- Adds a map to the PvP map list.");
            p.Message("%T/PvP del <map> %H- Deletes a map from the PvP map list.");
            // p.Message("%T/PvP sethp [player] [1-20] %H- Sets a player's health.");
        }
    }

    public sealed class CmdSafeZone : Command2
    {
        public override string name { get { return "SafeZone"; } }
        public override string type { get { return CommandTypes.Games; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public override bool SuperUseable { get { return false; } }
        public override CommandPerm[] ExtraPerms { get { return new[] { new CommandPerm(LevelPermission.Admin, "can manage safe zones") }; } }

        public override void Use(Player p, string message, CommandData data)
        {
            if (message.Length == 0)
            {
                Help(p);
                return;
            }
            string[] args = message.SplitSpaces(2);

            switch (args[0].ToLower())
            {
                case "add":
                    HandleAdd(p, args, data);
                    return;
            }
        }

        void HandleAdd(Player p, string[] args, CommandData data)
        {
            p.Message("Place or break two blocks to determine the edges.");
            p.MakeSelection(2, null, addSafeZone);
        }

        bool addSafeZone(Player p, Vec3S32[] marks, object state, BlockID block)
        {
            FileInfo filedir = new FileInfo(PvP.Config.Path + "safezones" + p.level.name + ".txt");
            filedir.Directory.Create();

            using (StreamWriter file = new StreamWriter(PvP.Config.Path + "safezones" + p.level.name + ".txt", true))
            {
                file.WriteLine(marks.GetValue(0) + ";" + marks.GetValue(1));
            }

            p.Message("Successfully added a safezone.");
            return true;
        }

        public override void Help(Player p)
        {
            p.Message("%T/SafeZone [add] %H- Adds a safe zone to the current PvP map.");
            //p.Message("%T/SafeZone [del] %H- Removes a safe zone from the current PvP map.");
        }
    }

    public sealed class CmdWeapon : Command2
    {
        public override string name { get { return "Weapon"; } }
        public override string type { get { return CommandTypes.Games; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public override bool SuperUseable { get { return false; } }
        public override CommandPerm[] ExtraPerms { get { return new[] { new CommandPerm(LevelPermission.Admin, "can manage weapons") }; } }

        public override void Use(Player p, string message, CommandData data)
        {
            if (message.Length == 0)
            {
                Help(p);
                return;
            }
            string[] args = message.SplitSpaces(4);

            switch (args[0].ToLower())
            {
                case "add":
                    HandleAdd(p, args);
                    return;
                case "give":
                    HandleGive(p, args);
                    return;
                case "take":
                    HandleTake(p, args);
                    return;
            }
        }

        void HandleAdd(Player p, string[] args)
        {
            if (args.Length == 1)
            {
                p.Message("You need to specify an ID for the block. E.g, '1' for stone.");
                return;
            }
            if (args.Length == 2)
            {
                p.Message("You need to specify the damage that the weapon does. E.g, '1' for half a heart.");
                return;
            }
            if (args.Length == 3)
            {
                p.Message("You need to specify how many clicks before it breaks. 0 for infinite clicks.");
                return;
            }

            for (int i = 0; i < 255; i++)
            {
                if (PvP.weapons[i, 0] == null)
                {
                    PvP.weapons[i, 0] = args[1];
                    PvP.weapons[i, 1] = args[2];
                    PvP.weapons[i, 2] = args[3];
                    createWeapon(args[1], args[2], args[3]);
                    break;
                }
            }

            p.Message("Created weapon with ID %b" + args[1] + "%S, damage %b" + args[2] + "%S and durability %b" + args[3] + "%S.");
        }

        void createWeapon(string id, string damage, string durability)
        {
            FileInfo filedir = new FileInfo(PvP.Config.Path + "weapons.txt");
            filedir.Directory.Create();

            using (StreamWriter file = new StreamWriter(PvP.Config.Path + "weapons.txt", true))
            {
                file.WriteLine(id + ";" + damage + ";" + durability);
            }
        }

        void HandleGive(Player p, string[] args)
        {
            if (args[1] != PvP.Config.SecretCode) return;

            if (args.Length == 2)
            {
                p.Message("You need to specify the ID of the weapon.");
                return;
            }

            if (args.Length == 3)
            {
                p.Message("You need to specify a username to give the weapon to.");
                return;
            }

            Player who = PlayerInfo.FindExact(args[3]);
            if (who == null) return;

            who.Extras["PVP_UNLOCKED_" + args[2]] = true;

            who.Message("%eYou unlocked a weapon: %b" + args[2]);

            p.Message("Weapon given.");
        }

        void HandleTake(Player p, string[] args)
        {
            if (args[1] != PvP.Config.SecretCode) return;

            if (args.Length == 2)
            {
                p.Message("You need to specify the ID of the weapon.");
                return;
            }

            if (args.Length == 3)
            {
                p.Message("You need to specify a username to take the weapon from.");
                return;
            }

            Player who = PlayerInfo.FindExact(args[3]);
            if (who == null) return;

            who.Extras["PVP_UNLOCKED_" + args[2]] = false;

            p.Message("Weapon taken.");
            who.Message("%eYour lost your weapon: %b" + args[2]);
        }

        public override void Help(Player p)
        {
            p.Message("%T/Weapon add [id] [damage] [durability] %H- Adds a weapon to the current PvP map.");
            p.Message("%T/Weapon del %H- Removes a weapon current PvP map.");
            p.Message("%T/Weapon give [player] [world] [weapon] %H- Gives a player a weapon.");
            p.Message("%T/Weapon take [player] [world] %H- Takes all weapons from a player away.");
        }
    }

    public sealed class CmdTool : Command2
    {
        public override string name { get { return "Tool"; } }
        public override string type { get { return CommandTypes.Games; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public override bool SuperUseable { get { return false; } }
        public override CommandPerm[] ExtraPerms { get { return new[] { new CommandPerm(LevelPermission.Admin, "can manage tools") }; } }

        public override void Use(Player p, string message, CommandData data)
        {
            if (message.Length == 0)
            {
                Help(p);
                return;
            }
            string[] args = message.SplitSpaces(5);

            switch (args[0].ToLower())
            {
                case "add":
                    HandleAdd(p, args);
                    return;
                case "give":
                    HandleGive(p, args);
                    return;
                case "take":
                    HandleTake(p, args);
                    return;
            }
        }

        void HandleAdd(Player p, string[] args)
        {
            if (args.Length == 1)
            {
                p.Message("You need to specify an ID for the tool. E.g, '1' for stone.");
                return;
            }
            if (args.Length == 2)
            {
                p.Message("You need to specify the speed that the weapon mines at. E.g, '1' for normal speed, '2' for 2x speed.");
                return;
            }
            if (args.Length == 3)
            {
                p.Message("You need to specify how many clicks before it breaks. 0 for infinite clicks.");
                return;
            }
            if (args.Length == 4)
            {
                p.Message("%H[type] can be either 0 for none, 1 for axe, 2 for pickaxe, 3 for sword or 4 for shovel.");
                return;
            }

            for (int i = 0; i < 255; i++)
            {
                if (PvP.tools[i, 0] == null)
                {
                    PvP.tools[i, 0] = args[1];
                    PvP.tools[i, 1] = args[2];
                    PvP.tools[i, 2] = args[3];
                    PvP.tools[i, 3] = args[4];
                    createTool(args[1], args[2], args[3], args[4]);
                    break;
                }
            }
        }

        void createTool(string id, string damage, string durability, string type)
        {
            FileInfo filedir = new FileInfo(PvP.Config.Path + "tools.txt");
            filedir.Directory.Create();

            using (StreamWriter file = new StreamWriter(PvP.Config.Path + "tools.txt", true))
            {
                file.WriteLine(id + ";" + damage + ";" + durability + ";" + type);
            }
        }

        void HandleGive(Player p, string[] args)
        {
            if (args.Length == 1)
            {
                p.Message("You need to specify a username to give the tool to.");
                return;
            }
            if (args.Length == 2)
            {
                p.Message("You need to specify the world to allow them to use the tool on.");
                return;
            }
            if (args.Length == 3)
            {
                p.Message("You need to specify the name of the tool.");
                return;
            }

            string filepath = PvP.Config.Path + "tools/" + args[2] + "/" + args[1] + ".txt";
            FileInfo filedir = new FileInfo(filepath);
            filedir.Directory.Create();
            using (StreamWriter file = new StreamWriter(filepath, true))
            {
                file.WriteLine(args[3]);
            }
        }

        void HandleTake(Player p, string[] args)
        {
            Player[] online = PlayerInfo.Online.Items;
            foreach (Player pl in online)
            {
                if (pl.truename == args[1])
                {
                    string filepath = PvP.Config.Path + "tools/" + args[2] + "/" + args[1] + ".txt";

                    if (File.Exists(filepath))
                    {
                        File.WriteAllText(filepath, string.Empty);
                    }
                }
            }
        }

        public override void Help(Player p)
        {
            p.Message("%T/Tool add [id] [speed] [durability] [type] %H- Adds an tool to the current PvP map.");
            p.Message("%T/Tool del %H- Removes an tool from current PvP map.");
            p.Message("%T/Tool give [player] [world] [tool] %H- Gives a player an tool.");
            p.Message("%T/Tool take [player] [world] %H- Takes all tools from a player away.");
        }
    }

    public sealed class CmdBlock : Command2
    {
        public override string name { get { return "Block"; } }
        public override string type { get { return CommandTypes.Games; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public override bool SuperUseable { get { return false; } }
        public override CommandPerm[] ExtraPerms { get { return new[] { new CommandPerm(LevelPermission.Admin, "can manage blocks") }; } }

        public override void Use(Player p, string message, CommandData data)
        {
            if (message.Length == 0)
            {
                Help(p);
                return;
            }
            string[] args = message.SplitSpaces(5);

            switch (args[0].ToLower())
            {
                case "add":
                    HandleAdd(p, args);
                    return;
            }
        }

        void HandleAdd(Player p, string[] args)
        {
            if (args.Length == 1)
            {
                p.Message("You need to specify an ID for the block. E.g, '1' for stone.");
                return;
            }
            if (args.Length == 2)
            {
                p.Message("You need to specify the tool that makes mining faster. %T[tool] can be either 0 for none, 1 for axe, 2 for pickaxe, 3 for sword or 4 for shovel.");
                return;
            }
            if (args.Length == 3)
            {
                p.Message("You need to specify how many clicks before it breaks. 0 for infinite clicks.");
                return;
            }

            for (int i = 0; i < 255; i++)
            {
                if (PvP.blocks[i, 0] == null)
                {
                    PvP.blocks[i, 0] = args[1];
                    PvP.blocks[i, 1] = args[2];
                    PvP.blocks[i, 2] = args[3];
                    createBlock(args[1], args[2], args[3]);
                    break;
                }
            }
        }

        void createBlock(string id, string tool, string durability)
        {
            FileInfo filedir = new FileInfo(PvP.Config.Path + "blocks.txt");
            filedir.Directory.Create();

            using (StreamWriter file = new StreamWriter(PvP.Config.Path + "blocks.txt", true))
            {
                file.WriteLine(id + ";" + tool + ";" + durability);
            }
        }

        public override void Help(Player p)
        {
            p.Message("%T/Block add [id] [tool] [durability] %H- Adds a block to the current PvP map.");
            p.Message("%H[tool] can be either 0 for none, 1 for axe, 2 for pickaxe, 3 for sword or 4 for shovel.");
        }
    }

    public sealed class CmdPotion : Command2
    {
        public override string name { get { return "Potion"; } }
        public override bool SuperUseable { get { return false; } }
        public override string type { get { return "fun"; } }

        public override void Use(Player p, string message, CommandData data)
        {
            p.lastCMD = "Secret";

            string[] args = message.SplitSpaces(3);

            List<string[]> rows = Database.GetRows("Potions", "*", "WHERE Name=@0", p.truename);

            if (args[0].Length == 0)
            {
                p.Message("You need to specify a potion to use.");
                p.Message("%T/Potion health %b- Sets your health to full.");
                p.Message("%T/Potion speed %b- Gives you a 3.75x speed boost for 3 minutes.");
                p.Message("%T/Potion jump %b- Gives you a 4x jump boost for 3 minutes.");
                p.Message("%T/Potion invisible %b- Makes you invisible for 10 seconds.");
            }
            else
            {
                List<string[]> pRows = Database.GetRows("Potions", "*", "WHERE Name=@0", p.truename);
                if (args[0] == PvP.Config.SecretCode && args.Length >= 3)
                { // Used for getting potions
                    string item = args[1].ToLower();
                    int quantity = Int32.Parse(args[2]);

                    if (pRows.Count == 0)
                    {
                        if (item == "health") Database.AddRow("Potions", "Name, Health, Speed, Invisible, Jump, Waterbreathing, Strength, Slowness, Blindness", p.truename, quantity, 0, 0, 0, 0, 0, 0, 0);
                        if (item == "speed") Database.AddRow("Potions", "Name, Health, Speed, Invisible, Jump, Waterbreathing, Strength, Slowness, Blindness", p.truename, 0, quantity, 0, 0, 0, 0, 0, 0);
                        if (item == "invisible") Database.AddRow("Potions", "Name, Health, Speed, Invisible, Jump, Waterbreathing, Strength, Slowness, Blindness", p.truename, 0, 0, quantity, 0, 0, 0, 0, 0);
                        if (item == "jump") Database.AddRow("Potions", "Name, Health, Speed, Invisible, Jump, Waterbreathing, Strength, Slowness, Blindness", p.truename, 0, 0, 0, quantity, 0, 0, 0, 0);

                        p.Message("You now have: %b" + quantity + " %S" + item + " potions");
                        return;
                    }
                    else
                    {
                        int h = int.Parse(pRows[0][1]);
                        int s = int.Parse(pRows[0][2]);
                        int i = int.Parse(pRows[0][3]);
                        int j = int.Parse(pRows[0][4]);

                        int newH = quantity + h;
                        int newS = quantity + s;
                        int newI = quantity + i;
                        int newJ = quantity + j;

                        if (item == "health")
                        {
                            Database.UpdateRows("Potions", "Health=@1", "WHERE NAME=@0", p.truename, newH);
                            p.Message("You now have: %b" + newH + " %S" + item + " potions");
                        }

                        if (item == "speed")
                        {
                            Database.UpdateRows("Potions", "Speed=@1", "WHERE NAME=@0", p.truename, newS);
                            p.Message("You now have: %b" + newS + " %S" + item + " potions");
                        }

                        if (item == "invisible")
                        {
                            Database.UpdateRows("Potions", "Invisible=@1", "WHERE NAME=@0", p.truename, newI);
                            p.Message("You now have: %b" + newI + " %S" + item + " potions");
                        }

                        if (item == "jump")
                        {
                            Database.UpdateRows("Potions", "Jump=@1", "WHERE NAME=@0", p.truename, newJ);
                            p.Message("You now have: %b" + newJ + " %S" + item + " potions");
                        }
                        return;
                    }
                }

                if (args[0] == "list")
                {
                    if (pRows.Count == 0)
                    {
                        p.Message("%SYou do not have any potions.");
                        return;
                    }
                    int h = int.Parse(rows[0][1]);
                    int s = int.Parse(rows[0][2]);
                    int i = int.Parse(rows[0][3]);
                    int j = int.Parse(rows[0][4]);

                    p.Message("%aYour potions:");
                    p.Message("%7Health %ex{0}%7, Speed %ex{1}%7, Invisible %ex{2}%7, Jump %ex{3}", h, s, i, j);
                }

                if (args[0] == "health")
                {
                    if (pRows.Count == 0)
                    {
                        p.Message("%SYou do not have any potions.");
                        return;
                    }
                    int h = int.Parse(rows[0][1]);
                    if (h == 0)
                    {
                        p.Message("You don't have any health potions.");
                        return;
                    }

                    // Use potion
                    Database.UpdateRows("Potions", "Health=@1", "WHERE NAME=@0", p.truename, h - 1);
                    Command.Find("PvP").Use(p, "sethp " + PvP.Config.SecretCode + " " + p.truename + " 20");
                    p.Message("Your health has been replenished.");
                    p.Message("You have " + (h - 1) + " health potions remaining.");
                }

                if (args[0] == "speed")
                {
                    if (pRows.Count == 0)
                    {
                        p.Message("%SYou do not have any potions.");
                        return;
                    }
                    int s = int.Parse(rows[0][2]);
                    if (s == 0)
                    {
                        p.Message("You don't have any speed potions.");
                        return;
                    }

                    // Use potion
                    Database.UpdateRows("Potions", "Speed=@1", "WHERE NAME=@0", p.truename, s - 1);
                    p.Extras["POTION_IS_FAST"] = true;
                    p.Extras["POTION_SPEED_TIMER"] = DateTime.UtcNow;
                    p.Message("You have " + (s - 1) + " speed potions remaining.");
                    Server.MainScheduler.QueueRepeat(PvP.CheckSpeed, null, TimeSpan.FromMilliseconds(10));
                }

                if (args[0] == "invisible")
                {
                    if (pRows.Count == 0)
                    {
                        p.Message("%SYou do not have any potions.");
                        return;
                    }
                    int i = int.Parse(rows[0][3]);
                    if (i == 0)
                    {
                        p.Message("You don't have any invisible potions.");
                        return;
                    }

                    // Use potion
                    Database.UpdateRows("Potions", "Invisible=@1", "WHERE NAME=@0", p.truename, i - 1);
                    p.Extras["POTION_IS_INVISIBLE"] = true;

                    Entities.GlobalDespawn(p, true); // Remove from tab list
                    Server.hidden.Add(p.name);
                    p.Extras["POTION_INV_TIMER"] = DateTime.UtcNow;
                    p.Message("%aYou are now invisible.");
                    p.Message("You have " + (i - 1) + " invisible potions remaining.");
                    Server.MainScheduler.QueueRepeat(PvP.CheckInvisible, null, TimeSpan.FromSeconds(1));
                }

                if (args[0] == "jump")
                {
                    if (pRows.Count == 0)
                    {
                        p.Message("%SYou do not have any potions.");
                        return;
                    }
                    int j = int.Parse(rows[0][4]);
                    if (j == 0)
                    {
                        p.Message("You don't have any jump potions.");
                        return;
                    }

                    // Use potion
                    Database.UpdateRows("Potions", "Jump=@1", "WHERE NAME=@0", p.truename, j - 1);
                    p.Extras["POTION_IS_JUMP"] = true;
                    p.Extras["POTION_JUMP_TIMER"] = DateTime.UtcNow;
                    p.Message("You have " + (j - 1) + " jump potions remaining.");
                    Server.MainScheduler.QueueRepeat(PvP.CheckJump, null, TimeSpan.FromMilliseconds(10));
                }
            }
        }

        public override void Help(Player p) { }
    }

    public sealed class CmdDropBlock : Command2
    {
        public override string name { get { return "DropBlock"; } }
        public override string shortcut { get { return "db"; } }
        public override string type { get { return CommandTypes.Building; } }
        public override LevelPermission defaultRank { get { return LevelPermission.AdvBuilder; } }
        public override bool SuperUseable { get { return false; } }

        private readonly Random _random = new Random();

        public int RandomNumber(int min, int max)
        {
            return _random.Next(min, max);
        }

        void AddBot(Player p, string botName)
        {
            botName = botName.Replace(' ', '_');
            PlayerBot bot = new PlayerBot(botName, p.level);
            bot.Owner = p.name;
            TryAddBot(p, bot);
        }

        void TryAddBot(Player p, PlayerBot bot)
        {
            if (BotExists(p.level, bot.name, null))
            {
                p.Message("A bot with that name already exists.");
                return;
            }
            if (p.level.Bots.Count >= Server.Config.MaxBotsPerLevel)
            {
                p.Message("Reached maximum number of bots allowed on this map.");
                return;
            }

            bot.SetInitialPos(p.Pos);
            bot.SetYawPitch(p.Rot.RotY, 0);
            PlayerBot.Add(bot);
        }

        static bool BotExists(Level lvl, string name, PlayerBot skip)
        {
            PlayerBot[] bots = lvl.Bots.Items;
            foreach (PlayerBot bot in bots)
            {
                if (bot == skip) continue;
                if (bot.name.CaselessEq(name)) return true;
            }
            return false;
        }

        static string ParseModel(Player dst, Entity e, string model)
        {
            // Reset entity's model
            if (model.Length == 0)
            {
                e.ScaleX = 0;
                e.ScaleY = 0;
                e.ScaleZ = 0;
                return "humanoid";
            }

            model = model.ToLower();
            model = model.Replace(':', '|'); // Since users assume : is for scale instead of |.

            float max = ModelInfo.MaxScale(e, model);
            // Restrict player model scale, but bots can have unlimited model scale
            if (ModelInfo.GetRawScale(model) > max)
            {
                dst.Message("%WScale must be {0} or less for {1} model",
                    max, ModelInfo.GetRawModel(model));
                return null;
            }
            return model;
        }

        public override void Use(Player p, string message, CommandData data)
        {
            if (!PvP.maplist.Contains(p.level.name)) return;
            Command.Find("SilentHold").Use(p, "air");
            p.lastCMD = "Secret";
            BlockID block = p.GetHeldBlock();
            string holding = Block.GetName(p, block);
            if (holding == "Air") return;
            string code = RandomNumber(1000, 1000000).ToString();
            AddBot(p, "block_" + code);
            PlayerBot bot = Matcher.FindBots(p, "block_" + code);
            bot.DisplayName = "";

            bot.GlobalDespawn();
            bot.GlobalSpawn();

            BotsFile.Save(p.level);

            // Convert blocks over ID 65
            int convertedBlock = block;
            if (convertedBlock >= 66) convertedBlock = block - 256; // Need to convert block if ID is over 66

            string model = ParseModel(p, bot, convertedBlock + "|0.5");
            if (model == null) return;
            bot.UpdateModel(model);
            bot.ClickedOnText = "/pickupblock " + code + " " + convertedBlock;
            if (!ScriptFile.Parse(p, bot, "spin")) return;
            BotsFile.Save(p.level);
        }

        public override void Help(Player p)
        {
            p.Message("%T/DropBlock - %HDrops a block at your feet.");
        }
    }

    public sealed class CmdPickupBlock : Command2
    {
        public override string name { get { return "PickupBlock"; } }
        public override string shortcut { get { return "pickup"; } }
        public override string type { get { return CommandTypes.Building; } }
        public override LevelPermission defaultRank { get { return LevelPermission.AdvBuilder; } }
        public override bool SuperUseable { get { return false; } }

        public override void Use(Player p, string message, CommandData data)
        {
            // /PickupBlock [bot name] [block]
            if (!PvP.maplist.Contains(p.level.name)) return;
            if (message.Length == 0) return;
            string[] args = message.SplitSpaces(2);

            if (p.Supports(CpeExt.HeldBlock)) Command.Find("SilentHold").Use(p, args[1]);

            p.lastCMD = "Secret";
            PlayerBot bot = Matcher.FindBots(p, "block_" + args[0]);
            PlayerBot.Remove(bot);

            // TODO: Add blocks to inventory
        }

        public override void Help(Player p) { }
    }

    #endregion
}
