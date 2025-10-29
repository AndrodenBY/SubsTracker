namespace SubsTracker.BLL.RedisSettings;

public static class RedisConstants
{
    public static readonly TimeSpan LockExpiry = TimeSpan.FromSeconds(10);
    public static readonly TimeSpan LockWaitTime = TimeSpan.FromSeconds(5);
    public static readonly TimeSpan LockRetryTime = TimeSpan.FromMilliseconds(200);
    public static readonly TimeSpan ExpirationTime = TimeSpan.FromMinutes(3);
}