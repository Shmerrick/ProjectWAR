using Core.Infrastructure.Network;
using LauncherServer.Dtos;

namespace Launcher.NetWork;

[PacketSerializerContext(
    typeof(CheckVersionRequest),
    typeof(CheckVersionResponse),
    typeof(CreateAccountRequest),
    typeof(CreateAccountResponse),
    typeof(GetInfoRequest),
    typeof(GetInfoResponse),
    typeof(StartRequest),
    typeof(StartResponse))]
public partial class LauncherSerializerContext
{
}
