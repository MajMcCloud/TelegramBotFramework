using InlineAndReplyCombination.Forms;
using InlineAndReplyCombination.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Attributes;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.Form;

namespace InlineAndReplyCombination.Baseclasses
{
    public class MultipleChoiceForm : AutoCleanForm
    {
        [SaveState]
        public UserDetails UserDetails { get; set; }

        ButtonGrid ReplyButtonGrid;

        public String ReplyButtonTitle { get; set; } = "Restart";

        protected int CurrentStep = 1;
        protected int MaxSteps = 3;

        public MultipleChoiceForm()
        {
            this.Init += MultipleChoiceForm_Init;
        }

        private async Task MultipleChoiceForm_Init(object sender, TelegramBotBase.Args.InitEventArgs e)
        {
            //Reply keyboard
            var bf = new ButtonForm();

            bf.AddButtonRow(ReplyButtonTitle, "replyButtonID");

            ReplyButtonGrid = new ButtonGrid(bf);

            ReplyButtonGrid.Title = $"Step {CurrentStep} / {MaxSteps}";
            ReplyButtonGrid.KeyboardType = TelegramBotBase.Enums.EKeyboardType.ReplyKeyboard;

            ReplyButtonGrid.ButtonClicked += ReplyButtonGrid_ButtonClicked;

            AddControl(ReplyButtonGrid);
        }


        private async Task ReplyButtonGrid_ButtonClicked(object sender, TelegramBotBase.Args.ButtonClickedEventArgs e)
        {
            if (e.Button == null)
                return;

            switch (e.Button.Value)
            {
                case "replyButtonID":

                    await PressReplyButton();


                    break;

            }

        }


        public virtual Task PressReplyButton()
        {
            return Task.CompletedTask;
        }

        public override Task NavigateTo(FormBase newForm, params object[] args)
        {
            //Move user details over to navigating form
            if (newForm is MultipleChoiceForm mcf)
            {
                mcf.UserDetails = UserDetails;
            }

            return base.NavigateTo(newForm, args);
        }


    }
}
