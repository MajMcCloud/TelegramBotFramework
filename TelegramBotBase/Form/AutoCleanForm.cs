using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TelegramBotBase.Base;

namespace TelegramBotBase.Form
{
    public class AutoCleanForm : FormBase
    {
        List<Message> OldMessages { get; set; }

        public eDeleteMode DeleteMode { get; set; }

        public enum eDeleteMode
        {
            OnEveryCall = 0,
            OnLeavingForm = 1
        }

        public AutoCleanForm()
        {
            this.OldMessages = new List<Message>();
            this.DeleteMode = eDeleteMode.OnEveryCall;

        }

        public override async Task Init(params object[] args)
        {
            if (this.Device == null)
                return;

            this.Device.MessageSent += Device_MessageSent;
        }

        private void Device_MessageSent(object sender, MessageSentEventArgs e)
        {
            this.OldMessages.Add(e.Message);
        }

        public override async Task PreLoad(MessageResult message)
        {
            if (this.DeleteMode != eDeleteMode.OnEveryCall)
                return;

            while (this.OldMessages.Count > 0)
            {
                if (!await this.Device.DeleteMessage(this.OldMessages[0].MessageId))
                {
                    //Nachricht konnte nicht gelöscht werden, vermutlich existiert diese nicht mehr
                    if (this.OldMessages.Count > 0)
                        this.OldMessages.RemoveAt(0);
                }
            }
        }

        /// <summary>
        /// Fügt eine Nachricht zu der Liste der löschenden hinzu.
        /// </summary>
        /// <param name="Id"></param>
        public void AddMessage(Message m)
        {
            this.OldMessages.Add(m);
        }

        /// <summary>
        /// Behält die Nachricht mit der angegebenen Id.
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
        /// Behält die zuletzt gesendete Nachricht.
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

            foreach (var m in this.OldMessages)
            {
                await this.Device.DeleteMessage(m.MessageId);
            }
        }
    }
}
