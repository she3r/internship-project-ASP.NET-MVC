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
        bool UpdatePackage(PackageDtoWithId packageDto);
        bool UpdateDelivery(DeliveryDtoWithId delivery);
        bool UpdateDeliveries(IEnumerable<DeliveryDtoWithId> deliveryList);
        bool DeletePackage(int? id);
        bool DeleteDelivery(int? id);
        bool DeleteDeliveries(IEnumerable<int> ids);

        int getNumOfPackages();
        int getNumOfDeliveries();
        int getNumOfDeliveries(int? id);

        int getPackagePosition(int? id);



    }
}
