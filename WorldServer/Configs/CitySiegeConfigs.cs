using FrameWork;

namespace WorldServer.Configs
{
    public enum SiegeStatus
    {
        DISABLED,
        ENABLED,
        ALWAYS_ON
    }

    [aConfigAttributes("Configs/CitySiege.xml")]
    public class CitySiegeConfigs : aConfig
    {
        public string Enabled = "true";
        public int ParticipantsPerInstance = 48;
        public int Stage1DurationMinutes = 45;
        public int Stage2DurationMinutes = 30;
        public int Stage3DurationMinutes = 30;
        public int Stage1ObjectiveTimeBonusMinutes = 5;
        public int TransitionDurationSeconds = 60;

        public SiegeStatus GetSiegeStatus()
        {
            switch (Enabled.ToLower())
            {
                case "false":
                    return SiegeStatus.DISABLED;
                case "always":
                case "always_on":
                    return SiegeStatus.ALWAYS_ON;
                default:
                    return SiegeStatus.ENABLED;
            }
        }
    }
}
