using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;
using TelegramBotBaseTest.Tests.Controls.Subclass;

namespace TelegramBotBaseTest.Tests.Controls
{
    public class MultiViewForm : AutoCleanForm
    {
        private MultiViewTest _mvt;

        private ButtonGrid _bg;

        public MultiViewForm()
        {
            DeleteMode = EDeleteMode.OnLeavingForm;
            Init += MultiViewForm_Init;
        }

        private Task MultiViewForm_Init(object sender, InitEventArgs e)
        {
            _mvt = new MultiViewTest();

            AddControl(_mvt);

            _bg = new ButtonGrid
            {
                ButtonsForm = new ButtonForm()
            };
            _bg.ButtonsForm.AddButtonRow("Back", "$back$");
            _bg.ButtonClicked += Bg_ButtonClicked;
            _bg.KeyboardType = EKeyboardType.ReplyKeyboard;
            AddControl(_bg);
            return Task.CompletedTask;
        }

        private async Task Bg_ButtonClicked(object sender, ButtonClickedEventArgs e)
        {
            switch(e.Button.Value)
            {
                case "$back$":

                    var mn = new Menu();
                    await NavigateTo(mn);

                    break;
            }
        }


    }
}
