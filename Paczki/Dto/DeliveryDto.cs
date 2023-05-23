using Paczki.Models;

namespace Paczki.Dto
{
    public class DeliveryDto
    {
        public string Name { get; set; }
        public decimal Weight { get; set; }
    }

    public class DeliveryDtoWithId
    {
        public string Name { get; set; }
        public decimal Weight { get; set; }
        public int Id { get; set; }
        public bool IsModified { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreationDateTime { get; set; }
        public int PackageRefId { get; set; }
    }
}
