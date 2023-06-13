using Brobot.Frontend.Components;
using Brobot.Shared.Requests;
using Brobot.Shared.Responses;
using Microsoft.AspNetCore.Components;
using Radzen.Blazor;

namespace Brobot.Frontend.Pages;

public partial class Reminders : ComponentBase
{
    private List<ScheduledMessageResponse>? reminders;
    private ChannelResponse[]? channels;

    private bool PageReady => reminders != null && channels != null;

    private ScheduledMessageRequest editScheduleMessage = new ScheduledMessageRequest
    {
        MessageText = string.Empty
    };
    
    private FormModal? scheduledMessageModal;
    private ConfirmationModal? deleteConfirmationModal;

    private ScheduledMessageResponse? reminderToDelete;
    private RadzenDataGrid<ScheduledMessageResponse>? grid;

    protected override async Task OnInitializedAsync()
    {
        reminders = (await ApiService.GetScheduledMessages()).ToList();
        channels = await ApiService.GetChannels();
    }

    private void NewScheduledMessage()
    {
        editScheduleMessage = new ScheduledMessageRequest
        {
            MessageText = string.Empty,
            SendDate = DateTime.UtcNow,
        };
        scheduledMessageModal?.ShowModal();
    }
    
    private void EditScheduledMessage(ScheduledMessageResponse message)
    {
        editScheduleMessage = new ScheduledMessageRequest
        {
            MessageText = message.MessageText,
            SendDate = message.SendDate?.DateTime ?? DateTime.UtcNow,
            ChannelId = message.Channel.Id,
            Id = message.Id
        };
        scheduledMessageModal?.ShowModal();
    }

    private async Task DeleteScheduledMessage()
    {
        if (reminderToDelete == null)
        {
            if (deleteConfirmationModal != null)
            {
                await deleteConfirmationModal.HideModal();
            }
            return;
        }
        
        await ApiService.DeleteScheduledMessage(reminderToDelete.Id);
        var reminderToRemove = reminders?.FirstOrDefault((r) => r.Id == reminderToDelete.Id);
        if (reminderToRemove != null)
        {
            reminders?.Remove(reminderToRemove);
        }
        
        if (deleteConfirmationModal != null)
        {
            await deleteConfirmationModal.HideModal();
        }
        
        if (grid != null)
        {
            await grid.Reload();
        }
    }

    private async Task SubmitEditMessage()
    {
        var message = editScheduleMessage.Id == null
            ? await ApiService.CreateScheduledMessage(editScheduleMessage)
            : await ApiService.EditScheduledMessage(editScheduleMessage.Id.Value, editScheduleMessage);

        
        var existingReminder = reminders?.FirstOrDefault((r) => r.Id == message.Id);
        if (existingReminder == null)
        {
            reminders?.Add(message);
            if (grid != null)
            {
                await grid.Reload();
            }
        }
        else
        {
            existingReminder.MessageText = message.MessageText;
            existingReminder.SendDate = message.SendDate;
            existingReminder.Channel = message.Channel;
        }

        if (scheduledMessageModal != null)
        {
            await scheduledMessageModal.HideModal();
        }
    }

    private async Task ShowDeleteConfirmationModal(ScheduledMessageResponse message)
    {
        reminderToDelete = message;
        if (deleteConfirmationModal != null)
        {
            await deleteConfirmationModal.ShowModal();
        }
    }

    private async Task DeleteConfirmationClosed(bool confirmed)
    {
        if (confirmed && reminderToDelete != null)
        {
            await ApiService.DeleteScheduledMessage(reminderToDelete.Id);
            reminders?.Remove(reminderToDelete);
            if (grid != null)
            {
                await grid.Reload();
            }
        }

        reminderToDelete = null;
    }
}