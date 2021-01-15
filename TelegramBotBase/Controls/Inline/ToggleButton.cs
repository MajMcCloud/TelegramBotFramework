using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBase.Controls.Inline
{
    public class ToggleButton : ControlBase
    {

        public String UncheckedIcon { get; set; } = Localizations.Default.Language["ToggleButton_OffIcon"];

        public String CheckedIcon { get; set; } = Localizations.Default.Language["ToggleButton_OnIcon"];

        public String CheckedString { get; set; } = Localizations.Default.Language["ToggleButton_On"];

        public String UncheckedString { get; set; } = Localizations.Default.Language["ToggleButton_Off"];

        public String ChangedString { get; set; } = Localizations.Default.Language["ToggleButton_Changed"];

        public String Title { get; set; } = Localizations.Default.Language["ToggleButton_Title"];

        public int? MessageId { get; set; }

        public bool Checked { get; set; }

        private bool RenderNecessary = true;

        private static readonly object __evToggled = new object();

        private readonly EventHandlerList Events = new EventHandlerList();


        public ToggleButton()
        {


        }

        public ToggleButton(String CheckedString, String UncheckedString)
        {
            this.CheckedString = CheckedString;
            this.UncheckedString = UncheckedString;
        }

        public event EventHandler Toggled
        {
            add
            {
                this.Events.AddHandler(__evToggled, value);
            }
            remove
            {
                this.Events.RemoveHandler(__evToggled, value);
            }
        }

        public void OnToggled(EventArgs e)
        {
            (this.Events[__evToggled] as EventHandler)?.Invoke(this, e);
        }

        public override async Task Action(MessageResult result, String value = null)
        {

            if (result.Handled)
                return;

            await result.ConfirmAction(this.ChangedString);

            switch (value ?? "unknown")
            {
                case "on":

                    if (this.Checked)
                        return;

                    RenderNecessary = true;

                    this.Checked = true;

                    OnToggled(new EventArgs());

                    break;

                case "off":

                    if (!this.Checked)
                        return;

                    RenderNecessary = true;

                    this.Checked = false;

                    OnToggled(new EventArgs());

                    break;

                default:

                    RenderNecessary = false;

                    break;

            }

            result.Handled = true;

        }

        public override async Task Render(MessageResult result)
        {
            if (!RenderNecessary)
                return;

            var bf = new ButtonForm(this);

            ButtonBase bOn = new ButtonBase((this.Checked ? CheckedIcon : UncheckedIcon) + " " + CheckedString, "on");

            ButtonBase bOff = new ButtonBase((!this.Checked ? CheckedIcon : UncheckedIcon) + " " + UncheckedString, "off");

            bf.AddButtonRow(bOn, bOff);

            if (this.MessageId != null)
            {
                var m = await this.Device.Edit(this.MessageId.Value, this.Title, bf);
            }
            else
            {
                var m = await this.Device.Send(this.Title, bf, disableNotification: true);
                if (m != null)
                {
                    this.MessageId = m.MessageId;
                }
            }

            this.RenderNecessary = false;


        }

    }
}
