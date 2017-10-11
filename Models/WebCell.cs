namespace Solomonic.WebGrid.Models
{
    public class WebCell<TData>
    {
        public WebCell(WebColumn<TData> column, int row, int col, TData data)
        {
            Column = column;
            RowIndex = row;
            ColIndex = col;
            Data = data;
        }

        public int RowIndex { get; private set; }
        public int ColIndex { get; private set; }
        public WebColumn<TData> Column { get; private set; }
        public TData Data { get; private set; }
    }
}
