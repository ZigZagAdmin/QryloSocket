using Microsoft.AspNetCore.SignalR;
using QryloSocketAPI.Models;
using QryloSocketAPI.Utilities;
using Exception = System.Exception;

namespace QryloSocketAPI.Hubs;

public partial class SocketHub
{
    public async Task SendMessage(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn, string message, bool isAction)
    {
        try
        {
            await messagesService.SendMessage(userId, userCreatedOn, conversationId, conversationCreatedOn, message, isAction);
            await Clients.Group(ConversationConnection(conversationId.ToString())).SendAsync(Topics.MESSAGE_RECEIVED, new { userId, userCreatedOn, conversationId, conversationCreatedOn, message, isAction });
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync(Topics.ERROR, ExceptionHandler.Exception(e));
        }
    }
    
    public async Task DeleteMessage(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn, Guid messageId, long messageCreatedOn)
    {
        try
        {
            await messagesService.DeleteMessage(userId, userCreatedOn, conversationId, conversationCreatedOn, messageId, messageCreatedOn);
            await Clients.Group(ConversationConnection(conversationId.ToString())).SendAsync(Topics.MESSAGE_DELETED, new { userId, userCreatedOn, conversationId, conversationCreatedOn, messageId, messageCreatedOn });
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync(Topics.ERROR, ExceptionHandler.Exception(e));
        }
    }
    
    public async Task UpdateMessage(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn, Guid messageId, long messageCreatedOn, string message)
    {
        try
        {
            await messagesService.UpdateMessage(userId, userCreatedOn, conversationId, conversationCreatedOn, messageId, messageCreatedOn, message);
            await Clients.Group(ConversationConnection(conversationId.ToString())).SendAsync(Topics.MESSAGE_UPDATED, new { userId, userCreatedOn, conversationId, conversationCreatedOn, messageId, messageCreatedOn, message });
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync(Topics.ERROR, ExceptionHandler.Exception(e));
        }
    }
    
    public async Task ReadMessage(Guid userId, long userCreatedOn, Guid messageId, long messageCreatedOn, Guid conversationId, long conversationCreatedOn)
    {
        try  
        {
            await messagesService.ReadMessage(messageId, messageCreatedOn, userId, userCreatedOn);
            await Clients.Group(ConversationConnection(conversationId.ToString())).SendAsync(Topics.MESSAGE_READ, new { userId, userCreatedOn, conversationId, conversationCreatedOn, messageId, messageCreatedOn });
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync(Topics.ERROR, ExceptionHandler.Exception(e));
        }
    }
    
    public async Task ReportMessage(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn, Guid messageId, long messageCreatedOn, string comment)
    {
        try
        {
            await messagesService.ReportMessage(userId, userCreatedOn, conversationId, conversationCreatedOn, messageId, messageCreatedOn, comment);
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync(Topics.ERROR, ExceptionHandler.Exception(e));
        }
    }
    
    public async Task PinMessage(Guid userId, long userCreatedOn, Guid conversationId, long conversationCreatedOn, Guid messageId, long messageCreatedOn)
    {
        try
        {
            await messagesService.PinMessage(userId, userCreatedOn, conversationId, conversationCreatedOn, messageId, messageCreatedOn);
            await Clients.Group(ConversationConnection(conversationId.ToString())).SendAsync(Topics.MESSAGE_PINNED, new { userId, userCreatedOn, conversationId, conversationCreatedOn, messageId, messageCreatedOn });
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync(Topics.ERROR, ExceptionHandler.Exception(e));
        }
    }
}