using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Localizations;

namespace TelegramBotBase.Controls.Inline;

[DebuggerDisplay("{Text}")]
public class Label : ControlBase
{
    private bool _renderNecessary = true;

    private string _text = Default.Language["Label_Text"];

    public String Text
    {
        get
        {
            return _text;
        }
        set
        {
            if (_text == value)
                return;

            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException($"{nameof(Text)}", $"{nameof(Text)} property must have been a value unequal to null/empty");
            }

            _text = value;
            _renderNecessary = true;

        }
    }



    private ParseMode _parseMode = ParseMode.Markdown;

    public ParseMode ParseMode
    {
        get
        {
            return _parseMode;
        }
        set
        {
            _parseMode = value;
            _renderNecessary = true;
        }
    }


    public Label()
    {
    }

    public Label(string text)
    {
        _text = text;
    }

    public Label(string text, ParseMode parseMode)
    {
        _text = text;
        _parseMode = parseMode;
    }

    public int? MessageId { get; set; }



    public override async Task Render(MessageResult result)
    {
        if (!_renderNecessary)
        {
            return;
        }

        Message m;

        //Update ?
        if (MessageId != null)
        {
            m = await Device.Raw(a => a.EditMessageTextAsync(Device.DeviceId, MessageId.Value, Text, _parseMode));
            _renderNecessary = false;

            return;
        }

        //New Message
        m = await Device.Raw(a => a.SendTextMessageAsync(Device.DeviceId, Text, disableNotification: true, parseMode: _parseMode));
        if (m != null)
        {
            MessageId = m.MessageId;
        }

        _renderNecessary = false;
    }

    public override async Task Cleanup()
    {
        if (this.MessageId == null)
            return;


        await Device.DeleteMessage(MessageId.Value);
    }
}