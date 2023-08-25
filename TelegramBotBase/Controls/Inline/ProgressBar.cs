using System;
using System.Threading.Tasks;
using TelegramBotBase.Base;

namespace TelegramBotBase.Controls.Inline;

/// <summary>
///     A simple control for show and managing progress.
/// </summary>
public class ProgressBar : ControlBase
{
    public enum EProgressStyle
    {
        standard = 0,
        squares = 1,
        circles = 2,
        lines = 3,
        squaredLines = 4,
        custom = 10
    }

    private EProgressStyle _mEStyle = EProgressStyle.standard;

    private int _mIMax = 100;

    private int _mIValue;

    public ProgressBar()
    {
        ProgressStyle = EProgressStyle.standard;

        Value = 0;
        Max = 100;

        RenderNecessary = true;
    }

    public ProgressBar(int value, int max, EProgressStyle style)
    {
        Value = value;
        Max = max;
        ProgressStyle = style;

        RenderNecessary = true;
    }

    public EProgressStyle ProgressStyle
    {
        get => _mEStyle;
        set
        {
            _mEStyle = value;
            LoadStyle();
        }
    }


    public int Value
    {
        get => _mIValue;
        set
        {
            if (value > Max)
            {
                return;
            }

            if (_mIValue != value)
            {
                RenderNecessary = true;
            }

            _mIValue = value;
        }
    }

    public int Max
    {
        get => _mIMax;
        set
        {
            if (_mIMax != value)
            {
                RenderNecessary = true;
            }

            _mIMax = value;
        }
    }

    public int? MessageId { get; set; }

    private bool RenderNecessary { get; set; }

    public int Steps
    {
        get
        {
            return ProgressStyle switch
            {
                EProgressStyle.standard => 1,
                EProgressStyle.squares => 10,
                EProgressStyle.circles => 10,
                EProgressStyle.lines => 5,
                EProgressStyle.squaredLines => 5,
                _ => 1
            };
        }
    }

    /// <summary>
    ///     Filled block (reached percentage)
    /// </summary>
    public string BlockChar { get; set; }

    /// <summary>
    ///     Unfilled block (not reached yet)
    /// </summary>
    public string EmptyBlockChar { get; set; }

    /// <summary>
    ///     String at the beginning of the progress bar
    /// </summary>
    public string StartChar { get; set; }

    /// <summary>
    ///     String at the end of the progress bar
    /// </summary>
    public string EndChar { get; set; }

    public override async Task Cleanup()
    {
        if (MessageId == null || MessageId == -1)
        {
            return;
        }


        await Device.DeleteMessage(MessageId.Value);
    }

    public void LoadStyle()
    {
        StartChar = "";
        EndChar = "";

        switch (ProgressStyle)
        {
            case EProgressStyle.circles:

                BlockChar = "⚫️ ";
                EmptyBlockChar = "⚪️ ";

                break;
            case EProgressStyle.squares:

                BlockChar = "⬛️ ";
                EmptyBlockChar = "⬜️ ";

                break;
            case EProgressStyle.lines:

                BlockChar = "█";
                EmptyBlockChar = "▁";

                break;
            case EProgressStyle.squaredLines:

                BlockChar = "▇";
                EmptyBlockChar = "—";

                StartChar = "[";
                EndChar = "]";

                break;
            case EProgressStyle.standard:
            case EProgressStyle.custom:

                BlockChar = "";
                EmptyBlockChar = "";

                break;
        }
    }

    public override async Task Render(MessageResult result)
    {
        if (!RenderNecessary)
        {
            return;
        }

        if (Device == null)
        {
            return;
        }

        var message = "";
        var blocks = 0;
        var maxBlocks = 0;

        switch (ProgressStyle)
        {
            case EProgressStyle.standard:

                message = Value.ToString("0") + "%";

                break;

            case EProgressStyle.squares:
            case EProgressStyle.circles:
            case EProgressStyle.lines:
            case EProgressStyle.squaredLines:
            case EProgressStyle.custom:

                blocks = (int)Math.Floor((decimal)Value / Steps);

                maxBlocks = Max / Steps;

                message += StartChar;

                for (var i = 0; i < blocks; i++)
                {
                    message += BlockChar;
                }

                for (var i = 0; i < maxBlocks - blocks; i++)
                {
                    message += EmptyBlockChar;
                }

                message += EndChar;

                message += " " + Value.ToString("0") + "%";

                break;

            default:

                return;
        }

        if (MessageId == null)
        {
            var m = await Device.Send(message);

            MessageId = m.MessageId;
        }
        else
        {
            await Device.Edit(MessageId.Value, message);
        }

        RenderNecessary = false;
    }
}