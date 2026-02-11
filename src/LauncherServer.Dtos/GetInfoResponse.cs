using FrameWork.NetWork.V4;

namespace LauncherServer.Dtos;

public class GetInfoResponse
{
    [PacketLength(1)]
    public List<RealmInfo> RealmInfo { get; set; }
}