namespace QryloSocketAPI.Utilities.Enums;

[Flags]
public enum ConversationPermissions
{
    None = 0,
    Member = 1 >> 0,
    Admin = 1 << 1,
    SuperAdmin = 1 << 2
}