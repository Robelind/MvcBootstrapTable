using FluentAssertions;
using MvcBootstrapTable.Builders;
using MvcBootstrapTable.Config;
using Xunit;

namespace Test.Builders
{
    public class FilteringBuilderTest
    {
        private readonly FilteringBuilder _builder;
        private readonly FilteringConfig _config;

        public FilteringBuilderTest()
        {
            _config = new FilteringConfig();
            _builder = new FilteringBuilder(_config);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Threshold(bool apply)
        {
            FilteringBuilder builder =_builder.Threshold(2, apply);

            _config.Threshold.Should().Be(apply ? 2 : 0);
            builder.Should().BeSameAs(_builder);
        }

        [Theory]
        [InlineData(new[] {"C1"}, true)]
        [InlineData(new[] {"C1, C2"}, true)]
        [InlineData(new[] {"C1"}, false)]
        public void CssClass(string[] cssClasses, bool apply)
        {
            FilteringBuilder builder = null;

            foreach(var cssClass in cssClasses)
            {
                builder = _builder.CssClass(cssClass, apply);
            }

            if(apply)
            {
                _config.CssClasses.ShouldBeEquivalentTo(cssClasses);
            }
            else
            {
                _config.CssClasses.Should().BeEmpty();
            }
            builder.Should().BeSameAs(_builder);
        }
    }
}
