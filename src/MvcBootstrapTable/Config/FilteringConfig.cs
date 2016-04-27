using System.Collections.Generic;

namespace MvcBootstrapTable.Config
{
    internal class FilteringConfig
    {
        public FilteringConfig()
        {
            CssClasses = new List<string>();
        }

        public int Threshold { get; set; }
        public IList<string> CssClasses { get; private set; }
    }
}
