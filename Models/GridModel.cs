using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic;

namespace Solomonic.WebGrid.Models
{
    /// <summary>
    /// model that is sent between the controller and view
    /// </summary>
    /// <typeparam name="TData">type of entities</typeparam>
    public class GridModel<TData>
    {
        public GridModel()
        {
            WebGridSettings = new WebGridSettings();
        }

        /// <summary>
        /// Model of WebGrid
        /// </summary>
        public WebGrid<TData> Grid { get; set; }

        /// <summary>
        /// Model of rows with column's data that send from view to controller
        /// </summary>
        public WebRow[] Rows { get; set; }

        /// <summary>
        /// The table gridSettings (filtering, srorting, paging, seraching)
        /// </summary>
        public WebGridSettings WebGridSettings { get; set; }

        /// <summary>
        /// Apply gridSettings for Table.Data: filtering, srorting, paging, seraching;
        /// And Convert IQueryable Table.Data to List
        /// </summary>
        public void ApplySettings()
        {
            var settings = WebGridSettings;
            if (!string.IsNullOrWhiteSpace(settings.CacheName))
            {
                System.Web.HttpContext.Current.Session[settings.CacheName] = settings;
            }
            if (Grid.Datas != null)
            {
                var linq = Grid.Datas;//.ToList().AsQueryable();
                linq = ApplyFilters(linq, settings);

                settings.TotalCount = linq.Count();
                settings.WebGridPagerSettings.CalcPages(settings.TotalCount);

                if (settings.TotalCount > 0)
                {
                    linq = ApplySorting(linq, settings);
                    linq = ApplyPaging(linq, settings);
                }
                else
                {
                    linq = null;
                }
                Grid.Datas = linq;
            }
        }

        public bool SaveChanges { get; set; }

        private IQueryable<TData> ApplySorting(IQueryable<TData> linq, WebGridSettings gridSettings)
        {
            if (gridSettings.WebSort != null && !string.IsNullOrWhiteSpace(gridSettings.WebSort.ColumnName))
            {
                var column = Grid.Columns.FirstOrDefault(a => a.Name == gridSettings.WebSort.ColumnName);
                if (column != null && column.SortQuery != null)
                {
                    return column.SortQuery(linq, gridSettings.WebSort.Descending);
                }
            }

            if (gridSettings.WebSort == null)
            {
                gridSettings.WebSort = new WebSort();
            }

            var propertyInfos = typeof(TData).GetProperties();
            if (propertyInfos.All(p => p.Name != gridSettings.WebSort.ColumnName))
            {
                var key = propertyInfos.FirstOrDefault(p => Attribute.IsDefined(p, typeof(KeyAttribute)));
                gridSettings.WebSort.ColumnName = (key ?? propertyInfos[0]).Name;
            }

            return
                linq.OrderBy(string.Format("it.{0} {1}", gridSettings.WebSort.ColumnName,
                    gridSettings.WebSort.Descending ? "Desc" : "Asc"));
        }

        private IQueryable<TData> ApplyPaging(IQueryable<TData> linq, WebGridSettings gridSettings)
        {
            var skip = (gridSettings.WebGridPagerSettings.CurrentPage - 1) * gridSettings.WebGridPagerSettings.ItemsPerPage;
            var take = gridSettings.WebGridPagerSettings.ItemsPerPage;

            return
                linq.Skip(skip)
                    .Take(take);
        }

        private IQueryable<TData> ApplyFilters(IQueryable<TData> linq, WebGridSettings gridSettings)
        {
            foreach (var filter in gridSettings.Filters.Where(a => !string.IsNullOrWhiteSpace(a.Value.FilterString)))
            {
                var column = Grid.Columns.FirstOrDefault(a => a.Name == filter.Key);
                if (column != null && column.FilterQuery != null)
                {
                    linq = column.FilterQuery(linq, filter.Value.FilterString.Trim());
                }
            }

            if (!string.IsNullOrWhiteSpace(gridSettings.SearchString))
            {
                /*linq = Table.Columns.Where(column => column.SearchQuery != null)
                    .Aggregate(linq, (current, column) => column.SearchQuery(current, gridSettings.SearchString));*/
                linq = Grid.SearchQuery != null ? Grid.SearchQuery(linq, gridSettings.SearchString.Trim()) : linq;
            }
            return linq;
        }

    }    
}
