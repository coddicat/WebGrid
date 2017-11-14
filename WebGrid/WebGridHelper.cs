using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using Solomonic.WebGrid.Models;

namespace Solomonic.WebGrid
{
    public static class WebGridHelper
    {
        internal static string GetData<TData>(WebColumn<TData> column, TData data)
        {
            if (column.DataValue != null)
            {
                var value = column.DataValue.Compile()(data);
                var format = CommonHelper.GetDisplayFormat(column.DataValue) ?? "{0}";
                return GetHtmlData(string.Format(column.FormatProvider ?? CultureInfo.InvariantCulture, format, value ?? ""));
            }
            else
            {
                var prop = data.GetType().GetProperty(column.Name);
                if (prop != null)
                {
                    var value = prop.GetValue(data);
                    //var format = Common.GetDisplayFormat(prop) ?? "{0}";
                    return string.Format(column.FormatProvider ?? CultureInfo.InvariantCulture, "{0}",
                        value);
                }
            }

            return null;
        }

        internal static string GetDataText<TData>(WebColumn<TData> column, TData data)
        {
            if (column.DataValue != null)
            {
                var value = column.DataValue.Compile()(data);
                var format = CommonHelper.GetDisplayFormat(column.DataValue) ?? "{0}";
                return GetHtmlData(string.Format(column.FormatProvider ?? CultureInfo.InvariantCulture, format, value ?? ""));
            }
            else
            {
                var prop = data.GetType().GetProperty(column.Name);
                if (prop != null)
                {
                    var format = CommonHelper.GetDisplayFormat(prop) ?? "{0}";
                    var value = prop.GetValue(data);
                    return string.Format(column.FormatProvider ?? CultureInfo.InvariantCulture, format,
                        value.GetDescription() ?? value);
                }
            }


            return null;
        }

        internal static string HighlightSearch(string value, string highlightClass, string search)
        {
            int index = value.IndexOf(search, StringComparison.InvariantCultureIgnoreCase);
            if (index >= 0)
            {
                string inner = value.Substring(index, search.Length);
                return value.Replace(inner, string.Format("<span class='{0}'>{1}</span>", highlightClass, inner));
            }
            return value;
        }

        internal static string GetHtmlData(object data)
        {
            var result = data as HelperResult;
            if (result != null)
            {
                return result.ToHtmlString();
            }
            var s = data as IHtmlString;
            return s != null ? s.ToHtmlString() : HttpUtility.HtmlEncode(data);
        }

        internal static HelperResult RowIdentity<TData>(TData rowData, int rowIndex, Func<TData, object> idFunc)
        {
            var key = new TagBuilder("input");
            key.Attributes.Add("type", "hidden");
            key.Attributes.Add("name", string.Format("Rows[{0}].Key", rowIndex));
            key.Attributes.Add("value", string.Format("{0}", idFunc(rowData)));

            var changed = new TagBuilder("input");
            changed.Attributes.Add("type", "hidden");
            changed.Attributes.Add("name", string.Format("Rows[{0}].Changed", rowIndex));
            changed.Attributes.Add("value", "False");

            //return new MvcHtmlString(hidden.ToString());
            return new HelperResult(a =>
            {
                a.Write(key);
                a.Write(changed);
            });
        }

        /// <summary>
        /// CellFormat for column with checkboxes for select the row
        /// </summary>
        /// <typeparam name="TData">type of entities</typeparam>
        /// <param name="cell">current cell with parameters</param>
        /// <param name="attributes">html attributess for cell</param>
        /// <returns></returns>
        public static HelperResult CellCheckRow<TData>(WebCell<TData> cell, object attributes = null)
        {
            var checkbox = new TagBuilder("input");
            checkbox.MergeAttributes(attributes.ToDictionary());
            checkbox.AddCssClass("solomonic-table-input");
            checkbox.Attributes.Add("type", "checkbox");
            checkbox.Attributes.Add("name", string.Format("Rows[{0}].Checked", cell.RowIndex));
            checkbox.Attributes.Add("value", "true");
            checkbox.Attributes.Add("solomonic-selectRow", "true");

            //return new MvcHtmlString(checkbox.ToString());
            return new HelperResult(a => a.Write(checkbox));
        }

