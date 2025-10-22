using Microsoft.AspNetCore.SignalR;

namespace Backend_QLTE.ChildService.Api.Hubs
{
    public class SOSHub : Hub
    {
        // Khi client kết nối, có thể log hoặc lưu UserId
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        // Hàm gửi tin nhắn test
        public async Task SendTest(string message)
        {
            await Clients.All.SendAsync("ReceiveSOS", message);
        }
    }
}
