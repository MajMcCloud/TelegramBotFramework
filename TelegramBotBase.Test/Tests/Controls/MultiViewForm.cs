using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.Form;

namespace TelegramBotBaseTest.Tests.Controls
{
    public class MultiViewForm : AutoCleanForm
    {

        Subclass.MultiViewTest mvt = null;

        ButtonGrid bg = null;

        public MultiViewForm()
        {
            this.DeleteMode = TelegramBotBase.Enums.eDeleteMode.OnLeavingForm;
            this.Init += MultiViewForm_Init;
        }

        private async Task MultiViewForm_Init(object sender, TelegramBotBase.Args.InitEventArgs e)
        {
            mvt = new Subclass.MultiViewTest();

            AddControl(mvt);

            bg = new ButtonGrid();
            bg.ButtonsForm = new ButtonForm();
            bg.ButtonsForm.AddButtonRow("Back", "$back$");
            bg.ButtonClicked += Bg_ButtonClicked;
            bg.KeyboardType = TelegramBotBase.Enums.eKeyboardType.ReplyKeyboard;
            AddControl(bg);
        }

        private async Task Bg_ButtonClicked(object sender, TelegramBotBase.Args.ButtonClickedEventArgs e)
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
