namespace QryloSocketAPI.Models;

public record Topics
{
    public const string ERROR = "Error";
    // Conversations   
    public const string CONVERSATION_CREATED = "ConversationCreated";
    public const string CONVERSATION_UPDATED= "ConversationUpdated";
    public const string CONVERSATION_DELETED = "ConversationDeleted";
    public const string MEMBER_INVITED = "MemberInvited";
    public const string MEMBER_REMOVED = "MemberRemoved";
    public const string MEMBER_BLOCKED = "MemberBlocked";
    public const string MEMBER_UPDATED = "MemberUpdated";

    // Messages
    public const string MESSAGE_RECEIVED = "MessageReceived";
    public const string MESSAGE_UPDATED = "MessageUpdated";
    public const string MESSAGE_DELETED = "MessageDeleted";
    public const string MESSAGE_DELIVERED = "MessageDelivered";
    public const string MESSAGE_PINNED = "MessagePinned";
    public const string MESSAGE_READ = "MessageRead";
}