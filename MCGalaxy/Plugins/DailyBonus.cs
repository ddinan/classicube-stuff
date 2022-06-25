using System;
using System.Collections.Generic;
using System.IO;

using MCGalaxy;
using MCGalaxy.Commands;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Events;

namespace Core
{
    public class DailyBonus : Plugin
    {
        public static PlayerExtList dailyList;

        public override string creator { get { return "Venk"; } }
        public override string MCGalaxy_Version { get { return "1.9.1.2"; } }
        public override string name { get { return "DailyBonus"; } }

        public static bool AutoReward = false; // Reward users automoatically on join (true) or force them to type /daily (false)
        public static int amount = 5; // The amount given to the player
        public override void Load(bool startup)
        {
            dailyList = PlayerExtList.Load("text/dailybonus.txt"); // Load the list so we can start using it
            if (AutoReward) OnPlayerConnectEvent.Register(HandlePlayerConnect, Priority.High);
            else Command.Register(new CmdDailyBonus());
        }

        public override void Unload(bool shutdown)
        {
            if (AutoReward) OnPlayerConnectEvent.Unregister(HandlePlayerConnect);
            else Command.Unregister(Command.Find("DailyBonus"));
        }

        void HandlePlayerConnect(Player p)
        {
            string date = DateTime.UtcNow.ToShortDateString();
            string lastDate = dailyList.FindData(p.name);

            if (lastDate == null || lastDate != date)
            { // Check if they've already claimed their bonus
              // Add the player's current date to the list
                dailyList.AddOrReplace(p.name, date);
                dailyList.Save();

                p.Message("%SYou claimed your daily bonus of &b" + amount + " %S" + Server.Config.Currency + "%S.");
                p.SetMoney(p.money + amount);
            }

            if (lastDate == date)
            {
                p.Message("%cYou have already claimed your daily bonus for today.");
            }
        }
    }

    public sealed class CmdDailyBonus : Command2
    {
        public override string name { get { return "DailyBonus"; } }
        public override bool SuperUseable { get { return false; } }
        public override CommandAlias[] Aliases { get { return new[] { new CommandAlias("Daily") }; } }
        public override string type { get { return CommandTypes.Economy; } }

        public override void Use(Player p, string message, CommandData data)
        {
            string date = DateTime.UtcNow.ToShortDateString();
            string lastDate = DailyBonus.dailyList.FindData(p.name);

            if (lastDate == null || lastDate != date)
            { // Check if they've already claimed their bonus
              // Add the player's current date to the list
                DailyBonus.dailyList.AddOrReplace(p.name, date);
                DailyBonus.dailyList.Save();

                p.Message("%SYou claimed your daily bonus of &b" + DailyBonus.amount + " %S" + Server.Config.Currency + "%S.");
                p.SetMoney(p.money + DailyBonus.amount);
            }

            if (lastDate == date)
            {
                p.Message("%cYou have already claimed your daily bonus for today.");
            }
        }

        public override void Help(Player p)
        {
            p.Message("%T/DailyBonus - %HClaims your daily bonus for today.");
        }
    }
}