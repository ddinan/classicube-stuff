using System;
using System.Collections.Generic;
using System.IO;

using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Events;

namespace Core {
    public class DailyBonus : Plugin_Simple {
        public static PlayerExtList dailyList;
        
        public override string creator { get { return "VenkSociety"; } }
        public override string MCGalaxy_Version { get { return "1.9.1.2"; } }
        public override string name { get { return "DailyBonus"; } }

        public override void Load(bool startup) {
            OnPlayerConnectEvent.Register(HandlePlayerConnect, Priority.High);
            dailyList = PlayerExtList.Load("text/dailybonus.txt");
        }

        public override void Unload(bool shutdown) {
            OnPlayerConnectEvent.Unregister(HandlePlayerConnect);
        }

        void HandlePlayerConnect(Player p) {
            string date = DateTime.UtcNow.ToShortDateString();
            string lastDate = dailyList.FindData(p.name);

            if (lastDate == null || lastDate != date) {
                dailyList.AddOrReplace(p.name, date);
                dailyList.Save();

                p.Message("%SYou claimed your daily bonus of &b5 %S" + Server.Config.Currency + "%S.");
                p.SetMoney(p.money + 5);
            }
        }
    }
}
