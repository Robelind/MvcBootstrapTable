using System.Collections.Generic;
using FluentAssertions;
using MvcBootstrapTable;
using MvcBootstrapTable.Builders;
using MvcBootstrapTable.Config;
using Xunit;
using NSubstitute;

namespace Test.Builders
{
    public class RowBuilderTest
    {
        private readonly IBuilderFactory _builderFactory;
        private readonly RowBuilder<TableEntity> _builder;
        private readonly TableEntity _entity;

        public RowBuilderTest()
        {
            _entity = new TableEntity();
            _builderFactory = Substitute.For<IBuilderFactory>();
            _builder = new RowBuilder<TableEntity>(_entity, _builderFactory);
        }

        [Fact]
        public void Instantiation()
        {
            _builder.Config.Should().NotBeNull();
            _builder.Config.Entity.Should().BeSameAs(_entity);
            _builder.Config.Active.Should().BeTrue();
            _builder.Config.NavigationUrl.Should().BeNull();
            _builder.Config.RowClick.Should().BeNull();
            _builder.Config.CssClasses.Should().BeEmpty();
            _builder.Config.CellConfigs.Should().BeEmpty();
            _builder.Config.State.Should().Be(ContextualState.Default);
        }

        [Fact]
        public void NavigationUrl()
        {
            RowBuilder<TableEntity> builder =_builder.NavigationUrl("Url");

            _builder.Config.NavigationUrl.Should().Be("Url");
            builder.Should().BeSameAs(_builder);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Contextual(bool apply)
        {
            RowBuilder<TableEntity> builder =_builder.Contextual(ContextualState.Danger, apply);

            _builder.Config.State.Should().Be(apply ? ContextualState.Danger : ContextualState.Default);
            builder.Should().BeSameAs(_builder);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RowClick(bool apply)
        {
            RowBuilder<TableEntity> builder =_builder.RowClick("jsFunc", apply);

            _builder.Config.RowClick.Should().Be(apply ? "jsFunc" : null);
            builder.Should().BeSameAs(_builder);
        }

        [Theory]
        [InlineData(new[] {"C1"}, true)]
        [InlineData(new[] {"C1, C2"}, true)]
        [InlineData(new[] {"C1"}, false)]
        public void CssClass(string[] cssClasses, bool apply)
        {
            RowBuilder<TableEntity> builder = null;

            foreach(var cssClass in cssClasses)
            {
                builder = _builder.CssClass(cssClass, apply);
            }

            if(apply)
            {
                _builder.Config.CssClasses.ShouldBeEquivalentTo(cssClasses);
            }
            else
            {
                _builder.Config.CssClasses.Should().BeEmpty();
            }
            builder.Should().BeSameAs(_builder);
        }

        [Fact]
        public void Cells()
        {
            CellsBuilder cellsBuilder = new CellsBuilder(null, null);
            CellsBuilder actionBuilder = null;
            Dictionary<string, CellConfig> config = null;

            _builderFactory.CellsBuilder(Arg.Do<Dictionary<string, CellConfig>>(fc => config = fc))
                .Returns(cellsBuilder);

            RowBuilder<TableEntity> builder =_builder.Cells(fb => actionBuilder = fb);

            _builderFactory.Received(1).CellsBuilder(Arg.Any<Dictionary<string, CellConfig>>());
            actionBuilder.Should().BeSameAs(cellsBuilder);
            _builder.Config.CellConfigs.Should().BeSameAs(config);
            builder.Should().BeSameAs(_builder);
        }
    }
}
