using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using QryloSocketAPI.Models;
using QryloSocketAPI.Services;
using Serilog;
using Exception = System.Exception;

namespace QryloSocketAPI.Hubs;

public partial class SocketHub(
    IConversationsService conversationsService,
    IMessagesService messagesService,
    IConversationMemberService conversationMemberService,
    ICachingService<string> cachingService) : Hub
{
    private static readonly ConcurrentDictionary<string, string> ConnectedUsers = new();
    private static readonly ConcurrentDictionary<string, List<string>> ConnectedConversations = new();

    private static string UserConnection(string userId, string connectionId) =>
        $"user:{userId}:connection:{connectionId}";

    private static string ConversationConnection(string conversationId) => $"conversation:{conversationId}";

    public async Task Connect(Guid userId)
    {
        var conversations = await conversationsService.GetConversations(userId);
        if (cachingService.IsRedisConnected())
        {
            await cachingService.UpsertAsync(UserConnection(userId.ToString(), Context.ConnectionId), string.Empty,
                TimeSpan.FromHours(12));
            foreach (var conversation in conversations)
            {
                await cachingService.UpsertAsync(ConversationConnection(conversation), string.Empty,
                    TimeSpan.FromHours(12));
                await Groups.AddToGroupAsync(Context.ConnectionId, ConversationConnection(conversation));
            }
        }
        else
        {
            ConnectedUsers[Context.ConnectionId] = userId.ToString();
            foreach (var conversation in conversations)
            {
                if (ConnectedConversations.TryGetValue(conversation, out var connectedConversation))
                {
                    connectedConversation.Add(Context.ConnectionId);
                }
                else
                {
                    ConnectedConversations[conversation] = [Context.ConnectionId];
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, ConversationConnection(conversation));
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        if (cachingService.IsRedisConnected())
        {
            await cachingService.RemoveByPatternAsync(UserConnection("*", Context.ConnectionId));
        }
        else
        {
            ConnectedUsers.Remove(Context.ConnectionId, out _);
            ConnectedConversations.Remove(Context.ConnectionId, out _);
        }

        if (exception != null) Log.Fatal(exception.ToString());
        await base.OnDisconnectedAsync(exception);
    }
}