using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Rendering;

namespace MvcBootstrapTable.Rendering
{
    internal class TableNode
    {
        public TableNode(TagBuilder element)
        {
            Element = element;
            InnerContent = new List<TableNode>();
        }

        public TableNode(string element)
        : this(new TagBuilder(element))
        {
        }

        public TableNode(string element, TableNode inner)
        : this(new TagBuilder(element))
        {
            InnerContent.Add(inner);
        }

        public TagBuilder Element { get; set; }
        public List<TableNode> InnerContent { get; private set; }
    }
}
