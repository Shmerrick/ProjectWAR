/*
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
 
using System;

namespace FrameWork
{
    public enum PacketHandlerType
    {
        TCP = 0x01,
        UDP = 0x02
    }

    public delegate void PacketFunction(BaseClient client,PacketIn packet);

    [AttributeUsage(AttributeTargets.Method)]
    public class PacketHandlerAttribute : Attribute
    {
        public PacketHandlerType Type { get; }

        public int Opcode { get; }

        // Client State level for handle this packet
        public int State { get; }

        public string Description { get; }

        public PacketHandlerAttribute(PacketHandlerType type, int opcode, string desc)
            : this(type,opcode,0,desc)
        {

        }

        public PacketHandlerAttribute(PacketHandlerType type, int opcode, int statelevel, string desc)
        {
            Type = type;
            Opcode = opcode;
            Description = desc;
            State = statelevel;
        }
    }
}
