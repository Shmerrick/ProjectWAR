using System.Collections.Generic;
using FrameWork;
using System;
using WorldServer.World.Objects;

namespace WorldServer.Managers.Commands
{
    public class GmCommandHandler
    {
        public delegate bool GmComHandler(Player plr, ref List<string> values);
        public GmCommandHandler(string name, GmComHandler handler, List<GmCommandHandler> handlers, EGmLevel accessRequired, int valueCount, string description)
        {
            Name = name;
            Handler = handler;
            Handlers = handlers;
            AccessRequired = accessRequired;
            ValueCount = valueCount;
            Description = description;
        }

        public string Name;
        public GmComHandler Handler;
        public List<GmCommandHandler> Handlers;
        public EGmLevel AccessRequired;
        /// <summary>
        /// Expected argument count, for legacy purpose.
        /// </summary>
        public int ValueCount;
        public string Description;
    }
    
}
