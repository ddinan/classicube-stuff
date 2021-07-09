using System;
using System.IO;
using System.Threading;

using MCGalaxy.Bots;
using MCGalaxy.Maths;

namespace MCGalaxy
{

    public sealed class RoamAI : Plugin
    {
        BotInstruction ins;

        public override string name { get { return "RoamAI"; } }
        public override string MCGalaxy_Version { get { return "1.9.3.1"; } }
        public override string creator { get { return "Venk"; } }

        public override void Load(bool startup)
        {
            ins = new RoamInstruction();
            BotInstruction.Instructions.Add(ins);
        }

        public override void Unload(bool shutdown)
        {
            BotInstruction.Instructions.Remove(ins);
        }
    }

    /* 
        Current AI behaviour:
        
        -   50% chance to stand still (moving when 0-2, still when 3-5)
        -   If not moving, wait for waitTime duration before executing next task
        -   Choose random coord within 8x8 block radius of player and try to go to it
        -   Do action for walkTime duration
        
     */

    public sealed class Metadata { public int waitTime; public int walkTime; }

    sealed class RoamInstruction : BotInstruction
    {
        public RoamInstruction() { Name = "roam"; }

        private readonly Random _random = new Random();

        public int RandomNumber(int min, int max)
        {
            return _random.Next(min, max);
        }

        public void DoStuff(PlayerBot bot, Metadata meta)
        {
            int stillChance = RandomNumber(0, 5); // Chance for the NPC to stand still
            int walkTime = RandomNumber(4, 8) * 5; // Time in milliseconds to execute a task
            int waitTime = RandomNumber(2, 5) * 5; // Time in milliseconds to wait before executing the next task

            int dx = RandomNumber(bot.Pos.X - (8 * 32), bot.Pos.X + (8 * 32)); // Random X location on the map within a 8x8 radius of the bot for the it to walk towards.
            int dz = RandomNumber(bot.Pos.Z - (8 * 32), bot.Pos.Z + (8 * 32)); // Random Z location on the map within a 8x8 radius of the bot for the it to walk towards.

            if (stillChance > 2)
            {
                meta.walkTime = walkTime;
            }

            else
            {
                Coords target;
                target.X = dx;
                target.Y = bot.Pos.Y;
                target.Z = dz;
                target.RotX = bot.Rot.RotX;
                target.RotY = bot.Rot.RotY;
                bot.TargetPos = new Position(target.X, target.Y, target.Z);

                bot.movement = true;

                if (bot.Pos.BlockX == bot.TargetPos.BlockX && bot.Pos.BlockZ == bot.TargetPos.BlockZ)
                {
                    bot.SetYawPitch(target.RotX, target.RotY);
                    bot.movement = false;
                }

                bot.AdvanceRotation();

                FaceTowards(bot);

                meta.walkTime = walkTime;
                bot.movement = false;
                meta.waitTime = waitTime;
            }
        }

        static void FaceTowards(PlayerBot bot)
        {
            int dstHeight = ModelInfo.CalcEyeHeight(bot);

            int dx = (bot.TargetPos.X) - bot.Pos.X, dy = bot.Rot.RotY, dz = (bot.TargetPos.Z) - bot.Pos.Z;
            Vec3F32 dir = new Vec3F32(dx, dy, dz);
            dir = Vec3F32.Normalise(dir);

            Orientation rot = bot.Rot;
            DirUtils.GetYawPitch(dir, out rot.RotY, out rot.HeadX);
            bot.Rot = rot;
        }

        public override bool Execute(PlayerBot bot, InstructionData data)
        {
            Metadata meta = (Metadata)data.Metadata;

            if (meta.walkTime > 0)
            {
                meta.walkTime--;
                bot.movement = true;
                return true;
            }
            if (meta.waitTime > 0)
            {
                meta.waitTime--;
                return true;
            }

            DoStuff(bot, meta);

            bot.movement = false;
            bot.NextInstruction();
            return true;
        }

        public override InstructionData Parse(string[] args)
        {
            InstructionData data =
                default(InstructionData);
            data.Metadata = new Metadata();
            return data;
        }

        public void Output(Player p, string[] args, StreamWriter w)
        {
            if (args.Length > 3)
            {
                w.WriteLine(Name + " " + ushort.Parse(args[3]));
            }
            else
            {
                w.WriteLine(Name);
            }
        }

        struct Coords
        {
            public int X, Y, Z;
            public byte RotX, RotY;
        }

        public override string[] Help { get { return help; } }
        static string[] help = new string[] {
            "%T/BotAI add [name] roam",
            "%HCauses the bot behave freely.",
        };
    }
}