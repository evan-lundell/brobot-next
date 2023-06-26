using Blazored.Toast.Services;
using Brobot.Frontend.Components;
using Brobot.Frontend.Services;
using Brobot.Shared.Requests;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Components;
using Radzen.Blazor;

namespace Brobot.Frontend.Shared;

public partial class UpcomingHotOps : ComponentBase, ITabbable
{
    private List<HotOpResponse>? _hotOps;
    
    public required RadzenDataGrid<HotOpResponse> Grid { get; set; }
    public required FormModal EditHotOpModal { get; set; }
    public required ConfirmationModal DeleteHotOpModal { get; set; }
    private HotOpRequest _editHotOpRequest = new();
    private HotOpResponse? _hotOpToDelete;
    private ChannelResponse[]? _channels;
    
    [Inject]
    public required ApiService ApiService { get; set; }
    
    [Inject] 
    public required IToastService ToastService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _channels = await ApiService.GetChannels();
        }
        catch (Exception ex)
        {
            ToastService.ShowError($"Failed to initialize. {ex.Message}");
        }
    }

    private async Task NewHotOp()
    {
        var now = DateTime.UtcNow;
        _editHotOpRequest = new HotOpRequest
        {
            StartDate = now,
            EndDate = now
        };
        
        await EditHotOpModal.ShowModal();
    }
    
    private async Task EditHotOp(HotOpResponse hotOp)
    {
        _editHotOpRequest = new HotOpRequest
        {
            Id = hotOp.Id,
            StartDate = hotOp.StartDate,
            EndDate = hotOp.EndDate,
            ChannelId = hotOp.Channel.Id
        };
        await EditHotOpModal.ShowModal();
    }

    private async Task SubmitHotOp()
    {
        try
        {
            var hotOp = _editHotOpRequest.Id == null
                ? await ApiService.HotOpService.Create(_editHotOpRequest)
                : await ApiService.HotOpService.Update(_editHotOpRequest.Id.Value, _editHotOpRequest);

            var existingHotOp = _hotOps?.FirstOrDefault((ho) => ho.Id == hotOp.Id);
            if (existingHotOp == null)
            {
                _hotOps?.Add(hotOp);
                await Grid.Reload();
            }
            else
            {
                existingHotOp.StartDate = hotOp.StartDate;
                existingHotOp.EndDate = hotOp.EndDate;
                existingHotOp.Channel = hotOp.Channel;
            }
        }
        catch (Exception ex)
        {
            ToastService.ShowError($"Failed to save hot op. {ex.Message}");
        }

        await EditHotOpModal.HideModal();
    }

    private async Task ShowDeleteConfirmationModal(HotOpResponse hotOp)
    {
        _hotOpToDelete = hotOp;
        await DeleteHotOpModal.ShowModal();
    }

    private async Task DeleteConfirmationClosed(bool confirmed)
    {
        if (!confirmed || _hotOpToDelete == null)
        {
            return;
        }

        try
        {
            await ApiService.HotOpService.Delete(_hotOpToDelete.Id);
            _hotOps?.Remove(_hotOpToDelete);
            await Grid.Reload();
        }
        catch (Exception ex)
        {
            ToastService.ShowError($"Failed to delete hot op. {ex.Message}");
        }

        _hotOpToDelete = null;
    }
    
    public async Task TabSelected()
    {
        try
        {
            _hotOps ??= (await ApiService.HotOpService.GetUpcomingHotOps()).ToList();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            ToastService.ShowError($"Failed to get Hot Ops. {ex.Message}");
        }
    }
}