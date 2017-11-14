using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Mvc.Html;

using Solomonic.WebGrid.Models;

namespace Solomonic.WebGrid
{
    public static class WebGrid
    {
        /// <summary>
        /// Display WebGrid to HTML
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <param name="helper"></param>
        /// <param name="webGrid"></param>
        /// <param name="webSettings"></param>
        /// <param name="autoRefresh"></param>
        /// <param name="htmlAttributes"></param>
        /// <param name="innerHtmlAttributes"></param>
        /// <returns></returns>
        public static MvcHtmlString For<TModel, TData>(this HtmlHelper<TModel> helper,
            Expression<Func<TModel, WebGrid<TData>>> webGrid, Expression<Func<TModel, WebGridSettings>> webSettings, bool autoRefresh, object htmlAttributes = null, object innerHtmlAttributes = null)
        {
            var webgrid = webGrid.Compile()(helper.ViewData.Model);
            var settings = webSettings.Compile()(helper.ViewData.Model);

            var table = new TagBuilder("table");
            htmlAttributes = htmlAttributes ?? new {@class = "table table-bordered"};
            innerHtmlAttributes = innerHtmlAttributes ?? new { @class = "table table-bordered" };
            table.MergeAttributes(htmlAttributes.ToDictionary());
            table.AddCssClass("solomonic-table");
            table.Attributes.Add("solomonic-autorefresh", string.Format("{0}", autoRefresh));

            table.InnerHtml += Caption(settings, webgrid.SearchQuery != null, webgrid.SaveChangesButton);
            table.InnerHtml += Thead(webgrid, innerHtmlAttributes, settings);
            table.InnerHtml += Tbody(webgrid, innerHtmlAttributes, settings);
            table.InnerHtml += Tfoot(webgrid, settings);

            return new MvcHtmlString(table.ToString());
        }

        public static MvcHtmlString AjaxGrid<TEntity>(this WebViewPage webViewPage, string formId, string actionName, string viewName = null, string controllerName = null, object routeValues = null, object htmlAttributes = null, string onRender = null)
        {
            var divId = Guid.NewGuid().ToString("N");

            var form = new TagBuilder("form");
            form.MergeAttributes(htmlAttributes.ToDictionary());
            form.Attributes.Add("action", webViewPage.Url.Action(actionName, controllerName, routeValues));
            form.Attributes.Add("data-ajax", "true");
            form.Attributes.Add("data-ajax-begin", "$solomonic.webGrid.ajaxOnBegin(this)");
            form.Attributes.Add("data-ajax-failure", "$solomonic.webGrid.ajaxOnFailure(this, xhr, status, error)");
            form.Attributes.Add("data-ajax-method", "POST");
            form.Attributes.Add("data-ajax-mode", "replace");
            form.Attributes.Add("data-ajax-success", "$solomonic.webGrid.Refresh(this)" + (!string.IsNullOrWhiteSpace(onRender) ? ";" + onRender : ""));
            form.Attributes.Add("data-ajax-update", "#" + divId);
            form.Attributes.Add("method", "post");
            form.Attributes.Add("id", formId);
            var div = new TagBuilder("div");
            div.Attributes.Add("id", divId);
            string cacheName = $"WebGridSettingsCaching/{formId}/{actionName}/{controllerName}";
            WebGridSettings fromCache = System.Web.HttpContext.Current.Session[cacheName] as WebGridSettings;
            var gridModel = fromCache != null 
                ? new GridModel<TEntity>
                {
                    WebGridSettings = fromCache,
                } 
                : new GridModel<TEntity>();
            gridModel.WebGridSettings.CacheName = cacheName;
            div.InnerHtml += webViewPage.Html.Partial(viewName ?? "_" + actionName, gridModel);
            form.InnerHtml += div;

            return new MvcHtmlString(form.ToString());

        }


