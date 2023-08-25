using System.Collections.Generic;
using System.Linq;

namespace TelegramBotBase.Controls.Inline;

public class TreeViewNode
{
    public TreeViewNode(string text, string value)
    {
        Text = text;
        Value = value;
    }

    public TreeViewNode(string text, string value, string url) : this(text, value)
    {
        Url = url;
    }

    public TreeViewNode(string text, string value, params TreeViewNode[] childnodes) : this(text, value)
    {
        foreach (var c in childnodes)
        {
            AddNode(c);
        }
    }

    public string Text { get; set; }

    public string Value { get; set; }

    public string Url { get; set; }

    public List<TreeViewNode> ChildNodes { get; set; } = new();

    public TreeViewNode ParentNode { get; set; }


    public void AddNode(TreeViewNode node)
    {
        node.ParentNode = this;
        ChildNodes.Add(node);
    }

    public TreeViewNode FindNodeByValue(string value)
    {
        return ChildNodes.FirstOrDefault(a => a.Value == value);
    }

    public string GetPath()
    {
        var s = "\\" + Value;
        var p = this;
        while (p.ParentNode != null)
        {
            s = "\\" + p.ParentNode.Value + s;
            p = p.ParentNode;
        }

        return s;
    }
}