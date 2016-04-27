using System.Collections.Generic;

namespace MvcBootstrapTable.Config
{
    internal class UpdateConfig
    {
        public UpdateConfig()
        {
            CustomQueryPars = new Dictionary<string, string>();
        }

        public string Url { get; set; }
        public string BusyIndicatorId { get; set; }
        public string Start { get; set; }
        public string Success { get; set; }
        public string Error { get; set; }
        public string Complete { get; set; }
        public Dictionary<string, string> CustomQueryPars { get; set; }
    }
}
