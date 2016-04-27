using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Html.Abstractions;

namespace MvcBootstrapTable.Rendering
{
    internal interface ITableNodeParser
    {
        IHtmlContent Parse(IEnumerable<TableNode> nodes);
    }

    internal class TableNodeParser : ITableNodeParser
    {
        public IHtmlContent Parse(IEnumerable<TableNode> nodes)
        {
            this.ParseNode(nodes.First());

            return(nodes.First().Element);
        }

        private void ParseNode(TableNode node)
        {
            foreach(TableNode tableNode in node.InnerContent)
            {
                node.Element.InnerHtml.Append(tableNode.Element);
                this.ParseNode(tableNode);
            }            
        }
    }
}
