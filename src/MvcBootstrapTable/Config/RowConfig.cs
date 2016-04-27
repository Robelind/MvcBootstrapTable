using System.Collections.Generic;

namespace MvcBootstrapTable.Config
{
    internal class RowConfig<T> : ConfigBase
    {
        public T Entity { get; set; }

        public RowConfig(T entity)
        {
            Entity = entity;
            Active = true;
            CellConfigs = new Dictionary<string, CellConfig>();
        }

        public bool Active { get; set; }
        public string NavigationUrl { get; set; }
        public string RowClick { get; set; }
        public Dictionary<string, CellConfig> CellConfigs;
    }
}
