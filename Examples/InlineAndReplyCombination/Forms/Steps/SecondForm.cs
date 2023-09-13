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
    public class SecondForm : MultipleChoiceForm
    {

        ButtonGrid? InlineButtonGrid;

        public static List<Tuple<String, String>> AllowedInlineInputs;

        static SecondForm()
        {
            AllowedInlineInputs = new List<Tuple<string, string>>()
            {
                new("Green", "green"),
                new("Yellow", "yellow"),
                new("Red", "red"),
                new("Purple", "purple"),
                new("Black", "black"),
                new("Gold", "gold")
            };
        }

        public SecondForm()
        {

            Init += SecondForm_Init;

            ReplyButtonTitle = "Go back";
            CurrentStep = 2;

        }

        private Task SecondForm_Init(object sender, TelegramBotBase.Args.InitEventArgs e)
        {

            //Inline Keyboard
            var bf_ages = new ButtonForm();

            //Add all options in a single column
            bf_ages.AddSplitted(AllowedInlineInputs.Select(a => new ButtonBase(a.Item1, a.Item2)), 1);

            InlineButtonGrid = new ButtonGrid(bf_ages);
            InlineButtonGrid.ConfirmationText = "Thank you";
            InlineButtonGrid.Title = "Please choose your favourite color:";
            InlineButtonGrid.ButtonClicked += InlineButtonGrid_ButtonClicked;

            InlineButtonGrid.KeyboardType = TelegramBotBase.Enums.EKeyboardType.InlineKeyBoard;

            AddControl(InlineButtonGrid);

            return Task.CompletedTask;
        }

        private async Task InlineButtonGrid_ButtonClicked(object sender, TelegramBotBase.Args.ButtonClickedEventArgs e)
        {
            //Not found
            if (!AllowedInlineInputs.Any(a => a.Item2 == e.Button.Value))
            {
                await Device.Send("Invalid input!");
                return;
            }

            if (UserDetails == null)
            {
                return;
            }

            UserDetails.FavouriteColor = e.Button?.Value ?? "unknown";

            var tf = new ThirdForm();

            await NavigateTo(tf);
        }


        public override async Task PressReplyButton()
        {
            var mf = new MainForm();

            await NavigateTo(mf);
        }
    }
}
