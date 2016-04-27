using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MvcBootstrapTable.Rendering;
using Xunit;

namespace Test.Rendering
{
    public class TableUpdaterTest
    {
        private readonly TableUpdater _updater;
        private readonly TableState _tableState;
        private readonly TableEntity[] _entities;

        public TableUpdaterTest()
        {
            _tableState = new TableState {Filter = new Dictionary<string, string>()};
            _updater = new TableUpdater(_tableState);
            _entities = new[]
            {
                new TableEntity {Property = "XXX", Property2 = "AAA", Property3 = 114},
                new TableEntity {Property = "YYY", Property2 = "BBB", Property3 = 112},
                new TableEntity {Property = "AAA", Property2 = "AAB", Property3 = 111},
                new TableEntity {Property = "ZZZ", Property2 = "XXX", Property3 = 115},
                new TableEntity {Property = "BBB", Property2 = "aaYYY", Property3 = 113},
                new TableEntity {Property = "BBB", Property2 = "AaXXX", Property3 = 113},
                new TableEntity {Property = "BBB", Property2 = "YYY", Property3 = 113},
            };
        }

        [Fact]
        public void Default()
        {
            TableModel<TableEntity> model = _updater.Update(_entities);

            model.Entities.ShouldBeEquivalentTo(_entities, cfg => cfg.WithStrictOrdering());
            model.EntityCount.Should().Be(_entities.Length);
        }

        [Fact]
        public void Filtering()
        {
            _tableState.Filter.Add("Property2", "Aa");

            IEnumerable<TableEntity> expected = _entities.Where(e =>
                e.Property2.StartsWith("AA", StringComparison.InvariantCultureIgnoreCase));
            
            TableModel<TableEntity> model = _updater.Update(_entities);

            model.Entities.ShouldBeEquivalentTo(expected, cfg => cfg.WithStrictOrdering());
            model.EntityCount.Should().Be(expected.Count());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Sorting(bool ascending)
        {
            IEnumerable<TableEntity> expected = ascending
                ? _entities.OrderBy(e => e.Property)
                : _entities.OrderByDescending(e => e.Property);

            _tableState.SortProp = "Property";
            _tableState.AscSort = ascending;

            TableModel<TableEntity> model = _updater.Update(_entities);

            model.Entities.ShouldBeEquivalentTo(expected, cfg => cfg.WithStrictOrdering());
            model.EntityCount.Should().Be(_entities.Count());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void Paging(int currentPage)
        {
            IEnumerable<TableEntity> expected = _entities.Skip((currentPage - 1) * 3).Take(3).ToList();

            _tableState.PageSize = 3;
            _tableState.Page = currentPage;

            TableModel<TableEntity> model = _updater.Update(_entities);

            model.Entities.ShouldBeEquivalentTo(expected, cfg => cfg.WithStrictOrdering());
            model.EntityCount.Should().Be(_entities.Count());
        }

        [Theory]
        [InlineData(0, true, false)]
        [InlineData(0, false, true)]
        [InlineData(0, true, true)]
        [InlineData(1, false, false)]
        [InlineData(2, false, false)]
        [InlineData(3, false, false)]
        [InlineData(1, true, false)]
        [InlineData(2, true, false)]
        [InlineData(3, true, false)]
        [InlineData(1, false, true)]
        [InlineData(2, false, true)]
        [InlineData(3, false, true)]
        [InlineData(1, true, true)]
        [InlineData(2, true, true)]
        [InlineData(3, true, true)]
        public void Combinations(int currentPage, bool filtered, bool sorted)
        {
            IEnumerable<TableEntity> entities = _entities;
            int expectedCount = entities.Count();

            _tableState.PageSize = currentPage != 0 ? 3 : 0;
            _tableState.Page = currentPage;
            _tableState.SortProp = sorted ? "Property" : null;
            if(filtered)
            {
                _tableState.Filter.Add("Property2", "Aa");
                entities = entities.Where(e => e.Property2.StartsWith("AA", StringComparison.InvariantCultureIgnoreCase));
                expectedCount = entities.Count();
            }
            if(sorted)
            {
                entities = entities.OrderByDescending(e => e.Property);
            }
            if(currentPage != 0)
            {
                entities = entities.Skip((currentPage - 1) * 3).Take(3).ToList();
            }

            TableModel<TableEntity> model = _updater.Update(_entities);

            model.Entities.ShouldBeEquivalentTo(entities, cfg => cfg.WithStrictOrdering());
            model.EntityCount.Should().Be(expectedCount);
        }
    }
}
