using System.Collections.Generic;

namespace Solomonic.WebGrid.Models
{
    //row data with column's data
    public class WebRow
    {
        /// <summary>
        /// Key of row and entity
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// row was check
        /// </summary>
        public bool Checked { get; set; }

        /// <summary>
        /// columns names with values
        /// </summary>
        public Dictionary<string, string> ColumnValues { get; set; }

        /// <summary>
        /// 'True' when data in row was changed
        /// </summary>
        public bool Changed { get; set; }

    }
}
