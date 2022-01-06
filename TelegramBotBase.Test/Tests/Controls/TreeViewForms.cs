using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Form;
using TelegramBotBase.Controls;
using TelegramBotBase.Base;
using TelegramBotBase.Args;
using TelegramBotBase.Controls.Inline;

namespace TelegramBotBaseTest.Tests.Controls
{
    public class TreeViewForms : AutoCleanForm
    {
        public TreeView view { get; set; }

        private int? MessageId { get; set; }

        public TreeViewForms()
        {
            this.DeleteMode = TelegramBotBase.Enums.eDeleteMode.OnLeavingForm;
            this.Init += TreeViewForms_Init;
        }

        private async Task TreeViewForms_Init(object sender, InitEventArgs e)
        {
            view = new TreeView();

            var tvn = new TreeViewNode("Cars", "cars");

            tvn.AddNode(new TreeViewNode("Porsche", "porsche", new TreeViewNode("Website", "web", "https://www.porsche.com/germany/"), new TreeViewNode("911", "911"), new TreeViewNode("918 Spyder", "918")));
            tvn.AddNode(new TreeViewNode("BMW", "bmw"));
            tvn.AddNode(new TreeViewNode("Audi", "audi"));
            tvn.AddNode(new TreeViewNode("VW", "vw"));
            tvn.AddNode(new TreeViewNode("Lamborghini", "lamborghini"));

            view.Nodes.Add(tvn);

            tvn = new TreeViewNode("Fruits", "fruits");

            tvn.AddNode(new TreeViewNode("Apple", "apple"));
            tvn.AddNode(new TreeViewNode("Orange", "orange"));
            tvn.AddNode(new TreeViewNode("Lemon", "lemon"));

            view.Nodes.Add(tvn);

            this.AddControl(view);

        }

        public override async Task Action(MessageResult message)
        {
            await message.ConfirmAction();

            if (message.Handled)
                return;

            switch (message.RawData)
            {
                case "back":

                    message.Handled = true;

                    var start = new Menu();

                    await this.NavigateTo(start);

                    break;

            }

        }

        public override async Task Render(MessageResult message)
        {
            String s = "";

            s += "Selected Node: " + (this.view.SelectedNode?.Text ?? "(null)") + "\r\n";

            s += "Visible Node: " + (this.view.VisibleNode?.Text ?? "(top)") + "\r\n";

            s += "Visible Path: " + this.view.GetPath() + "\r\n";
            s += "Selected Path: " + (this.view.SelectedNode?.GetPath() ?? "(null)") + "\r\n";

            ButtonForm bf = new ButtonForm();
            bf.AddButtonRow(new ButtonBase("Back", "back"));

            if (MessageId != null)
            {
                await this.Device.Edit(this.MessageId.Value, s, bf);
            }
            else
            {
                var m = await this.Device.Send(s, bf);
                this.MessageId = m.MessageId;
            }
        }

    }
}
