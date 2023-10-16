using PhiChat.Api.Functions.Message;
using PhiChat.Api.Functions.User;

namespace PhiChat.Api.Controllers.Message
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class MessageController : Controller
    {
        IMessageFunction _messageFunction;
        IUserFunction _userFunction;

        public MessageController(IMessageFunction messageFunction, IUserFunction userFunction)
        {
            _messageFunction = messageFunction;
            _userFunction = userFunction;
        }

        [HttpPost("Initialize")]
        public async Task<ActionResult> Initialize([FromBody] MessageInitalizeRequest request)
        {
            var response = new MessageInitalizeResponse
            {
                FriendInfo = _userFunction.GetUserById(request.ToUserId),
                Messages = await _messageFunction.GetMessages(request.FromUserId, request.ToUserId)
            };

            return Ok(response);
        }

        [HttpPost("ReadMessage")]
        public async Task<ActionResult> ReadMessage([FromBody] MessageInitalizeRequest request)
        {
            await _messageFunction.MarkMessageIsRead(request.FromUserId, request.ToUserId);

            var response = new MessageInitalizeResponse
            {
                FriendInfo = _userFunction.GetUserById(request.ToUserId),
                Messages = await _messageFunction.GetMessages(request.FromUserId, request.ToUserId)
            };

            return Ok(response);
        }


    }
}
