using System.Collections.Generic;
using Solomonic.WebGrid.Models;

namespace Solomonic.WebGrid
{
    public class WebGridSettings
    {
        public WebGridSettings()
        {
            WebGridPagerSettings = new WebGridPagerSettings();
            WebSort = new WebSort();
            Filters = new Dictionary<string, WebColumnFilter>();
        }
        public WebSort WebSort { get; set; }
        public Dictionary<string, WebColumnFilter> Filters { get; set; }
        public string SearchString { get; set; }
        public WebGridPagerSettings WebGridPagerSettings { get; set; }
        public int TotalCount { get; set; }
        public bool IsChanged { get; set; }
        public string CacheName { get; set; }
    }
}
