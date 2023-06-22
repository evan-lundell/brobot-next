using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Brobot.Frontend.Components;

public abstract class ModalBase : ComponentBase
{
    [Inject]
    public required IJSRuntime JsRuntime { get; set; }
    
    [Parameter]
    public required string Id { get; set; }

    [Parameter]
    public required string Title { get; set; }
    
    [Parameter]
    public required RenderFragment ModalBody { get; set; }
    
    [Parameter]
    public Action? OnModalClosed { get; set; }

    public virtual async Task ShowModal()
    {
        await JsRuntime.InvokeVoidAsync("showModal", $"#{Id}");
    }

    public virtual async Task HideModal()
    {
        await JsRuntime.InvokeVoidAsync("hideModal", $"#{Id}");
        OnModalClosed?.Invoke();
    }
}