using System;
using System.IO;
using System.Threading;

using MCGalaxy.Bots;
using MCGalaxy.Maths;
using MCGalaxy.Network;

namespace MCGalaxy
{

    public sealed class MobAI : Plugin
    {
        BotInstruction hostile;
        BotInstruction roam;
        BotInstruction smart;
        BotInstruction spleef;

        public override string name { get { return "RoamAI"; } }
        public override string MCGalaxy_Version { get { return "1.9.3.1"; } }
        public override string creator { get { return "Venk"; } }

        public override void Load(bool startup)
        {
            hostile = new HostileInstruction();
            roam = new RoamInstruction();
            smart = new SmartInstruction();
            spleef = new SpleefInstruction();

            BotInstruction.Instructions.Add(hostile);
            BotInstruction.Instructions.Add(roam);
            BotInstruction.Instructions.Add(smart);
            BotInstruction.Instructions.Add(spleef);
        }

        public override void Unload(bool shutdown)
        {
            BotInstruction.Instructions.Remove(hostile);
            BotInstruction.Instructions.Remove(roam);
            BotInstruction.Instructions.Remove(smart);
            BotInstruction.Instructions.Remove(spleef);
        }
    }

    public sealed class Metadata { public int waitTime; public int walkTime; public int explodeTime; }

    #region Hostile AI

    /* 
        Current AI behaviour:
        
        -   Chase player if within 12 block range
        -   Hit player if too close
        -   Assign movement speed based on mob model
        -   Explode if mob is a creeper


        -   50% chance to stand still (moving when 0-2, still when 3-5)
        -   If not moving, wait for waitTime duration before executing next task
        -   Choose random coord within 8x8 block radius of player and try to go to it
        -   Do action for walkTime duration
        
     */

    sealed class HostileInstruction : BotInstruction
    {
        public HostileInstruction() { Name = "hostile"; }

        internal static Player ClosestPlayer(PlayerBot bot, int search)
        {
            int maxDist = search * 32;
            Player[] players = PlayerInfo.Online.Items;
            Player closest = null;

            foreach (Player p in players)
            {
                if (p.level != bot.level || p.invincible || p.hidden) continue;

                int dx = p.Pos.X - bot.Pos.X, dy = p.Pos.Y - bot.Pos.Y, dz = p.Pos.Z - bot.Pos.Z;
                int playerDist = Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz);
                if (playerDist >= maxDist) continue;

                closest = p;
                maxDist = playerDist;
            }
            return closest;
        }

        static bool MoveTowards(PlayerBot bot, Player p, Metadata meta)
        {
            if (p == null) return false;

            int dx = p.Pos.X - bot.Pos.X, dy = p.Pos.Y - bot.Pos.Y, dz = p.Pos.Z - bot.Pos.Z;
            bot.TargetPos = p.Pos;
            bot.movement = true;

            Vec3F32 dir = new Vec3F32(dx, dy, dz);
            dir = Vec3F32.Normalise(dir);
            Orientation rot = bot.Rot;
            DirUtils.GetYawPitch(dir, out rot.RotY, out rot.HeadX);

            dx = Math.Abs(dx); dy = Math.Abs(dy); dz = Math.Abs(dz);

            if (bot.Model == "creeper")
            {
                if (dx < (3 * 32) && dz < (3 * 32))
                {
                    if (meta.explodeTime == 0)
                    {
                        meta.explodeTime = 10;
                    }
                }
                else meta.explodeTime = 0;
            }

            else
            {
                if ((dx <= 8 && dy <= 16 && dz <= 8)) HitPlayer(bot, p, rot);
            }

            bot.Rot = rot;


            return dx <= 8 && dy <= 16 && dz <= 8;
        }

        public static void HitPlayer(PlayerBot bot, Player p, Orientation rot)
        {
            // Send player backwards if hit
            // Code "borrowed" from PvP plugin

            int srcHeight = ModelInfo.CalcEyeHeight(bot);
            int dstHeight = ModelInfo.CalcEyeHeight(p);
            int dx2 = bot.Pos.X - p.Pos.X, dy2 = (bot.Pos.Y + srcHeight) - (p.Pos.Y + dstHeight), dz2 = bot.Pos.Z - p.Pos.Z;

            Vec3F32 dir2 = new Vec3F32(dx2, dy2, dz2);

            if (dir2.Length > 0) dir2 = Vec3F32.Normalise(dir2);

            float mult = 1 / ModelInfo.GetRawScale(p.Model);
            float plScale = ModelInfo.GetRawScale(p.Model);

            float VelocityY = 1.0117f * mult;

            if (dir2.Length <= 0) VelocityY = 0;

            if (p.Supports(CpeExt.VelocityControl))
            {
                // Intensity of force is in part determined by model scale
                p.Send(Packet.VelocityControl((-dir2.X * mult) * 0.57f, VelocityY, (-dir2.Z * mult) * 0.57f, 0, 1, 0));
            }

            // If we are very close to a player, switch from trying to look
            // at them to just facing the opposite direction to them

            rot.RotY = (byte)(p.Rot.RotY + 128);
            bot.Rot = rot;
        }

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

