namespace WorldServer.World.AI.PathFinding.AStar
{
    public struct PathFinderNode
    {
        public int F_Gone_Plus_Heuristic;
        public int Gone;
        public int Heuristic; // f = gone + heuristic
        public int X;
        public int Y;
        public int ParentX; // Parent
        public int ParentY;

        public int Z { get; internal set; }
    }
}