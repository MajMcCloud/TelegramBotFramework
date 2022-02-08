using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBaseTest.Tests
{
    public class Start : SplitterForm
    {
        public override async Task<bool> Open(MessageResult e)
        {
            var st = new Menu();
            await this.NavigateTo(st);

            return true;
        }


        public override async Task<bool> OpenGroup(MessageResult e)
        {
            var st = new Groups.LinkReplaceTest();
            await this.NavigateTo(st);

            return true;
        }

        public override Task<bool> OpenChannel(MessageResult e)
        {
            return base.OpenChannel(e);
        }

        public override Task<bool> OpenSupergroup(MessageResult e)
        {
            return base.OpenSupergroup(e);
        }

    }
}
