using Paczki.Models;

namespace Paczki.Dto
{
    public class DbHandleEditDeliveries
    {
        public Package? PackageUpdated { get; set; }
        public Package? PackageToAdd { get; set; }
        public List<DeliveryDtoWithId> DeliveriesToDelete { get; set; } = new List<DeliveryDtoWithId>();
        public List<DeliveryDtoWithId> DeliveriesToInsertOrUpdate { get; set; } = new List<DeliveryDtoWithId>();
    }
}
