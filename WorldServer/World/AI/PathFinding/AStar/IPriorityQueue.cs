namespace WorldServer.World.AI.PathFinding.AStar
{
    public interface IPriorityQueue<T>
    {
        int Count { get; }
        int Push(T item);
        T Pop();
        T Peek();

        void Clear();
    }
}