// To add: Set map MOTD to include +hold then every block you change to will update your model.
// E.g, /map motd +hold

using System;
using System.Collections.Generic;
using System.IO;

using MCGalaxy;
using MCGalaxy.Bots;
using MCGalaxy.Commands;
using MCGalaxy.Commands.CPE;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Events;
using MCGalaxy.Tasks;

using BlockID = System.UInt16;

namespace Core {
    public class HoldBlocks : Plugin_Simple {
        public static PlayerExtList dailyList;
        
        public override string creator { get { return "Venk"; } }
        public override string MCGalaxy_Version { get { return "1.9.2.8"; } }
        public override string name { get { return "HoldBlocks"; } }
        
        public static SchedulerTask Task;

        public override void Load(bool startup) {
            OnPlayerConnectEvent.Register(HandlePlayerConnect, Priority.High);
            Command.Register(new CmdSilentModel());
            Server.MainScheduler.QueueRepeat(DoBlockLoop, null, TimeSpan.FromMilliseconds(100));
        }

        public override void Unload(bool shutdown) {
            OnPlayerConnectEvent.Unregister(HandlePlayerConnect);
            Command.Unregister(Command.Find("SilentModel"));
            Server.MainScheduler.Cancel(Task);
        }
        
        void DoBlockLoop(SchedulerTask task) {
        	Player[] players = PlayerInfo.Online.Items;

            foreach (Player pl in players) {
        	    // Get MOTD of map
                LevelConfig cfg = LevelInfo.GetConfig(pl.level.name, out pl.level);
                if (!cfg.MOTD.ToLower().Contains("+hold")) break;
        	    BlockID block = pl.GetHeldBlock();
                string holding = Block.GetName(pl, block);
                
        	    if (pl.Extras.GetString("HOLDING_BLOCK") != holding) {
        	        int scale = block;
        	        if (scale >= 66) scale = block - 256; // Need to convert block if ID is over 66
        	        if (scale >= 100) Command.Find("SilentModel").Use(pl, "-own hold|1." + scale); 
        	        else if (scale >= 10) Command.Find("SilentModel").Use(pl, "-own hold|1.0" + scale);
        	        else if (scale > 0) Command.Find("SilentModel").Use(pl, "-own hold|1.00" + scale);
        	        else Command.Find("SilentModel").Use(pl, "-own humanoid|1");
        	    }
                pl.Extras["HOLDING_BLOCK"] = holding;
            }
        	
        	Task = task;
        }

        void HandlePlayerConnect(Player p) {
            p.Extras["HOLDING_BLOCK"] = null;
        }
    }
    
    public class CmdSilentModel : EntityPropertyCmd {
        public override string name { get { return "SilentModel"; } }
        public override string type { get { return CommandTypes.Other; } }
        public override LevelPermission defaultRank { get { return LevelPermission.AdvBuilder; } }
        public override CommandPerm[] ExtraPerms {
            get { return new[] { new CommandPerm(LevelPermission.Operator, "can change the model of others") }; }
        }

        public override void Use(Player p, string message, CommandData data) {
            if (message.IndexOf(' ') == -1) {
                message = "-own " + message;
                message = message.TrimEnd();
            }
            UseBotOrOnline(p, data, message, "model");
        }
        
        protected override void SetBotData(Player p, PlayerBot bot, string model) {
            model = ParseModel(p, bot, model);
            if (model == null) return;
            bot.UpdateModel(model);
            
            p.Message("You changed the model of bot {0} %Sto a &c", bot.ColoredName, model);
            BotsFile.Save(p.level);
        }
        
        protected override void SetOnlineData(Player p, Player who, string model) {
            string orig = model;
            model = ParseModel(p, who, model);
            if (model == null) return;
            who.UpdateModel(model);
            
            if (!model.CaselessEq("humanoid")) {
                Server.models.Update(who.name, model);
            } else {
                Server.models.Remove(who.name);
            }
            Server.models.Save();
            
            // Remove model scale too when resetting model
            //if (orig.Length == 0) CmdModelScale.UpdateSavedScale(who);
        }
        
        static string ParseModel(Player dst, Entity e, string model) {
            // Reset entity's model
            if (model.Length == 0) {
                e.ScaleX = 0; e.ScaleY = 0; e.ScaleZ = 0;
                return "humanoid";
            }
            
            model = model.ToLower();
            model = model.Replace(':', '|'); // since users assume : is for scale instead of |.
            
            float max = ModelInfo.MaxScale(e, model);
            // restrict player model scale, but bots can have unlimited model scale
            if (ModelInfo.GetRawScale(model) > max) {
                dst.Message("%WScale must be {0} or less for {1} model",
                            max, ModelInfo.GetRawModel(model));
                return null;
            }
            return model;
        }

        public override void Help(Player p) {}
    }
}
