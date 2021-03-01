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
    public class MultiToggleButton : ControlBase
    {
        /// <summary>
        /// This contains the selected icon.
        /// </summary>
        public String SelectedIcon { get; set; } = Localizations.Default.Language["MultiToggleButton_SelectedIcon"];

        /// <summary>
        /// This will appear on the ConfirmAction message (if not empty)
        /// </summary>
        public String ChangedString { get; set; } = Localizations.Default.Language["MultiToggleButton_Changed"];

        /// <summary>
        /// This holds the title of the control.
        /// </summary>
        public String Title { get; set; } = Localizations.Default.Language["MultiToggleButton_Title"];

        public int? MessageId { get; set; }

        private bool RenderNecessary = true;

        private static readonly object __evToggled = new object();

        private readonly EventHandlerList Events = new EventHandlerList();

        /// <summary>
        /// This will hold all options available.
        /// </summary>
        public List<ButtonBase> Options { get; set; }

        /// <summary>
        /// This will set if an empty selection (null) is allowed.
        /// </summary>
        public bool AllowEmptySelection { get; set; } = true;


        public MultiToggleButton()
        {
            Options = new List<ButtonBase>();
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
                default:

                    var s = value.Split('$');

                    if (s[0] == "check" && s.Length > 1)
                    {
                        int index = 0;
                        if (!int.TryParse(s[1], out index))
                        {
                            return;
                        }

                        if(SelectedOption== null || SelectedOption != this.Options[index])
                        {
                            this.SelectedOption = this.Options[index];
                            OnToggled(new EventArgs());
                        }
                        else if(this.AllowEmptySelection)
                        {
                            this.SelectedOption = null;
                            OnToggled(new EventArgs());
                        }

                        RenderNecessary = true;

                        return;
                    }


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

            var lst = new List<ButtonBase>();
            foreach (var o in this.Options)
            {
                var index = this.Options.IndexOf(o);
                if (o == this.SelectedOption)
                {
                    lst.Add(new ButtonBase(SelectedIcon + " " + o.Text, "check$" + index));
                    continue;
                }

                lst.Add(new ButtonBase(o.Text, "check$" + index));
            }

            bf.AddButtonRow(lst);

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

        public ButtonBase SelectedOption
        {
            get; set;
        }

    }
}
