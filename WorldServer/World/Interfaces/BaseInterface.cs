
using WorldServer.World.Objects;

namespace WorldServer.World.Interfaces
{
    public abstract class BaseInterface
    {
        public Object _Owner;
        public bool Loaded;
        public bool IsLoad => Loaded;

        public virtual void SetOwner(Object owner)
        {
            _Owner = owner;
        }

        public virtual bool Load()
        {
            Loaded = true;
            return true;
        }

        public virtual void Update(long tick)
        {

        }

        public virtual void Stop()
        {
        }

        public virtual void Save()
        {

        }

        public virtual bool HasObject()
        {
            return _Owner != null;
        }

        public virtual bool HasPlayer()
        {
            if (!HasObject())
                return false;

            return _Owner.IsPlayer();
        }
        public virtual bool HasPet()
        {
            if (!HasObject())
                return false;

            return _Owner.IsPet();
        }

        public virtual bool HasUnit()
        {
            if (!HasObject())
                return false;

            return _Owner.IsUnit();

        }

        public virtual Unit GetUnit()
        {
            if (!HasUnit())
                return null;

            return _Owner.GetUnit();
        }

        public virtual Player GetPlayer()
        {
            if (!HasPlayer())
                return null;

            return _Owner.GetPlayer();
        }

        public virtual Creature GetCreature()
        {
            return _Owner.GetCreature();
        }

        public virtual Pet GetPet()
        {
            if (!HasPet())
                return null;

            return _Owner.GetPet();
        }
    }
}
