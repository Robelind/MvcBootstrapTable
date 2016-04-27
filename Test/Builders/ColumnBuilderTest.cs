using FluentAssertions;
using MvcBootstrapTable.Builders;
using MvcBootstrapTable.Config;
using Xunit;
using NSubstitute;

namespace Test.Builders
{
    public class ColumnBuilderTest
    {
        private readonly ColumnConfig _columnConfig;
        private readonly IBuilderFactory _builderFactory;
        private readonly ColumnBuilder _builder;

        public ColumnBuilderTest()
        {
            _columnConfig = new ColumnConfig {Filtering = new FilteringConfig()};
            _builderFactory = Substitute.For<IBuilderFactory>();
            _builder = new ColumnBuilder(_columnConfig, _builderFactory);
        }

        [Fact]
        public void Header()
        {
            ColumnBuilder builder =_builder.Header("Header");

            _columnConfig.Header.Should().Be("Header");
            builder.Should().BeSameAs(_builder);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Visible(bool visible)
        {
            ColumnBuilder builder =_builder.Visible(visible);

            _columnConfig.Visible.Should().Be(visible);
            builder.Should().BeSameAs(_builder);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void Sortable(bool sortable, bool sorted)
        {
            if(sorted)
            {
                _builder.Sorted();
            }

            ColumnBuilder builder =_builder.Sortable(sortable);

            if(sorted)
            {
                _columnConfig.SortState.Should().Be(SortState.Ascending);
            }
            else
            {
                _columnConfig.SortState.Should().Be(sortable ? SortState.None : (SortState?)null);
            }
            builder.Should().BeSameAs(_builder);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Sorted(bool ascending)
        {
            ColumnBuilder builder =_builder.Sorted(ascending);

            _columnConfig.SortState.Should().Be(ascending ? SortState.Ascending : SortState.Descending);
            builder.Should().BeSameAs(_builder);
        }

        [Fact]
        public void Filterable()
        {
            FilteringBuilder filteringBuilder = new FilteringBuilder(null);
            FilteringBuilder actionBuilder = null;
            FilteringConfig config = null;

            _builderFactory.FilteringBuilder(Arg.Do<FilteringConfig>(fc => config = fc)).Returns(filteringBuilder);

            ColumnBuilder builder =_builder.Filterable(fb => actionBuilder = fb);

            _builderFactory.Received(1).FilteringBuilder(Arg.Any<FilteringConfig>());
            actionBuilder.Should().BeSameAs(filteringBuilder);
            config.Should().BeSameAs(_columnConfig.Filtering);
            builder.Should().BeSameAs(_builder);
        }

        [Theory]
        [InlineData(new[] {"C1"}, true)]
        [InlineData(new[] {"C1, C2"}, true)]
        [InlineData(new[] {"C1"}, false)]
        public void CssClass(string[] cssClasses, bool apply)
        {
            ColumnBuilder builder = null;

            foreach(var cssClass in cssClasses)
            {
                builder = _builder.CssClass(cssClass, apply);
            }

            if(apply)
            {
                _columnConfig.CssClasses.ShouldBeEquivalentTo(cssClasses);
            }
            else
            {
                _columnConfig.CssClasses.Should().BeEmpty();
            }
            builder.Should().BeSameAs(_builder);
        }
    }
}
