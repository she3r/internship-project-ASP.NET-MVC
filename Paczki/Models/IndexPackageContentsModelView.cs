namespace Paczki.Models
{
    public class IndexPackageContentsModelView
    {
        public List<Package> Query { get; set; }
        public bool ShowOpen { get; set; } = true;
        public bool ShowClosed { get; set; } = true;
        public int PageChoice { get; set; } = 1;
        public int NumOfAllPackages { get; set; }
        public int NumPackagesOnPage { get;set; }

        //public string? NewPackagesJSON { get; set; } = null;
        //public string? ToDeleteQueryPackageIDsJSON { get; set; } = null;
        //public string? ToUpdateQueryPackagesJSON { get; set; } = null;
    }

}
