using WorldServer.World.Objects;

namespace WorldServer.World.AI
{
    public class BossSpawn
    {
        public BrainType Type { get; set; }
        public uint ProtoId { get; set; }
        public Creature Creature { get; set; }
    }
}