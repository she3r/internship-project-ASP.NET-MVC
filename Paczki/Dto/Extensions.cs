using Paczki.Models;

namespace Paczki.Dto
{
    public static class Extensions
    {
        public static PackageDtoWithId AsPackageDtoWithId(this Package package)
        {
            return new()
            {
                Id = package.PackageId,
                Name = package.Name,
                IsOpened = package.Opened,
                DestinationCity = package.DestinationCity
            };
        }

        public static DeliveryDtoWithId AsDeliveryDtoWithId(this Delivery delivery)
        {
            return new()
            {
                Id = delivery.Id,
                PackageRefId = delivery.PackageRefId,
                Name = delivery.Name,
                Weight = delivery.Weight,
                IsModified = false,
                CreationDateTime = delivery.CreationDateTime

            };
        }

        public static SourceInfo AsSourceInfo(this EditPackageContentsModelView model)
        {
            return new()
            {
                IsPackageModified = model.IsPackageModified,
                AreDeliveriesDeleted = model.AreDeliveriesDeleted,
                SourceIndexPageNum = model.SourceIndexPageNum,
                SourceShowOpenedPage = model.SourceShowOpenedPage,
                SourceShowClosedPage = model.SourceShowClosedPage,
                PackageId = model.Package?.PackageId
            };
        }
    }
}
