using Microsoft.AspNetCore.SignalR;
using PhiChat.Api.Functions.Message;
using PhiChat.Api.Functions.User;

namespace PhiChat.Api.Controllers.ChatHub
{
    public class ChatHub : Hub
    {
        UserOperator _userOperator;
        IMessageFunction _messageFunction;
        ChatAppContext _chatAppContext;
        private static readonly Dictionary<int, string> _connectionMapping
            = new Dictionary<int, string>();

        public ChatHub(UserOperator userOperator, IMessageFunction messageFunction, ChatAppContext chatAppContext)
        {
            _userOperator = userOperator;
            _messageFunction = messageFunction;
            _chatAppContext = chatAppContext;
        }

        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }

        public async Task SendMessageToUser(int fromUserId, int toUserId, string message)
        {
            var connectionIds = _connectionMapping.Where(x => x.Key == toUserId)
                                                    .Select(x => x.Value).ToList();

            await _messageFunction.AddMessage(fromUserId, toUserId, message);

            await Clients.Clients(connectionIds)
                .SendAsync("ReceiveMessage", fromUserId, message);
        }

        public override Task OnConnectedAsync()
        {
            var userId = _userOperator.GetRequestUser().Id;
            if (!_connectionMapping.ContainsKey(userId))
                _connectionMapping.Add(userId, Context.ConnectionId);

            var entity = _chatAppContext.TblUsers.SingleOrDefault(x => x.Id == userId);

            if (entity != null)
            {
                entity.IsOnline = true;
                _chatAppContext.SaveChangesAsync();
            }

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = _userOperator.GetRequestUser().Id;
            _connectionMapping.Remove(userId);

            var entity = _chatAppContext.TblUsers.SingleOrDefault(x => x.Id == userId);

            if (entity != null)
            {
                entity.IsOnline = false;
                entity.LastLogonTime = DateTime.Now;
                _chatAppContext.SaveChangesAsync();
            }
            return base.OnDisconnectedAsync(exception);
        }
    }
}
