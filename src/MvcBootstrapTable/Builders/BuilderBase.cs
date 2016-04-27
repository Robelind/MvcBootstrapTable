using System.Collections.Generic;

namespace MvcBootstrapTable.Builders
{
    public class BuilderBase
    {
        protected T AddCssClass<T>(IList<string> cssClasses, string cssClass, bool condition)
        where T : BuilderBase
        {
            if(condition)
            {
                cssClasses.Add(cssClass);
            }
            return(this as T);
        }
    }
}