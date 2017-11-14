using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web.WebPages;

namespace Solomonic.WebGrid.Models
{
    public class WebColumn<TData>
    {
        /// <summary>
        /// WebColumn for WebGrids
        /// </summary>
        /// <param name="name">Name of column for identity</param>
        public WebColumn(string name)
        {
            Name = name;
        }
        public WebColumn(Expression<Func<TData, dynamic>> dataValue)
        {
            DataValue = dataValue;
            Name = CommonHelper.GetMemberName(dataValue);
        }

        /// <summary>
        /// Name of column for identity
        /// </summary>
        public string Name { get; private set; }

        /* -------- Column Caption ----------*/
        /// <summary>
        /// Caption of column
        /// </summary>
        public string Caption { get; set; }
        /// <summary>
        /// Custom caption format for column
        /// </summary>
        public HelperResult CaptionFormat { get; set; }

        /* -------- Data Cell -----------------*/
        /// <summary>
        /// Custom Data Value for entities for current column
        /// </summary>
        public Expression<Func<TData, dynamic>> DataValue { get; set; }
        /// <summary>
        /// Custom cell format for entities for current column
        /// </summary>
        public Func<WebCell<TData>, HelperResult> CellFormat { get; set; }

        /* --------- Filter & Search ------------------*/
        /// <summary>
        /// logic for filtering current column
        /// </summary>
        public Func<IQueryable<TData>, string, IQueryable<TData>> FilterQuery { get; set; }
        /// <summary>
        /// filterformat for current column
        /// </summary>
        public Func<WebColumn<TData>, HelperResult> FilterFormat { get; set; }
        /// <summary>
        /// Highlight Found string
        /// </summary>
        public bool HighlightSearch { get; set; }
        /* -----------------------------------*/

        /// <summary>
        /// Custom Sort Logic for entities for current column
        /// </summary>
        public Func<IQueryable<TData>, bool, IQueryable<TData>> SortQuery { get; set; }
        public bool Sortable { get; set; }

        public IFormatProvider FormatProvider { get; set; }
        public string ColumnWidth { get; set; }
        public object HtmlAttributes { get; set; }
    }
}
