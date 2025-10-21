using Backend_QLTE.ChildService.Application.DTOs.Client.Location;
using Backend_QLTE.ChildService.Application.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;

namespace Backend_QLTE.Hubs
{
    public class LocationHub : Hub
    {
        private readonly ILocationService _locationService;
        public LocationHub(ILocationService locationService)
        {
            _locationService = locationService;
        }
        
        public async Task SendLocation(string childId, double lat, double lng) // latitude vi do,longitude kinh do
        {
            // Gửi chỉ tới group của childId
            await Clients.Group(childId).SendAsync("ReceiveChildLocation", childId, lat, lng);


            var createLocation = new CreateLocationHistoryRequestDTO
            {
                ChildId = Guid.Parse(childId),
                ViDo = lat,
                KinhDo = lng,
                DoChinhXac = 0
            };
            await _locationService.CreateLocationHistoryAsync(createLocation);
            
        }

        // Parent join group để theo dõi childId
        public async Task JoinChildGroup(string childId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, childId);
        }

        // Parent rời group nếu cần
        public async Task LeaveChildGroup(string childId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, childId);
        }
    }
}
