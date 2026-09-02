using Common;
using FrameWork;
using WorldServer.World.Abilities.Buffs;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Objects.PublicQuests
{
    public class PQuestGameObject : GameObject
    {
        private const uint PerishedSoulEntry = 1080;
        private const ushort SoulHarvestEffectId = 171;

        public PQuestObjective Objective;

        public PQuestGameObject(GameObject_spawn spawn, PQuestObjective objective)
        {
            this.Spawn = spawn;
            Name = spawn.Proto.Name;
            this.Objective = objective;
            this.Respawn = 0;
        }

        public override void RezUnit()
        {
            GameObject go = Region.CreateGameObject(Spawn);
            go.Respawn = 0;
            Destroy();
        }

        public override void NotifyInteractionComplete(NewBuff buff)
        {
            Player player = CapturingPlayer;
            int previousCount = Objective?.Count ?? 0;

            base.NotifyInteractionComplete(buff);

            if (player == null || Objective == null || Objective.Count <= previousCount)
                return;

            if (Entry == PerishedSoulEntry)
                SendSoulHarvestEffect();

            PacketOut death = new PacketOut((byte)Opcodes.F_OBJECT_DEATH, 12);
            death.WriteUInt16(Oid);
            death.Fill(0, 10);
            DispatchPacket(death, true);
            Destroy();
        }

        private void SendSoulHarvestEffect()
        {
            PacketOut effect = new PacketOut((byte)Opcodes.F_PLAY_EFFECT, 20);
            effect.WriteUInt16(SoulHarvestEffectId);
            effect.WriteUInt16(0);
            effect.WriteUInt32((uint)WorldPosition.X);
            effect.WriteUInt32((uint)WorldPosition.Y);
            effect.WriteUInt32((uint)WorldPosition.Z);
            effect.WriteUInt32(0);
            DispatchPacket(effect, true);
        }
    }
}
