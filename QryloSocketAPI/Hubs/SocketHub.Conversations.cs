using Microsoft.AspNetCore.SignalR;
using QryloSocketAPI.Models;
using QryloSocketAPI.Utilities;
using QryloSocketAPI.Utilities.Enums;
using Exception = System.Exception;

namespace QryloSocketAPI.Hubs;

public partial class SocketHub
{
    public async Task CreateConversation(Guid userId, long userCreatedOn, string conversationName, Dictionary<Guid, ConversationPermissions> members, bool isPrivate)
    {
        try
        {
            var conversationId = await conversationsService.CreateConversation(userId, userCreatedOn, conversationName, members, isPrivate);
            await Clients.Users(members.Select(x => x.Key.ToString())).SendAsync(Topics.CONVERSATION_CREATED, new { conversationId, conversationName, isPrivate = conversationId });
            // foreach (var member in members)
            // {
            //     var keys = await cachingService.GetByPatternAsync(UserConnection(member.Key.ToString(), "*"));
            //     foreach (var key in keys)
            //     {
            //         await Clients.Client(key.Split(':')[^1]).SendAsync(Topics.CONVERSATION_CREATED, new { conversationId, conversationName, isPrivate = conversationId });
            //     }
            // }
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync(Topics.ERROR, ExceptionHandler.Exception(e));
        }
    }
    
    public async Task UpdateConversation(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn, string conversationName)
    {
        try
        {
            await conversationsService.UpdateConversation(userId, userCreatedOn, conversationId, conversationCreatedOn, conversationName);
            await Clients.Group(ConversationConnection(conversationId.ToString())).SendAsync(Topics.CONVERSATION_UPDATED, new { conversationId, conversationName, isPrivate = conversationId });
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync(Topics.ERROR, ExceptionHandler.Exception(e));
        }
    }
    
    public async Task DeleteConversation(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn)
    {
        try
        {
            await conversationsService.DeleteConversation(userId, userCreatedOn, conversationId, conversationCreatedOn);
            await Clients.Group(ConversationConnection(conversationId.ToString())).SendAsync(Topics.CONVERSATION_DELETED, new { conversationId, conversationCreatedOn });
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync(Topics.ERROR, ExceptionHandler.Exception(e));
        }
    }

    public async Task ConnectToConversation(Guid conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, ConversationConnection(conversationId.ToString()));
    }
    
    public async Task AddMember(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn, Guid memberId, long memberCreatedOn, ConversationPermissions permission)
    {
        try
        {
            await conversationsService.AddMember(userId, userCreatedOn, conversationId, conversationCreatedOn, memberId, memberCreatedOn, permission);
            await SendMessage(userId, userCreatedOn, conversationId, conversationCreatedOn, "User invited", true);
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync(Topics.ERROR, ExceptionHandler.Exception(e));
        }
    }
    
    public async Task RemoveMember(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn, Guid memberId, long memberCreatedOn)
    {
        try
        {
            await conversationsService.RemoveMember(userId, userCreatedOn, conversationId, conversationCreatedOn, memberId, memberCreatedOn);
            await SendMessage(userId, userCreatedOn, conversationId, conversationCreatedOn, "User removed", true);
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync(Topics.ERROR, ExceptionHandler.Exception(e));
        }
    }
    
    public async Task BlockMember(Guid userId, long userCreatedOn, Guid memberId, long createdOn, Guid conversationId, long conversationCreatedOn, Guid requesterId, long requesterCreatedOn)
    {
        try
        {
            await conversationsService.BlockMember(memberId, createdOn, requesterId, requesterCreatedOn);
            var keys = await cachingService.GetByPatternAsync(UserConnection(userId.ToString(), "*"));
            foreach (var key in keys)
            {
                await Clients.Client(key.Split(':')[^1]).SendAsync(Topics.MEMBER_BLOCKED, new { conversationId, conversationCreatedOn });
            }
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync(Topics.ERROR, ExceptionHandler.Exception(e));
        }
    }
}