using Core.Infrastructure.Network;

namespace LauncherServer.Dtos;

public class GetInfoResponse
{
    [PacketLength(1)]
    public List<RealmInfo> RealmInfo { get; set; }
}