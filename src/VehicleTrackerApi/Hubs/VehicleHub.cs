using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace VehicleTrackerApi.Hubs
{
    [Authorize]
    public class VehicleHub(ILogger<VehicleHub> logger) : Hub
    {
        private readonly ILogger<VehicleHub> _logger = logger;

    public override async Task OnConnectedAsync()
        {
            _logger.LogInformation(
                "SignalR client connected. ConnectionId {ConnectionId}.",
                Context.ConnectionId);
            _logger.LogInformation(
                "Connected user {User}",
                Context.UserIdentifier
            );
            await base.OnConnectedAsync();
        } 

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (exception != null)
            {
                _logger.LogWarning(
                    exception,
                    "SignalR client disconnected with error. ConnectionId {ConnectionId}.",
                    Context.ConnectionId);
            }
            else
            {
                _logger.LogInformation(
                    "SignalR client disconnected. ConnectionId {ConnectionId}.",
                    Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