        private static MvcHtmlString Tfoot<TData>(WebGrid<TData> webGrid, WebGridSettings webGridSettings)
        {
            var columns = webGrid.Columns.Count;
            var tfoot = new TagBuilder("tfoot");

            if (webGrid.FooterFormat != null) //----- Custom footer format
            {
                tfoot.InnerHtml += webGrid.FooterFormat(webGrid.Datas);
            }

            var tr = new TagBuilder("tr");
            var td = new TagBuilder("td");
            //td.Attributes.Add("colspan", string.Format("{0}", columns));

            td.InnerHtml += string.Format("<input type='hidden' name='WebGridSettings.WebGridPagerSettings.CurrentPage' value='{0}' />", webGridSettings.WebGridPagerSettings.CurrentPage);
            td.InnerHtml += string.Format("<input type='hidden' name='WebGridSettings.CacheName' value='{0}' />", webGridSettings.CacheName);

            if (webGridSettings.TotalCount > webGridSettings.WebGridPagerSettings.ItemsPerPage)
            {
                var from = (webGridSettings.WebGridPagerSettings.CurrentPage - 1) * webGridSettings.WebGridPagerSettings.ItemsPerPage + 1;
                var to = (webGridSettings.WebGridPagerSettings.CurrentPage) * webGridSettings.WebGridPagerSettings.ItemsPerPage;
                if (to > webGridSettings.TotalCount) to = webGridSettings.TotalCount;
                td.InnerHtml +=
                    string.Format(
                        string.Format("<div class='solomonic-showcount {1}'>{0}</div>", WebGridWords.ShowingOfEntries, WebGridClasses.ShowCount),
                        from, to, webGridSettings.TotalCount);
            }
            else if (webGridSettings.TotalCount > 0)
            {
                td.InnerHtml +=
                    string.Format(
                        string.Format("<div class='solomonic-showcount {1}'>{0}</div>", WebGridWords.ShowingEntries, WebGridClasses.ShowCount),
                        webGridSettings.TotalCount);
            }
            else
            {
                td.InnerHtml += string.Format("<div class='solomonic-showcount {1}'>{0}</div>", WebGridWords.NoEntries, WebGridClasses.ShowCount);
            }


            td.InnerHtml += Pager(webGridSettings.WebGridPagerSettings);

            tr.InnerHtml += td;
            tfoot.InnerHtml += tr;
            return new MvcHtmlString(tfoot.ToString());
        }

        private static MvcHtmlString Caption(WebGridSettings webGridSettings, bool search, bool saving)
        {
            var caption = new TagBuilder("Caption");

            var div = new TagBuilder("div");
            div.AddCssClass("solomonic-table-caption-buttons");
            if (saving)
            {
                div.InnerHtml +=
                    string.Format("<button title='{0}' class='solomonic-save-changes {1}' disabled='disabled'><span class='glyphicon glyphicon-saved'></span></button>", WebGridWords.SaveChanges, WebGridClasses.SaveChanges);
            }
            div.InnerHtml +=
                string.Format("<button title='{0}' class='solomonic-refresh {1}'><span class='glyphicon glyphicon-refresh'></span></button>", WebGridWords.Refresh, WebGridClasses.Refresh);
            caption.InnerHtml += div;

            caption.InnerHtml +=
                ItemsPerPage(webGridSettings.WebGridPagerSettings);

            if (search)
            {
                var divSearch = new TagBuilder("div");
                divSearch.AddCssClass("solomonic-table-search");
                var input = new TagBuilder("input");
                input.Attributes.Add("name", "WebGridSettings.SearchString");
                input.Attributes.Add("value", webGridSettings.SearchString);
                input.Attributes.Add("type", "text");
                input.AddCssClass(WebGridClasses.Search);
                divSearch.InnerHtml += string.Format("{0}:", WebGridWords.Search);
                divSearch.InnerHtml += input;
                divSearch.InnerHtml +=
                    "<a href='#' solomonic-searchrow=''><span class='glyphicon glyphicon-search'></span></a>";
                caption.InnerHtml += divSearch;
            }
            return new MvcHtmlString(caption.ToString());
        }

