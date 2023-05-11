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


    }
}
