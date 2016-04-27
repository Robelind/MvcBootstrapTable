using MvcBootstrapTable.Config;

namespace MvcBootstrapTable.Builders
{
    public class CellBuilder : BuilderBase
    {
        private readonly CellConfig _config;

        internal CellBuilder(CellConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Sets the <see cref="ContextualState"/> of the cell.
        /// </summary>
        /// <param name="state">Contextual state</param>
        /// <param name="condition">If true, the contextual state will be applied.</param>
        /// <returns>The cell builder instance.</returns>
        public CellBuilder Contextual(ContextualState state, bool condition = true)
        {
            _config.State = condition ? state : ContextualState.Default;
            return(this);
        }

        /// <summary>
        /// Sets a css class for the cell element.
        /// </summary>
        /// <param name="cssClass">Name of css class.</param>
        /// <param name="condition">If true, the css class will be set for the cell element.</param>
        /// <returns>The cell builder instance.</returns>
        public CellBuilder CssClass(string cssClass, bool condition = true)
        {
            return(this.AddCssClass<CellBuilder>(_config.CssClasses, cssClass, condition));
        }
    }
}
