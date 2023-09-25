using PhiChat.Api.Functions.Message;
using PhiChat.Api.Functions.User;

namespace PhiChat.Api.Controllers.ListChat
{
    public class ListChatInitializeResponse
    {
        public User User { get; set; } = null!;
        public IEnumerable<User> UserFriends { get; set; } = null!;
        public IEnumerable<LastestMessage> LastestMessages { get; set; } = null!;
    }
}
