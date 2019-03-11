namespace WorldServer.World.Positions
{
    public interface IPoint3D : IPoint2D
    {
        int Z { get; set; }
    }
}
