using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldServer
{
    public class InstanceDoor:InstanceObject 
    {
        public bool IsOpen
        {
            get
            {
                return VfxState == 1;
            }
            set
            {
                VfxState = (byte)(value ? 1 : 4);
                UpdateVfxState(VfxState);

                if (value)
                    Instance.DoorOpenned(this);
                else
                    Instance.DoorClosed(this);

            }
        }
        public InstanceDoor(Instance instance, Instance_Object info) : base(instance, info)
        {
        }
    }
}
