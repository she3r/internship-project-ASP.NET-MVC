namespace Paczki.Models
{
    public class IndexPackageContentsModelView
    {
        public IEnumerable<Package> Query { get; set; }
        public bool ShowOpen { get; set; } = true;
        public bool ShowClosed { get; set; } = true;
        public int PageChoice { get; set; } = 1;
        public int NumOfAllPackages { get; set; }
        public int NumPackagesOnPage { get;set; }

        public string? NewPackageName { get; set; } = null;
        public string? NewPackageCity { get; set; } = null;
    }

}
