using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Localizations;

namespace TelegramBotBase.Controls.Inline;

public class TreeView : ControlBase
{
    public TreeView()
    {
        Nodes = new List<TreeViewNode>();
        Title = Default.Language["TreeView_Title"];
    }

    public List<TreeViewNode> Nodes { get; set; }

    public TreeViewNode SelectedNode { get; set; }

    public TreeViewNode VisibleNode { get; set; }

    public string Title { get; set; }

    public int? MessageId { get; set; }

    public string MoveUpIcon { get; set; } = Default.Language["TreeView_LevelUp"];


    public override async Task Action(MessageResult result, string value = null)
    {
        await result.ConfirmAction();

        if (result.Handled)
        {
            return;
        }

        var val = result.RawData;

        switch (val)
        {
            case "up":
            case "parent":

                VisibleNode = VisibleNode?.ParentNode;

                result.Handled = true;

                break;
            default:

                var n = VisibleNode != null
                            ? VisibleNode.FindNodeByValue(val)
                            : Nodes.FirstOrDefault(a => a.Value == val);

                if (n == null)
                {
                    return;
                }


                if (n.ChildNodes.Count > 0)
                {
                    VisibleNode = n;
                }
                else
                {
                    SelectedNode = SelectedNode != n ? n : null;
                }

                result.Handled = true;


                break;
        }
    }


    public override async Task Render(MessageResult result)
    {
        var startnode = VisibleNode;

        var nodes = startnode?.ChildNodes ?? Nodes;

        var bf = new ButtonForm();

        if (startnode != null)
        {
            bf.AddButtonRow(new ButtonBase(MoveUpIcon, "up"), new ButtonBase(startnode.Text, "parent"));
        }

        foreach (var n in nodes)
        {
            var s = n.Text;
            if (SelectedNode == n)
            {
                s = "[ " + s + " ]";
            }

            bf.AddButtonRow(new ButtonBase(s, n.Value, n.Url));
        }


        if (MessageId != null)
        {
            var m = await Device.Edit(MessageId.Value, Title, bf);
        }
        else
        {
            var m = await Device.Send(Title, bf);
            MessageId = m.MessageId;
        }
    }

    public string GetPath()
    {
        return VisibleNode?.GetPath() ?? "\\";
    }
}