using Blazored.Toast.Services;
using Brobot.Frontend.Components;
using Brobot.Frontend.Services;
using Brobot.Shared.Requests;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Components;
using Radzen.Blazor;

namespace Brobot.Frontend.Shared;

public partial class UpcomingReminders : ComponentBase, ITabbable
{
    private List<ScheduledMessageResponse>? _reminders;
    private ChannelResponse[]? _channels;

    private bool GridReady => _reminders != null && _channels != null;

    private ScheduledMessageRequest _editScheduleMessage = new ScheduledMessageRequest
    {
        MessageText = string.Empty
    };
    
    private FormModal? _scheduledMessageModal;
    private ConfirmationModal? _deleteConfirmationModal;

    private ScheduledMessageResponse? _reminderToDelete;
    private RadzenDataGrid<ScheduledMessageResponse>? _grid;
    
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
            ToastService.ShowError($"Initialization failed. {ex.Message}");
        }
    }

    private void NewScheduledMessage()
    {
        _editScheduleMessage = new ScheduledMessageRequest
        {
            MessageText = string.Empty,
            SendDate = DateTime.UtcNow,
        };
        _scheduledMessageModal?.ShowModal();
    }
    
    private void EditScheduledMessage(ScheduledMessageResponse message)
    {
        _editScheduleMessage = new ScheduledMessageRequest
        {
            MessageText = message.MessageText,
            SendDate = message.SendDate?.DateTime ?? DateTime.UtcNow,
            ChannelId = message.Channel.Id,
            Id = message.Id
        };
        _scheduledMessageModal?.ShowModal();
    }
    
    private async Task SubmitEditMessage()
    {
        try
        {
            var message = _editScheduleMessage.Id == null
                ? await ApiService.ScheduledMessageService.Create(_editScheduleMessage)
                : await ApiService.ScheduledMessageService.Update(_editScheduleMessage.Id.Value, _editScheduleMessage);


            var existingReminder = _reminders?.FirstOrDefault(r => r.Id == message.Id);
            if (existingReminder == null)
            {
                _reminders?.Add(message);
                if (_grid != null)
                {
                    await _grid.Reload();
                }
            }
            else
            {
                existingReminder.MessageText = message.MessageText;
                existingReminder.SendDate = message.SendDate;
                existingReminder.Channel = message.Channel;
                ToastService.ShowSuccess("Successfully saved reminder");
            }
        }
        catch (Exception ex)
        {
            ToastService.ShowError($"Failed to save reminder. {ex.Message}");
        }

        if (_scheduledMessageModal != null)
        {
            await _scheduledMessageModal.HideModal();
        }
    }

    private async Task ShowDeleteConfirmationModal(ScheduledMessageResponse message)
    {
        _reminderToDelete = message;
        if (_deleteConfirmationModal != null)
        {
            await _deleteConfirmationModal.ShowModal();
        }
    }

    private async Task DeleteConfirmationClosed(bool confirmed)
    {
        if (confirmed && _reminderToDelete != null)
        {
            try
            {
                await ApiService.ScheduledMessageService.Delete(_reminderToDelete.Id);
                _reminders?.Remove(_reminderToDelete);
                if (_grid != null)
                {
                    await _grid.Reload();
                }
                ToastService.ShowSuccess("Successfully delete reminder");
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Failed to delete reminder. {ex.Message}");
            }
        }

        _reminderToDelete = null;
    }

    
    public async Task TabSelected()
    {
        try
        {
            _reminders ??= (await ApiService.ScheduledMessageService.GetUnsentScheduledMessages())
                .OrderBy(r => r.SendDate)
                .ToList();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            ToastService.ShowError($"Failed to get reminders. {ex.Message}");
        }
    }
}