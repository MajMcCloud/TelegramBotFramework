using InlineAndReplyCombination.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Attributes;
using TelegramBotBase.Base;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.Form;

namespace InlineAndReplyCombination.Forms.Steps
{
    public class Summary : AutoCleanForm
    {
        [SaveState]
        public UserDetails UserDetails { get; set; }

        ButtonGrid ReplyButtonGrid { get; set; }

        public Summary()
        {
            Init += Summary_Init;
        
        }

        private async Task Summary_Init(object sender, TelegramBotBase.Args.InitEventArgs e)
        {
            var bf = new ButtonForm();

            bf.AddButtonRow("Go back", "back");
            bf.AddButtonRow("Return to Start", "start");



            ReplyButtonGrid = new ButtonGrid(bf);
            ReplyButtonGrid.Title = "Thank you for your time!";
            ReplyButtonGrid.ButtonClicked += ReplyButtonGrid_ButtonClicked;

            AddControl(ReplyButtonGrid);

        }

        public override async Task Load(MessageResult message)
        {

            if (UserDetails == null)
            {
                var sf = new StartForm();
                await NavigateTo(sf);
                return;
            }

            await Device.Send($"Your inputs are:\r\n\r\nYour age: {UserDetails.AgeRange}\r\nYour favourite color: {UserDetails.FavouriteColor}\r\nYour favourite city: {UserDetails.FavouriteCity}");
        }

        private async Task ReplyButtonGrid_ButtonClicked(object sender, TelegramBotBase.Args.ButtonClickedEventArgs e)
        {
            switch (e.Button.Value ?? "")
            {
                case "start":

                    var sf = new StartForm();

                    await NavigateTo(sf);

                    break;

                case "back":

                    var tf = new ThirdForm();

                    tf.UserDetails = UserDetails;

                    await NavigateTo(tf);

                    break;

            }

        }
    }
}
