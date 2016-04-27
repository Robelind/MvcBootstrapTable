using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MvcBootstrapTable.Builders;
using MvcBootstrapTable.Config;
using MvcBootstrapTable.Rendering;
using NSubstitute;
using Xunit;

namespace Test.Builders
{
    public class TableBuilderTest
    {
        private readonly ITableRenderer<TableEntity> _renderer;
        private TableBuilder<TableEntity> _builder;
        private readonly IBuilderFactory _builderFactory;
        private ITableConfig<TableEntity> _config;
        private readonly ITableConfig<TableEntity> _tableConfig;

        public TableBuilderTest()
        {
            TableModel<TableEntity> model = new TableModel<TableEntity>(Enumerable.Empty<TableEntity>());

            _renderer = Substitute.For<ITableRenderer<TableEntity>>();
            _renderer.Render(Arg.Do<ITableConfig<TableEntity>>(tc => _config = tc), Arg.Any<int>());
            _builderFactory = Substitute.For<IBuilderFactory>();
            _tableConfig = Substitute.For<ITableConfig<TableEntity>>();
            _tableConfig.Paging.Returns(new PagingConfig());
            _tableConfig.Update.Returns(new UpdateConfig());
            _tableConfig.Columns.Returns(new Dictionary<string, ColumnConfig>());
            _builder = new TableBuilder<TableEntity>(model, _renderer, _builderFactory, _tableConfig); 
        }

        [Fact]
        public void Id()
        {
            TableBuilder<TableEntity> builder = _builder.Id("Id");
            this.RenderAndVerify();

            _tableConfig.Received().Id = "Id";
            builder.Should().BeSameAs(_builder);
        }

        [Fact]
        public void Name()
        {
            TableBuilder<TableEntity> builder = _builder.Name("Name");
            this.RenderAndVerify();

            _tableConfig.Received().Name = "Name";
            builder.Should().BeSameAs(_builder);
        }

