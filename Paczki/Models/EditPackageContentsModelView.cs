using Newtonsoft.Json.Linq;
using Paczki.Dto;
using System.Data.Entity;

namespace Paczki.Models
{
    public class EditPackageContentsModelView
    {
        public Package? Package { get; set; }
        public List<DeliveryDtoWithId> Query { get; set; }   // on modify IsModified = true; on delete just remove, we will update all deliveries for given package
        public bool IsPackageModified { get; set; } = false;
        public bool AreDeliveriesDeleted { get; set; } = false;
        public int SourceIndexPageNum { get; set; } = 1;
        public bool SourceShowOpenedPage { get; set; } = true;
        public bool SourceShowClosedPage { get; set; } = true;
    }
}