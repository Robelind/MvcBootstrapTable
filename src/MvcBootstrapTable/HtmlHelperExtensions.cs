using System;
using Microsoft.AspNet.Mvc.Rendering;
using MvcBootstrapTable.Builders;
using MvcBootstrapTable.Config;
using MvcBootstrapTable.Rendering;

// This project can output the Class library as a NuGet Package.
// To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
namespace MvcBootstrapTable
{
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Renders an Mvc Bootstrap Table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="htmlHelper">Html helper instance.</param>
        /// <param name="model">The model used to render the table.</param>
        /// <returns>Table builder.</returns>
        public static TableBuilder<T> MvcBootstrapTable<T>(this IHtmlHelper htmlHelper, TableModel<T> model)
            where T:class, new()
        {
            TableState tableState = new TableStateParser().Parse(htmlHelper.ViewContext.HttpContext);

            if(model == null)
            {
                throw(new ArgumentNullException("model"));
            }

            return(new TableBuilder<T>(model, new TableRenderer<T>(tableState, new TableNodeParser()), new BuilderFactory(), new TableConfig<T>()));
        }
    }
}
