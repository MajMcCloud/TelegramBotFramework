// ClubMateBot demo - SKILL.md 7.3: a custom ControlBase and its full lifecycle
// (Init -> Load -> Action -> Render -> Hidden -> Cleanup).
using TelegramBotBase.Base;
using TelegramBotBase.Controls.Inline;
using TelegramBotBase.Form;

namespace ClubMateBot.Controls;

/// <summary>
///     A hand written control: the registration quota of one event.
///     <para>
///     <see cref="ControlBase"/> gives six overridable steps. The framework drives them from
///     <c>FormBase</c>: <see cref="Init"/> once when <c>AddControl</c> wires Id/Device,
///     <see cref="Load"/> before every action, <see cref="Action"/> for every update that belongs to
///     this control, <see cref="Render"/> after the action phase, <see cref="Hidden"/> when the form
///     is left or a modal opens, and <see cref="Cleanup"/> when the form is closed.
///     </para>
///     <para>
///     Buttons are created with <c>new ButtonForm(this)</c>. That stamps every callback with
///     <c>#c&lt;Id&gt;_&lt;value&gt;</c>, so <c>FormBase.ActionControls</c> routes the update to this
///     control only and hands the part after the underscore to <see cref="Action"/> as
///     <paramref name="value"/>.
///     </para>
/// </summary>
public sealed class QuotaControl : ControlBase
{
    private static readonly ProgressBar.EProgressStyle[] Styles =
    [
        ProgressBar.EProgressStyle.standard,
        ProgressBar.EProgressStyle.squares,
        ProgressBar.EProgressStyle.circles,
        ProgressBar.EProgressStyle.lines,
        ProgressBar.EProgressStyle.squaredLines,
        ProgressBar.EProgressStyle.custom
    ];

    private readonly List<string> _trace = [];

    //A control may own other controls. Nested controls do not take part in the form's message loop,
    //so this control drives the bar itself (see Render).
    private readonly ProgressBar _bar = new(0, 100, ProgressBar.EProgressStyle.squaredLines);

    private int _actionCalls;
    private int _capacity = 12;
    private int _cleanupCalls;
    private int _hiddenCalls;
    private int _initCalls;
    private int _loadCalls;
    private int? _messageId;
    private int _registered;
    private int _renderCalls;
    private int _styleIndex = 4;

    public QuotaControl(string title = "Event 'Summer open-air' - registration quota")
    {
        Title = title;
    }

    public string Title { get; }

    public int Registered => _registered;

    public int Capacity => _capacity;

    /// <summary>Lifecycle trace, newest entry last. Shown in the control's own message.</summary>
    public IReadOnlyList<string> Trace => _trace;

    public override void Init()
    {
        _initCalls++;

        //AddControl has assigned Device before calling Init, nested controls need it explicitly.
        _bar.Device = Device;

        Track(nameof(Init), $"control id {Id} ('{ControlId}'), device session attached");
    }

    public override Task Load(MessageResult result)
    {
        _loadCalls++;

        Track(nameof(Load), $"update from {result.DeviceId}");

        return Task.CompletedTask;
    }

    public override async Task Action(MessageResult result, string? value = null)
    {
        _actionCalls++;

        if (result.Handled)
        {
            return;
        }

        switch (value)
        {
            case "register":
                if (_registered >= _capacity)
                {
                    await result.ConfirmAction("Quota is full");
                    Track(nameof(Action), "register refused (quota full)");
                    return;
                }

                _registered++;
                break;

            case "cancel":
                if (_registered > 0)
                {
                    _registered--;
                }

                break;

            case "capacity":
                _capacity = _capacity >= 24 ? 4 : _capacity + 4;
                break;

            case "style":
                _styleIndex = (_styleIndex + 1) % Styles.Length;
                _bar.ProgressStyle = Styles[_styleIndex];

                //LoadStyle() resets the characters, so a custom style has to define them afterwards.
                if (_bar.ProgressStyle == ProgressBar.EProgressStyle.custom)
                {
                    _bar.BlockChar = "🍺";
                    _bar.EmptyBlockChar = "🎪";
                    _bar.StartChar = "[";
                    _bar.EndChar = "]";
                }

                break;

            case "enable":
                //ControlBase.Enabled makes the form skip Action and Render for this control.
                Enabled = !Enabled;
                break;

            default:
                return;
        }

        result.Handled = true;

        await result.ConfirmAction($"Quota: {_registered}/{_capacity}");

        Track(nameof(Action), $"value='{value}' handled by {ControlId}");
    }

    public override async Task Render(MessageResult result)
    {
        _renderCalls++;

        var text = $"{Title}\n\n" +
                   $"Registered: {_registered}/{_capacity}\n" +
                   $"Enabled: {Enabled}\n" +
                   $"Bar style: {_bar.ProgressStyle}\n\n" +
                   $"Lifecycle: Init={_initCalls} Load={_loadCalls} Action={_actionCalls} " +
                   $"Render={_renderCalls} Hidden={_hiddenCalls} Cleanup={_cleanupCalls}\n" +
                   $"Last steps: {string.Join(" | ", _trace.TakeLast(4))}";

        if (_messageId != null)
        {
            //Edit returns null when the message is gone (AutoCleanForm or manual delete) -> resend.
            var edited = await Device.Edit(_messageId.Value, text, BuildButtons());

            if (edited == null)
            {
                _messageId = null;
            }
            else
            {
                _messageId = edited.MessageId;
            }
        }

        if (_messageId == null)
        {
            var sent = await Device.Send(text, BuildButtons(), disableNotification: true);

            if (sent != null)
            {
                _messageId = sent.MessageId;
            }
        }

        //The nested ProgressBar keeps its own message and only edits it when the value changed.
        _bar.Max = 100;
        _bar.Value = _capacity == 0 ? 0 : (int)Math.Round(_registered * 100.0 / _capacity);

        await _bar.Render(result);
    }

    public override Task Hidden(bool formClose)
    {
        _hiddenCalls++;

        Track(nameof(Hidden), formClose ? "form closed" : "leaving form / modal opened");

        return Task.CompletedTask;
    }

    public override async Task Cleanup()
    {
        _cleanupCalls++;

        if (_messageId != null)
        {
            await Device.DeleteMessage(_messageId.Value);
            _messageId = null;
        }

        await _bar.Cleanup();

        Track(nameof(Cleanup), "messages removed, handler released");
    }

    /// <summary>
    ///     new ButtonForm(this) is what stamps the callbacks with this control's id.
    /// </summary>
    private ButtonForm BuildButtons()
    {
        var bf = new ButtonForm(this);

        bf.AddButtonRow(new ButtonBase("Register", "register"),
                        new ButtonBase("Cancel", "cancel"),
                        new ButtonBase($"Capacity {_capacity}", "capacity"));

        bf.AddButtonRow(new ButtonBase("Bar style", "style"),
                        new ButtonBase(Enabled ? "Disable control" : "Enable control", "enable"));

        return bf;
    }

    private void Track(string step, string detail)
    {
        _trace.Add($"{step}: {detail}");

        if (_trace.Count > 12)
        {
            _trace.RemoveAt(0);
        }
    }
}
