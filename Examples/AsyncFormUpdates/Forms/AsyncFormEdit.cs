using TelegramBotBase.Attributes;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace AsyncFormUpdates.Forms;

public class AsyncFormEdit : FormBase
{
    [SaveState] private int _counter;

    private int _messageId;

    public override Task Load(MessageResult message)
    {
        _counter++;
        return Task.CompletedTask;
    }

    public override async Task Action(MessageResult message)
    {
        await message.ConfirmAction();

        switch (message.RawData ?? "")
        {
            case "back":
                var st = new Start();
                await NavigateTo(st);

                break;
        }
    }

    public override async Task Render(MessageResult message)
    {
        var bf = new ButtonForm();
        bf.AddButtonRow("Back", "back");

        if (_messageId != 0)
        {
            await Device.Edit(_messageId, $"Your current count is at: {_counter}", bf);
        }
        else
        {
            var m = await Device.Send($"Your current count is at: {_counter}", bf, disableNotification: true);
            _messageId = m.MessageId;
        }
    }
}