        private static MvcHtmlString TheadFilter<TData>(WebGrid<TData> webGrid)
        {
            var tr = new TagBuilder("tr");

            foreach (var webColumn in webGrid.Columns)
            {
                var th = new TagBuilder("th");
                if (webColumn.FilterFormat != null)
                {
                    var hidden = new TagBuilder("input");
                    hidden.Attributes.Add("type", "hidden");
                    hidden.Attributes.Add("name", string.Format("WebGridSettings.Filters[{0}].Type", webColumn.Name));
                    th.InnerHtml += hidden;

                    th.InnerHtml += webColumn.FilterFormat(webColumn);
                }

                tr.InnerHtml += th;
            }

            return new MvcHtmlString(tr.ToString());
        }

        private static MvcHtmlString Thead<TData>(WebGrid<TData> webGrid, object htmlAttributes, WebGridSettings webGridSettings = null)
        {
            var thead = new TagBuilder("thead");
            if (webGridSettings != null)
            {
                var hidden = new TagBuilder("input");
                hidden.Attributes.Add("type", "hidden");
                hidden.Attributes.Add("name", "WebGridSettings.WebSort.ColumnName");
                hidden.Attributes.Add("value", webGridSettings.WebSort.ColumnName);

                var hidden2 = new TagBuilder("input");
                hidden2.Attributes.Add("type", "hidden");
                hidden2.Attributes.Add("name", "WebGridSettings.WebSort.Descending");
                hidden2.Attributes.Add("value", string.Format("{0}", webGridSettings.WebSort.Descending));

                var hidden3 = "<input type='hidden' name='WebGridSettings.IsChanged' value='False' />";
                var hidden4 = "<input type='hidden' name='SaveChanges' value='False' />";

                thead.InnerHtml += hidden;
                thead.InnerHtml += hidden2;
                thead.InnerHtml += hidden3;
                thead.InnerHtml += hidden4;
            }

            var tr = new TagBuilder("tr");
            var th = new TagBuilder("th");
            //th.Attributes.Add("colspan",webTable.Columns.Count.ToString(CultureInfo.InvariantCulture));
            th.AddCssClass("solomonic-main-head-th");
            var headTable = new TagBuilder("table");
            headTable.MergeAttributes(htmlAttributes.ToDictionary());
            headTable.AddCssClass("solomonic-inner-head-table");
            headTable.InnerHtml += TheadInner(webGrid, webGridSettings);
            th.InnerHtml += headTable;
            tr.InnerHtml += th;
            thead.InnerHtml += tr;
            return new MvcHtmlString(thead.ToString());
        }

        private static MvcHtmlString TheadInner<TData>(WebGrid<TData> webGrid, WebGridSettings webGridSettings = null)
        {
            var thead = new TagBuilder("thead");
            var tr = new TagBuilder("tr");

            foreach (var webColumn in webGrid.Columns)
            {
                var th = new TagBuilder("th");
                th.MergeAttributes(webColumn.HtmlAttributes.ToDictionary());
                if (!string.IsNullOrWhiteSpace(webColumn.ColumnWidth))
                    th.Attributes.Add("width", webColumn.ColumnWidth);

                MvcHtmlString caption;

                if (webColumn.CaptionFormat != null)
                {
                    caption = new MvcHtmlString(webColumn.CaptionFormat.ToHtmlString());
                }
                else if (webColumn.Caption != null)
                {
                    caption = new MvcHtmlString(webColumn.Caption);
                }
                else
                {
                    caption = new MvcHtmlString(CommonHelper.GetDisplayName<TData>(webColumn.Name) ?? webColumn.Name);
                }

                if (webColumn.Sortable)
                {
                    var anchor = new TagBuilder("a");
                    anchor.Attributes.Add("href", "#");
                    anchor.Attributes.Add("solomonic-sortby", webColumn.Name);
                    anchor.InnerHtml += caption;
                    if (webGridSettings != null && webGridSettings.WebSort != null)
                    {
                        if (webGridSettings.WebSort.ColumnName == webColumn.Name)
                        {
                            if (webGridSettings.WebSort.Descending)
                            {
                                anchor.InnerHtml += "<span class='glyphicon glyphicon-sort-by-attributes-alt'></span>";
                            }
                            else
                            {
                                anchor.Attributes.Add("solomonic-sort-descending", "true");
                                anchor.InnerHtml += "<span class='glyphicon glyphicon-sort-by-attributes'></span>";
                            }
                        }
                    }
                    th.InnerHtml += anchor;
                }
                else
                {
                    th.InnerHtml += caption;
                }

                tr.InnerHtml += th;
            }

            thead.InnerHtml += tr;
            if (webGrid.Columns.Any(a => a.FilterFormat != null))
            {
                thead.InnerHtml += TheadFilter(webGrid);
            }

            return new MvcHtmlString(thead.ToString());
        }

