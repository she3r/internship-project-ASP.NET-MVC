using Paczki.Dto;
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
        public bool CreateDeliveries(IEnumerable<Delivery> deliveryList)
        {

            deliveryList.ToList().ForEach(delivery =>  _db.Deliveries.Add(delivery));
            _db.SaveChanges();
            return true;
        }
        bool IRepository.UpdateDelivery(DeliveryDtoWithId delivery)
        {

            Delivery? toUpdate = _db.Deliveries.Where(d => d.Id == delivery.Id).FirstOrDefault();
            if (toUpdate == null)
            {
                return false;
            }

            toUpdate.Name = delivery.Name;
            toUpdate.Weight = delivery.Weight;
            _db.SaveChanges();
            return true;
            
        }
        bool IRepository.UpdateDeliveries(IEnumerable<DeliveryDtoWithId> deliveryList)
        {
            foreach (var delivery in deliveryList)
            {
                Delivery? toUpdate = _db.Deliveries.Where(d => d.Id == delivery.Id).FirstOrDefault();
                if (toUpdate == null)
                {
                    return false;
                }

                toUpdate.Name = delivery.Name;
                toUpdate.Weight = delivery.Weight;
            }
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

        public bool DeleteDelivery(int? id)
        {
            if (id == null) return false;
            var toDelete = new Delivery() { Id = (int)id };
            _db.Deliveries.Attach(toDelete);
            _db.Deliveries.Remove(toDelete);
            _db.SaveChanges();
            return true;    

        }

        public bool DeleteDeliveries(IEnumerable<int> ids)
        {
            foreach(var id in ids)
            {
                if (id == null) return false;
                var toDelete = new Delivery() { Id = (int) id };
                _db.Deliveries.Attach(toDelete);
                _db.Deliveries.Remove(toDelete);
                _db.SaveChanges();
            }
            return true;
        }
    }
}
