using System;
using System.Threading;

using MCGalaxy;
using MCGalaxy.Tasks;

namespace MCGalaxy {

    public class Compass : Plugin {
        public override string creator { get { return "Venk"; } }
        public override string MCGalaxy_Version { get { return "1.9.1.2"; } }
        public override string name { get { return "Compass"; } }

        public override void Load(bool startup) {
            Server.MainScheduler.QueueRepeat(CheckDirection, null, TimeSpan.FromMilliseconds(100));
        }

        void CheckDirection(SchedulerTask task) {
            Player[] players = PlayerInfo.Online.Items;
            foreach (Player p in players) {
                if (Orientation.PackedToDegrees(p.Rot.RotY) >= 0 && Orientation.PackedToDegrees(p.Rot.RotY) < 45) {
                    p.SendCpeMessage(CpeMessageType.Status1, "%SFacing:");
                    p.SendCpeMessage(CpeMessageType.Status2, "%bNorth");
                }
                
                if (Orientation.PackedToDegrees(p.Rot.RotY) >= 45 && Orientation.PackedToDegrees(p.Rot.RotY) < 90) {
                    p.SendCpeMessage(CpeMessageType.Status1, "%SFacing:");
                    p.SendCpeMessage(CpeMessageType.Status2, "%bNortheast");
                }
                
                if (Orientation.PackedToDegrees(p.Rot.RotY) >= 90 && Orientation.PackedToDegrees(p.Rot.RotY) < 135) {
                    p.SendCpeMessage(CpeMessageType.Status1, "%SFacing:");
                    p.SendCpeMessage(CpeMessageType.Status2, "%bEast");
                }
                
                if (Orientation.PackedToDegrees(p.Rot.RotY) >= 135 && Orientation.PackedToDegrees(p.Rot.RotY) < 180) {
                    p.SendCpeMessage(CpeMessageType.Status1, "%SFacing:");
                    p.SendCpeMessage(CpeMessageType.Status2, "%bSoutheast");
                }
                
                if (Orientation.PackedToDegrees(p.Rot.RotY) >= 180 && Orientation.PackedToDegrees(p.Rot.RotY) < 225) {
                    p.SendCpeMessage(CpeMessageType.Status1, "%SFacing:");
                    p.SendCpeMessage(CpeMessageType.Status2, "%bSouth");
                }
                
                if (Orientation.PackedToDegrees(p.Rot.RotY) >= 225 && Orientation.PackedToDegrees(p.Rot.RotY) < 270) {
                    p.SendCpeMessage(CpeMessageType.Status1, "%SFacing:");
                    p.SendCpeMessage(CpeMessageType.Status2, "%bSouthwest");
                }
                
                if (Orientation.PackedToDegrees(p.Rot.RotY) >= 270 && Orientation.PackedToDegrees(p.Rot.RotY) < 315) {
                    p.SendCpeMessage(CpeMessageType.Status1, "%SFacing:");
                    p.SendCpeMessage(CpeMessageType.Status2, "%bWest");
                }
                
                if (Orientation.PackedToDegrees(p.Rot.RotY) >= 315 && Orientation.PackedToDegrees(p.Rot.RotY) < 361) {
                    p.SendCpeMessage(CpeMessageType.Status1, "%SFacing:");
                    p.SendCpeMessage(CpeMessageType.Status2, "%bNorthwest");
                }
            }
        }

        public override void Unload(bool shutdown) {}
    }
}
