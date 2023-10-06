using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Components;

namespace Brobot.Frontend.Shared;

public abstract class DashboardGraphBase : ComponentBase
{
    [Parameter, EditorRequired]
    public ChannelResponse[]? Channels { get; set; }
}