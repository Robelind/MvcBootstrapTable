using System.Collections.Generic;
using FluentAssertions;
using MvcBootstrapTable.Builders;
using MvcBootstrapTable.Config;
using NSubstitute;
using Xunit;

namespace Test.Builders
{
    public class ColumnsBuilderTest
    {
        private readonly Dictionary<string, ColumnConfig> _columnConfigs;
        private readonly SortingConfig _sortConfig;
        private readonly ColumnsBuilder<TableEntity> _builder;
        private readonly IBuilderFactory _builderFactory;

        public ColumnsBuilderTest()
        {
            _columnConfigs = new Dictionary<string, ColumnConfig>();
            _sortConfig = new SortingConfig();
            _builderFactory = Substitute.For<IBuilderFactory>();
            _builder = new ColumnsBuilder<TableEntity>(_columnConfigs, _sortConfig, _builderFactory);
        }

        [Fact]
        public void Column()
        {
            ColumnBuilder configBuilder = new ColumnBuilder(null, null);
            ColumnConfig config = null;

            _builderFactory.ColumnBuilder(Arg.Do<ColumnConfig>(c => config = c)).Returns(configBuilder);
            ColumnBuilder builder = _builder.Column(entity => entity.Property);

            _builderFactory.Received(1).ColumnBuilder(Arg.Any<ColumnConfig>());
            _columnConfigs.Should().HaveCount(1);
            _columnConfigs.Should().ContainKey("Property");
            ColumnConfig columnConfig = _columnConfigs["Property"];
            columnConfig.Should().BeSameAs(config);
            columnConfig.Visible.Should().BeTrue();
            columnConfig.SortState.Should().BeNull();
            columnConfig.CssClasses.Should().BeEmpty();
            columnConfig.Header.Should().BeNull();
            columnConfig.Filtering.Should().NotBeNull();
            columnConfig.Filtering.Threshold.Should().Be(0);
            columnConfig.Filtering.CssClasses.Should().BeEmpty();
            builder.Should().BeSameAs(configBuilder);
        }

        [Fact]
        public void IconLib()
        {
            _builder.IconLib("IconLib");

            _sortConfig.IconLib.Should().Be("IconLib");
        }

        [Fact]
        public void Ascending()
        {
            _builder.Ascending("CssClass");

            _sortConfig.AscendingCssClass.Should().Be("CssClass");
        }

        [Fact]
        public void Descending()
        {
            _builder.Descending("CssClass");

            _sortConfig.DescendingCssClass.Should().Be("CssClass");
        }
    }
}
