﻿using PhiChat.Api.Functions.User;

namespace PhiChat.Api.Functions.Message
{
    public class MessageFunction : IMessageFunction
    {
        ChatAppContext _chatAppContext;
        IUserFunction _userFunction;
        public MessageFunction(ChatAppContext chatAppContext, IUserFunction userFunction)
        {
            _chatAppContext = chatAppContext;
            _userFunction = userFunction;
        }

        public async Task<int> AddMessage(int fromUserId, int toUserId, string message)
        {
            var entity = new TblMessage
            {
                FromUserId = fromUserId,
                ToUserId = toUserId,
                Content = message,
                SendDateTime = DateTime.Now,
                IsRead = false
            };

            _chatAppContext.TblMessages.Add(entity);
            var result = await _chatAppContext.SaveChangesAsync();

            return result;
        }

        public async Task<IEnumerable<LastestMessage>> GetLastestMessage(int userId)
        {
            var result = new List<LastestMessage>();

            var userFriends = await _chatAppContext.TblUserFriends
                .Where(x => x.UserId == userId).ToListAsync();

            foreach (var userFriend in userFriends)
            {
                var lastMessage = await _chatAppContext.TblMessages
                    .Where(x => (x.FromUserId == userId && x.ToUserId == userFriend.FriendId)
                             || (x.FromUserId == userFriend.FriendId && x.ToUserId == userId))
                    .OrderByDescending(x => x.SendDateTime)
                    .FirstOrDefaultAsync();

                if (lastMessage != null)
                {
                    result.Add(new LastestMessage
                    {
                        UserId = userId,
                        Content = lastMessage.Content,
                        UserFriendInfo = _userFunction.GetUserById(userFriend.FriendId),
                        Id = lastMessage.Id,
                        IsRead = lastMessage.IsRead,
                        SendDateTime = lastMessage.SendDateTime
                    });
                }
            }
            return result;
        }

        public async Task<IEnumerable<Message>> GetMessages(int fromUserId, int toUserId)
        {
            var entities = await _chatAppContext.TblMessages
                .Where(x => (x.FromUserId == fromUserId && x.ToUserId == toUserId)
                    || (x.FromUserId == toUserId && x.ToUserId == fromUserId))
                .OrderBy(x => x.SendDateTime)
                .ToListAsync();

            foreach (var entity in entities)
            {
                entity.IsRead = true;
            }

            await _chatAppContext.SaveChangesAsync();

            return entities.Select(x => new Message
            {
                Id = x.Id,
                Content = x.Content,
                FromUserId = x.FromUserId,
                ToUserId = x.ToUserId,
                SendDateTime = x.SendDateTime,
                IsRead = x.IsRead,
            });
        }

        public async Task MarkMessageIsRead(int fromUserId, int toUserId)
        {
            var entities = await _chatAppContext.TblMessages
                .Where(x => (x.FromUserId == fromUserId && x.ToUserId == toUserId))
                .ToListAsync();

            foreach (var entity in entities)
            {
                entity.IsRead = true;
            }

            await _chatAppContext.SaveChangesAsync();
        }
    }
}
