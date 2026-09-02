using Common;
using FrameWork;
using WorldServer.World.Objects;
using WorldServer.World.Objects.PublicQuests;
using WorldServer.World.Positions;
using Object = WorldServer.World.Objects.Object;

namespace WorldServer.World.Scripting.PublicQuests.Destro
{
    [GeneralScript(false, "", 1474, 0)]
    public class RuinousPowers : AGeneralScript
    {
        private const uint PublicQuestEntry = 185;
        private const uint RitualObjectiveGuid = 801;
        private const ushort TransformationEffectId = 443;
        private const int RitualDurationMilliseconds = 37000;

        // Capture-verified position at which Kar'thok appears.
        private static readonly Point3D RitualCenter = new Point3D(858717, 839275, 6352);

        private PQuestCreature _wizard;

        public override void OnObjectLoad(Object obj)
        {
            PQuestCreature wizard = obj as PQuestCreature;
            if (wizard?.Objective?.Objective == null ||
                wizard.Objective.Quest?.Info == null ||
                wizard.Objective.Quest.Info.Entry != PublicQuestEntry ||
                wizard.Objective.Objective.Guid != RitualObjectiveGuid)
            {
                return;
            }

            _wizard = wizard;
            _wizard.IsInvulnerable = true;
            _wizard.MvtInterface.Move(RitualCenter);
            _wizard.EvtInterface.AddEvent(CompleteRitual, RitualDurationMilliseconds, 1);
        }

        private void CompleteRitual()
        {
            if (_wizard == null || _wizard.IsDisposed || _wizard.PendingDisposal)
                return;

            _wizard.PlayEffect(TransformationEffectId, RitualCenter);
            _wizard.Objective.Quest.HandleEvent(
                null,
                Objective_Type.QUEST_SCRIPTED_EVENT,
                RitualObjectiveGuid,
                1,
                0);
        }
    }
}
