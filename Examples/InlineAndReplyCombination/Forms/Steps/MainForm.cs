using InlineAndReplyCombination.Baseclasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TelegramBotBase.Base;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.Form;

namespace InlineAndReplyCombination.Forms.Steps
{
    public class MainForm : MultipleChoiceForm
    {

        ButtonGrid InlineButtonGrid;

        public static List<Tuple<String, String>> AllowedInlineInputs = null;

        static MainForm()
        {
            AllowedInlineInputs = new List<Tuple<string, string>>()
            {
                new("< 18", "<18"),
                new("18 to 25", "18t25"),
                new("25 to 35", "25t35"),
                new("35 to 50", "35t50"),
                new("over 50", "o50")
            };

        }

        public MainForm()
        {

            Init += MainForm_Init;

            ReplyButtonTitle = "Start over";
            CurrentStep = 1;
        }

        private async Task MainForm_Init(object sender, TelegramBotBase.Args.InitEventArgs e)
        {

            //Inline Keyboard
            var bf_ages = new ButtonForm();

            //Add all options in a single column
            bf_ages.AddSplitted(AllowedInlineInputs.Select(a => new ButtonBase(a.Item1, a.Item2)), 1);

            bf_ages.AddButtonRow("Some invalid input", "Invalid");

            InlineButtonGrid = new ButtonGrid(bf_ages);
            InlineButtonGrid.ConfirmationText = "Thank you";
            InlineButtonGrid.Title = "Please choose your age:";
            InlineButtonGrid.ButtonClicked += InlineButtonGrid_ButtonClicked;

            InlineButtonGrid.KeyboardType = TelegramBotBase.Enums.EKeyboardType.InlineKeyBoard;

            AddControl(InlineButtonGrid);

        }

        private async Task InlineButtonGrid_ButtonClicked(object sender, TelegramBotBase.Args.ButtonClickedEventArgs e)
        {
            //Not found
            if (!AllowedInlineInputs.Any(a => a.Item2 == e.Button.Value))
            {
                await Device.Send("Invalid input!");
                return;
            }

            this.UserDetails.AgeRange = e.Button?.Value ?? "unknown";

            var sf = new SecondForm();

            await NavigateTo(sf);
        }


        public override async Task PressReplyButton()
        {
            var sf = new StartForm();

            await NavigateTo(sf);
        }
    }
}
