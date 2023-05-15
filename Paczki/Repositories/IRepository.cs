using Paczki.Dto;
using Paczki.Models;

namespace Paczki.Repositories
{
    public interface IRepository
    {
        IEnumerable<Package> GetAllPackages();
        IEnumerable<Delivery> GetAllDelivery();
        IEnumerable<Delivery> GetPackageDeliveries(int? id);
        Package GetPackage(int? id);
        bool CreatePackage(Package package);
        bool CreateDelivery(Delivery delivery);
        bool CreateDeliveries(IEnumerable<Delivery> deliveryList);
        bool UpdateDelivery(DeliveryDtoWithId delivery);
        bool UpdateDeliveries(IEnumerable<DeliveryDtoWithId> deliveryList);
        bool DeleteDelivery(int? id);
        bool DeleteDeliveries(IEnumerable<int> ids);



    }
}
