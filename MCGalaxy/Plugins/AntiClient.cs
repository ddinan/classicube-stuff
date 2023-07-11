using System;
using System.Collections.Generic;
using MCGalaxy.Network;
using MCGalaxy;
using MCGalaxy.Events;
using MCGalaxy.Events.PlayerEvents;

namespace MCGalaxy {
    class CustomClassicProtocol : ClassicProtocol
    {
        public CustomClassicProtocol(INetSocket s) : base(s) { }

        protected override int HandlePacket(byte[] buffer, int offset, int left)
        {
            int processed = base.HandlePacket(buffer, offset, left);
            CheckClient();
            return processed;
        }

        void CheckClient()
        {
            if (appName != null && !appName.StartsWith("ClassiCube 1"))
            {
                // Log nothing to avoid logging
                player.Kick("Use a valid client! Use ClassiCube 1.x!");
            }
            else
            {
            }
        }
    }

    public class AntiClients : Plugin
    {
        public override string name => "AntiClients";
        public override string MCGalaxy_Version => "1.9.4.3";
        public override string creator => "UnknownShadow200 & happen3";

        private CustomClassicProtocol customProtocol;

        public override void Load(bool startup)
        {
            INetSocket.Protocols[Opcode.Handshake] = (socket) => {
                customProtocol = new CustomClassicProtocol(socket);
                return customProtocol;
            };

            OnPlayerFinishConnectingEvent.Register(KickInvalidClients, Priority.High);
        }

        public override void Unload(bool shutdown)
        {
            INetSocket.Protocols[Opcode.Handshake] = null;
            customProtocol = null;

            OnPlayerFinishConnectingEvent.Unregister(KickInvalidClients);
        }

        void KickInvalidClients(Player player)
        {
            string clientName = player.Session.ClientName();
            if (clientName == null || !clientName.StartsWith("ClassiCube 1"))
            {
                player.Kick("Invalid client! Please use ClassiCube.");
            }
        }
    }
}
