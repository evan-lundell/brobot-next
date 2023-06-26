using Blazored.Toast.Services;
using Brobot.Frontend.Services;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Components;

namespace Brobot.Frontend.Shared;

public partial class CompletedHotOps : ComponentBase, ITabbable
{
    private List<HotOpResponse>? _hotOps;
    
    public required HotOpScoreboard HotOpScoreboard { get; set; }
    
    [Inject] 
    public required ApiService ApiService { get; set; }
    
    [Inject]
    public required IToastService ToastService { get; set; }

    private Task ViewScoreboard(HotOpResponse hotOpResponse)
        => HotOpScoreboard.ShowScoreboard(hotOpResponse.Id);
    
    public async Task TabSelected()
    {
        try
        {
            _hotOps ??= (await ApiService.HotOpService.GetCompletedHotOps()).ToList();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            ToastService.ShowError($"Failed to initialize. {ex.Message}");
        }
    }
}