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
        int CreatePackage(Package package);
        bool CreatePackages(IEnumerable<Package> packageList);
        bool CreateDelivery(Delivery delivery);
        bool CreateDeliveries(IEnumerable<Delivery> deliveryList);
        bool UpdatePackage(PackageDtoWithId packageDto);
        bool UpdateClosePackage(PackageDtoWithId packageDto);
        bool UpdatePackages(IEnumerable<PackageDtoWithId> packageList);
        bool UpdateDelivery(DeliveryDtoWithId delivery);
        bool UpdateDeliveries(IEnumerable<DeliveryDtoWithId> deliveryList);
        bool DeletePackage(int? id);
        bool DeletePackages(IEnumerable<int> ids);
        bool DeleteDelivery(int? id);
        bool DeleteDeliveries(IEnumerable<int> ids);

        int GetNumOfPackages();
        int GetNumOfDeliveries();
        int GetNumOfDeliveries(int? id);

        int GetPackagePosition(int? id);



    }
}
