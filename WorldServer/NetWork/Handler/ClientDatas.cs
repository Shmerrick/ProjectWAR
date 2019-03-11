/*
 * Copyright (C) 2013 APS
 *	http://AllPrivateServer.com
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System.IO;
using FrameWork;
using WorldServer.Managers;
using WorldServer.World.Objects;

namespace WorldServer.NetWork.Handler
{
    public class ClientDatas : IPacketHandler
    {
        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_CLIENT_DATA, 0, "F_CLIENT_DATA")]
        public static void F_CLIENT_DATA(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;

            Player plr = cclient.Plr;

            if (plr == null)
                return;

            ushort offset = packet.GetUint16();
            ushort size = packet.GetUint16();

            MemoryStream ms = new MemoryStream(plr.Info.ClientData.Data) {Position = offset};
            ms.Write(packet.Read(size), 0, size);
            plr.Info.ClientData.Dirty = true;
            CharMgr.Database.SaveObject(plr.Info.ClientData);
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_UI_MOD, 0, "F_UI_MOD")]
        public static void F_UI_MOD(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;
            //Log.Dump("F_UI_MOD", packet, true);
        }
    }
}
