// ClubMateBot demo - SKILL.md 7.7 (ProgressBar): every style plus a live, non blocking update.
using Telegram.Bot.Types.Enums;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Controls.Inline;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;

namespace ClubMateBot.Forms;

/// <summary>
///     ProgressBar owns a message and only re-renders when Value, Max or ProgressStyle changed. A custom
///     style must set the characters AFTER assigning ProgressStyle, because assigning the style runs
///     LoadStyle() and resets them.
/// </summary>
public sealed class ProgressLabForm : AutoCleanForm
{
    private readonly List<ProgressBar> _bars = new();
    private Label _legend = null!;
    private ProgressBar _animated = null!;

    public ProgressLabForm()
    {
        DeleteMode = EDeleteMode.OnLeavingForm;
        DeleteSide = EDeleteSide.BotOnly;

        Init += OnInit;
        Opened += OnOpened;
    }

    private Task OnInit(object sender, InitEventArgs e)
    {
        AddBar(45, ProgressBar.EProgressStyle.standard);
        AddBar(45, ProgressBar.EProgressStyle.squares);
        AddBar(45, ProgressBar.EProgressStyle.circles);
        AddBar(45, ProgressBar.EProgressStyle.lines);
        AddBar(45, ProgressBar.EProgressStyle.squaredLines);

        var custom = new ProgressBar(45, 100, ProgressBar.EProgressStyle.custom);
        //LoadStyle() ran inside the constructor/property, so the characters have to come afterwards.
        custom.StartChar = "<";
        custom.BlockChar = "▰ ";
        custom.EmptyBlockChar = "▱ ";
        custom.EndChar = ">";
        AddBar(custom);

        _animated = new ProgressBar(0, 100, ProgressBar.EProgressStyle.squares);

        _legend = new Label(BuildLegend(), ParseMode.None);

        AddControl(_legend);
        AddControl(_animated);

        return Task.CompletedTask;
    }

    private void AddBar(int value, ProgressBar.EProgressStyle style)
    {
        var bar = new ProgressBar(value, 100, style);
        _bars.Add(bar);
        AddControl(bar);
    }

    private void AddBar(ProgressBar bar)
    {
        _bars.Add(bar);
        AddControl(bar);
    }

    private async Task OnOpened(object sender, EventArgs e)
    {
        //Navigation suppresses the message loop's render for this update; display the controls now.
        foreach (var control in Controls)
        {
            await control.Render(null!);
        }

        //The bars themselves have no buttons, so the form sends its own command row.
        var buttons = new ButtonForm();
        buttons.AddButtonRow(new ButtonBase("Run setup", "run"), new ButtonBase("Reset", "reset"));
        buttons.AddButtonRow(new ButtonBase("Back to gallery", "back"));

        await Device.Send("Progress bar controls", buttons, disableNotification: true,
                          parseMode: ParseMode.None);
    }

    public override async Task Action(MessageResult message)
    {
        switch (message.RawData)
        {
            case "back":
                message.Handled = true;
                await NavigateTo(new ControlsGalleryForm());
                return;
            case "run":
                message.Handled = true;
                await RunProgressAsync();
                return;
            case "reset":
                message.Handled = true;
                _animated.Value = 0;
                await _animated.Render(null!);
                return;
        }

        if (!message.Handled && message.RawData != "")
        {
            await message.ConfirmAction();
        }
    }

    private async Task RunProgressAsync()
    {
        _legend.Text = "Running the setup simulation...";

        //Steps is the granularity of the style: squares/circles = 10, lines = 5, standard = 1.
        var step = Math.Max(1, _animated.Steps) * 2;

        for (var value = 0; value <= 100; value += step)
        {
            _animated.Value = value;

            //Render explicitly instead of waiting for the next message loop - this is how long running
            //jobs report back. Never block the loop with Thread.Sleep, always await Task.Delay.
            await _animated.Render(null!);
            await Task.Delay(400);
        }

        _legend.Text = BuildLegend() + "\n\nSimulation finished.";
    }

    private string BuildLegend()
    {
        return "ProgressBar styles (all at 45%)\n" +
               $"1 standard     -> {_bars[0].Value}%\n" +
               $"2 squares      -> steps {_bars[1].Steps}\n" +
               $"3 circles      -> steps {_bars[2].Steps}\n" +
               $"4 lines        -> steps {_bars[3].Steps}\n" +
               $"5 squaredLines -> steps {_bars[4].Steps}\n" +
               "6 custom       -> characters set after the style\n\n" +
               "Last bar: animated by the button below.";
    }
}
