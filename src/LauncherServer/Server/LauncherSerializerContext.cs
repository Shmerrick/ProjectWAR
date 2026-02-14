using Core.Infrastructure.Network;
using LauncherServer.Dtos;

namespace LauncherServer.Server;

[PacketSerializerContext(
    typeof(CheckVersionRequest),
    typeof(CheckVersionResponse),
    typeof(Dtos.CreateAccountRequest),
    typeof(Dtos.CreateAccountResponse),
    typeof(Dtos.GetInfoRequest),
    typeof(Dtos.GetInfoResponse),
    typeof(StartRequest))]
public partial class LauncherSerializerContext
{
}
