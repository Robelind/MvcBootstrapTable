using FluentAssertions;
using MvcBootstrapTable.Builders;
using MvcBootstrapTable.Config;
using Xunit;

namespace Test.Builders
{
    public class UpdateBuilderTest
    {
        private readonly UpdateBuilder _builder;
        private readonly UpdateConfig _config;

        public UpdateBuilderTest()
        {
            _config = new UpdateConfig();
            _builder = new UpdateBuilder(_config);
        }

        [Fact]
        public void Url()
        {
            UpdateBuilder builder = _builder.Url("Url");

            _config.Url.Should().Be("Url");
            builder.Should().BeSameAs(_builder);
        }

        [Fact]
        public void Start()
        {
            UpdateBuilder builder = _builder.Start("jsFunc");

            _config.Start.Should().Be("jsFunc");
            builder.Should().BeSameAs(_builder);
        }

        [Fact]
        public void Success()
        {
            UpdateBuilder builder = _builder.Success("jsFunc");

            _config.Success.Should().Be("jsFunc");
            builder.Should().BeSameAs(_builder);
        }

        [Fact]
        public void Error()
        {
            UpdateBuilder builder = _builder.Error("jsFunc");

            _config.Error.Should().Be("jsFunc");
            builder.Should().BeSameAs(_builder);
        }

        [Fact]
        public void Complete()
        {
            UpdateBuilder builder = _builder.Complete("jsFunc");

            _config.Complete.Should().Be("jsFunc");
            builder.Should().BeSameAs(_builder);
        }

        [Fact]
        public void QueryParameter()
        {
            UpdateBuilder builder = _builder.QueryParameter("Name", "Value");

            _config.CustomQueryPars.Should().ContainKey("Name");
            _config.CustomQueryPars["Name"].Should().Be("Value");
            builder.Should().BeSameAs(_builder);
        }

        [Fact]
        public void BusyIndicatorId()
        {
            UpdateBuilder builder = _builder.BusyIndicatorId("Id");

            _config.BusyIndicatorId.Should().Be("Id");
            builder.Should().BeSameAs(_builder);
        }
    }
}