        private static MvcHtmlString Tbody<TData>(WebGrid<TData> webGrid, object htmlAttributes, WebGridSettings webGridSettings = null)
        {
            int rowIndex = 0;
            var tbody = new TagBuilder("tbody");
            var maintr = new TagBuilder("tr");
            var maintd = new TagBuilder("td"); maintd.AddCssClass("solomonic-table-main-td");
            var div = new TagBuilder("div"); div.AddCssClass("solomonic-table-main-div");
            //maintd.Attributes.Add("colspan", webTable.Columns.Count.ToString(CultureInfo.InvariantCulture));
            var innerTable = new TagBuilder("table");
            innerTable.MergeAttributes(htmlAttributes.ToDictionary());
            innerTable.AddCssClass("solomonic-inner-table");

            var empty = true;
            if (webGrid.Datas != null)
            {
                var list = webGrid.Datas.ToList();
                foreach (var rowData in list)
                {
                    empty = false;
                    var tr = new TagBuilder("tr");
                    tr.AddCssClass("dataRow");
                    tr.InnerHtml += WebGridHelper.RowIdentity(rowData, rowIndex, webGrid.KeyValue);
                    int colIndex = 0;
                    foreach (var webColumn in webGrid.Columns)
                    {
                        var td = new TagBuilder("td");
                        td.MergeAttributes(webColumn.HtmlAttributes.ToDictionary());
                        if (rowIndex == 0 && !string.IsNullOrWhiteSpace(webColumn.ColumnWidth))
                            td.Attributes.Add("width", webColumn.ColumnWidth);
                        if (webColumn.CellFormat != null)
                        {
                            td.InnerHtml += webColumn.CellFormat(new WebCell<TData>(webColumn, rowIndex, colIndex, rowData));
                        }
                        else
                        {
                            var data = WebGridHelper.GetDataText(webColumn, rowData);
                            td.InnerHtml += WebGridHelper.GetHighlightString(data, webColumn, webGridSettings, webGrid.HighlightClass);
                        }

                        tr.InnerHtml += td;
                        colIndex++;
                    }
                    rowIndex++;
                    innerTable.InnerHtml += tr;
                }
            }

            if (empty)
            {
                var tr = new TagBuilder("tr");
                tr.AddCssClass("solomonic-empty-row");
                var td = new TagBuilder("td");
                td.Attributes.Add("colspan", webGrid.Columns.Count.ToString(CultureInfo.InvariantCulture));
                tr.InnerHtml += td;
                innerTable.InnerHtml += tr;
            }

            if (webGrid.InnerFooterFormat != null) //----- Custom footer format
            {
                var tfoot = new TagBuilder("tfoot");
                tfoot.InnerHtml += webGrid.InnerFooterFormat(webGrid.Datas);
                innerTable.InnerHtml += tfoot;
            }


            div.InnerHtml += innerTable;
            maintd.InnerHtml += div;
            maintr.InnerHtml += maintd;
            tbody.InnerHtml += maintr;

            return new MvcHtmlString(tbody.ToString());
        }