        [Fact]
        public void Caption()
        {
            TableBuilder<TableEntity> builder = _builder.Caption("Caption");
            this.RenderAndVerify();

            _tableConfig.Received().Caption = "Caption";
            builder.Should().BeSameAs(_builder);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Striped(bool striped)
        {
            TableBuilder<TableEntity> builder = _builder.Striped(striped);
            this.RenderAndVerify();

            _tableConfig.Received().Striped = striped;
            builder.Should().BeSameAs(_builder);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Bordered(bool bordered)
        {
            TableBuilder<TableEntity> builder = _builder.Bordered(bordered);
            this.RenderAndVerify();

            _tableConfig.Received().Bordered = bordered;
            builder.Should().BeSameAs(_builder);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void HoverState(bool hoverState)
        {
            TableBuilder<TableEntity> builder = _builder.HoverState(hoverState);
            this.RenderAndVerify();

            _tableConfig.Received().HoverState = hoverState;
            builder.Should().BeSameAs(_builder);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Condensed(bool condensed)
        {
            TableBuilder<TableEntity> builder = _builder.Condensed(condensed);
            this.RenderAndVerify();

            _tableConfig.Received().Condensed = condensed;
            builder.Should().BeSameAs(_builder);
        }

        [Theory]
        [InlineData(new[] {"C1"}, true)]
        [InlineData(new[] {"C1, C2"}, true)]
        [InlineData(new[] {"C1"}, false)]
        public void CssClass(string[] cssClasses, bool apply)
        {
            TableBuilder<TableEntity> builder = null;
            List<string> classes = new List<string>();

            _tableConfig.CssClasses.Returns(classes);

            foreach(var cssClass in cssClasses)
            {
                builder = _builder.CssClass(cssClass, apply);
            }
            this.RenderAndVerify();

            if(apply)
            {
                classes.ShouldBeEquivalentTo(cssClasses);
            }
            else
            {
                classes.Should().BeEmpty();
            }
            builder.Should().BeSameAs(_builder);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RowClick(bool apply)
        {
            TableBuilder<TableEntity> builder = _builder.RowClick("jsFunc", apply);
            this.RenderAndVerify();

            if(apply)
            {
                _tableConfig.Received().RowClick = "jsFunc";
            }
            else
            {
                _tableConfig.DidNotReceive().RowClick = Arg.Any<string>();
            }
            builder.Should().BeSameAs(_builder);
        }

        [Fact]
        public void Footer()
        {
            FooterBuilder footerBuilder = new FooterBuilder(null);
            FooterBuilder actionBuilder = null;
            FooterConfig config = null;

            _builderFactory.FooterBuilder(Arg.Do<FooterConfig>(fc => config = fc)).Returns(footerBuilder);

            TableBuilder<TableEntity> builder =_builder.Footer(fb => actionBuilder = fb);
            this.RenderAndVerify();

            _builderFactory.Received(1).FooterBuilder(Arg.Any<FooterConfig>());
            actionBuilder.Should().BeSameAs(footerBuilder);
            config.Should().BeSameAs(_config.Footer);
            builder.Should().BeSameAs(_builder);
        }

        [Fact]
        public void Paging()
        {
            PagingBuilder pagingBuilder = new PagingBuilder(null);
            PagingBuilder actionBuilder = null;
            PagingConfig config = null;

            _builderFactory.PagingBuilder(Arg.Do<PagingConfig>(c => config = c)).Returns(pagingBuilder);

            TableBuilder<TableEntity> builder = _builder.Paging(fb => actionBuilder = fb);
            this.RenderAndVerify();

            _builderFactory.Received(1).PagingBuilder(Arg.Any<PagingConfig>());
            actionBuilder.Should().BeSameAs(pagingBuilder);
            config.Should().BeSameAs(_config.Paging);
            builder.Should().BeSameAs(_builder);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Update(bool urlSet)
        {
            UpdateBuilder updateBuilder = new UpdateBuilder(new UpdateConfig());
            UpdateBuilder actionBuilder = null;
            UpdateConfig config = null;
            Exception exception = null;
            TableBuilder<TableEntity> builder = null;

            _builderFactory.UpdateBuilder(Arg.Do<UpdateConfig>(c => { config = c; c.Url = urlSet ? "" : null; }))
                .Returns(updateBuilder);

            try
            {
                builder = _builder.Update(b => actionBuilder = b);
                this.RenderAndVerify();
            }
            catch(Exception e)
            {
                exception = e;
            }

            _builderFactory.Received(1).UpdateBuilder(Arg.Any<UpdateConfig>());
            actionBuilder.Should().BeSameAs(updateBuilder);
            if(urlSet)
            {
                exception.Should().BeNull();
                config.Should().BeSameAs(_config.Update);
                builder.Should().BeSameAs(_builder);
            }
            else
            {
                exception.Should().BeOfType<ArgumentNullException>();
            }
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        [InlineData(true, true, false)]
        [InlineData(true, false, true)]
        public void RowConfig(bool rowsConfigured, bool pagingConfiguredBefore, bool pagingConfiguredAfter)
        {
            List<RowBuilder<TableEntity>> builders = new List<RowBuilder<TableEntity>>();
            List<RowBuilder<TableEntity>> actionBuilders = new List<RowBuilder<TableEntity>>();
            List<RowConfig<TableEntity>> configs = new List<RowConfig<TableEntity>>();
            List<TableEntity> entities = new List<TableEntity>();
            List<TableEntity> configEntities = new List<TableEntity>();
            
            _builderFactory.RowBuilder(Arg.Do<TableEntity>(e => entities.Add(e))).Returns(_ =>
            {
                RowBuilder<TableEntity> builder = new RowBuilder<TableEntity>(null, null);
                builders.Add(builder);
                return(builder);
            });
            IEnumerable<TableEntity> tableEntities = new []
            {
                new TableEntity(), new TableEntity(), new TableEntity(), new TableEntity(), 
            };
            TableModel<TableEntity> model = new TableModel<TableEntity>(tableEntities);

            _builder = new TableBuilder<TableEntity>(model, _renderer, _builderFactory, _tableConfig);
            _tableConfig.Rows.Returns(configs);
            _tableConfig.Paging.Returns(new PagingConfig {PageSize = pagingConfiguredBefore ? 2 : 0});
            _tableConfig.Update.Returns(new UpdateConfig {Url = "_"});

            if(rowsConfigured)
            {
                _builder.RowConfig((b, e) => {actionBuilders.Add(b); configEntities.Add(e);});
            }
            if(pagingConfiguredAfter)
            {
                _tableConfig.Paging.Returns(new PagingConfig {PageSize = 2});
            }
            this.RenderAndVerify(tableEntities.Count());

            if(rowsConfigured)
            {
                _builderFactory.Received(pagingConfiguredBefore ? 2 : tableEntities.Count())
                    .RowBuilder(Arg.Any<TableEntity>());
                actionBuilders.ShouldAllBeEquivalentTo(builders);
                configEntities.ShouldAllBeEquivalentTo(entities);
            }
            configs.Should().HaveCount(pagingConfiguredBefore || pagingConfiguredAfter ? 2 : tableEntities.Count());
        }

        [Fact]
        public void ColumnConfig()
        {
            ColumnsBuilder<TableEntity> columnsBuilder = new ColumnsBuilder<TableEntity>(null, null, null);
            ColumnsBuilder<TableEntity> actionBuilder = null;
            Dictionary<string, ColumnConfig> config = null;
            SortingConfig sortingConfig = null;

            _builderFactory.ColumnsBuilder<TableEntity>(Arg.Do<Dictionary<string, ColumnConfig>>(c => config = c),
                Arg.Do<SortingConfig>(c => sortingConfig = c)).Returns(columnsBuilder);

            TableBuilder<TableEntity> builder = _builder.ColumnConfig(fb => actionBuilder = fb);
            this.RenderAndVerify();

            _builderFactory.Received(1).ColumnsBuilder<TableEntity>(Arg.Any<Dictionary<string, ColumnConfig>>(),
                Arg.Any<SortingConfig>());
            actionBuilder.Should().BeSameAs(columnsBuilder);
            config.Should().BeSameAs(_config.Columns);
            sortingConfig.Should().BeSameAs(_config.Sorting);
            builder.Should().BeSameAs(_builder);
        }

        [Theory]
        [InlineData(false, false, false, false)]
        [InlineData(false, false, false, true)]
        [InlineData(true, false, false, false)]
        [InlineData(true, false, false, true)]
        [InlineData(false, true, false, false)]
        [InlineData(false, true, false, true)]
        [InlineData(false, false, true, false)]
        [InlineData(false, false, true, true)]
        public void OperationsConfig(bool paging, bool filtering, bool sorting, bool updateConfigured)
        {
            Dictionary<string, ColumnConfig> columns = new Dictionary<string, ColumnConfig>()
            {
                {"C1", new ColumnConfig {Filtering = new FilteringConfig()}},
                {"C2", new ColumnConfig {Filtering = new FilteringConfig {Threshold = filtering ? 2 : 0} }},
                {"C3", new ColumnConfig {Filtering = new FilteringConfig(),
                    SortState = sorting ? SortState.None : (SortState?)null}},
            };
            Exception exception = null;
            bool valid = (!paging && !filtering && !sorting) || updateConfigured;

            _tableConfig.Paging.Returns(new PagingConfig {PageSize = paging ? 2 : 0});
            _tableConfig.Columns.Returns(columns);
            _tableConfig.Update.Returns(new UpdateConfig {Url = updateConfigured ? "_" : null});

            try
            {
                this.RenderAndVerify();
            }
            catch(Exception e)
            {
                exception = e;
            }

            if(valid)
            {
                exception.Should().BeNull();
            }
            else
            {
                exception.Should().NotBeNull();
            }
        }
        
        private void RenderAndVerify(int expectedCount = 0)
        {
            _builder.Render();
            _renderer.Received(1).Render(Arg.Any<ITableConfig<TableEntity>>(), expectedCount);
            _config.Should().BeSameAs(_tableConfig);
        }
    }
}
