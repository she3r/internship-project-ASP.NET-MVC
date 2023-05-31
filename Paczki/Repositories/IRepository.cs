using Paczki.Dto;
using Paczki.Models;

namespace Paczki.Repositories
{
    public interface IRepository
    {
        int TransactEditView(DbHandleEditDeliveries dbHandleEditDeliveries);
        IEnumerable<Package> GetAllPackages();
        //IEnumerable<Delivery> GetAllDelivery();
        IEnumerable<Delivery> GetPackageDeliveries(int? id);
        Package GetPackage(int? id);
        int CreatePackage(Package package);
        //bool CreatePackages(IEnumerable<Package> packageList);
        //bool CreateDelivery(Delivery delivery);
        //bool CreateDeliveries(IEnumerable<Delivery> deliveryList);
        bool UpdatePackage(Package toUpdate);
        bool UpdateClosePackage(Package toClose);
        bool UpdateOpenPackage(Package toOpen);
        //bool UpdatePackages(IEnumerable<PackageDtoWithId> packageList);
        //bool UpdateDelivery(DeliveryDtoWithId delivery);
        //bool UpdateDeliveries(IEnumerable<DeliveryDtoWithId> deliveryList);
        bool UpdateOrInsertDeliveries(IEnumerable<DeliveryDtoWithId> deliveryList);
        bool DeletePackage(int? id);
        //bool DeletePackages(IEnumerable<int> ids);
        //bool DeleteDelivery(int? id);
        //bool DeleteDeliveries(IEnumerable<int> ids);
        bool DeleteDeliveries(IEnumerable<DeliveryDtoWithId> deliveries);

        int GetNumOfPackages(bool countOpened=true, bool countClosed=true);
        //int GetNumOfDeliveries();
        //int GetNumOfDeliveries(int? id);

        //int GetPackagePosition(int? id);



    }
}
