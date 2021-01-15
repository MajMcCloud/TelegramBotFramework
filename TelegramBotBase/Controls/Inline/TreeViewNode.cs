using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotBase.Controls.Inline
{
    public class TreeViewNode
    {
        public String Text { get; set; }

        public String Value { get; set; }

        public String Url { get; set; }

        public List<TreeViewNode> ChildNodes { get; set; } = new List<TreeViewNode>();

        public TreeViewNode ParentNode { get; set; }

        public TreeViewNode(String Text, String Value)
        {
            this.Text = Text;
            this.Value = Value;
        }

        public TreeViewNode(String Text, String Value, String Url) : this(Text, Value)
        {
            this.Url = Url;
        }

        public TreeViewNode(String Text, String Value, params TreeViewNode[] childnodes) : this(Text, Value)
        {
            foreach(var c in childnodes)
            {
                AddNode(c);
            }
        }


        public void AddNode(TreeViewNode node)
        {
            node.ParentNode = this;
            ChildNodes.Add(node);
        }

        public TreeViewNode FindNodeByValue(String Value)
        {
            return this.ChildNodes.FirstOrDefault(a => a.Value == Value);
        }

        public String GetPath()
        {
            String s = "\\" + this.Value;
            var p = this;
            while (p.ParentNode != null)
            {
                s = "\\" + p.ParentNode.Value + s;
                p =  p.ParentNode;
            }
            return s;
        }

    }
}
