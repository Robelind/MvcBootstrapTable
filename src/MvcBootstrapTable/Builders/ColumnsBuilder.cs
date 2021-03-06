﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MvcBootstrapTable.Config;

namespace MvcBootstrapTable.Builders
{
    public class ColumnsBuilder<T>
    {
        private readonly Dictionary<string, ColumnConfig> _columnConfigs;
        private readonly SortingConfig _sortConfig;
        private readonly IBuilderFactory _builderFactory;

        internal ColumnsBuilder(Dictionary<string, ColumnConfig> columnConfigs, SortingConfig sortConfig,
            IBuilderFactory builderFactory)
        {
            _columnConfigs = columnConfigs;
            _sortConfig = sortConfig;
            _builderFactory = builderFactory;
        }

        /// <summary>
        /// Configures a column.
        /// </summary>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="expression">Column property expression.</param>
        /// <returns>Column builder.</returns>
        public ColumnBuilder Column<TVal>(Expression<Func<T, TVal>> expression)
        {
            string columnProperty = ((MemberExpression)expression.Body).Member.Name;
            ColumnConfig columnConfig = new ColumnConfig();

            _columnConfigs.Add(columnProperty, columnConfig);

            return(_builderFactory.ColumnBuilder(columnConfig));
        }

        /// <summary>
        /// Sets the icon library to be used for the sorting indicator icons.
        /// </summary>
        /// <param name="iconLib">Name of icon library.</param>
        /// <returns>Columns builder instance.</returns>
        /// <remarks>
        /// Default is "glyphicon".
        /// </remarks>
        public ColumnsBuilder<T> IconLib(string iconLib)
        {
            _sortConfig.IconLib = iconLib;
            return(this);
        }

        /// <summary>
        /// Sets the css class for the ascending sort indicator.
        /// </summary>
        /// <param name="iconClass">Name of css class.</param>
        /// <returns>Columns builder instance.</returns>
        /// <remarks>
        /// Default is "glyphicon-chevron-up".
        /// </remarks>
        public ColumnsBuilder<T> Ascending(string iconClass)
        {
            _sortConfig.AscendingCssClass = iconClass;
            return(this);
        }

        /// <summary>
        /// Sets the css class for the descending sort indicator.
        /// </summary>
        /// <param name="iconClass">Name of css class.</param>
        /// <returns>Columns builder instance.</returns>
        /// <remarks>
        /// Default is "glyphicon-chevron-down".
        /// </remarks>
        public ColumnsBuilder<T> Descending(string iconClass)
        {
            _sortConfig.DescendingCssClass = iconClass;
            return(this);
        }
    }
}
