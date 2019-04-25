using Common;

namespace WorldServer.World.Objects.Instances
{
    public class InstanceDoor : InstanceObject 
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
                    Instance.DoorOpened(this);
                else
                    Instance.DoorClosed(this);

            }
        }

        public InstanceDoor(Instance instance, Instance_Object info) : base(instance, info)
        {

        }
    }
}
