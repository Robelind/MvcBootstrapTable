using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

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
                node.Element.InnerHtml.AppendHtml(tableNode.Element);
                this.ParseNode(tableNode);
            }            
        }
    }
}
