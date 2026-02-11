using System.Collections.Generic;

namespace WorldServer.World.AI.PathFinding.AStar
{
    public interface IPathFinder
    {
        List<PathFinderNode> FindPath(Point start, Point end);
    }
}