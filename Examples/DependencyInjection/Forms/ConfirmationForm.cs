using DependencyInjection.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.DependencyInjection;

namespace DependencyInjection.Forms
{
    public class ConfirmationForm : FormBase
    {
        private readonly BotDbContext _dbContext;

        public ConfirmationForm(BotDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task Load(MessageResult message)
        {
            var user = await _dbContext.Users.FindAsync(Device.DeviceId);

            if (user == null)
            {
                await this.NavigateTo<StartForm>();
                return;
            }
        }

        public override async Task Action(MessageResult message)
        {
            await message.ConfirmAction("Go back");

            switch (message.RawData)
            {

                case "back":

                    await this.NavigateTo<StartForm>();

                    break;

            }


        }

        public override async Task Render(MessageResult message)
        {
            var user = await _dbContext.Users.FindAsync(Device.DeviceId);
            if (user == null)
                return;

            var bf = new ButtonForm();
            bf.AddButtonRow("Back", "back");

            await Device.Send($"ConfirmationForm: Your last message was: {user.LastMessage}. Click \"Back\" to get back.", bf);
        }




    }
}
