using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBase.Example.Tests.Register;

public class Start : AutoCleanForm
{
    public override async Task Action(MessageResult message)
    {
        var call = message.GetData<CallbackData>();

        await message.ConfirmAction();


        if (call == null)
        {
            return;
        }

        switch (call.Value)
        {
            case "form":

                var form = new PerForm();

                await NavigateTo(form);

                break;
            case "step":

                var step = new PerStep();

                await NavigateTo(step);

                break;
            case "backtodashboard":

                var start = new Menu();

                await NavigateTo(start);

                break;
        }
    }

    public override async Task Render(MessageResult message)
    {
        var btn = new ButtonForm();

        btn.AddButtonRow(new ButtonBase("#4.1 Per Form", new CallbackData("a", "form").Serialize()));
        btn.AddButtonRow(new ButtonBase("#4.2 Per Step", new CallbackData("a", "step").Serialize()));
        btn.AddButtonRow(new ButtonBase("Back", new CallbackData("a", "backtodashboard").Serialize()));

        await Device.Send("Choose your test:", btn);
    }
}