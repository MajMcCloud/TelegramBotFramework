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
    public class ThirdForm : MultipleChoiceForm
    {

        ButtonGrid? InlineButtonGrid;

        public static List<Tuple<String, String>> AllowedInlineInputs;

        static ThirdForm()
        {
            AllowedInlineInputs = new List<Tuple<string, string>>()
            {
                new("Berlin", "Berlin"),
                new("Vienna", "Vienna"),
                new("Rome", "Rome"),
                new("London", "London"),
                new("Moscow", "Moscow"),
                new("Bukarest", "Bukarest")
            };
        }

        public ThirdForm()
        {
            Init += SecondForm_Init;

            ReplyButtonTitle = "Go back";
            CurrentStep = 3;

        }

        private Task SecondForm_Init(object sender, TelegramBotBase.Args.InitEventArgs e)
        {

            //Inline Keyboard
            var bf_ages = new ButtonForm();

            //Add all options in a single column
            bf_ages.AddSplitted(AllowedInlineInputs.Select(a => new ButtonBase(a.Item1, a.Item2)), 1);

            InlineButtonGrid = new ButtonGrid(bf_ages);
            InlineButtonGrid.ConfirmationText = "Thank you";
            InlineButtonGrid.Title = "Please choose your favourite city:";
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

            UserDetails.FavouriteCity = e.Button?.Value ?? "unknown";

            var sum = new Summary();
            sum.UserDetails = this.UserDetails;
            await NavigateTo(sum);

        }


        public override async Task PressReplyButton()
        {
            var sf = new SecondForm();

            await NavigateTo(sf);
        }
    }
}
