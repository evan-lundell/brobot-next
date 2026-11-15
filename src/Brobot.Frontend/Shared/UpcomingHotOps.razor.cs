using Blazored.Toast.Services;
using Brobot.Frontend.Components;
using Brobot.Frontend.Services;
using Brobot.Shared;
using Brobot.Shared.Requests;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
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

    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationState { get; set; }

    private bool _isAdmin;
    private ulong? _currentUserId;
    private DiscordUserResponse[]? _users;

    protected override async Task OnInitializedAsync()
    {
        if (AuthenticationState is not null)
        {
            var user = (await AuthenticationState).User;
            _isAdmin = user.IsInRole(Constants.AdminRoleName);
            if (ulong.TryParse(user.FindFirst(Brobot.Shared.Claims.ClaimTypes.DiscordId)?.Value, out var discordId))
            {
                _currentUserId = discordId;
            }
        }

        await Task.WhenAll(LoadChannels(), _isAdmin ? LoadUsers() : Task.CompletedTask);
    }

    private async Task LoadChannels()
    {
        try
        {
            _channels = await ApiService.GetChannels();
        }
        catch (Exception ex)
        {
            ToastService.ShowError($"Failed to load channels. {ex.Message}");
        }
    }

    private async Task LoadUsers()
    {
        try
        {
            // The current user is offered as "Me" at the top of the owner list, so leave them out here
            _users = [.. (await ApiService.GetDiscordUsers()).Where(u => u.Id != _currentUserId)];
        }
        catch (Exception ex)
        {
            ToastService.ShowError($"Failed to load users. {ex.Message}");
        }
    }

    private async Task NewHotOp()
    {
        // Hot Ops must start and end on the minute (the API rejects seconds), so don't seed the form with them
        var utcNow = DateTime.UtcNow;
        var now = new DateTime(utcNow.Ticks - utcNow.Ticks % TimeSpan.TicksPerMinute, DateTimeKind.Utc);
        _editHotOpRequest = new HotOpRequest
        {
            StartDate = now,
            EndDate = now,
            // Preselect "Me" in the owner dropdown; non-admins never see it and the API treats null as the caller
            UserId = _isAdmin ? _currentUserId : null
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
            ChannelId = hotOp.Channel.Id,
            UserId = hotOp.DiscordUser.Id
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

            var index = _hotOps?.FindIndex(ho => ho.Id == hotOp.Id) ?? -1;
            if (index >= 0)
            {
                _hotOps![index] = hotOp;
            }
            else
            {
                _hotOps?.Add(hotOp);
            }
            await Grid.Reload();
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
            _hotOps ??= (await ApiService.HotOpService.GetUpcomingHotOps(allUsers: _isAdmin)).ToList();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            ToastService.ShowError($"Failed to get Hot Ops. {ex.Message}");
        }
    }
}