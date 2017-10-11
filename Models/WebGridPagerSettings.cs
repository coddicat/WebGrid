namespace Solomonic.WebGrid.Models
{
    public class WebGridPagerSettings
    {
        public WebGridPagerSettings()
        {
            PagesCount = 1;
            CurrentPage = 1;
            ItemsPerPage = 10;
        }
        public int PagesCount { get; set; }
        public int CurrentPage { get; set; }
        public int ItemsPerPage { get; set; }

        public void CalcPages(int totalCount)
        {
            if (ItemsPerPage <= 0) ItemsPerPage = 10;
            PagesCount = totalCount / ItemsPerPage;
            if (totalCount % ItemsPerPage > 0)
                PagesCount++;
            Fix();
        }

        public void Fix()
        {
            if (PagesCount < 0)
                PagesCount = 0;
            if (PagesCount > 0)
            {
                if (CurrentPage < 1)
                    CurrentPage = 1;
                if (CurrentPage > PagesCount)
                    CurrentPage = PagesCount;
            }
            else
            {
                CurrentPage = 0;
            }

            if (ItemsPerPage <= 0)
                ItemsPerPage = 10;
        }
    }
}
