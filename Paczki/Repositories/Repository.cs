using Paczki.Models;

namespace Paczki.Repositories
{
    public class Repository : IRepository
    {
        private readonly AppDbContext _db;
        public Repository(AppDbContext db)
        {
            _db = db;
        }
        public bool CreateDelivery(Delivery delivery)
        {
            _db.Deliveries.Add(delivery);
            _db.SaveChanges();
            return true;
        }

        public bool CreatePackage(Package package)
        {
            _db.Packages.Add(package);
            _db.SaveChanges();
            return true;
        }

        public IEnumerable<Delivery> GetAllDelivery()
        {
            return _db.Deliveries.ToList();
        }

        public IEnumerable<Package> GetAllPackages()
        {
            return _db.Packages.ToList();
        }

        public Package? GetPackage(int? id)
        {
            return _db.Packages.Find(id);
        }

        public IEnumerable<Delivery> GetPackageDeliveries(int? id)
        {
            return _db.Deliveries.Where(delivery => delivery.PackageRefId == id).ToList();
        }
    }
}
