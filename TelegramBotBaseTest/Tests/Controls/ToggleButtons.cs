using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Controls;
using TelegramBotBase.Form;

namespace TelegramBaseTest.Tests.Controls
{
    public class ToggleButtons : AutoCleanForm
    {
        public ToggleButtons()
        {
            this.DeleteMode = TelegramBotBase.Enums.eDeleteMode.OnLeavingForm;

            this.Init += ToggleButtons_Init;
        }

        private async Task ToggleButtons_Init(object sender, TelegramBotBase.Base.InitEventArgs e)
        {

            var tb = new ToggleButton();
            tb.Checked = true;
            tb.Toggled += Tb_Toggled;

            this.AddControl(tb);

            tb = new ToggleButton();
            tb.Checked = false;
            tb.Toggled += Tb_Toggled;

            this.AddControl(tb);

            tb = new ToggleButton();
            tb.Checked = true;
            tb.Toggled += Tb_Toggled;

            this.AddControl(tb);

        }

        private void Tb_Toggled(object sender, EventArgs e)
        {
            var tb = sender as ToggleButton;
            Console.WriteLine(tb.ID.ToString() + " was pressed, and toggled to " + (tb.Checked ? "Checked" : "Unchecked"));
        }
    }
}
