using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Telegram.Bot;
using TelegramBotBase.Base;

namespace TelegramBotBase.Builder;

/// <summary>
///     Network settings of the bot, configured via <see cref="BotBaseBuilder.ConfigureNetworking" />.
///     Without any configuration the bot connects directly (no proxy) with a timeout of 60 seconds
///     and keeps pending updates.
/// </summary>
public sealed class NetworkingOptions
{
    /// <summary>
    ///     The default request timeout (60 seconds).
    /// </summary>
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(60);

    private ConnectionKind _kind = ConnectionKind.Direct;

    private string _proxyAddress;

    private NetworkCredential _proxyCredential;

    private string _proxyHost;

    private int _proxyPort;

    private HttpClient _httpClient;

    private TelegramBotClient _botClient;

    internal NetworkingOptions()
    {
    }

    /// <summary>
    ///     Timeout of the requests to the Telegram API. Defaults to <see cref="DefaultTimeout" />.
    /// </summary>
    public TimeSpan Timeout { get; set; } = DefaultTimeout;

    /// <summary>
    ///     If <c>true</c>, all updates that arrived while the bot was offline are dropped on start.
    ///     Defaults to <c>false</c>.
    /// </summary>
    public bool DropPendingUpdates { get; set; }

    /// <summary>
    ///     Connects through the proxy at the given address, i.e. <c>http://127.0.0.1:8080</c>.
    /// </summary>
    public NetworkingOptions UseProxy(string proxyAddress, NetworkCredential credential = null)
    {
        Reset(ConnectionKind.ProxyAddress);
        _proxyAddress = proxyAddress;
        _proxyCredential = credential;
        return this;
    }

    /// <summary>
    ///     Connects through the proxy at the given host and port.
    /// </summary>
    public NetworkingOptions UseProxy(string proxyHost, int proxyPort)
    {
        Reset(ConnectionKind.ProxyHostAndPort);
        _proxyHost = proxyHost;
        _proxyPort = proxyPort;
        return this;
    }

    /// <summary>
    ///     Sends all requests through the given <see cref="HttpClient" />.
    /// </summary>
    public NetworkingOptions UseHttpClient(HttpClient httpClient)
    {
        if (httpClient == null)
        {
            throw new ArgumentNullException(nameof(httpClient));
        }

        Reset(ConnectionKind.HttpClient);
        _httpClient = httpClient;
        return this;
    }

    /// <summary>
    ///     Uses an existing <see cref="TelegramBotClient" />. <see cref="Timeout" /> is still applied to it.
    /// </summary>
    public NetworkingOptions UseBotClient(TelegramBotClient botClient)
    {
        if (botClient == null)
        {
            throw new ArgumentNullException(nameof(botClient));
        }

        Reset(ConnectionKind.BotClient);
        _botClient = botClient;
        return this;
    }

    private void Reset(ConnectionKind kind)
    {
        _kind = kind;
        _proxyAddress = null;
        _proxyCredential = null;
        _proxyHost = null;
        _proxyPort = 0;
        _httpClient = null;
        _botClient = null;
    }

    internal void Validate(ICollection<string> errors)
    {
        if (Timeout <= TimeSpan.Zero && Timeout != System.Threading.Timeout.InfiniteTimeSpan)
        {
            errors.Add($"Networking: the timeout has to be positive, but was {Timeout}.");
        }

        switch (_kind)
        {
            case ConnectionKind.ProxyAddress:
                if (!Uri.TryCreate(_proxyAddress, UriKind.Absolute, out _))
                {
                    errors.Add($"Networking: the proxy address '{_proxyAddress}' is not a valid absolute URI.");
                }

                break;

            case ConnectionKind.ProxyHostAndPort:
                if (string.IsNullOrWhiteSpace(_proxyHost))
                {
                    errors.Add("Networking: the proxy host must not be empty.");
                }

                if (_proxyPort is < IPEndPoint.MinPort + 1 or > IPEndPoint.MaxPort)
                {
                    errors.Add($"Networking: the proxy port has to be between 1 and {IPEndPoint.MaxPort}, but was {_proxyPort}.");
                }

                break;
        }
    }

    internal MessageClient CreateClient(string apiKey, bool threadPool)
    {
        MessageClient client = _kind switch
        {
            ConnectionKind.ProxyAddress => threadPool
                                               ? new ThreadPoolMessageClient(apiKey, new Uri(_proxyAddress), _proxyCredential)
                                               : new MessageClient(apiKey, new Uri(_proxyAddress), _proxyCredential),
            ConnectionKind.ProxyHostAndPort => threadPool
                                                   ? new ThreadPoolMessageClient(apiKey, _proxyHost, _proxyPort)
                                                   : new MessageClient(apiKey, _proxyHost, _proxyPort),
            ConnectionKind.HttpClient => threadPool
                                             ? new ThreadPoolMessageClient(apiKey, _httpClient)
                                             : new MessageClient(apiKey, _httpClient),
            ConnectionKind.BotClient => threadPool
                                            ? new ThreadPoolMessageClient(apiKey, _botClient)
                                            : new MessageClient(apiKey, _botClient),
            _ => threadPool
                     ? new ThreadPoolMessageClient(apiKey)
                     : new MessageClient(apiKey)
        };

        client.TelegramClient.Timeout = Timeout;
        client.DropPendingUpdates = DropPendingUpdates;

        return client;
    }

    private enum ConnectionKind
    {
        Direct,
        ProxyAddress,
        ProxyHostAndPort,
        HttpClient,
        BotClient
    }
}
