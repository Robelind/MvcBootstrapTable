using System;
using MvcBootstrapTable.Config;

namespace MvcBootstrapTable.Builders
{
    public class FilteringBuilder : BuilderBase
    {
        private readonly FilteringConfig _config;

        internal FilteringBuilder(FilteringConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Sets the number of characters to be entered to trigger filtering.
        /// </summary>
        /// <param name="threshold">Number of characters</param>
        /// <param name="condition">If true, filtering will be activated.</param>
        /// <returns>Filtering builder instance.</returns>
        public FilteringBuilder Threshold(int threshold, bool condition = true)
        {
            if(threshold <= 0)
            {
                throw(new ArgumentException("Filtering threshold must be larger than zero."));
            }
            _config.Threshold = condition ? threshold : 0;
            return(this);
        }

        /// <summary>
        /// Sets a css class for the filtering text input element.
        /// </summary>
        /// <param name="cssClass">Name of css class.</param>
        /// <param name="condition">If true, the css class will be set for the text input element.</param>
        /// <returns>Filtering builder instance.</returns>
        public FilteringBuilder CssClass(string cssClass, bool condition = true)
        {
            return(this.AddCssClass<FilteringBuilder>(_config.CssClasses, cssClass, condition));
        }
    }
}
