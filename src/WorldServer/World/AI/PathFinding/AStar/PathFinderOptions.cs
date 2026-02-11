namespace WorldServer.World.AI.PathFinding.AStar
{
    public class PathFinderOptions
    {
        public PathFinderOptions()
        {
            Formula = HeuristicFormula.Manhattan;
            HeuristicEstimate = 2;
            SearchLimit = 2000;
            Diagonals = true;
        }

        public HeuristicFormula Formula { get; set; }

        public bool Diagonals { get; set; }

        public bool HeavyDiagonals { get; set; }

        public int HeuristicEstimate { get; set; }

        public bool PunishChangeDirection { get; set; }

        public bool TieBreaker { get; set; }

        public int SearchLimit { get; set; }
    }
}