using Microsoft.AspNetCore.SignalR;
using QryloSocketAPI.Models;
using QryloSocketAPI.Utilities;
using QryloSocketAPI.Utilities.Enums;
using Exception = System.Exception;

namespace QryloSocketAPI.Hubs;

public partial class SocketHub
{
     public async Task AddMember(Guid userId, Guid conversationId, Guid memberId,  ConversationPermissions permission)
    {
        try
        {
            await conversationMemberService.AddMember(userId, conversationId, memberId, permission);
            await SendMessage(userId, conversationId, [new MessagePart(MessagePartTypes.Text, 1, "User invited")], true);
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync(Topics.ERROR, ExceptionHandler.Exception(e));
        }
    }
    
    public async Task UpdateMemberPermission(Guid userId, Guid conversationId, Guid memberId, ConversationPermissions permission)
    {
        try
        {
            await conversationMemberService.UpdateMemberPermission(userId, conversationId, memberId, permission);
            var keys = await cachingService.GetByPatternAsync(UserConnection(userId.ToString(), "*"));
            foreach (var key in keys)
            {
                await Clients.Client(key.Split(':')[^1]).SendAsync(Topics.MEMBER_UPDATED, new { conversationId, isPrivate = conversationId });
            }
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync(Topics.ERROR, ExceptionHandler.Exception(e));
        }
    }
    
    public async Task RemoveMember(Guid userId, Guid conversationId, Guid memberId)
    {
        try
        {
            await conversationMemberService.RemoveMember(userId, conversationId, memberId);
            await SendMessage(userId, conversationId, [new MessagePart(MessagePartTypes.Text, 1, "User removed")], true);
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync(Topics.ERROR, ExceptionHandler.Exception(e));
        }
    }
    
    public async Task BlockMember(Guid userId, Guid memberId, Guid conversationId, Guid requesterId)
    {
        try
        {
            await conversationMemberService.BlockMember(conversationId, memberId, requesterId);
            var keys = await cachingService.GetByPatternAsync(UserConnection(userId.ToString(), "*"));
            foreach (var key in keys)
            {
                await Clients.Client(key.Split(':')[^1]).SendAsync(Topics.MEMBER_BLOCKED, new { conversationId });
            }
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync(Topics.ERROR, ExceptionHandler.Exception(e));
        }
    }
}