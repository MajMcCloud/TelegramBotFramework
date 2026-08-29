using System;

namespace TelegramBotBase.RequestDispatchers;

/// <summary>Configuration for throttling and concurrency of outgoing Telegram Bot API requests</summary>
public class RequestDispatcherSettings
{
    /// <summary>Global cap across all chats. Telegram allows ~30/sec</summary>
    public int MaxRequestsPerSecond { get; set; } = 30;
    
    /// <summary>Per-chat cap (applies to private chats, groups, channels alike). Telegram allows 1/sec</summary>
    public int MaxRequestsPerSecondPerChat { get; set; } = 1;
    
    /// <summary>
    /// Sliding-expiration idle timeout for cached per-chat rate limiters. A chat's limiter
    /// is evicted from memory if unused for this long; the next request for that chat
    /// allocates a fresh one. Bounds memory growth as the number of distinct chats grows
    /// </summary>
    public TimeSpan ChatLimiterCacheTimeout { get; set; } = TimeSpan.FromMinutes(5);
    
    /// <summary>Additional throttle specifically for groups/supergroups. Telegram allows ~20/min</summary>
    public int MaxRequestsPerMinutePerGroup { get; set; } = 20;
    
    /// <summary>
    /// Sliding-expiration idle timeout for cached per-group rate limiters. A group's limiter
    /// is evicted from memory if unused for this long; the next request for that group
    /// allocates a fresh one. Bounds memory growth as the number of distinct groups grows
    /// </summary>
    public TimeSpan GroupLimiterCacheTimeout { get; set; } = TimeSpan.FromMinutes(10);
    
    /// <summary>
    /// If true, HTTP 429 pauses ALL outgoing traffic for `retry_after` seconds.
    /// If false, only the offending dispatch is paused
    /// </summary>
    public bool GlobalPauseOn429 { get; set; } = true;
    
    /// <summary>Whether to honor Telegram's `retry_after` value on HTTP 429 responses</summary>
    public bool RespectRetryAfterHeader { get; set; } = true;
    
    /// <summary>Max retry attempts for a message before it's dropped or dead-lettered</summary>
    public int MaxRetryAttempts { get; set; } = 3;
    
    /// <summary>Base delay used for exponential backoff when RespectRetryAfterHeader is false</summary>
    public TimeSpan FallbackRetryAfter { get; set; } = TimeSpan.FromSeconds(3);
    
    /// <summary>Max number of concurrent HTTP requests to the Bot API</summary>
    public int MaxConcurrentRequests { get; set; } = 10;
}