using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.WebPages;

namespace Solomonic.WebGrid.Models
{
    /// <summary>
    /// Model of WebGrid
    /// </summary>
    /// <typeparam name="TData">type of entities</typeparam>
    public class WebGrid<TData>
    {
        /// <summary>
        /// Model of WebGrid
        /// </summary>
        /// <param name="keyValue">Unique Key for entities</param>
        public WebGrid(Func<TData, string> keyValue)
        {
            KeyValue = keyValue;
        }

        /// <summary>
        /// Unique Key for entities
        /// </summary>
        public Func<TData, string> KeyValue { get; set; }

        /// <summary>
        /// Columns of WebGrid
        /// </summary>
        public IList<WebColumn<TData>> Columns { get; set; }

        /// <summary>
        /// Entites for webgrid
        /// </summary>
        public IQueryable<TData> Datas { get; set; }

        /// <summary>
        /// logic for searching current column
        /// </summary>
        public Func<IQueryable<TData>, string, IQueryable<TData>> SearchQuery { get; set; }


        /// <summary>
        /// Add new column to webgrid
        /// </summary>
        /// <param name="column">new column</param>
        /// <returns>current webgrid</returns>
        public WebGrid<TData> AddColumn(WebColumn<TData> column)
        {
            if (Columns == null) Columns = new List<WebColumn<TData>>();
            Columns.Add(column);
            return this;
        }

        /// <summary>
        /// class for Highlighted found strings in columns
        /// </summary>
        public string HighlightClass { get; set; }

        /// <summary>
        /// show save changes button
        /// </summary>
        public bool SaveChangesButton { get; set; }


        public Func<IQueryable<TData>, HelperResult> FooterFormat { get; set; }
        public Func<IQueryable<TData>, HelperResult> InnerFooterFormat { get; set; }
    }
}
