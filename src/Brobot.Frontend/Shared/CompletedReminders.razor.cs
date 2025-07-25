using Blazored.Toast.Services;
using Brobot.Frontend.Services;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Components;
using Radzen.Blazor;

namespace Brobot.Frontend.Shared;

public partial class CompletedReminders : ComponentBase, ITabbable
{
    [Inject]
    public required ApiService ApiService { get; set; }
    
    [Inject]
    public required IToastService ToastService { get; set; }
    
    private List<ScheduledMessageResponse>? _reminders;
    private bool GridReady => _reminders != null;
    public RadzenDataGrid<ScheduledMessageResponse>? Grid { get; private set; }
    
    public async Task TabSelected()
    {
        try
        {
            _reminders = (await ApiService.ScheduledMessageService.GetSentScheduledMessages())
                .OrderBy(r => r.SendDate)
                .ToList();
            StateHasChanged();
        }
        catch (Exception e)
        {
            ToastService.ShowError($"Failed to load completed reminders. {e.Message}");
        }
    }
}