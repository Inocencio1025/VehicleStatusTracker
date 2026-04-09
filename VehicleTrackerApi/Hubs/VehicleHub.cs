using Microsoft.AspNetCore.SignalR;

namespace VehicleTrackerApi.Hubs
{
    public class VehicleHub : Hub
    {
        private readonly ILogger<VehicleHub> _logger;

        public VehicleHub(ILogger<VehicleHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation(
                "SignalR client connected. ConnectionId {ConnectionId}.",
                Context.ConnectionId);
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
