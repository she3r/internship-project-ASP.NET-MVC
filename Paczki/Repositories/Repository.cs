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
        public bool DeleteDeliveries(IEnumerable<Delivery> deliveries)
        {
            foreach (var d in deliveries)
            {
                if (d == null) return false;
                _db.Deliveries.Attach(d);
                _db.Deliveries.Remove(d);
                _db.SaveChanges();
            }
            return true;
        }

        public int getNumOfPackages()
        {
            return _db.Packages.Count();
        }

        public int getNumOfDeliveries()
        {
            return _db.Deliveries.Count();
        }

        public int getNumOfDeliveries(int? id)
        {
            return _db.Deliveries.Where(el => el.Id == id).ToList().Count();
        }

        private void _updateDateTime(Package toUpdate, PackageDtoWithId packageDto)
        {
            bool toUpdateIsOpen = toUpdate.Opened;
            bool packageDtoIsOpen = packageDto.IsOpened;
            if(toUpdateIsOpen && !packageDtoIsOpen)
            {
                // closing
                toUpdate.ClosedDateTime = DateTime.Now;
            }
        }

        public bool UpdatePackage(PackageDtoWithId packageDto)
        {
            Package? toUpdate = _db.Packages.Where(p => p.PackageId == packageDto.Id).FirstOrDefault();
            if (toUpdate == null)
            {
                return false;
            }
            _updateDateTime(toUpdate, packageDto);
            

            toUpdate.Name = packageDto.Name;
            toUpdate.Opened = packageDto.IsOpened;
            toUpdate.DestinationCity = packageDto.DestinationCity;
            _db.SaveChanges();
            return true;
        }

        public bool DeletePackage(int? id)
        {
            if (id == null) return false;
            var toDelete = _db.Packages.Where(p => p.PackageId == id).FirstOrDefault();
            if (toDelete == null) return false;
            _db.Packages.Attach(toDelete);
            _db.Packages.Remove(toDelete);
            _db.SaveChanges();
            return true;
        }
        public int getPackagePosition(int? id)
        {
            return _db.Packages.ToList().FindIndex(p => p.PackageId == id);
        }
    }
}
