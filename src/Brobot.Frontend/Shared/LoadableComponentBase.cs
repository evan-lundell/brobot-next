using Microsoft.AspNetCore.Components;

namespace Brobot.Frontend.Shared;

public abstract class LoadableComponentBase : ComponentBase
{
    [Parameter]
    public bool Loaded { get; set; }
    
    [Parameter]
    public EventCallback<bool> LoadedChanged { get; set; }

    protected async Task UpdateLoaded(bool value)
    {
        Loaded = value;
        await LoadedChanged.InvokeAsync(Loaded);
    }
}