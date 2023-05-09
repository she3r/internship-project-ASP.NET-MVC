using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Paczki.Models
{

    public class Package
    {
        [Key] 
        public int PackageId { get; set; }   // unique key
        [Required,MaxLength(100),MinLength(3)]
        public string Name { get; set; }
        [Required]
        public bool Opened { get; set; }

        public DateTime CreationDateTime { get;set; }=DateTime.Now;
        public DateTime ClosedDateTime { get; set; }

        public string DestinationCity { get; set; } = "";

        [ForeignKey("PackageRefId")]
        public ICollection<Delivery> Deliveries{ get; set;}
        
    }
}
