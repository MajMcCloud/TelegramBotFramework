using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBase.Controls.Inline
{
    public class TreeView : ControlBase
    {
        public List<TreeViewNode> Nodes { get; set; }

        public TreeViewNode SelectedNode { get; set; }

        public TreeViewNode VisibleNode { get; set; }

        public String Title { get; set; }

        private int? MessageId { get; set; }

        public String MoveUpIcon { get; set; } = Localizations.Default.Language["TreeView_LevelUp"];

        public TreeView()
        {
            this.Nodes = new List<TreeViewNode>();
            this.Title = Localizations.Default.Language["TreeView_Title"];
        }


        public override async Task Action(MessageResult result, String value = null)
        {
            await result.ConfirmAction();

            if (result.Handled)
                return;

            var val = result.RawData;

            switch (val)
            {
                case "up":
                case "parent":

                    this.VisibleNode = (this.VisibleNode?.ParentNode);

                    result.Handled = true;

                    break;
                default:

                    var n = (this.VisibleNode != null ? this.VisibleNode.FindNodeByValue(val) : this.Nodes.FirstOrDefault(a => a.Value == val));

                    if (n == null)
                        return;


                    if (n.ChildNodes.Count > 0)
                    {
                        this.VisibleNode = n;
                    }
                    else
                    {
                        this.SelectedNode = (this.SelectedNode != n ? n : null);
                    }

                    result.Handled = true;


                    break;

            }

        }


        public override async Task Render(MessageResult result)
        {
            var startnode = this.VisibleNode;

            var nodes = (startnode?.ChildNodes ?? this.Nodes);

            ButtonForm bf = new ButtonForm();

            if (startnode != null)
            {
                bf.AddButtonRow(new ButtonBase(this.MoveUpIcon, "up"), new ButtonBase(startnode.Text, "parent"));
            }

            foreach (var n in nodes)
            {
                var s = n.Text;
                if (this.SelectedNode == n)
                {
                    s = "[ " + s + " ]";
                }

                bf.AddButtonRow(new ButtonBase(s, n.Value, n.Url));
            }



            if (this.MessageId != null)
            {
                var m = await this.Device.Edit(this.MessageId.Value, this.Title, bf);
            }
            else
            {
                var m = await this.Device.Send(this.Title, bf);
                this.MessageId = m.MessageId;
            }
        }

        public String GetPath()
        {
            return (this.VisibleNode?.GetPath() ?? "\\");
        }


    }
}
