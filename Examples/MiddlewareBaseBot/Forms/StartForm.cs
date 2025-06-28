using TelegramBotBase.Base;
using TelegramBotBase.Form;

internal sealed class StartForm : FormBase
{
    public override async Task PreLoad(MessageResult message)
    {
        await this.Device.Send("PreLoad");

        await Task.Delay(200);
    }

    public override async Task Load(MessageResult message)
    {
        await this.Device.Send("Load");

        await Task.Delay(200);
    }

    public override async Task Edited(MessageResult message)
    {
        await this.Device.Send("Edited");

        await Task.Delay(200);
    }

    public override async Task Action(MessageResult message)
    {
        await this.Device.Send("Action");

        await Task.Delay(200);
    }

    public override async Task SentData(DataResult data)
    {
        await this.Device.Send("SentData");

        await Task.Delay(200);
    }

    public override async Task Render(MessageResult message)
    {
        await this.Device.Send("Render");

        await Task.Delay(200);
    }
}