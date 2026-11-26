using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBotBase.Base;

internal sealed class WebhookReceiver
{
    private const int MaxSecretLength = 256;

    private readonly string _url;
    private readonly string _secretToken;
    private readonly byte[] _secretBytes;
    private readonly int? _maxConnections;
    private readonly bool _deleteOnStop;
    private readonly Func<InputFileStream> _certificateFactory;

    private readonly Dictionary<long, DeviceUpdates> _devices = new();
    private bool _acceptingUpdates;
    private TaskCompletionSource<bool> _drained;

    internal WebhookReceiver(string url, string secretToken, int? maxConnections, bool deleteOnStop,
                             Func<InputFileStream> certificateFactory)
    {
        _url = url;
        _secretToken = secretToken;
        _secretBytes = Encoding.UTF8.GetBytes(secretToken ?? string.Empty);
        _maxConnections = maxConnections;
        _deleteOnStop = deleteOnStop;
        _certificateFactory = certificateFactory;
    }

    internal void Validate(ICollection<string> errors)
    {
        if (!Uri.TryCreate(_url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
        {
            errors.Add("Webhook URL must be an absolute HTTPS URL.");
        }

        if (string.IsNullOrEmpty(_secretToken) || _secretToken.Length > MaxSecretLength ||
            _secretToken.Any(character => character is not (>= 'A' and <= 'Z' or >= 'a' and <= 'z' or
                                                            >= '0' and <= '9' or '_' or '-')))
        {
            errors.Add("Webhook secret token must contain 1–256 characters from A–Z, a–z, 0–9, underscore or hyphen.");
        }

        if (_maxConnections is < 1 or > 100)
        {
            errors.Add("Webhook maximum connections must be between 1 and 100.");
        }
    }

    internal bool IsValidSecret(string secretToken)
    {
        return secretToken is { Length: > 0 and <= MaxSecretLength } &&
               CryptographicOperations.FixedTimeEquals(_secretBytes, Encoding.UTF8.GetBytes(secretToken));
    }

    internal async Task Start(MessageClient client, CancellationToken cancellationToken)
    {
        var certificate = _certificateFactory?.Invoke();
        using (certificate?.Content)
        {
            await client.TelegramClient.SetWebhook(_url, certificate: certificate, maxConnections: _maxConnections,
                                                   allowedUpdates: client.AllowedUpdates,
                                                   dropPendingUpdates: client.DropPendingUpdates,
                                                   secretToken: _secretToken, cancellationToken: cancellationToken);
        }

        lock (_devices)
        {
            _acceptingUpdates = true;
        }
    }

    internal Task Drain()
    {
        lock (_devices)
        {
            _acceptingUpdates = false;
            if (_devices.Count == 0)
            {
                return Task.CompletedTask;
            }

            _drained = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            return _drained.Task;
        }
    }

    internal Task DeleteIfConfigured(MessageClient client)
    {
        return _deleteOnStop ? client.TelegramClient.DeleteWebhook() : Task.CompletedTask;
    }

    internal async Task HandleUpdateAsync(MessageClient client, Update update, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = new UpdateResult(update, null);
        var deviceId = result.DeviceId;
        DeviceUpdates device;
        lock (_devices)
        {
            if (!_acceptingUpdates)
            {
                throw new InvalidOperationException("The bot is not receiving webhook updates.");
            }

            if (!_devices.TryGetValue(deviceId, out device))
            {
                device = new DeviceUpdates();
                _devices.Add(deviceId, device);
            }

            device.Pending++;
        }

        try
        {
            await device.Gate.WaitAsync(cancellationToken);
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                await client.OnMessageLoop(result);
            }
            finally
            {
                device.Gate.Release();
            }
        }
        finally
        {
            lock (_devices)
            {
                if (--device.Pending == 0)
                {
                    _devices.Remove(deviceId);
                    device.Gate.Dispose();
                }

                if (_devices.Count == 0 && _drained != null)
                {
                    _drained.SetResult(true);
                    _drained = null;
                }
            }
        }
    }

    private sealed class DeviceUpdates
    {
        public readonly SemaphoreSlim Gate = new(1, 1);

        public int Pending;
    }
}
