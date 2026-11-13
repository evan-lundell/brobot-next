using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Brobot.Frontend.Components;

public abstract class ModalBase : ComponentBase
{
    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    [Inject]
    private ILogger<ModalBase> Logger { get; set; } = default!;
    
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
        try
        {
            await JsRuntime.InvokeVoidAsync("showModal", $"#{Id}");
        }
        catch (JSException ex)
        {
            Logger.LogError(ex, "Failed to show modal {ModalId}", Id);
        }
    }

    public virtual async Task HideModal()
    {
        try
        {
            await JsRuntime.InvokeVoidAsync("hideModal", $"#{Id}");
        }
        catch (JSException ex)
        {
            Logger.LogError(ex, "Failed to hide modal {ModalId}", Id);
        }
        OnModalClosed?.Invoke();
    }
}