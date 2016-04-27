using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace MvcBootstrapTable.Rendering
{
    public class TableBinder : IModelBinder
    {
        public Task<ModelBindingResult> BindModelAsync(ModelBindingContext bindingContext)
        {
            TableState tableState = new TableStateParser().Parse(bindingContext.OperationBindingContext.HttpContext);

            return(Task.FromResult(ModelBindingResult.Success("key", new TableUpdater(tableState))));
        }
    }
}