        /// <summary>
        /// CaptionFormat for column with the checkbox 'select all'
        /// </summary>
        /// <param name="attributes">html attributess</param>
        /// <returns></returns>
        public static HelperResult CaptionSelectAll(object attributes = null)
        {
            var checkbox = new TagBuilder("input");
            checkbox.MergeAttributes(attributes.ToDictionary());

            checkbox.Attributes.Add("type", "checkbox");
            checkbox.Attributes.Add("solomonic-selectAll", "true");


            //return new MvcHtmlString(checkbox.ToString());
            return new HelperResult(a => a.Write(checkbox));

        }

        /// <summary>
        /// CellFormat for column with dropdown lists with values
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="cell"></param>
        /// <param name="values"></param>
        /// <param name="selectedFunc">function that returned true for selected item of values</param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public static HelperResult CellDropDown<TData, TKey>(WebCell<TData> cell, IDictionary<TKey, string> values, Func<TData, KeyValuePair<TKey, string>, bool> selectedFunc = null, object attributes = null)
        {
            var dropdown = new TagBuilder("select");
            dropdown.MergeAttributes(attributes.ToDictionary());
            dropdown.AddCssClass("solomonic-table-input");
            dropdown.Attributes.Add("name", string.Format("Rows[{0}].ColumnValues[{1}]", cell.RowIndex, cell.Column.Name));
            foreach (var value in values)
            {
                var option = new TagBuilder("option");
                option.Attributes.Add("value", value.Key.ToString());
                option.InnerHtml += value.Value;

                if (selectedFunc != null)
                {
                    var selected = selectedFunc(cell.Data, value);
                    if (selected)
                    {
                        option.Attributes.Add("selected", "true");
                    }
                }
                else
                {
                    var cellValue = GetData(cell.Column, cell.Data);
                    if (cellValue == value.Key.ToString())
                    {
                        option.Attributes.Add("selected", "true");
                    }
                }

                dropdown.InnerHtml += option;
            }

            return new HelperResult(a => a.Write(dropdown));

            //return new MvcHtmlString(dropdown.ToString());
        }

        /// <summary>
        /// CellFormat for column with CellRadioGroup list with values
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="cell"></param>
        /// <param name="values"></param>
        /// <param name="checkedFunc"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public static HelperResult CellRadioGroup<TData>(WebCell<TData> cell, IDictionary<string, string> values, Func<TData, KeyValuePair<string, string>, bool> checkedFunc = null, object attributes = null)
        {
            var div = new TagBuilder("div");
            div.MergeAttributes(attributes.ToDictionary());
            div.AddCssClass("solomonic-table-input");
            var name = string.Format("Rows[{0}].ColumnValues[{1}]", cell.RowIndex, cell.Column.Name);
            foreach (var value in values)
            {
                var label = new TagBuilder("label");
                var radio = new TagBuilder("input");
                radio.Attributes.Add("type", "radio");
                radio.Attributes.Add("name", name);
                radio.Attributes.Add("value", value.Key);

                if (checkedFunc != null)
                {
                    var selected = checkedFunc(cell.Data, value);
                    if (selected)
                    {
                        radio.Attributes.Add("checked", "true");
                    }
                }
                else
                {
                    var cellValue = GetData(cell.Column, cell.Data);
                    if (cellValue == value.Key)
                    {
                        radio.Attributes.Add("checked", "true");
                    }
                }

                label.InnerHtml += radio;
                label.InnerHtml += value.Value;

                div.InnerHtml += label;
            }


            return new HelperResult(a => a.Write(div));

            //return new MvcHtmlString(div.ToString());
        }

        /// <summary>
        /// CellFormat for column with textbox
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="cell"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public static HelperResult CellTextBox<TData>(WebCell<TData> cell, object attributes = null)
        {
            var textbox = new TagBuilder("input");
            textbox.MergeAttributes(attributes.ToDictionary());
            textbox.AddCssClass("solomonic-table-input");
            if (!textbox.Attributes.ContainsKey("type"))
                textbox.Attributes.Add("type", "text");
            textbox.Attributes.Add("name", string.Format("Rows[{0}].ColumnValues[{1}]", cell.RowIndex, cell.Column.Name));


            textbox.Attributes.Add("value", GetDataText(cell.Column, cell.Data));
            return new HelperResult(a => a.Write(textbox));
            //return new MvcHtmlString(textbox.ToString());
        }

        /// <summary>
        /// CellFormat for column with textarea data
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="cell"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public static HelperResult CellTextArea<TData>(WebCell<TData> cell, object attributes = null)
        {
            var textarea = new TagBuilder("textarea");
            textarea.MergeAttributes(attributes.ToDictionary());
            textarea.AddCssClass("solomonic-table-input");
            textarea.Attributes.Add("name", string.Format("Rows[{0}].ColumnValues[{1}]", cell.RowIndex, cell.Column.Name));


            textarea.InnerHtml = GetDataText(cell.Column, cell.Data);

            return new HelperResult(a => a.Write(textarea));
            // return new MvcHtmlString(textarea.ToString());
        }

