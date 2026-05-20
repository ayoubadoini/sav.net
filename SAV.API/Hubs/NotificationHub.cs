using Microsoft.AspNetCore.SignalR;

namespace SAV.API.Hubs;

public class NotificationHub : Hub
{
    public async Task SendNotification(string userId, string message)
    {
        await Clients.User(userId).SendAsync("ReceiveNotification", message);
    }

    public async Task SendComplaintUpdate(int complaintId, string status)
    {
        await Clients.All.SendAsync("ComplaintStatusChanged", complaintId, status);
    }

    public async Task SendInterventionUpdate(int interventionId, string status)
    {
        await Clients.All.SendAsync("InterventionStatusChanged", interventionId, status);
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
    }
}
