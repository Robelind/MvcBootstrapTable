using System.Collections.Generic;

namespace MvcBootstrapTable.Config
{
    internal abstract class ConfigBase
    {
        protected ConfigBase()
        {
            CssClasses = new List<string>();
            State = ContextualState.Default;
        }

        public ContextualState State { get; set; }
        public IList<string> CssClasses { get; private set; }
    }
}
