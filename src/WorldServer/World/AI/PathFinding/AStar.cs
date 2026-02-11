using System;
using System.Collections.Generic;
using System.Linq;

namespace WorldServer.World.AI.PathFinding
{
    public class AStarLocation
    {
        public int F;
        public int G;
        public int H;
        public AStarLocation Parent;
        public int X;
        public int Y;
    }


    public class AStarSearch
    {
        public AStarLocation StartLocation { get; set; }
        public AStarLocation EndLocation { get; set; }
        public AStarLocation CurrentLocation { get; set; }
        public string[,] Map { get; set; }
        public List<AStarLocation> ShortestPath { get; set; }
        public List<AStarLocation> Occlusions { get; set; }


        public void Calculate()
        {
            var openList = new List<AStarLocation>();
            var closedList = new List<AStarLocation>();
            ShortestPath = new List<AStarLocation>();
            var g = 0;

            // start by adding the original position to the open list
            openList.Add(StartLocation);

            while (openList.Count > 0)
            {
                // get the square with the lowest F score
                var lowest = openList.Min(l => l.F);
                CurrentLocation = openList.First(l => l.F == lowest);

                // add the current square to the closed list
                closedList.Add(CurrentLocation);

                // remove it from the open list
                openList.Remove(CurrentLocation);

                // if we added the destination to the closed list, we've found a path
                if (closedList.FirstOrDefault(l => l.X == EndLocation.X && l.Y == EndLocation.Y) != null)
                    break;

                var adjacentSquares = GetWalkableAdjacentSquares(CurrentLocation.X, CurrentLocation.Y, Map);
                g++;

                foreach (var adjacentSquare in adjacentSquares)
                {
                    // if this adjacent square is already in the closed list, ignore it
                    if (closedList.FirstOrDefault(l => l.X == adjacentSquare.X
                                                       && l.Y == adjacentSquare.Y) != null)
                        continue;

                    // if it's not in the open list...
                    if (openList.FirstOrDefault(l => l.X == adjacentSquare.X
                                                     && l.Y == adjacentSquare.Y) == null)
                    {
                        // compute its score, set the parent
                        adjacentSquare.G = g;
                        adjacentSquare.H = ComputeHScore(adjacentSquare.X, adjacentSquare.Y, EndLocation.X,
                            EndLocation.Y);
                        adjacentSquare.F = adjacentSquare.G + adjacentSquare.H;
                        adjacentSquare.Parent = CurrentLocation;

                        // and add it to the open list
                        openList.Insert(0, adjacentSquare);
                    }
                    else
                    {
                        // test if using the current G score makes the adjacent square's F score
                        // lower, if yes update the parent because it means it's a better path
                        if (g + adjacentSquare.H < adjacentSquare.F)
                        {
                            adjacentSquare.G = g;
                            adjacentSquare.F = adjacentSquare.G + adjacentSquare.H;
                            adjacentSquare.Parent = CurrentLocation;
                        }
                    }
                }
            }

            while (CurrentLocation != null)
            {
                ShortestPath.Add(CurrentLocation);
                CurrentLocation = CurrentLocation.Parent;
            }
        }


        //    // assume path was found; let's show it
        //    while (current != null)
        //    {
        //        Console.SetCursorPosition(current.X, current.Y);
        //        Console.Write('_');
        //        Console.SetCursorPosition(current.X, current.Y);
        //        current = current.Parent;
        //        System.Threading.Thread.Sleep(1000);
        //    }

        //    // end

        //    Console.ReadLine();
        //}

        private static List<AStarLocation> GetWalkableAdjacentSquares(int x, int y, string[,] map)
        {
            var proposedLocations = new List<AStarLocation>
            {
                new AStarLocation {X = x, Y = y - 1},
                new AStarLocation {X = x, Y = y + 1},
                new AStarLocation {X = x - 1, Y = y},
                new AStarLocation {X = x + 1, Y = y}
            };

            return proposedLocations.Where(l => map[l.Y, l.X] == " " || map[l.Y, l.X] == "B").ToList();
        }

        private static int ComputeHScore(int x, int y, int targetX, int targetY)
        {
            return Math.Abs(targetX - x) + Math.Abs(targetY - y);
        }
    }
}