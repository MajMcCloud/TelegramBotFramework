using TelegramBotBase.Base;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.Form;

namespace BotAndWebApplication.BotStuff
{
    public class StartForm : FormBase
    {
        ButtonGrid? _grid = null;

        int MyCounter { get; set; } = 0;

        public StartForm()
        {
            this.Opened += StartForm_Opened;
        }

        private async Task StartForm_Opened(object sender, EventArgs e)
        {
            await Device.Send("Welcome to StartForm !");

            var form = new ButtonForm();

            form.AddButtonRow(new ButtonBase("Increase!", "increase"));

            form.AddButtonRow(new ButtonBase("Decrease!", "decrease"));

            _grid = new ButtonGrid(form);
            _grid.KeyboardType = TelegramBotBase.Enums.EKeyboardType.InlineKeyBoard;
            _grid.ButtonClicked += _grid_ButtonClicked;

            _grid.Title = $"Increase your counter! (current {MyCounter})";

            AddControl(_grid);
        }

        private Task _grid_ButtonClicked(object sender, TelegramBotBase.Args.ButtonClickedEventArgs e)
        {
            if (e.Button == null || e.Button.Value == null)
                return Task.CompletedTask;

            switch (e.Button.Value)
            {
                case "increase":

                    MyCounter++;

                    _grid.Title = $"Increase your counter! (current {MyCounter})";
                    _grid.Updated();

                    break;
                case "decrease":

                    MyCounter--;

                    _grid.Title = $"Increase your counter! (current {MyCounter})";
                    _grid.Updated();

                    break;

            }

            return Task.CompletedTask;
        }
    }
}
