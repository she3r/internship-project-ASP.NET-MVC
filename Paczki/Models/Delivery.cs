using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Paczki.Models
{
    public class Delivery
    {
        [Key]
        public int Id { get; set; }
        public int PackageRefId { get; set; }
        public Package Package { get; set; }
        [Required]
        public string Name { get;set; }
        public DateTime CreationDateTime { get; set; } = DateTime.Now;
        [Column(TypeName = "decimal(18,2)")]
        public decimal Weight { get; set; }
    }


}
