using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PartialView.pustok.Models
{
    public class Blog:BaseEntity
    {
        [Required]
        public string Name { get; set; }
        [Required]

        public string Descripton { get; set; }
        [Required]

        public string AuthorName { get; set; }
        public string ImageName { get; set; }
        [NotMapped]
        public IFormFile Photo { get; set; }

    }
}
