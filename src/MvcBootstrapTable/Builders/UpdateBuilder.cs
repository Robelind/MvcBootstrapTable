using MvcBootstrapTable.Config;

namespace MvcBootstrapTable.Builders
{
    public class UpdateBuilder : BuilderBase
    {
        private readonly UpdateConfig _config;

        internal UpdateBuilder(UpdateConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Sets the url used to update the table.
        /// </summary>
        /// <param name="url">Url</param>
        /// <returns>Update builder instance.</returns>
        /// <remarks>
        /// The url will be used for the AJAX calls when doing sorting, filtering and paging.
        /// </remarks>
        public UpdateBuilder Url(string url)
        {
            _config.Url = url;
            return(this);
        }

        /// <summary>
        /// The name of a Javascript function to call when an update
        /// operation starts.
        /// </summary>
        /// <param name="jsFunc">Name of the Javascript function.</param>
        /// <returns>Update builder instance.</returns>
        public UpdateBuilder Start(string jsFunc)
        {
            _config.Start = jsFunc;
            return(this);
        }

        /// <summary>
        /// The name of a Javascript function to call when an update
        /// operation is successful.
        /// </summary>
        /// <param name="jsFunc">Name of the Javascript function.</param>
        /// <returns>Update builder instance.</returns>
        public UpdateBuilder Success(string jsFunc)
        {
            _config.Success = jsFunc;
            return(this);
        }

        /// <summary>
        /// The name of a Javascript function to call if an update
        /// operation fails.
        /// </summary>
        /// <param name="jsFunc">Name of the Javascript function.</param>
        /// <returns>Update builder instance.</returns>
        public UpdateBuilder Error(string jsFunc)
        {
            _config.Error = jsFunc;
            return(this);
        }

        /// <summary>
        /// The name of a Javascript function to call when an update
        /// operation is complete.
        /// </summary>
        /// <param name="jsFunc">Name of the Javascript function.</param>
        /// <returns>Update builder instance.</returns>
        /// <remarks>
        /// The method will always be called, in conjunction with
        /// success or error handler (if configured).
        /// </remarks>
        public UpdateBuilder Complete(string jsFunc)
        {
            _config.Complete = jsFunc;
            return(this);
        }

        /// <summary>
        /// Adds a custom query parameter to be sent to the table update action method.
        /// </summary>
        /// <param name="name">Name of the parameter.</param>
        /// <param name="value">Value for the parameter.</param>
        /// <returns>Update builder instance.</returns>
        public UpdateBuilder QueryParameter(string name, object value)
        {
            _config.CustomQueryPars.Add(name, value.ToString());
            return(this);
        }

        /// <summary>
        /// Sets the id of an element to be displayed as a busy indicator when AJAX operations
        /// are in progress.
        /// </summary>
        /// <param name="id">Id of element.</param>
        /// <returns></returns>
        public UpdateBuilder BusyIndicatorId(string id)
        {
            _config.BusyIndicatorId = id;
            return(this);
        }
    }
}