        private static MvcHtmlString Pager(WebGridPagerSettings settings, object htmlAttributes = null)
        {
            var pagination = new TagBuilder("nav");
            pagination.MergeAttributes(htmlAttributes.ToDictionary());
            pagination.AddCssClass("solomonic-pager");
            pagination.AddCssClass(WebGridClasses.Pager);

            settings.Fix();

            #region --- calc min and max ----



            var min = settings.CurrentPage - (WebGridParams.PaginationCount) / 2;
            var max = settings.CurrentPage + (WebGridParams.PaginationCount - 1 )/ 2;
            if (min < 1)
            {
                min = 1;
                max = min + (WebGridParams.PaginationCount - 1);
            }
            if (max > settings.PagesCount)
            {
                max = settings.PagesCount;
                min = max - (WebGridParams.PaginationCount - 1);
                if (min < 1) min = 1;
            }


            #endregion

            #region --- render ---


            var ul = new TagBuilder("ul");
            ul.AddCssClass("pagination");
            #region --- to first page -----

            if (true)
            {
                var toFirst = new TagBuilder("li");
                var anchor = new TagBuilder("a");
                anchor.Attributes.Add("href", "#");
                if (settings.CurrentPage <= 1)
                {
                    toFirst.AddCssClass("disabled");
                }
                else
                {
                    anchor.Attributes.Add("solomonic-topage", "1");
                }                
                anchor.InnerHtml += WebGridWords.FirstPage;
                toFirst.InnerHtml += anchor;

                ul.InnerHtml += toFirst;
            }

            #endregion
            #region --- pages -----

            for (var i = min; i <= max; i++)
            {
                var page = new TagBuilder("li");
                var anchor = new TagBuilder("a");
                anchor.Attributes.Add("href", "#");
                if (i == settings.CurrentPage)
                {
                    page.AddCssClass("active");
                    page.AddCssClass("disable");
                }
                else
                {
                    anchor.Attributes.Add("solomonic-topage", string.Format("{0}", i));
                }
                anchor.InnerHtml += i;
                page.InnerHtml += anchor;
                ul.InnerHtml += page;
            }

            #endregion
            #region --- to last page -----

            if (true)
            {
                var toLast = new TagBuilder("li");
                var anchor = new TagBuilder("a");
                anchor.Attributes.Add("href", "#");
                if (settings.CurrentPage == settings.PagesCount)
                {
                    toLast.AddCssClass("disabled");
                }
                else
                {
                    anchor.Attributes.Add("solomonic-topage", settings.PagesCount.ToString());
                }
                anchor.InnerHtml += WebGridWords.LastPage;
                toLast.InnerHtml += anchor;

                ul.InnerHtml += toLast;
            }

            #endregion

            pagination.InnerHtml += ul;            
            #endregion
          
            return new MvcHtmlString(pagination.ToString());
        }

        private static MvcHtmlString ItemsPerPage(WebGridPagerSettings settings, object htmlAttributes = null)
        {
            settings.Fix();

            var div = new TagBuilder("div");
            div.MergeAttributes(htmlAttributes.ToDictionary());
            div.AddCssClass("solomonic-itemsperpage");
            div.InnerHtml += WebGridWords.EntriesPerPage;

            var dropdown = new TagBuilder("select");
            dropdown.Attributes.Add("name", "WebGridSettings.WebGridPagerSettings.ItemsPerPage");
            dropdown.Attributes.Add("solomonic-itemsperpage", "");
            dropdown.AddCssClass(WebGridClasses.EntriesPerPage);
            foreach (var value in WebGridParams.ItemsPerPage)
            {
                var option = new TagBuilder("option");
                option.Attributes.Add("value", string.Format("{0}", value));
                option.InnerHtml += value;
                if (value == settings.ItemsPerPage)
                    option.Attributes.Add("selected", "true");
                dropdown.InnerHtml += option;
            }

            div.InnerHtml += dropdown;

            return new MvcHtmlString(div.ToString());
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public static MvcHtmlString StyleSheet()
        {
            return new MvcHtmlString(GetResource(Assembly.GetExecutingAssembly(), "Solomonic.WebGrid", "solomonic-webgrid.css"));
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public static MvcHtmlString JavaScripts()
        {
            return new MvcHtmlString(GetResource(Assembly.GetExecutingAssembly(), "Solomonic.WebGrid", "solomonic-webgrid.js"));
        }

        private static string GetResource(Assembly assembly, string resourceAssembly, string fileName)
        {
            var resourceName = string.Format("{0}.{1}", resourceAssembly, fileName);

            using (var stream = assembly.GetManifestResourceStream(resourceName))
                if (stream != null)
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
            return null;
        }

    }
}
