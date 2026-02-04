using Microsoft.AspNetCore.SignalR;
using QryloSocketAPI.Models;
using QryloSocketAPI.Utilities;
using Exception = System.Exception;

namespace QryloSocketAPI.Hubs;

public partial class SocketHub
{
    public async Task SendMessage(Guid userId, Guid conversationId, List<MessagePart> parts, bool isAction)
    {
        try
        {
            await messagesService.SendMessage(userId, conversationId, parts, isAction);
            await Clients.Group(ConversationConnection(conversationId.ToString())).SendAsync(Topics.MESSAGE_RECEIVED, new { userId, conversationId, parts, isAction });
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync(Topics.ERROR, ExceptionHandler.Exception(e));
        }
    }
    
    public async Task DeleteMessage(Guid userId, Guid conversationId, Guid messageId)
    {
        try
        {
            await messagesService.DeleteMessage(userId, conversationId, messageId);
            await Clients.Group(ConversationConnection(conversationId.ToString())).SendAsync(Topics.MESSAGE_DELETED, new { userId, conversationId, messageId });
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync(Topics.ERROR, ExceptionHandler.Exception(e));
        }
    }
    
    public async Task UpdateMessage(Guid userId, Guid conversationId, Guid messageId, List<MessagePart> parts)
    {
        try
        {
            await messagesService.UpdateMessage(userId, conversationId, messageId, parts);
            await Clients.Group(ConversationConnection(conversationId.ToString())).SendAsync(Topics.MESSAGE_UPDATED, new { userId, conversationId, messageId, parts });
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync(Topics.ERROR, ExceptionHandler.Exception(e));
        }
    }
    
    public async Task ReadMessage(Guid userId, Guid messageId, Guid conversationId)
    {
        try  
        {
            await messagesService.ReadMessage(messageId, userId);
            await Clients.Group(ConversationConnection(conversationId.ToString())).SendAsync(Topics.MESSAGE_READ, new { userId, conversationId, messageId });
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync(Topics.ERROR, ExceptionHandler.Exception(e));
        }
    }
    
    public async Task ReportMessage(Guid userId, Guid conversationId, Guid messageId, string comment)
    {
        try
        {
            await messagesService.ReportMessage(userId, conversationId, messageId, comment);
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync(Topics.ERROR, ExceptionHandler.Exception(e));
        }
    }
    
    public async Task PinMessage(Guid userId, Guid conversationId, Guid messageId)
    {
        try
        {
            await messagesService.PinMessage(userId, conversationId, messageId);
            await Clients.Group(ConversationConnection(conversationId.ToString())).SendAsync(Topics.MESSAGE_PINNED, new { userId, conversationId, messageId });
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync(Topics.ERROR, ExceptionHandler.Exception(e));
        }
    }
}