            if (bot.Model == "skeleton" || bot.Model == "creeper") bot.movementSpeed = (int)Math.Round(3m * (short)97 / 100m);
            if (bot.Model == "zombie") bot.movementSpeed = (int)Math.Round(3m * (short)94 / 100m);

            if (bot.movementSpeed == 0) bot.movementSpeed = 1;

            int search = 12;

            Player closest = ClosestPlayer(bot, search);

            if (closest == null)
            {
                if (bot.Model == "creeper")
                {
                    meta.explodeTime = 0;
                }

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
            }

            else
            {
                if (bot.Model == "creeper")
                {
                    if (meta.explodeTime > 0)
                    {
                        meta.explodeTime--;

                        if (meta.explodeTime == 1)
                        {
                            if (closest.level.physics > 1 && closest.level.physics != 5) closest.level.MakeExplosion((ushort)(bot.Pos.X / 32), (ushort)(bot.Pos.Y / 32), (ushort)(bot.Pos.Z / 32), 0);
                            Command.Find("Effect").Use(closest, "explosion " + (bot.Pos.X / 32) + " " + (bot.Pos.Y / 32) + " " + (bot.Pos.Z / 32) + " 0 0 0 true");

                            Orientation rot = bot.Rot;

                            HitPlayer(bot, closest, rot);
                            meta.explodeTime = 0;
                            PlayerBot.Remove(bot);
                            return true;
                        }

                        bot.movement = true;
                        return true;
                    }
                }
            }

