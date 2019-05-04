using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TelegramBotBase.Base;

namespace TelegramBotBase.Form
{
    /// <summary>
    /// A form which cleans up old messages sent within
    /// </summary>
    public class AutoCleanForm : FormBase
    {
        List<Message> OldMessages { get; set; }

        public eDeleteMode DeleteMode { get; set; }

        public eSide DeleteSide { get; set; }

        public enum eDeleteMode
        {
            None = 0,
            OnEveryCall = 1,
            OnLeavingForm = 2
        }

        public enum eSide
        {
            BotOnly = 0,
            UserOnly = 1,
            Both = 2
        }

        public AutoCleanForm()
        {
            this.OldMessages = new List<Message>();
            this.DeleteMode = eDeleteMode.OnEveryCall;
            this.DeleteSide = eSide.BotOnly;

        }

        public override async Task Init(params object[] args)
        {
            if (this.Device == null)
                return;

            this.Device.MessageSent += Device_MessageSent;

            this.Device.MessageReceived += Device_MessageReceived;
        }


        private void Device_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (this.DeleteSide == eSide.BotOnly)
                return;

            this.OldMessages.Add(e.Message);
        }

        private void Device_MessageSent(object sender, MessageSentEventArgs e)
        {
            if (this.DeleteSide == eSide.UserOnly)
                return;

            this.OldMessages.Add(e.Message);
        }

        public override async Task PreLoad(MessageResult message)
        {
            if (this.DeleteMode != eDeleteMode.OnEveryCall)
                return;

            await MessageCleanup();
        }

        /// <summary>
        /// Adds a message to this of removable ones
        /// </summary>
        /// <param name="Id"></param>
        public void AddMessage(Message m)
        {
            this.OldMessages.Add(m);
        }

        /// <summary>
        /// Keeps the message by removing it from the list
        /// </summary>
        /// <param name="Id"></param>
        public void LeaveMessage(int Id)
        {
            var m = this.OldMessages.FirstOrDefault(a => a.MessageId == Id);
            if (m == null)
                return;

            this.OldMessages.Remove(m);
        }

        /// <summary>
        /// Keeps the last sent message
        /// </summary>
        public void LeaveLastMessage()
        {
            if (this.OldMessages.Count == 0)
                return;

            this.OldMessages.RemoveAt(this.OldMessages.Count - 1);
        }

        public async override Task Closed()
        {
            if (this.DeleteMode != eDeleteMode.OnLeavingForm)
                return;

            await MessageCleanup();
        }


        /// <summary>
        /// Cleans up all remembered messages.
        /// </summary>
        /// <returns></returns>
        public async Task MessageCleanup()
        {
            while (this.OldMessages.Count > 0)
            {
                if (!await this.Device.DeleteMessage(this.OldMessages[0].MessageId))
                {
                    //Message can't be deleted cause it seems not to exist anymore
                    if (this.OldMessages.Count > 0)
                        this.OldMessages.RemoveAt(0);
                }
            }
        }
    }
}
