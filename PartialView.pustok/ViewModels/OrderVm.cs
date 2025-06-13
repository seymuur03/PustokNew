using System.ComponentModel.DataAnnotations;

namespace PartialView.pustok.ViewModels
{
    public class OrderVm
    {
        [Required]
        public string ZipCode { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Town { get; set; }
        [Required]
        public string Address { get; set; }
    }
}
