using System;

namespace TelegramBotBase.RequestDispatchers;

/// <summary>Configuration for throttling and concurrency of outgoing Telegram Bot API requests</summary>
public class RequestDispatcherSettings
{
    /// <summary>Global cap across all chats. Telegram allows ~30/sec</summary>
    public int MaxRequestsPerSecond { get; set; } = 30;
    
    /// <summary>
    /// Per-second cap for a single private chat. Telegram accepts bursts of ~30 messages to one user without
    /// a 429 and only starts delaying replies after that, so 5/sec keeps a wide margin while staying responsive
    /// </summary>
    public int MaxRequestsPerSecondPerChat { get; set; } = 5;

    /// <summary>
    /// Per-second cap for a single group, supergroup or channel. Telegram paces group messages at ~1/sec
    /// on its side, holding further requests open instead of answering them
    /// </summary>
    public int MaxRequestsPerSecondPerGroup { get; set; } = 1;
    
    /// <summary>
    /// Sliding-expiration idle timeout for cached per-chat rate limiters. A chat's limiter
    /// is evicted from memory if unused for this long; the next request for that chat
    /// allocates a fresh one. Bounds memory growth as the number of distinct chats grows
    /// </summary>
    public TimeSpan ChatLimiterCacheTimeout { get; set; } = TimeSpan.FromMinutes(5);
    
    /// <summary>Additional throttle for groups, supergroups and channels. Telegram allows ~20/min</summary>
    public int MaxRequestsPerMinutePerGroup { get; set; } = 20;
    
    /// <summary>
    /// Sliding-expiration idle timeout for cached per-group rate limiters. A group's limiter
    /// is evicted from memory if unused for this long; the next request for that group
    /// allocates a fresh one. Bounds memory growth as the number of distinct groups grows
    /// </summary>
    public TimeSpan GroupLimiterCacheTimeout { get; set; } = TimeSpan.FromMinutes(10);
    
    /// <summary>
    /// If true, HTTP 429 pauses ALL outgoing traffic for `retry_after` seconds.
    /// If false, only the offending dispatch is paused. Telegram's 429s are scoped to a single chat
    /// (e.g. too many requests queued for one group), so pausing everything penalizes unrelated chats
    /// </summary>
    public bool GlobalPauseOn429 { get; set; } = false;
    
    /// <summary>Whether to honor Telegram's `retry_after` value on HTTP 429 responses</summary>
    public bool RespectRetryAfterHeader { get; set; } = true;
    
    /// <summary>Max retry attempts for a message before it's dropped or dead-lettered</summary>
    public int MaxRetryAttempts { get; set; } = 3;
    
    /// <summary>Base delay used for exponential backoff when RespectRetryAfterHeader is false</summary>
    public TimeSpan FallbackRetryAfter { get; set; } = TimeSpan.FromSeconds(5);
    
    /// <summary>
    /// Max number of concurrent HTTP requests to the Bot API. Telegram may hold a throttled group request open
    /// for up to a minute, so a small pool lets a few busy groups stall every other chat
    /// </summary>
    public int MaxConcurrentRequests { get; set; } = 30;

    /// <summary>
    /// Time a queued request waits before it is promoted by one priority level, so low-priority
    /// requests cannot starve behind a steady stream of higher-priority ones. Promotion never lifts a
    /// request above <see cref="RequestPriority.UI"/>. Zero or negative disables aging
    /// </summary>
    public TimeSpan PriorityAgingInterval { get; set; } = TimeSpan.FromSeconds(2);
}