namespace WorldServer.World.AI.PathFinding.AStar
{
    /// <summary>
    ///     A point in a matrix. Pxy where X is the row and Y is the column.
    /// </summary>
    public struct Point
    {
        /// <summary>
        ///     The row in the matrix
        /// </summary>
        public int X { get; }

        /// <summary>
        ///     The column in the matrix
        /// </summary>
        public int Y { get; }

        public Point(int x = 0, int y = 0)
        {
            X = x;
            Y = y;
        }

        public static bool operator ==(Point a, Point b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Point a, Point b)
        {
            return !a.Equals(b);
        }

        public bool Equals(Point other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object other)
        {
            return Equals((Point) other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();

                return hash;
            }
        }

        public override string ToString()
        {
            return $"[{X}.{Y}]";
        }
    }
}