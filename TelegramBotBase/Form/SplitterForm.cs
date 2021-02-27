using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;


namespace TelegramBotBase.Form
{
    /// <summary>
    /// This is used to split incomming requests depending on the chat type.
    /// </summary>
    public class SplitterForm : FormBase
    {

        private static object __evOpenSupergroup = new object();
        private static object __evOpenGroup = new object();
        private static object __evOpenChannel = new object();
        private static object __evOpen = new object();


        public override async Task Load(MessageResult message)
        {

            if (message.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Channel)
            {
                if (await OpenChannel(message))
                {
                    return;
                }
            }
            if (message.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Supergroup)
            {
                if (await OpenSupergroup(message))
                {
                    return;
                }
                if (await OpenGroup(message))
                {
                    return;
                }
            }
            if (message.Message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Group)
            {
                if (await OpenGroup(message))
                {
                    return;
                }
            }

            await Open(message);
        }


        public virtual async Task<bool> OpenSupergroup(MessageResult e)
        {
            return false;
        }

        public virtual async Task<bool> OpenChannel(MessageResult e)
        {
            return false;
        }

        public virtual async Task<bool> Open(MessageResult e)
        {
            return false;
        }

        public virtual async Task<bool> OpenGroup(MessageResult e)
        {
            return false;
        }




        public override Task Action(MessageResult message)
        {
            return base.Action(message);
        }

        public override Task PreLoad(MessageResult message)
        {
            return base.PreLoad(message);
        }

        public override Task Render(MessageResult message)
        {
            return base.Render(message);
        }

        public override Task SentData(DataResult message)
        {
            return base.SentData(message);
        }

    }
}
