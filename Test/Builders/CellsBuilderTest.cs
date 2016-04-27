using System.Collections.Generic;
using FluentAssertions;
using MvcBootstrapTable;
using MvcBootstrapTable.Builders;
using MvcBootstrapTable.Config;
using NSubstitute;
using Xunit;

namespace Test.Builders
{
    public class CellsBuilderTest
    {
        private readonly Dictionary<string, CellConfig> _configs;
        private readonly IBuilderFactory _builderFactory;
        private readonly CellsBuilder _builder;

        public CellsBuilderTest()
        {
            _configs = new Dictionary<string, CellConfig>();
            _builderFactory = Substitute.For<IBuilderFactory>();
            _builder = new CellsBuilder(_configs, _builderFactory);
        }

        [Fact]
        public void Cell()
        {
            CellBuilder configBuilder = new CellBuilder(null);
            CellConfig config = null;
            TableEntity entity = new TableEntity();

            _builderFactory.CellBuilder(Arg.Do<CellConfig>(c => config = c)).Returns(configBuilder);

            CellBuilder builder = _builder.Cell(() => entity.Property);

            _builderFactory.Received(1).CellBuilder(Arg.Any<CellConfig>());
            _configs.Should().HaveCount(1);
            _configs.Should().ContainKey("Property");
            CellConfig cellConfig = _configs["Property"];
            cellConfig.Should().BeSameAs(config);
            cellConfig.CssClasses.Should().BeEmpty();
            cellConfig.State.Should().Be(ContextualState.Default);
            builder.Should().BeSameAs(configBuilder);
        }
    }
}
