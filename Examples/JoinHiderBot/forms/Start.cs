using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace JoinHiderBot.forms
{
    public class Start :  SplitterForm
    {
        public override async Task<bool> Open(MessageResult e)
        {
            await this.Device.Send("This bot works only in groups.");

            return true;
        }

        public override async Task<bool> OpenGroup(MessageResult e)
        {
            var gmf = new GroupManageForm();

            await this.NavigateTo(gmf);

            return true;
        }

    }
}
