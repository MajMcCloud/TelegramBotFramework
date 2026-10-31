// ClubMateBot demo - SKILL.md 2/3 (builder stages) and 17 (hosting): startup configuration.
namespace ClubMateBot;

/// <summary>
///     Startup configuration. The API key only ever comes from the environment - it is never
///     hard-coded and never committed.
/// </summary>
public sealed class BotConfig
{
    /// <summary>
    ///     Name of the environment variable holding the bot token.
    ///     Set it with: $env:API_KEY = "123456:ABC..."
    /// </summary>
    public const string ApiKeyEnvironmentVariable = "API_KEY";

    public required string ApiKey { get; init; }

    /// <summary>Full file path of the JSON state file used by UseJSON.</summary>
    public string StateFile { get; init; } = Path.Combine(AppContext.BaseDirectory, "states.json");

    /// <summary>"de" or "en". Anything else falls back to the German default.</summary>
    public string Language { get; init; } = "de";

    /// <summary>Use UseThreadPool instead of UseSingleThread. All services must be thread safe then.</summary>
    public bool UseThreadPool { get; init; }

    /// <summary>Worker threads for UseThreadPool (0 keeps the library default).</summary>
    public int WorkerThreads { get; init; }

    /// <summary>I/O completion threads for UseThreadPool (0 keeps the library default).</summary>
    public int IoThreads { get; init; }

    /// <summary>Drop all updates that piled up while the bot was offline.</summary>
    public bool ThrowPendingUpdates { get; init; } = true;

    /// <summary>Push the command list to Telegram on startup (UploadBotCommands).</summary>
    public bool UploadCommands { get; init; } = true;

    /// <summary>
    ///     User ids allowed to use the bot. Empty means everybody - that is the default for a demo.
    ///     Used by <see cref="Middleware.AuthGate" />.
    /// </summary>
    public IReadOnlySet<long> AllowedUsers { get; init; } = new HashSet<long>();

    /// <summary>Print every incoming update type on the console.</summary>
    public bool Verbose { get; init; }

    /// <summary>
    ///     Reads API_KEY from the environment and applies the command line switches:
    ///     --lang=de|en, --threadpool[:w,io], --state=&lt;file&gt;, --users=1,2,3,
    ///     --keep-pending, --no-upload-commands, --verbose, --quickstart
    /// </summary>
    public static BotConfig FromEnvironment(string[] args)
    {
        var apiKey = Environment.GetEnvironmentVariable(ApiKeyEnvironmentVariable);

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                $"{ApiKeyEnvironmentVariable} is not set. Set the environment variable before starting, e.g. " +
                $"$env:{ApiKeyEnvironmentVariable} = \"<token from BotFather>\"");
        }

        var config = new BotConfig { ApiKey = apiKey };

        var allowed = new HashSet<long>();
        var language = config.Language;
        var stateFile = config.StateFile;
        var useThreadPool = config.UseThreadPool;
        var throwPending = config.ThrowPendingUpdates;
        var uploadCommands = config.UploadCommands;
        var verbose = config.Verbose;
        var workerThreads = 0;
        var ioThreads = 0;

        foreach (var raw in args)
        {
            // Strip the leading dashes so both "--lang=en" and "/lang=en" work.
            var arg = raw.TrimStart('-', '/');
            var separator = arg.IndexOf('=');
            var name = separator < 0 ? arg : arg[..separator];
            var value = separator < 0 ? "" : arg[(separator + 1)..];

            switch (name.ToLowerInvariant())
            {
                case "lang":
                    language = string.IsNullOrWhiteSpace(value) ? language : value.ToLowerInvariant();
                    break;
                case "state":
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        stateFile = value;
                    }

                    break;
                case "threadpool":
                    useThreadPool = true;

                    // Optional override: --threadpool=8,4
                    if (value.Contains(','))
                    {
                        var parts = value.Split(',');

                        int.TryParse(parts[0], out workerThreads);
                        int.TryParse(parts[1], out ioThreads);
                    }

                    break;
                case "users":
                    foreach (var id in value.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (long.TryParse(id.Trim(), out var userId))
                        {
                            allowed.Add(userId);
                        }
                    }

                    break;
                case "keep-pending":
                    throwPending = false;
                    break;
                case "no-upload-commands":
                    uploadCommands = false;
                    break;
                case "verbose":
                    verbose = true;
                    break;
                case "quickstart":
                    // Handled in Program - uses BotBaseBuilder.QuickStart<T>() instead of the full chain.
                    break;
                default:
                    Console.WriteLine($"[config] Ignoring unknown switch '{raw}'.");
                    break;
            }
        }

        return new BotConfig
        {
            ApiKey = apiKey,
            Language = language,
            StateFile = stateFile,
            UseThreadPool = useThreadPool,
            WorkerThreads = workerThreads,
            IoThreads = ioThreads,
            ThrowPendingUpdates = throwPending,
            UploadCommands = uploadCommands,
            AllowedUsers = allowed,
            Verbose = verbose
        };
    }
}
