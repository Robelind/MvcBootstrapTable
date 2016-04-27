using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Primitives;
using MvcBootstrapTable.Rendering;
using NSubstitute;
using Xunit;

namespace Test.Rendering
{
    public class TableStateParserTest
    {
        private readonly TableStateParser _parser;
        private readonly IReadableStringCollection _queryPars;
        private readonly HttpContext _httpContext;

        public TableStateParserTest()
        {
            HttpRequest request = Substitute.For<HttpRequest>();

            _httpContext = Substitute.For<HttpContext>();
            _httpContext.Request.Returns(request);
            _queryPars = Substitute.For<IReadableStringCollection>();
            request.Query.Returns(_queryPars);
            _parser = new TableStateParser();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void SortProperty(bool hasValue)
        {
            this.ArrangeAndAct("sort", hasValue).SortProp.Should().Be(hasValue ? "Value" : null);
        }

        [Theory]
        [InlineData(false, "")]
        [InlineData(true, "True")]
        [InlineData(true, "False")]
        public void SortDirection(bool hasValue, string value)
        {
            this.ArrangeAndAct("asc", hasValue, value).AscSort.Should().Be(hasValue && value == "True");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Page(bool hasValue)
        {
            this.ArrangeAndAct("page", hasValue, "5").Page.Should().Be(hasValue ? 5 : 1);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void PageSize(bool hasValue)
        {
            this.ArrangeAndAct("pageSize", hasValue, "5").PageSize.Should().Be(hasValue ? 5 : 0);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CurrentFilter(bool hasValue)
        {
            this.ArrangeAndAct("currentFilter", hasValue, "Property").CurrentFilter.Should()
                .Be(hasValue ? "Property" : null);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ContainerId(bool hasValue)
        {
            this.ArrangeAndAct("containerId", hasValue, "Id").ContainerId.Should().Be(hasValue ? "Id" : null);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void Filter(int count)
        {
            List<string> filters = new List<string>();

            for(int i = 1; i <= count; i++)
            {
                filters.Add("Prop" + i);
                filters.Add("Value" + i);
            }
            _queryPars["filter[]"].Returns(new StringValues(filters.ToArray()));

            TableState tableState = _parser.Parse(_httpContext);

            tableState.Filter.Should().HaveCount(count);
            for(int i = 1; i <= count; i++)
            {
                tableState.Filter["Prop" + i].Should().Be("Value" + i);
            }
        }

        private TableState ArrangeAndAct(string key, bool hasValue, string value = "Value")
        {
            _queryPars[key].Returns(hasValue ? new StringValues(value) : StringValues.Empty);

            return(_parser.Parse(_httpContext));
        }
    }
}
