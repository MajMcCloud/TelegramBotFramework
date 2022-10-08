using System.Threading.Tasks;
using TelegramBotBase.Attributes;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace AsyncFormUpdates.forms
{
    public class AsyncFormUpdate : AutoCleanForm
    {
        [SaveState] private int _counter;


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

            await Device.Send($"Your current count is at: {_counter}", bf, disableNotification: true);
        }
    }
}