using System.Data.Entity;

namespace Paczki.Models
{
    public class EditPackageContentsModelView
    {
        public Package? Package { get; set; }
        public IEnumerable<Delivery>? Query { get; set; }
    }
}
