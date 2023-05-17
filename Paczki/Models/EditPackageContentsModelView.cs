using Newtonsoft.Json.Linq;
using System.Data.Entity;

namespace Paczki.Models
{
    public class EditPackageContentsModelView
    {
        public int PackageId { get; init; }
        //public bool IsOpened { get; set; }
        public Package? Package { get; set; }
        public IEnumerable<Delivery>? Query { get; set; }=new List<Delivery>();
        public string NewPackageName { get; set; } = "";
        public string NewPackageCity { get; set; } = "";
        public string? JsonStaticDeliveriesModified { get; set; } = "";
        public string? JsonStaticDeliveriesToDelete { get; set; } = "";
        public string? JsonTempDeliveries { get; set; } = "";
    }
}
    //< input type = "hidden" id = "json-static-deliveries-modified" name = "json-static-deliveries-modified" value = "" />
    //< input type = "hidden" id = "json-temp-deliveries" name = "json-temp-deliveries" value = "" />
    //< input type = "hidden" id = "json-static-deliveries-to-delete" name = "json-static-deliveries-to-delete" value = "" />
    //< input type = "hidden" name = "package-id" value = "@currPackageID" />