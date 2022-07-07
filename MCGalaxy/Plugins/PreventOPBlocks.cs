using System;
using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Games;

using BlockID = System.UInt16;

namespace PreventOPBlocks
{
    public sealed class PreventOPBlocks : Plugin
    {
        public override string creator { get { return "Venk"; } }
        public override string name { get { return "PreventOPBlocks"; } }
        public override string MCGalaxy_Version { get { return "1.9.4.1"; } }

        public override void Load(bool startup)
        {
            OnBlockChangingEvent.Register(HandleBlockChanged, Priority.Low);
        }

        public override void Unload(bool shutdown)
        {
            OnBlockChangingEvent.Unregister(HandleBlockChanged);
        }

        static bool OnGameMap(string map)
        {
            if (!ZSGame.Instance.Running) return false;
            if (map == ZSGame.Instance.Map.name) return true;
            return false;
        }

        void HandleBlockChanged(Player p, ushort x, ushort y, ushort z, BlockID block, bool placing, ref bool cancel)
        {
            if (OnGameMap(p.level.name) && placing)
            {
                if (p.level.Props[block].OPBlock)
                {
                    p.RevertBlock(x, y, z);
                    cancel = true;
                    return;
                }
            }
        }
    }
}