        /// <summary>
        /// FilterFormat for column with filter like dropdown
        /// </summary>
        /// <typeparam name="TData">type of entity</typeparam>
        /// <param name="webColumn">current column</param>
        /// <param name="webSettings">current settings</param>
        /// <param name="values">values for dropdown</param>
        /// <param name="attributes">html attributes</param>
        /// <returns></returns>
        public static HelperResult FilterDropDown<TData, TKey>(WebColumn<TData> webColumn, WebGridSettings webSettings, IDictionary<TKey, string> values,
            object attributes = null)
        {
            var dropdown = new TagBuilder("select");
            dropdown.MergeAttributes(attributes.ToDictionary());
            dropdown.Attributes.Add("name", string.Format("WebGridSettings.Filters[{0}].FilterString", webColumn.Name));
            dropdown.Attributes.Add("solomonic-dropdownfilter", "");
            var filterValues = (values?.ToDictionary(x => x.Key.ToString(), x => x.Value) ?? new Dictionary<string, string>()).ToList();
            filterValues.Insert(0, new KeyValuePair<string, string>("", WebGridWords.NoFilter));

            foreach (var value in filterValues.ToDictionary(a => a.Key, a => a.Value))
            {
                var option = new TagBuilder("option");
                option.Attributes.Add("value", value.Key.ToString());
                option.InnerHtml += value.Value;

                if (webSettings != null && webSettings.Filters != null &&
                    webSettings.Filters.ContainsKey(webColumn.Name) && webSettings.Filters[webColumn.Name].FilterString == value.Key.ToString())
                {
                    option.Attributes.Add("selected", "true");
                }

                dropdown.InnerHtml += option;
            }

            return new HelperResult(a => a.Write(dropdown));
            //return new MvcHtmlString(dropdown.ToString());
        }

        public static HelperResult FilterDropDown<TData, TValues>(WebColumn<TData> webColumn, WebGridSettings webSettings, object attributes = null) where TValues : struct, IConvertible
        {
            if (!typeof(TValues).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }
            var values = Enum.GetValues(typeof (TValues))
                .Cast<TValues>()
                .ToDictionary(v => (object)(v.ToString()), v => v.GetDescription() ?? v.ToString());
            return FilterDropDown(webColumn, webSettings, values, attributes);
        }

        /// <summary>
        /// FilterFormat for column with filter like textbox
        /// </summary>
        /// <typeparam name="TData">type of entity</typeparam>
        /// <param name="webColumn">current column</param>
        /// <param name="webSettings">current settings</param>
        /// <param name="attributes">html attributes</param>
        /// <returns></returns>
        public static HelperResult FilterTextBox<TData>(WebColumn<TData> webColumn, WebGridSettings webSettings, object attributes = null)
        {
            var textbox = new TagBuilder("input");
            textbox.MergeAttributes(attributes.ToDictionary());
            textbox.Attributes.Add("name", string.Format("WebGridSettings.Filters[{0}].FilterString", webColumn.Name));
            textbox.Attributes.Add("type", "text");

            if (webSettings != null && webSettings.Filters != null &&
                    webSettings.Filters.ContainsKey(webColumn.Name))
            {
                textbox.Attributes.Add("value", webSettings.Filters[webColumn.Name].FilterString);
            }

            return new HelperResult(a => a.Write(textbox));
            //new MvcHtmlString(textbox.ToString());
        }


        /// <summary>
        /// Column for selecting rows
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static WebColumn<T> SelectRowColumn<T>(string name = "SelectRowColumn")
        {
            return new WebColumn<T>(name)
            {
                CaptionFormat = CaptionSelectAll(),
                CellFormat = cell => CellCheckRow(cell),
                ColumnWidth = "30px"
            };
        }

        public static string GetHighlightString<T>(string str, WebColumn<T> column, WebGridSettings webSettings, string highlightClass = "bg-info")
        {
            highlightClass = string.IsNullOrWhiteSpace(highlightClass) ? "bg-info" : highlightClass;

            return column.HighlightSearch && webSettings != null &&
                   !string.IsNullOrWhiteSpace(webSettings.SearchString)
            ? HighlightSearch(str, highlightClass, webSettings.SearchString)
            : str;
        }
    }
}
