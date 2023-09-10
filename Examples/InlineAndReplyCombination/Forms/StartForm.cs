using InlineAndReplyCombination.Baseclasses;
using InlineAndReplyCombination.Forms.Steps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.Form;

namespace InlineAndReplyCombination.Forms
{
    public class StartForm : AutoCleanForm
    {
        ButtonGrid buttonGrid;

        public StartForm()
        {
            this.Init += StartForm_Init;

        }

        private async Task StartForm_Init(object sender, TelegramBotBase.Args.InitEventArgs e)
        {
            var bf = new ButtonForm();

            bf.AddButtonRow("Start registration", "start");

            buttonGrid = new ButtonGrid(bf);

            buttonGrid.Title = "Welcome to The InlineAndReplyCombination Bot!";
            buttonGrid.ButtonClicked += ButtonGrid_ButtonClicked;
            buttonGrid.KeyboardType = TelegramBotBase.Enums.EKeyboardType.ReplyKeyboard;

            AddControl(buttonGrid);

        }

        private async Task ButtonGrid_ButtonClicked(object sender, TelegramBotBase.Args.ButtonClickedEventArgs e)
        {
            if(e.Button == null)
            {
                return;
            }


            switch(e.Button.Value)
            {
                case "start":

                    var mf = new MainForm();

                    mf.UserDetails = new Model.UserDetails();

                    await NavigateTo(mf);

                    break;

            }


        }


    }
}
