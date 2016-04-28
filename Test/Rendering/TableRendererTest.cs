using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.AspNet.Html.Abstractions;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Extensions.WebEncoders;
using MvcBootstrapTable;
using MvcBootstrapTable.Builders;
using MvcBootstrapTable.Config;
using MvcBootstrapTable.Rendering;
using NSubstitute;
using Xunit;

namespace Test.Rendering
{
    public class TableRendererTest
    {
        private readonly TableState _tableState;
        private readonly TableRenderer<TableEntity> _renderer;
        private readonly ITableConfig<TableEntity> _tableConfig;
        private IEnumerable<TableNode> _nodes;
        private string _containerId;

        public TableRendererTest()
        {
            ITableNodeParser nodeParser = Substitute.For<ITableNodeParser>();

            _tableState = new TableState
            {
                Filter = new Dictionary<string, string>(), Page = 1, PageSize = 3,
                SortProp = "SortProp", AscSort = true
            };
            _tableConfig = Substitute.For<ITableConfig<TableEntity>>();
            _tableConfig.Columns.Returns(new Dictionary<string, ColumnConfig>());
            _tableConfig.Paging.Returns(new PagingConfig());
            _tableConfig.Footer.Returns(new FooterConfig());
            _tableConfig.Sorting.Returns(new SortingConfig());
            _tableConfig.Update.Returns(new UpdateConfig
            {
                Url = "Url", BusyIndicatorId = "BusyId", Start = "start", Success = "success",
                Error = "error",
            });
            _tableConfig.Paging.Returns(new PagingConfig());
            nodeParser.Parse(Arg.Do<IEnumerable<TableNode>>(n => _nodes = n)).Returns(new HtmlString("Content"));
            _renderer = new TableRenderer<TableEntity>(_tableState, nodeParser);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Basic(bool container)
        {
            _tableState.ContainerId = container ? null : "Id";

            this.ActAndValidate();

            if(container)
            {
                Guid dummy;

                _nodes.ElementAt(0).Element.Attributes.Should().ContainKey("Id");
                Guid.TryParse(_nodes.ElementAt(0).Element.Attributes["Id"], out dummy).Should().BeTrue();
                _nodes.ElementAt(0).Element.Attributes.Should().ContainKey("class");
                _nodes.ElementAt(0).Element.Attributes["class"].Should().Be("TableContainer");
            }
            else
            {
                _nodes.ElementAt(0).Element.Attributes.Should().BeEmpty();
            }
            _nodes.ElementAt(0).InnerContent.Should().HaveCount(1);
            TableNode node = _nodes.ElementAt(0).InnerContent.First();
            node.Element.TagName.Should().Be(container ? "div" : "table");
            if(container)
            {
                node.InnerContent.Should().HaveCount(1);
                node.InnerContent.First().Element.TagName.Should().Be("table");
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TableAttributes(bool attributes)
        {
            _tableConfig.Id = attributes ? "Id" : null;
            _tableConfig.Name = attributes ? "Name" : null;
            _tableConfig.Striped = attributes;
            _tableConfig.Bordered = attributes;
            _tableConfig.Condensed = attributes;
            _tableConfig.HoverState = attributes;
            if(attributes)
            {
                _tableConfig.CssClasses.Returns(new[] {"Custom1", "Custom2"});
            }

            this.ActAndValidate();

            TagBuilder table = this.Node("table").Element;
            table.Attributes.Should().HaveCount(attributes ? 3 : 1);
            this.VerifyAttribute(table, "class", "table");
            if(attributes)
            {
                this.VerifyAttribute(table, "id", "Id");
                this.VerifyAttribute(table, "name", "Name");
                this.VerifyAttribute(table, "class", "table-striped");
                this.VerifyAttribute(table, "class", "table-bordered");
                this.VerifyAttribute(table, "class", "table-hover");
                this.VerifyAttribute(table, "class", "table-condensed");
                this.VerifyAttribute(table, "class", "Custom1");
                this.VerifyAttribute(table, "class", "Custom2");
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Caption(bool caption)
        {
            _tableConfig.Caption = caption ? "Caption" : null;

            this.ActAndValidate();

            TableNode table = this.Node("table");
            if(caption)
            {
                TableNode captionNode = table.InnerContent.SingleOrDefault(n => n.Element.TagName == "caption");

                captionNode.Should().NotBeNull();
                this.VerifyInnerHtml(captionNode.Element, "Caption");
            }
            else
            {
                table.InnerContent.Any(n => n.Element.TagName == "caption").Should().BeFalse();
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ColumnHeaders(bool anyConfigured)
        {
            _tableConfig.Columns.Add("Property", new ColumnConfig
            {
                Header = anyConfigured ? "Header1" : ""
            });
            _tableConfig.Columns.Add("Property2", new ColumnConfig
            {
                Header = anyConfigured ? "Header2" : null
            });
            _tableConfig.Columns.Add("Property3", new ColumnConfig());
            _tableConfig.Columns["Property2"].CssClasses.Add("CssClass1");
            _tableConfig.Columns["Property2"].CssClasses.Add("CssClass2");

            this.ActAndValidate();

            TableNode header = this.Node("thead");
            if(anyConfigured)
            {
                header.Should().NotBeNull();
                header.InnerContent.Should().HaveCount(1);
                TableNode headerRow = header.InnerContent.First();
                headerRow.Element.TagName.Should().Be("tr");
                headerRow.InnerContent.Should().HaveCount(3);
                headerRow.InnerContent.Select(ic => ic.Element.TagName).Should().OnlyContain(s => s == "th");
                this.VerifyInnerHtml(headerRow.InnerContent.ElementAt(0).Element, "Header1");
                this.VerifyInnerHtml(headerRow.InnerContent.ElementAt(1).Element, "Header2");
                this.VerifyInnerHtml(headerRow.InnerContent.ElementAt(2).Element, "");
            }
            else
            {
                header.Should().BeNull();
            }
        }

        [Theory]
        [InlineData(false, null, null, false)]
        [InlineData(true, null, null, false)]
        [InlineData(true, SortState.Ascending, null, false)]
        [InlineData(true, SortState.Descending, null, false)]
        [InlineData(true, SortState.Ascending, "Property", true)]
        [InlineData(true, SortState.Ascending, "Property2", true)]
        [InlineData(true, null, "Property", true)]
        [InlineData(true, null, "Property", false)]
        internal void ColumnHeadersSorting(bool anyConfigured, SortState? sortedInitialState,
            string currentSortProp, bool currentAsc)
        {
            Dictionary<string, ColumnConfig> columns = new Dictionary<string, ColumnConfig>
            {
                {"Property", new ColumnConfig
                    {
                        Header = anyConfigured ? "Header1" : "", SortState = SortState.None,
                    }
                },
                {"Property2", new ColumnConfig { SortState = sortedInitialState,}},
                { "Property3", new ColumnConfig()},
            };
            _tableConfig.Columns.Returns(columns);
            _tableConfig.Sorting.Returns(new SortingConfig
            {
                IconLib = "IconLib", AscendingCssClass = "Asc", DescendingCssClass = "Desc"
            });
            _tableState.SortProp = currentSortProp;
            if(currentSortProp != null)
            {
                _tableState.AscSort = currentAsc;
            }
            else
            {
                _tableState.ContainerId = null;
            }

            this.ActAndValidate();

            TableNode headerRow = this.VerifyHeader(anyConfigured);
            if(headerRow != null)
            {
                for(int i = 0; i < _tableConfig.Columns.Count; i++)
                {
                    TableNode header = headerRow.InnerContent.ElementAt(i);
                    KeyValuePair<string, ColumnConfig> column = _tableConfig.Columns.ElementAt(i);

                    if(column.Value.SortState.HasValue)
                    {
                        header.InnerContent.Should().HaveCount(2);

                        TableNode sortAsc = header.InnerContent.First();
                        TableNode sortDesc = header.InnerContent.Last();
                        sortAsc.Element.TagName.Should().Be("a");
                        sortDesc.Element.TagName.Should().Be("a");
                        this.VerifyCssClass(sortAsc.Element, "SortIcon");
                        this.VerifyCssClass(sortDesc.Element, "SortIcon");

                        sortAsc.InnerContent.Should().HaveCount(1);
                        sortDesc.InnerContent.Should().HaveCount(1);
                        TableNode ascIcon = sortAsc.InnerContent.First();
                        TableNode descIcon = sortDesc.InnerContent.First();
                        this.VerifyCssClasses(ascIcon.Element, new [] {"IconLib", "Asc"});
                        this.VerifyCssClasses(descIcon.Element, new [] {"IconLib", "Desc"});

                        if((sortedInitialState.HasValue && column.Key == "Property2" && currentSortProp == null) ||
                            column.Key == currentSortProp)
                        {
                            if(sortedInitialState == SortState.Ascending || currentAsc)
                            {
                                this.VerifyCssClass(sortAsc.Element, "ActiveSort");
                                this.VerifyCssClass(sortDesc.Element, "ActiveSort", false);
                            }
                            else
                            {
                                this.VerifyCssClass(sortDesc.Element, "ActiveSort");
                                this.VerifyCssClass(sortAsc.Element, "ActiveSort", false);
                            }
                        }
                        else
                        {
                            this.VerifyCssClass(sortAsc.Element, "ActiveSort", false);
                            this.VerifyCssClass(sortDesc.Element, "ActiveSort", false);
                        }
                    }
                    else
                    {
                        header.InnerContent.Should().BeEmpty();
                    }                 
                }
            }
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(true, false, true)]
        [InlineData(true, true, false)]
        [InlineData(true, true, true)]
        public void ColumnHeadersFiltering(bool anyConfigured, bool filtered, bool currentFilter)
        {
            _tableConfig.Columns.Add("Property", new ColumnConfig
            {
                Header = anyConfigured ? "Header1" : "",
                Filtering = new FilteringConfig {Threshold = 2},
            });
            _tableConfig.Columns.Add("Property2", new ColumnConfig
            {
                Filtering = new FilteringConfig {Threshold = 2},
            });
            _tableConfig.Columns.ElementAt(1).Value.Filtering.CssClasses.Add("C1");
            _tableConfig.Columns.ElementAt(1).Value.Filtering.CssClasses.Add("C2");
            if(filtered)
            {
                _tableState.Filter.Add("Property", "abc");
                _tableState.Filter.Add("Property2", "xxx");
            }
            _tableState.CurrentFilter = currentFilter ? "Property2" : null;
            
            this.ActAndValidate();

            TableNode headerRow = this.VerifyHeader(anyConfigured, true);
            if(headerRow != null)
            {
                for(int i = 0; i < _tableConfig.Columns.Count; i++)
                {
                    TableNode headerCell = headerRow.InnerContent.ElementAt(i);
                    if(_tableConfig.Columns.ElementAt(i).Value.Filtering.Threshold > 0)
                    {
                        headerCell.InnerContent.Should().HaveCount(1);
                        TableNode filter = headerCell.InnerContent.First();
                        filter.Element.TagName.Should().Be("input");
                        this.VerifyAttribute(filter.Element, "type", "text");
                        this.VerifyAttribute(filter.Element, "data-filter-prop",
                            _tableConfig.Columns.ElementAt(i).Key);
                        this.VerifyAttribute(filter.Element, "data-filter-threshold",
                            _tableConfig.Columns.ElementAt(i).Value.Filtering.Threshold.ToString());
                        this.VerifyCssClasses(filter.Element,
                            _tableConfig.Columns.ElementAt(i).Value.Filtering.CssClasses.Concat(new []{"form-control"}));

                        var filterValue = _tableState.Filter.SingleOrDefault(fv =>
                            _tableConfig.Columns.ElementAt(i).Key == fv.Key);
                        if(filterValue.Key != null)
                        {
                            this.VerifyAttribute(filter.Element, "value", filterValue.Value);
                        }
                        if(_tableConfig.Columns.ElementAt(i).Key == _tableState.CurrentFilter)
                        {
                            this.VerifyAttribute(filter.Element, "data-filter-focus", "");
                        }
                        else
                        {
                            this.VerifyAttribute(filter.Element, "data-filter-focus");
                        }
                    }
                    else
                    {
                        headerCell.InnerContent.Should().BeEmpty();
                    }
                }
            }
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        [InlineData(false, true, true)]
        [InlineData(true, false, false)]
        [InlineData(true, true, false)]
        [InlineData(true, true, true)]
        public void Rows(bool values, bool rowClick, bool globalRowClick)
        {
            TableEntity entity = new TableEntity
            {
                Property = "Value1", Property2 = values ? "Value2" : null, Property3 = 123,
            };
            RowConfig<TableEntity> row = new RowConfig<TableEntity>(entity)
            {
                State = values ? ContextualState.Danger : ContextualState.Default,
                NavigationUrl = values ? "NavUrl" : null,
                RowClick = rowClick ? "RowClick" : null,
            };
            if(values)
            {
                row.CssClasses.Add("C1");
                row.CssClasses.Add("C2");
                row.CellConfigs.Add("Property", new CellConfig{State = ContextualState.Warning});
                row.CellConfigs.ElementAt(0).Value.CssClasses.Add("Css1");
                row.CellConfigs.ElementAt(0).Value.CssClasses.Add("Css2");
            }
            _tableConfig.RowClick.Returns(globalRowClick ? "GlobalRowClick" : null);
            _tableConfig.Rows.Returns(new[]
            {
                new RowConfig<TableEntity>(new TableEntity()),
                row,
                new RowConfig<TableEntity>(new TableEntity()),
            });

            this.ActAndValidate();

            TableNode body = this.Node("tbody");
            body.Should().NotBeNull();
            body.InnerContent.Should().HaveCount(3);
            body.InnerContent.Select(ic => ic.Element.TagName).Should().OnlyContain(tn => tn == "tr");
            for(int i = 0; i < body.InnerContent.Count; i++)
            {
                TableNode rowNode = body.InnerContent.ElementAt(i);
                List<string> cssClasses = new List<string>(_tableConfig.Rows.ElementAt(i).CssClasses);
                bool rowValues = i == 1 && values;

                if(rowValues)
                {
                    cssClasses.Add("danger");
                }
                this.VerifyCssClasses(rowNode.Element, cssClasses);
                this.VerifyAttribute(rowNode.Element, "style", rowValues || (i == 1 && rowClick) || globalRowClick
                    ? "cursor: pointer" : null);
                if(rowValues)
                {
                    this.VerifyAttribute(rowNode.Element, "onclick", "window.location.href = 'NavUrl'");
                }
                else if(i == 1 && rowClick)
                {
                    this.VerifyAttribute(rowNode.Element, "onclick", "RowClick");
                }
                else if(globalRowClick)
                {
                    this.VerifyAttribute(rowNode.Element, "onclick", "GlobalRowClick(this)");
                }
                else
                {
                    this.VerifyAttribute(rowNode.Element, "onclick");
                }

                rowNode.InnerContent.Should().HaveCount(3);
                rowNode.InnerContent.Select(ic => ic.Element.TagName).Should().OnlyContain(tn => tn == "td");
                if(i == 1)
                {
                    this.VerifyInnerHtml(rowNode.InnerContent.ElementAt(0).Element, entity.Property);
                    this.VerifyInnerHtml(rowNode.InnerContent.ElementAt(1).Element,
                        values ? entity.Property2 : string.Empty);
                    this.VerifyInnerHtml(rowNode.InnerContent.ElementAt(2).Element,
                        entity.Property3.ToString());

                    this.VerifyCssClasses(rowNode.InnerContent.ElementAt(0).Element,
                        values ? new[] {"warning", "Css1", "Css2"} : Enumerable.Empty<string>());
                    this.VerifyCssClasses(rowNode.InnerContent.ElementAt(1).Element, Enumerable.Empty<string>());
                    this.VerifyCssClasses(rowNode.InnerContent.ElementAt(2).Element, Enumerable.Empty<string>());
                }
            }
        }

        [Theory]
        [InlineData(false, false, false, false, false, false)]
        [InlineData(false, false, false, false, true, false)]
        [InlineData(false, false, false, false, false, true)]
        [InlineData(false, false, true, false, true, false)]
        [InlineData(false, false, true, false, false, true)]
        [InlineData(true, false, false, false, false, false)]
        [InlineData(true, true, false, false, false, false)]
        [InlineData(false, false, true, false, false, false)]
        [InlineData(false, false, true, true, false, false)]
        public void FooterElements(bool text, bool css, bool paging, bool multiplePages, bool pageInfo,
            bool directPageAccess)
        {
            FooterConfig config = new FooterConfig
            {
                Text = text ? "Text" : null,
                State = css ? ContextualState.Danger : ContextualState.Default,
            };

            if(css)
            {
                config.CssClasses.Add("Css1");
                config.CssClasses.Add("Css2");
            }
            _tableConfig.Footer.Returns(config);
            _tableConfig.Paging.Returns(new PagingConfig
            {
                PageSize = paging ? 2 : 0, DirectPageAccess = directPageAccess, PageInfo = pageInfo,
            });

            this.ActAndValidate(multiplePages ? 5 : 2);

            TableNode footer = this.Node("tfoot");
            if(text || (paging && (multiplePages || pageInfo)))
            {
                IEnumerable<string> cssClasses = css
                    ? config.CssClasses.Concat(new [] {"danger"})
                    : Enumerable.Empty<string>();
                List<Action<TableNode>> verifyActions = new List<Action<TableNode>>();

                footer.Should().NotBeNull();
                TableNode footerRow = this.OnlyInner(footer, "tr");
                TableNode footerCell = this.OnlyInner(footerRow, "td");
                this.VerifyCssClasses(footerCell.Element, cssClasses);
                this.VerifyAttribute(footerCell.Element, "colspan", "3");

                if(pageInfo)
                {
                    verifyActions.Add(this.VerifyFooterPageInfo);
                }
                if(paging && multiplePages)
                {
                    if(directPageAccess)
                    {
                        verifyActions.Add(this.VerifyFooterDirectPageAccess);
                    }
                    verifyActions.Add(this.VerifyFooterNavLast);
                    verifyActions.Add(this.VerifyFooterNavNext);
                    verifyActions.Add(this.VerifyFooterNavPrev);
                    verifyActions.Add(this.VerifyFooterNavFirst);
                }
                if(text)
                {
                    verifyActions.Add(this.VerifyFooterText);
                }
                footerCell.InnerContent.Should().HaveSameCount(verifyActions);
                for(int i = 0; i < verifyActions.Count; i++)
                {
                    verifyActions[i](footerCell.InnerContent[i]);
                }
            }
            else
            {
                footer.Should().BeNull();
            }
        }

        private void VerifyFooterText(TableNode node)
        {
            node.Element.TagName.Should().Be("div");
            this.VerifyCssClass(node.Element, "pull-right", false);
            this.VerifyCssClass(node.Element, "FooterTextContainer");
        }

        private void VerifyFooterPageInfo(TableNode node)
        {
            node.Element.TagName.Should().Be("div");
            this.VerifyCssClass(node.Element, "pull-right");
            this.VerifyCssClass(node.Element, "NavInfoContainer");
        }

        private void VerifyFooterDirectPageAccess(TableNode node)
        {
            node.Element.TagName.Should().Be("div");
            this.VerifyCssClass(node.Element, "pull-right");
            this.VerifyCssClass(node.Element, "NavAccessContainer");
        }

        private void VerifyFooterNavFirst(TableNode node)
        {
            this.VerifyFooterNav(node, "NavFirst");
        }

        private void VerifyFooterNavPrev(TableNode node)
        {
            this.VerifyFooterNav(node, "NavPrevious");
        }

        private void VerifyFooterNavNext(TableNode node)
        {
            this.VerifyFooterNav(node, "NavNext");
        }

        private void VerifyFooterNavLast(TableNode node)
        {
            this.VerifyFooterNav(node, "NavLast");
        }

        private void VerifyFooterNav(TableNode node, string cssClass)
        {
            node.Element.TagName.Should().Be("div");
            this.VerifyCssClass(node.Element, "pull-right");
            this.VerifyCssClass(node.Element, "NavBtnContainer");
            TableNode link = this.OnlyInner(node, "a");
            this.VerifyCssClass(link.Element, cssClass);
        }

        [Fact]
        public void FooterText()
        {
            _tableConfig.Footer.Text = "Text";

            this.ActAndValidate();

            TableNode footerCell = this.OnlyInner(this.OnlyInner(this.Node("tfoot"), "tr"), "td");
            TableNode container = footerCell.InnerContent.Single(ic =>
                ic.Element.Attributes["class"].Contains("FooterTextContainer"));
            TableNode text = this.OnlyInner(container, "span");
            this.VerifyCssClass(text.Element, "FooterText");
            this.VerifyInnerHtml(text.Element, "Text");
        }

        [Theory]
        [InlineData(0, 2)]
        [InlineData(2, 2)]
        [InlineData(2, 3)]
        [InlineData(2, 4)]
        public void FooterPageInfo(int pageSize, int entityCount)
        {
            int expectedLastPage = pageSize > 0
                ? entityCount / pageSize + (entityCount % pageSize > 0 ? 1 : 0)
                : 1;

            _tableConfig.Paging.PageInfo = true;
            _tableConfig.Paging.PageSize = 2;
            _tableState.Page = 1;
            _tableState.PageSize = pageSize;

            this.ActAndValidate(entityCount);

            TableNode footerCell = this.OnlyInner(this.OnlyInner(this.Node("tfoot"), "tr"), "td");
            TableNode container = footerCell.InnerContent.Single(ic =>
                ic.Element.Attributes["class"].Contains("NavInfoContainer"));
            TableNode info = this.OnlyInner(container, "span");
            this.VerifyInnerHtml(info.Element, $"1/{expectedLastPage}");
        }

        [Theory]
        [InlineData(2, 3, 1)]
        [InlineData(2, 3, 2)]
        [InlineData(2, 4, 1)]
        [InlineData(2, 4, 2)]
        [InlineData(2, 5, 1)]
        [InlineData(2, 5, 2)]
        [InlineData(2, 5, 3)]
        public void FooterDirectPageAccess(int pageSize, int entityCount, int currentPage)
        {
            int pageCount = entityCount / pageSize + (entityCount % pageSize > 0 ? 1 : 0);
            Guid id;

            _tableConfig.Paging.DirectPageAccess = true;
            _tableConfig.Paging.PageSize = 2;
            _tableState.Page = currentPage;
            _tableState.PageSize = pageSize;

            this.ActAndValidate(entityCount);

            TableNode footerCell = this.OnlyInner(this.OnlyInner(this.Node("tfoot"), "tr"), "td");
            TableNode container = footerCell.InnerContent.Single(ic =>
                ic.Element.Attributes["class"].Contains("NavAccessContainer"));
            container.InnerContent.Should().HaveCount(2);
            TableNode pages = container.InnerContent.First();
            pages.Element.TagName.Should().Be("select");
            pages.Element.Attributes.Should().ContainKey("data-pageselector-id");
            pages.Element.Attributes["data-pageselector-id"].Should().StartWith("#");
            Guid.TryParse(pages.Element.Attributes["data-pageselector-id"].Substring(1), out id)
                .Should().BeTrue();
            pages.InnerContent.Should().HaveCount(pageCount);
            for(int i = 1; i <= pageCount; i++)
            {
                TableNode page = pages.InnerContent.ElementAt(i - 1);

                page.Element.TagName.Should().Be("option");
                this.VerifyInnerHtml(page.Element, i.ToString());
                this.VerifyAttribute(page.Element, "selected", i == currentPage ? "" : null);
                page.Element.Attributes.Should().ContainKey("value");
                page.Element.Attributes["value"].Should().Contain($"&page={i}");
            }

            TableNode pageSelector = container.InnerContent.Last();
            pageSelector.Element.TagName.Should().Be("a");
            this.VerifyAttribute(pageSelector.Element, "id", id.ToString());
        }

        [Theory]
        [InlineData(2, 3, 1, true)]
        [InlineData(2, 3, 2, true)]
        [InlineData(2, 4, 1, true)]
        [InlineData(2, 4, 2, true)]
        [InlineData(2, 5, 1, false)]
        [InlineData(2, 5, 2, false)]
        [InlineData(2, 5, 3, false)]
        public void FooterNavigation(int pageSize, int entityCount, int currentPage, bool icons)
        {
            int pageCount = entityCount / pageSize + (entityCount % pageSize > 0 ? 1 : 0);

            _tableConfig.Paging.IconLib = icons ? "IconLib" : null;
            _tableConfig.Paging.FirstCssClass = "First";
            _tableConfig.Paging.NextCssClass = "Next";
            _tableConfig.Paging.PreviousCssClass = "Prev";
            _tableConfig.Paging.LastCssClass = "Last";
            _tableConfig.Paging.PageSize = 2;
            _tableState.Page = currentPage;
            _tableState.PageSize = pageSize;

            this.ActAndValidate(entityCount);

            TableNode footerCell = this.OnlyInner(this.OnlyInner(this.Node("tfoot"), "tr"), "td");
            this.VerifyNavElement(footerCell, "NavFirst", currentPage == 1, icons ? "First" : null);
            this.VerifyNavElement(footerCell, "NavPrevious", currentPage == 1, icons ? "Prev" : null);
            this.VerifyNavElement(footerCell, "NavNext", currentPage == pageCount, icons ? "Next" : null);
            this.VerifyNavElement(footerCell, "NavLast", currentPage == pageCount, icons ? "Last" : null);
        }

        private void VerifyNavElement(TableNode footer, string css, bool disabled, string iconClass)
        {
            TableNode nav = footer.InnerContent.Single(ic =>
                ic.Element.Attributes["class"].Contains("NavBtnContainer")
                && ic.InnerContent.First().Element.Attributes["class"].Contains(css))
                .InnerContent.First();
            this.VerifyCssClasses(nav.Element, new []{"btn", "btn-default"});
            this.VerifyAttribute(nav.Element, "disabled", disabled ? "disabled" : null);
            if(iconClass != null)
            {
                this.VerifyCssClasses(this.OnlyInner(nav, "span").Element, new []{"IconLib", iconClass});
            }
            else
            {
                nav.InnerContent.Should().BeEmpty();
            }
        }

        [Fact]
        public void Javascript()
        {
            this.ActAndValidate();

            this.VerifyInnerHtml(_nodes.First().InnerContent.First().Element,
                string.Format(JsCode.Code, _containerId));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void FilteringLinks(bool filteringConfigured)
        {
            _tableConfig.Columns.Add("Property", new ColumnConfig
            {
                Filtering = new FilteringConfig {Threshold = filteringConfigured ? 2 : 0},
            });

            this.ActAndValidate();

            TableNode innerContainer = _nodes.First().InnerContent.First();
            innerContainer.InnerContent.Should().HaveCount(filteringConfigured ? 3 : 1);
            if(filteringConfigured)
            {
                TableNode link = innerContainer.InnerContent.ElementAt(1);
                TableNode template = innerContainer.InnerContent.ElementAt(2);

                link.Element.TagName.Should().Be("a");
                this.VerifyAjax(link.Element, null);
                template.Element.TagName.Should().Be("input");
                this.VerifyAttribute(link.Element, "id", "FilterLink");
                this.VerifyAttribute(template.Element, "id", "FilterLinkTemplate");
                this.VerifyAttribute(template.Element, "type", "hidden");
            }
        }

        [Theory]
        [InlineData(false, null, null, false, false)]
        [InlineData(true, "Property2", "ContainerId", true, true)]
        public void Links(bool ascSort, string sortProp, string containerId, bool filter, bool custom)
        {
            Dictionary<string, ColumnConfig> columns = new Dictionary<string, ColumnConfig>
            {
                {"Property", new ColumnConfig {
                    Header = "Header1", SortState = SortState.None,
                    Filtering = new FilteringConfig {Threshold = 2},
                }},
                {"Property2", new ColumnConfig {
                    Header = "Header2", SortState = SortState.None,
                    Filtering = new FilteringConfig {Threshold = 2},
                }},
                {"Property3", new ColumnConfig {
                    Header = "Header3", SortState = SortState.None,
                    Filtering = new FilteringConfig {Threshold = 2},
                }},
            };

            _tableConfig.Columns.Returns(columns);
            _tableConfig.Paging.PageSize = 2;
            _tableConfig.Paging.DirectPageAccess = true;
            _tableState.AscSort = ascSort;
            _tableState.SortProp = sortProp;
            _tableState.ContainerId = containerId;
            _tableState.Page = 2;
            _containerId = containerId;
            if(filter)
            {
                _tableState.Filter.Add("Prop1", "Value1");
                _tableState.Filter.Add("Prop2", "Value2");
            }
            if(custom)
            {
                _tableConfig.Update.Returns(new UpdateConfig {CustomQueryPars = new Dictionary<string, string>
                {
                    {"Custom1", "Val1"}, {"Custom2", "Val2"}, 
                }, Url = "Url"});
            }

            this.ActAndValidate(5);

            List<string> genericQueryPars = new List<string>(new[]
            {
                $"pageSize={_tableConfig.Paging.PageSize}",
                $"containerId={containerId ?? _containerId}",
            });
            TableNode node = this.Node("thead");
            IEnumerable<string> expectedQueryPars;

            if(filter)
            {
                genericQueryPars.Add("filter[]=Prop1");
                genericQueryPars.Add("filter[]=Value1");
                genericQueryPars.Add("filter[]=Prop2");
                genericQueryPars.Add("filter[]=Value2");
            }
            genericQueryPars.AddRange(_tableConfig.Update.CustomQueryPars.Select(queryPar =>
                $"{queryPar.Key}={queryPar.Value}"));

            IEnumerable<string> pars = genericQueryPars.Concat(new[] {$"page={_tableState.Page}"});
            for(int i = 0; i < node.InnerContent.First().InnerContent.Count; i++)
            {
                TableNode th = node.InnerContent.First().InnerContent[i];
                TableNode asc = th.InnerContent.First(ic => ic.Element.TagName == "a");
                TableNode desc = th.InnerContent.Last(ic => ic.Element.TagName == "a");
                string prop = _tableConfig.Columns.ElementAt(i).Key;
                
                expectedQueryPars = pars.Concat(new [] {"asc=True", $"sort={prop}"});
                this.VerifyAjax(asc.Element, expectedQueryPars);
                expectedQueryPars = pars.Concat(new [] {"asc=False", $"sort={prop}"});
                this.VerifyAjax(desc.Element, expectedQueryPars);
            }

            node = this.Node("tfoot");
            TableNode footer = node.InnerContent.First().InnerContent.First();
            IEnumerable<TableNode> navs = footer.InnerContent
                .Where(ic => ic.Element.Attributes["class"].Contains("NavBtnContainer"))
                .SelectMany(nc => nc.InnerContent);
            TableNode nav = navs.Single(ic => ic.Element.Attributes["class"].Contains("NavFirst"));

            pars = genericQueryPars.Concat(new[] {$"sort={_tableState.SortProp}", $"asc={_tableState.AscSort}"});
            expectedQueryPars = pars.Concat(new[] {"page=1"});
            this.VerifyAjax(nav.Element, expectedQueryPars);
            nav = navs.Single(ic => ic.Element.Attributes["class"].Contains("NavPrevious"));
            expectedQueryPars = pars.Concat(new[] {"page=1"});
            this.VerifyAjax(nav.Element, expectedQueryPars);
            nav = navs.Single(ic => ic.Element.Attributes["class"].Contains("NavNext"));
            expectedQueryPars = pars.Concat(new[] {"page=3"});
            this.VerifyAjax(nav.Element, expectedQueryPars);
            nav = navs.Single(ic => ic.Element.Attributes["class"].Contains("NavLast"));
            expectedQueryPars = pars.Concat(new[] {"page=3"});
            this.VerifyAjax(nav.Element, expectedQueryPars);

            TableNode filteringLinkTemplate = containerId != null
                ? _nodes.First().InnerContent.ElementAt(2)
                : _nodes.First().InnerContent.First().InnerContent.ElementAt(2);
            expectedQueryPars = genericQueryPars.Concat(new[]
            {
                $"sort={_tableState.SortProp}", $"asc={_tableState.AscSort}", $"page={_tableState.Page}"
            }).Where(p => !p.StartsWith("filter"));
            this.VerifyUrl(filteringLinkTemplate.Element.Attributes["value"], expectedQueryPars);

            // Page selection.
            TableNode pageSelection = footer.InnerContent
                .Single(ic => ic.Element.Attributes["class"].Contains("NavAccessContainer"));
            for(int i = 0; i < pageSelection.InnerContent.First().InnerContent.Count; i++)
            {
                TableNode pageSel = pageSelection.InnerContent.First().InnerContent[i];

                expectedQueryPars = genericQueryPars.Concat(new[]
                {
                    $"sort={_tableState.SortProp}", $"asc={_tableState.AscSort}", $"page={i + 1}"
                });
                this.VerifyUrl(pageSel.Element.Attributes["value"], expectedQueryPars);
            }
            TableNode pageSelector = pageSelection.InnerContent.Last();
            this.VerifyAjax(pageSelector.Element, null);
        }

        [Fact]
        public void TableStructure()
        {
            _tableConfig.Caption = "Caption";
            _tableConfig.Columns.Add("P", new ColumnConfig {Header = "Header"});
            _tableConfig.Footer.Text = "Text";

            this.ActAndValidate();

            TableNode table = this.Node("table");
            table.InnerContent.Should().HaveCount(4);
            table.InnerContent[0].Element.TagName.Should().Be("caption");
            table.InnerContent[1].Element.TagName.Should().Be("thead");
            table.InnerContent[2].Element.TagName.Should().Be("tbody");
            table.InnerContent[3].Element.TagName.Should().Be("tfoot");
        }

        private TableNode OnlyInner(TableNode node, string element)
        {
            node.InnerContent.Should().HaveCount(1);
            node.InnerContent.First().Element.TagName.Should().Be(element);

            return(node.InnerContent.First());
        }

        private string VerifyAjax(TagBuilder element, IEnumerable<string> queryPars)
        {
            this.VerifyAttribute(element, "data-ajax", "true");
            this.VerifyAttribute(element, "data-ajax-mode", "replace");
            this.VerifyAttribute(element, "data-ajax-update", $"#{_containerId}");
            this.VerifyAttribute(element, "data-ajax-loading", $"#{_tableConfig.Update.BusyIndicatorId}");
            this.VerifyAttribute(element, "data-ajax-begin", $"{_tableConfig.Update.Start}");
            this.VerifyAttribute(element, "data-ajax-success", $"{_tableConfig.Update.Success}");
            this.VerifyAttribute(element, "data-ajax-failure", $"{_tableConfig.Update.Error}");
            this.VerifyAttribute(element, "data-ajax-complete", $"{_tableConfig.Update.Complete}");
            element.Attributes.Should().ContainKey("data-ajax-url");
            if(queryPars != null)
            {
                this.VerifyUrl(element.Attributes["data-ajax-url"], queryPars);
            }
            else
            {
                element.Attributes["data-ajax-url"].Should().BeEmpty();
            }

            return(element.Attributes["data-ajax-url"].Replace(_tableConfig.Update.Url, ""));
        }

        private void VerifyUrl(string url, IEnumerable<string> queryPars)
        {
            url.Should().StartWith($"{_tableConfig.Update.Url}?");
            url.Count(c => c == '?').Should().Be(1);

            url = url.Replace($"{_tableConfig.Update.Url}", "");
            foreach(var queryPar in queryPars)
            {
                if(url.Contains($"?{queryPar}"))
                {
                    url = url.Replace($"?{queryPar}", "");
                }
                else
                {
                    url.Should().Contain($"&{queryPar}");
                    url = url.Replace($"&{queryPar}", "");
                }
            }
            url.Should().BeEmpty();
        }

        private TableNode VerifyHeader(bool expected, bool filterRow = false)
        {
            TableNode headerRow = null;
            TableNode header = this.Node("thead");

            if(expected)
            {
                header.Should().NotBeNull();
                header.InnerContent.Should().HaveCount(filterRow ? 2 : 1);
                headerRow = header.InnerContent.First();
                headerRow.Element.TagName.Should().Be("tr");
                headerRow.InnerContent.Should().HaveCount(3);
                headerRow.InnerContent.Select(ic => ic.Element.TagName).Should().OnlyContain(s => s == "th");
                if(filterRow)
                {
                    headerRow = header.InnerContent.Last();
                    headerRow.Element.TagName.Should().Be("tr");
                    headerRow.InnerContent.Should().HaveCount(3);
                    headerRow.InnerContent.Select(ic => ic.Element.TagName).Should().OnlyContain(s => s == "td");
                }
            }
            else
            {
                header.Should().BeNull();
            }

            return(headerRow);
        }

        private void ActAndValidate(int entityCount = 1)
        {
            TextWriter textWriter = new StringWriter();

            IHtmlContent content = _renderer.Render(_tableConfig, entityCount);

            _nodes.Should().NotBeNull();
            _nodes.Should().HaveCount(1);
            _nodes.ElementAt(0).Element.TagName.Should().Be("div");
            content.WriteTo(textWriter, new HtmlEncoder());
            textWriter.ToString().Should().Be("Content");
            if(_tableState.ContainerId == null)
            {
                Guid id;

                Guid.TryParse(_nodes.ElementAt(0).Element.Attributes["Id"], out id).Should().BeTrue();
                _containerId = id.ToString();
            }
        }

        private void VerifyInnerHtml(TagBuilder element, string content)
        {
            TextWriter textWriter = new StringWriter();

            element.InnerHtml.WriteTo(textWriter, new HtmlEncoder());
            textWriter.ToString().Should().Be(content);
        }

        private TableNode Node(string element)
        {
            return(this.FindElement(_nodes.ElementAt(0), element));
        }

        private TableNode FindElement(TableNode node, string tag)
        {
            TableNode retNode = node.Element.TagName == tag ? node : null;

            if(retNode == null)
            {
                foreach(var tableNode in node.InnerContent)
                {
                    retNode = this.FindElement(tableNode, tag);
                    if(retNode != null)
                    {
                        break;
                    }
                }
            }

            return(retNode);
        }

        private void VerifyAttribute(TagBuilder element, string key, string value = null)
        {
            if(value != null)
            {
                element.Attributes.Should().ContainKey(key);
                if(key == "class")
                {
                    element.Attributes[key].Should().Contain(value);
                }
                else
                {
                    element.Attributes[key].Should().Be(value);
                }
            }
            else
            {
                element.Attributes.Should().NotContainKey(key);
            }
        }

        private void VerifyCssClasses(TagBuilder element, IEnumerable<string> classes, bool shouldExist = true)
        {
            if(classes.Any())
            {
                foreach(var @class in classes)
                {
                    this.VerifyCssClass(element, @class, shouldExist);
                }
            }
            else
            {
                element.Attributes.Should().NotContainKey("class");
            }
        }

        private void VerifyCssClass(TagBuilder element, string value, bool shouldExist = true)
        {
            if(shouldExist)
            {
                element.Attributes.Should().ContainKey("class");
            }

            if(shouldExist)
            {
                element.Attributes["class"].Should().Contain(value);
            }
            else
            {
                if(element.Attributes.ContainsKey("class"))
                {
                    element.Attributes["class"].Should().NotContain(value);
                }
            }
        }
    }
}
