using System.Collections.Generic;
using MvcBootstrapTable.Config;

namespace MvcBootstrapTable.Builders
{
    internal interface IBuilderFactory
    {
        FooterBuilder FooterBuilder(FooterConfig config);
        PagingBuilder PagingBuilder(PagingConfig config);
        UpdateBuilder UpdateBuilder(UpdateConfig config);
        RowBuilder<T> RowBuilder<T>(T entity);
        ColumnsBuilder<T> ColumnsBuilder<T>(Dictionary<string, ColumnConfig> columnConfigs, SortingConfig sortConfig);
        ColumnBuilder ColumnBuilder(ColumnConfig config);
        FilteringBuilder FilteringBuilder(FilteringConfig config);
        CellBuilder CellBuilder(CellConfig config);
        CellsBuilder CellsBuilder(Dictionary<string, CellConfig> configs);
    }

    internal class BuilderFactory : IBuilderFactory
    {
        public FooterBuilder FooterBuilder(FooterConfig config)
        {
            return(new FooterBuilder(config));
        }

        public PagingBuilder PagingBuilder(PagingConfig config)
        {
            return(new PagingBuilder(config));
        }

        public UpdateBuilder UpdateBuilder(UpdateConfig config)
        {
            return(new UpdateBuilder(config));
        }

        public RowBuilder<T> RowBuilder<T>(T entity)
        {
            return(new RowBuilder<T>(entity, this));
        }

        public ColumnsBuilder<T> ColumnsBuilder<T>(Dictionary<string, ColumnConfig> columnConfigs, SortingConfig sortConfig)
        {
            return(new ColumnsBuilder<T>(columnConfigs, sortConfig, this));
        }

        public ColumnBuilder ColumnBuilder(ColumnConfig config)
        {
            return(new ColumnBuilder(config, this));
        }

        public FilteringBuilder FilteringBuilder(FilteringConfig config)
        {
            return(new FilteringBuilder(config));
        }

        public CellsBuilder CellsBuilder(Dictionary<string, CellConfig> configs)
        {
            return(new CellsBuilder(configs, this));
        }

        public CellBuilder CellBuilder(CellConfig config)
        {
            return(new CellBuilder(config));
        }
    }
}
