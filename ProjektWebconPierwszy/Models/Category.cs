using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ProjektWebconPierwszy.Models
{
    public class Category
    {
        [Key]  // unique key
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [DisplayName("Display order")]
        [Range(1,100,ErrorMessage ="Display order must be between 1 and 100")]
        public int DisplayOrder { get; set; }
        public DateTime CreatedDateTime { get; set; }=DateTime.Now;

    }
}
