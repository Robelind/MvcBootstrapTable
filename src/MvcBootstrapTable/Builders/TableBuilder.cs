using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Html.Abstractions;
using MvcBootstrapTable.Config;
using MvcBootstrapTable.Rendering;

namespace MvcBootstrapTable.Builders
{
    public class TableBuilder<T> : BuilderBase where T : new()
    {
        private readonly TableModel<T> _model;
        private readonly ITableRenderer<T> _renderer;
        private readonly IBuilderFactory _builderFactory;
        private readonly ITableConfig<T> _config;

        internal TableBuilder(TableModel<T> model, ITableRenderer<T> renderer, IBuilderFactory builderFactory,
            ITableConfig<T> config)
        {
            _model = model;
            _renderer = renderer;
            _builderFactory = builderFactory;
            _config = config;
        }

        /// <summary>
        /// Sets the id attribute for the table.
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>The table builder instance.</returns>
        public TableBuilder<T> Id(string id)
        {
            _config.Id = id;
            return(this);
        }

        /// <summary>
        /// Sets the name attribute for the table.
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>The table builder instance.</returns>
        public TableBuilder<T> Name(string name)
        {
            _config.Name = name;
            return(this);
        }

        /// <summary>
        /// Sets the name caption for the table.
        /// </summary>
        /// <param name="caption">Caption</param>
        /// <returns>The table builder instance.</returns>
        public TableBuilder<T> Caption(string caption)
        {
            _config.Caption = caption;
            return(this);
        }

        /// <summary>
        /// Sets whether the table should be rendered in a striped fashion.
        /// </summary>
        /// <param name="striped">If true, the table is rendered in a striped fashion.</param>
        /// <returns>The table builder instance.</returns>
        public TableBuilder<T> Striped(bool striped = true)
        {
            _config.Striped = striped;
            return(this);
        }

        /// <summary>
        /// Sets whether the table should be rendered bordered.
        /// </summary>
        /// <param name="bordered">If true, the table is rendered bordered.</param>
        /// <returns>The table builder instance.</returns>
        public TableBuilder<T> Bordered(bool bordered = true)
        {
            _config.Bordered = bordered;
            return(this);
        }

        /// <summary>
        /// Sets whether the table rows should have a hover state.
        /// </summary>
        /// <param name="hoverState">If true, the table rows will have a hover state.</param>
        /// <returns>The table builder instance.</returns>
        public TableBuilder<T> HoverState(bool hoverState = true)
        {
            _config.HoverState = hoverState;
            return(this);
        }

        /// <summary>
        /// Sets whether the table should be rendered in a condensed fashion.
        /// </summary>
        /// <param name="condensed">If true, the table is rendered in a condensed fashion.</param>
        /// <returns>The table builder instance.</returns>
        public TableBuilder<T> Condensed(bool condensed = true)
        {
            _config.Condensed = condensed;
            return(this);
        }

        /// <summary>
        /// Sets a css class for the table element.
        /// </summary>
        /// <param name="cssClass">Name of css class.</param>
        /// <param name="condition">If true, the css class will be set for the table element.</param>
        /// <returns>The table builder instance.</returns>
        public TableBuilder<T> CssClass(string cssClass, bool condition = true)
        {
            return(this.AddCssClass<TableBuilder<T>>(_config.CssClasses, cssClass, condition));
        }

        /// <summary>
        /// Configures a java script function to be called when a row in
        /// the table is clicked.
        /// </summary>
        /// <param name="jsFunc">Name of java script function.</param>
        /// <param name="condition">If true, the java script function will be called.</param>
        /// <returns>The table builder instance.</returns>
        /// <remarks>
        /// The java script function will be passed the row instance as a parameter.
        /// </remarks>
        public TableBuilder<T> RowClick(string jsFunc, bool condition = true)
        {
            if(condition)
            {
                _config.RowClick = jsFunc;
            }
            return(this);
        }

        /// <summary>
        /// Configures the table footer.
        /// </summary>
        /// <param name="configAction">Configuration action</param>
        /// <returns>The table builder instance.</returns>
        public TableBuilder<T> Footer(Action<FooterBuilder> configAction)
        {
            configAction(_builderFactory.FooterBuilder(_config.Footer));
            return(this);
        }

        /// <summary>
        /// Configured paging.
        /// </summary>
        /// <param name="configAction">Configuration action</param>
        /// <returns>The table builder instance.</returns>
        public TableBuilder<T> Paging(Action<PagingBuilder> configAction)
        {
            configAction(_builderFactory.PagingBuilder(_config.Paging));
            return(this);
        }

        /// <summary>
        /// Configures table updating.
        /// </summary>
        /// <param name="configAction">Configuration action</param>
        /// <returns>The table builder instance.</returns>
        public TableBuilder<T> Update(Action<UpdateBuilder> configAction)
        {
            configAction(_builderFactory.UpdateBuilder(_config.Update));
            if(_config.Update.Url == null)
            {
                throw new ArgumentNullException("Update url");
            }
            
            return(this);
        }

        /// <summary>
        /// Configures the rows of the table.
        /// </summary>
        /// <param name="configAction">Configuration action</param>
        /// <returns>The table builder instance.</returns>
        /// <remarks>
        /// If using paging, configure it before doing row configuration.
        /// </remarks>
        public TableBuilder<T> Rows(Action<RowBuilder<T>, T> configAction)
        {
            IEnumerable<T> entities = _config.Paging.PageSize > 0
                ? _model.Entities.Take(_config.Paging.PageSize)
                : _model.Entities;

            foreach(T entity in entities)
            {
                RowBuilder<T> builder = _builderFactory.RowBuilder(entity);

                configAction(builder, entity);
                _config.Rows.Add(builder.Config);
            }

            return(this);
        }

        /// <summary>
        /// Configures the columns of the table.
        /// </summary>
        /// <param name="configAction">Configuration action</param>
        /// <returns>The table builder instance.</returns>
        public TableBuilder<T> Columns(Action<ColumnsBuilder<T>> configAction)
        {
            configAction(_builderFactory.ColumnsBuilder<T>(_config.Columns, _config.Sorting));
            return(this);
        }

        /// <summary>
        /// Renders the table.
        /// </summary>
        /// <returns></returns>
        public IHtmlContent Render()
        {
            this.CheckConfiguration();
            
            if(!_config.Rows.Any())
            {
                 IEnumerable<T> entities = _config.Paging.PageSize > 0
                    ? _model.Entities.Take(_config.Paging.PageSize)
                    : _model.Entities;

                // No row configuration has been performed.
                // Create row configs from the entities.
                foreach(var row in entities.Select(e => new RowConfig<T>(e)))
                {
                    _config.Rows.Add(row);
                }
            }

            if(_config.Paging.PageSize > 0 && _config.Rows.Count() > _config.Paging.PageSize)
            {
                IEnumerable<RowConfig<T>> rows = _config.Rows.Take(_config.Paging.PageSize).ToList();
                
                // Apply paging.
                _config.Rows.Clear();
                foreach(var row in rows)
                {
                    _config.Rows.Add(row);
                }
            }

            return(_renderer.Render(_config, _model.EntityCount));
        }

        private void CheckConfiguration()
        {
            if((_config.Paging.PageSize > 0 || _config.Columns.Any(c => c.Value.SortState.HasValue)
               || _config.Columns.Any(c => c.Value.Filtering.Threshold > 0)) &&
               string.IsNullOrEmpty(_config.Update.Url))
            {
                throw(new Exception("Update url must be configured if using paging, sorting or filtering."));
            }
        }
    }
}