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
        //public bool CreateDelivery(Delivery delivery)
        //{
        //    _db.Deliveries.Add(delivery);
        //    _db.SaveChanges();
        //    return true;
        //}
        //public bool CreateDeliveries(IEnumerable<Delivery> deliveryList)
        //{
        //    deliveryList.ToList().ForEach(delivery =>  _db.Deliveries.Add(delivery));
        //    _db.SaveChanges();
        //    return true;
        //}
        //public bool CreatePackages(IEnumerable<Package> packageList)
        //{
        //    packageList.ToList().ForEach(package => _db.Packages.Add(package));
        //    _db.SaveChanges();
        //    return true;
        //}
        public bool UpdateClosePackage(Package toClose)
        {
            if (toClose == null)
            {
                return false;
            }
            toClose.Opened = false;
            UpdatePackage(toClose);
            _db.SaveChanges();
            return true;
        }
        public bool UpdateOpenPackage(Package toOpen)
        {
            if (toOpen == null)
            {
                return false;
            }
            toOpen.Opened = true;
            UpdatePackage(toOpen);
            _db.SaveChanges();
            return true;
        }

        //bool IRepository.UpdateDelivery(DeliveryDtoWithId delivery)
        //{

        //    Delivery? toUpdate = _db.Deliveries.Where(d => d.Id == delivery.Id).FirstOrDefault();
        //    if (toUpdate == null)
        //    {
        //        return false;
        //    }

        //    toUpdate.Name = delivery.Name;
        //    toUpdate.Weight = delivery.Weight;
        //    _db.SaveChanges();
        //    return true;
            
        //}

        //public bool UpdatePackages(IEnumerable<PackageDtoWithId> packageList)
        //{
        //    foreach (var package in packageList)
        //    {
        //        Package? toUpdate = _db.Packages.Where(d => d.PackageId == package.Id).FirstOrDefault();
        //        if (toUpdate == null)
        //        {
        //            return false;
        //        }

        //        toUpdate.Name = package.Name is null ? toUpdate.Name : package.Name;
        //        toUpdate.DestinationCity = package.DestinationCity is null ? toUpdate.DestinationCity : package.DestinationCity;
        //        if(toUpdate.Opened != package.IsOpened && package.IsOpened == false)
        //        {
        //            toUpdate.ClosedDateTime = DateTime.Now;
        //        }
        //        toUpdate.Opened = package.IsOpened;
        //    }
        //    _db.SaveChanges();
        //    return true;
        //}
        //bool IRepository.UpdateDeliveries(IEnumerable<DeliveryDtoWithId> deliveryList)
        //{
        //    foreach (var delivery in deliveryList)
        //    {
        //        if (!delivery.IsModified) { continue; }
        //        Delivery? toUpdate = _db.Deliveries.Where(d => d.Id == delivery.Id).FirstOrDefault();
        //        if (toUpdate == null)
        //        {
        //            return false;
        //        }

        //        toUpdate.Name = delivery.Name;
        //        toUpdate.Weight = delivery.Weight;
        //    }
        //    _db.SaveChanges();
        //    return true;
        //}

        public int TransactEditView(DbHandleEditDeliveries dbHandleEditDeliveries)
        {
            int packageID = 0;
            if(dbHandleEditDeliveries.PackageToAdd != null)
            {
                packageID = CreatePackage(dbHandleEditDeliveries.PackageToAdd);
            }
            if (dbHandleEditDeliveries.PackageUpdated != null)
            {
                bool resultUpdatePackage = UpdatePackage(dbHandleEditDeliveries.PackageUpdated);
                if (!resultUpdatePackage) return -2;
            }
            if(dbHandleEditDeliveries.DeliveriesToDelete.Count() > 0)
            {
                bool resultDeleteDeliveries = DeleteDeliveries(dbHandleEditDeliveries.DeliveriesToDelete);
                if (!resultDeleteDeliveries) return -2;
            }
            if(dbHandleEditDeliveries.DeliveriesToInsertOrUpdate.Count() > 0)
            {
                bool resultUpdateOrInsertDeliveries = UpdateOrInsertDeliveries(dbHandleEditDeliveries.DeliveriesToInsertOrUpdate);
                if (!resultUpdateOrInsertDeliveries) return -2;
            }
            _db.SaveChanges();
            return packageID;
        }

        public bool UpdateOrInsertDeliveries(IEnumerable<DeliveryDtoWithId> deliveryList)
        {
            foreach(var delivery in deliveryList)
            {
                if (!delivery.IsModified) { continue; }
                Delivery? toUpdate = _db.Deliveries.Where(d => d.Id == delivery.Id && d.PackageRefId == d.PackageRefId).FirstOrDefault();
                if(toUpdate == null && _db.Packages.Any(p => p.PackageId == delivery.PackageRefId)) {
                    var toCreate = new Delivery()
                    {
                        PackageRefId = delivery.PackageRefId,
                        Package = GetPackage(delivery.PackageRefId),
                        Name = delivery.Name,
                        CreationDateTime = delivery.CreationDateTime,
                        Weight = delivery.Weight
                    };
                    _db.Deliveries.Add(toCreate);          
                }
                else if(toUpdate != null)
                {
                    toUpdate.Name = delivery.Name;
                    toUpdate.Weight = delivery.Weight;
                }
            }
            //_db.SaveChanges();
            return true;
        }
        public int CreatePackage(Package package)
        {
            _db.Packages.Add(package);
            //_db.SaveChanges();

            return package.PackageId;
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

        //public bool DeleteDelivery(int? id)
        //{
        //    if (id == null) return false;
        //    var toDelete = new Delivery() { Id = (int)id };
        //    _db.Deliveries.Attach(toDelete);
        //    _db.Deliveries.Remove(toDelete);
        //    _db.SaveChanges();
        //    return true;    

        //}

        //public bool DeleteDeliveries(IEnumerable<int> ids)
        //{
        //    foreach(var id in ids)
        //    {
        //        if (id == null) return false;
        //        var toDelete = new Delivery() { Id = (int) id };
        //        _db.Deliveries.Attach(toDelete);
        //        _db.Deliveries.Remove(toDelete);              
        //    }
        //    _db.SaveChanges();
        //    return true;
        //}

        public bool DeleteDeliveries(IEnumerable<DeliveryDtoWithId> deliveries)
        {
            foreach(var delivery in deliveries)
            {
                Delivery? toRemove = _db.Deliveries.Where(d => d.Id == delivery.Id && d.PackageRefId == d.PackageRefId).FirstOrDefault();
                if(toRemove != null)
                {
                    _db.Deliveries.Attach(toRemove);
                    _db.Deliveries.Remove(toRemove);
                }
            }
            //_db.SaveChanges();
            return true;
        }
        //public bool DeleteDeliveries(IEnumerable<Delivery> deliveries)
        //{
        //    foreach (var d in deliveries)
        //    {
        //        if (d == null) return false;
        //        _db.Deliveries.Attach(d);
        //        _db.Deliveries.Remove(d);
        //        _db.SaveChanges();
        //    }
        //    return true;
        //}
        //public bool DeletePackages(IEnumerable<int> ids)
        //{
        //    foreach (var id in ids)
        //    {
        //        var toDelete = new Package() { PackageId = id };
        //        _db.Packages.Attach(toDelete);
        //        _db.Packages.Remove(toDelete);              
        //    }
        //    _db.SaveChanges();
        //    return true;
        //}

        public int GetNumOfPackages(bool countOpened = true, bool countClosed = true)
        {
            IEnumerable<Package> query = _db.Packages.ToList();
            if (!countOpened)
            {
                query = query.Where(p => !p.Opened);
            }
            if (!countClosed)
            {
                query = query.Where(p => p.Opened);
            }
            return query.Count();
        }

        //public int GetNumOfDeliveries()
        //{
        //    return _db.Deliveries.Count();
        //}

        //public int GetNumOfDeliveries(int? id)
        //{
        //    return _db.Deliveries.Where(el => el.Id == id).ToList().Count();
        //}

        private void UpdateDateTime(Package toUpdate)
        {
            var prevPackage = _db.Packages.Where(p => p.PackageId == toUpdate.PackageId).Single();
            if(!toUpdate.Opened && prevPackage.Opened)
                prevPackage.ClosedDateTime = DateTime.Now;
            
        }

        public bool UpdatePackage(Package? toUpdate)
        {
            if (toUpdate == null)
            {
                return false;
            }
            UpdateDateTime(toUpdate);
            Package? prevPackage = _db.Packages.Where(p => p.PackageId == toUpdate.PackageId).FirstOrDefault();
            if (string.IsNullOrEmpty(toUpdate.Name) || prevPackage == null) {
                return false;
            }
            prevPackage.Name = toUpdate.Name;
            if (string.IsNullOrEmpty(toUpdate.DestinationCity)) {
                prevPackage.DestinationCity = "";
            }
            else 
                prevPackage.DestinationCity = toUpdate.DestinationCity;
            prevPackage.Opened = toUpdate.Opened;
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
        //public int GetPackagePosition(int? id)
        //{
        //    return _db.Packages.ToList().FindIndex(p => p.PackageId == id);
        //}
    }
}