            bool overlapsPlayer = MoveTowards(bot, closest, meta);
            if (overlapsPlayer && closest != null) { bot.NextInstruction(); return false; }


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
            "%T/BotAI add [name] hostile",
            "%HCauses the bot behave as a hostile mob.",
        };
    }

    #endregion

    #region Roam AI

    /* 
        Current AI behaviour:
        
        -   50% chance to stand still (moving when 0-2, still when 3-5)
        -   If not moving, wait for waitTime duration before executing next task
        -   Choose random coord within 8x8 block radius of player and try to go to it
        -   Do action for walkTime duration
        
     */

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

    #endregion

    #region Smart AI

    /* 
        Current AI behaviour:
        
        -   Chase player if within 12 block range
        -   Hit player if too close
        -   Assign movement speed based on mob model
        -   Explode if mob is a creeper


        -   50% chance to stand still (moving when 0-2, still when 3-5)
        -   If not moving, wait for waitTime duration before executing next task
        -   Choose random coord within 8x8 block radius of player and try to go to it
        -   Do action for walkTime duration
        
     */

    sealed class SmartInstruction : BotInstruction
    {
        public SmartInstruction() { Name = "smart"; }

        internal static Player ClosestPlayer(PlayerBot bot, int search)
        {
            int maxDist = search * 32;
            Player[] players = PlayerInfo.Online.Items;
            Player closest = null;

            foreach (Player p in players)
            {
                if (p.level != bot.level || p.invincible || p.hidden) continue;

                int dx = p.Pos.X - bot.Pos.X, dy = p.Pos.Y - bot.Pos.Y, dz = p.Pos.Z - bot.Pos.Z;
                int playerDist = Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz);
                if (playerDist >= maxDist) continue;

                closest = p;
                maxDist = playerDist;
            }
            return closest;
        }

        public static bool InRange(Player a, PlayerBot b, int dist)
        {
            int dx = Math.Abs(a.Pos.X - b.Pos.X);
            int dy = Math.Abs(a.Pos.Y - b.Pos.Y);
            int dz = Math.Abs(a.Pos.Z - b.Pos.Z);
            return dx <= dist && dy <= dist && dz <= dist;
        }

        static bool MoveTowards(PlayerBot bot, Player p, Metadata meta)
        {
            if (p == null) return false;
            int dist = (int)(0.875 * 32);

            int dx = p.Pos.X - bot.Pos.X, dy = p.Pos.Y - bot.Pos.Y, dz = p.Pos.Z - bot.Pos.Z;
            bot.TargetPos = p.Pos;
            bot.movement = true;

            Vec3F32 dir = new Vec3F32(dx, dy, dz);
            dir = Vec3F32.Normalise(dir);
            Orientation rot = bot.Rot;
            DirUtils.GetYawPitch(dir, out rot.RotY, out rot.HeadX);

            dx = Math.Abs(dx); dy = Math.Abs(dy); dz = Math.Abs(dz);
            if (InRange(p, bot, dist)) p.Message("%cInfect");

            bot.Rot = rot;


            return dx <= 8 && dy <= 16 && dz <= 8;
        }

        private readonly Random _random = new Random();

        public int RandomNumber(int min, int max)
        {
            return _random.Next(min, max);
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

            Player closest = ClosestPlayer(bot, 30);

            if (closest == null)
            {
                bot.movement = false;
                bot.NextInstruction();
            }

            bool overlapsPlayer = MoveTowards(bot, closest, meta);
            if (overlapsPlayer && closest != null) { bot.NextInstruction(); return false; }


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
            "%T/BotAI add [name] smart",
            "%HCauses the bot behave as a smart mob.",
        };
    }

    #endregion

    #region Spleef AI

    /* 
        Current AI behaviour:
        
        -   Chase player if within 50 block range
        -   Delete block below the player if within 5 block range


        -   50% chance to not delete a block (deleting when 0-2, not deleting when 3-5) (partial CPS simulation?)
        -   If not moving, wait for waitTime duration before executing next task
        -   Choose random coord within 8x8 block radius of player and try to go to it
        -   Do action for walkTime duration
        
     */

    sealed class SpleefInstruction : BotInstruction
    {
        public SpleefInstruction() { Name = "spleef"; }

        internal static Player ClosestPlayer(PlayerBot bot, int search)
        {
            int maxDist = search * 32;
            Player[] players = PlayerInfo.Online.Items;
            Player closest = null;

            foreach (Player p in players)
            {
                if (p.level != bot.level || p.invincible || p.hidden) continue;

                int dx = p.Pos.X - bot.Pos.X, dy = p.Pos.Y - bot.Pos.Y, dz = p.Pos.Z - bot.Pos.Z;
                int playerDist = Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz);
                if (playerDist >= maxDist) continue;

                closest = p;
                maxDist = playerDist;
            }
            return closest;
        }

        public static bool InRange(Player a, PlayerBot b, int dist)
        {
            int dx = Math.Abs(a.Pos.X - b.Pos.X);
            int dy = Math.Abs(a.Pos.Y - b.Pos.Y);
            int dz = Math.Abs(a.Pos.Z - b.Pos.Z);
            return dx <= dist && dy <= dist && dz <= dist;
        }

        static int lastY = 0;

        static bool MoveTowards(PlayerBot bot, Player p, Metadata meta)
        {
            if (p == null) return false;
            int dist = (int)(0.875 * 32);

            int dx = p.Pos.X - bot.Pos.X, dy = p.Pos.Y - bot.Pos.Y, dz = p.Pos.Z - bot.Pos.Z;
            bot.TargetPos = p.Pos;
            bot.movement = true;

            Vec3F32 dir = new Vec3F32(dx, dy, dz);
            dir = Vec3F32.Normalise(dir);
            Orientation rot = bot.Rot;
            DirUtils.GetYawPitch(dir, out rot.RotY, out rot.HeadX);

            dx = Math.Abs(dx); dy = Math.Abs(dy); dz = Math.Abs(dz);
            //if (InRange(p, bot, dist)) p.Message("%cInfect");

            bot.Rot = rot;

            if (dx < (5 * 32) && dz < (5 * 32)) // 5 block reach
            {
                Random rnd = new Random();
                // This code serves as a sort of 'CPS mechanism' to ensure that the bot does not perfectly delete every single block
                int chance = rnd.Next(0, 4); // 33% chance of deleting the block
                if (chance < 3)
                {
                    p.level.UpdateBlock(p, (ushort)(p.Pos.X / 32), (ushort)((p.Pos.Y / 32) - 2), (ushort)(p.Pos.Z / 32), Block.Air);

                    if ((p.Pos.Y / 32) > lastY) p.level.UpdateBlock(p, (ushort)(p.Pos.X / 32), (ushort)((p.Pos.Y / 32) - 3), (ushort)(p.Pos.Z / 32), Block.Air);
                }

                lastY = (p.Pos.Y / 32);
            }

            return dx <= 8 && dy <= 16 && dz <= 8;
        }

        private readonly Random _random = new Random();

        public int RandomNumber(int min, int max)
        {
            return _random.Next(min, max);
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

            Player closest = ClosestPlayer(bot, 10);

            if (closest == null)
            {
                bot.movement = false;
                bot.NextInstruction();
            }

            bool overlapsPlayer = MoveTowards(bot, closest, meta);
            if (overlapsPlayer && closest != null) { bot.NextInstruction(); return false; }


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
            "%T/BotAI add [name] spleef",
            "%HCauses the bot to try and spleef you.",
        };
    }

    #endregion
}