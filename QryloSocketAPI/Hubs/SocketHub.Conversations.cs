using Microsoft.AspNetCore.SignalR;
using QryloSocketAPI.Models;
using QryloSocketAPI.Utilities;
using QryloSocketAPI.Utilities.Enums;
using Exception = System.Exception;

namespace QryloSocketAPI.Hubs;

public partial class SocketHub
{
    public async Task CreateConversation(Guid userId,  string conversationName, Dictionary<Guid, ConversationPermissions> members, bool isPrivate)
    {
        try
        {
            var conversationId = await conversationsService.CreateConversation(userId, conversationName, members, isPrivate);
            await Clients.Users(members.Select(x => x.Key.ToString())).SendAsync(Topics.CONVERSATION_CREATED, new { conversationId, conversationName, isPrivate = conversationId });
            foreach (var member in members)
            {
                var keys = await cachingService.GetByPatternAsync(UserConnection(member.Key.ToString(), "*"));
                foreach (var key in keys)
                {
                    await Clients.Client(key.Split(':')[^1]).SendAsync(Topics.CONVERSATION_CREATED, new { conversationId, conversationName, isPrivate = conversationId });
                }
            }
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync(Topics.ERROR, ExceptionHandler.Exception(e));
        }
    }
    
    public async Task UpdateConversation(Guid userId, Guid conversationId, string conversationName)
    {
        try
        {
            await conversationsService.UpdateConversation(userId, conversationId, conversationName);
            await Clients.Group(ConversationConnection(conversationId.ToString())).SendAsync(Topics.CONVERSATION_UPDATED, new { conversationId, conversationName, isPrivate = conversationId });
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync(Topics.ERROR, ExceptionHandler.Exception(e));
        }
    }
    
    public async Task DeleteConversation(Guid userId, Guid conversationId )
    {
        try
        {
            await conversationsService.DeleteConversation(userId, conversationId);
            await Clients.Group(ConversationConnection(conversationId.ToString())).SendAsync(Topics.CONVERSATION_DELETED, new { conversationId });
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
}