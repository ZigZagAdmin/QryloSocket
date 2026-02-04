namespace QryloSocketAPI.Utilities;

public class Utility
{    
    public static string GetMemberPermissionKey(string conversationId = "*", string memberId = "*", string permission = "*") => $"conversationMembers:{conversationId}:{memberId}:{permission}";

    public static long GetUnixTimestampMilliseconds(Guid uuid)
    {
        if(uuid == Guid.Empty) throw new ArgumentException("Uuid is empty!");
        
        Span<byte> bytes = stackalloc byte[16];
        uuid.TryWriteBytes(bytes, bigEndian: true, out _);

        return
            ((long)bytes[0] << 40) |
            ((long)bytes[1] << 32) |
            ((long)bytes[2] << 24) |
            ((long)bytes[3] << 16) |
            ((long)bytes[4] << 8)  |
            bytes[5];
    }
}