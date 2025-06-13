using PartialView.pustok.Models;

namespace PartialView.pustok.ViewModels
{
    public class CheckoutVm
    {
        public OrderVm OrderVm { get; set; }
        public List<CheckoutItemVm> CheckoutItems { get; set; }
        public decimal TotalPrice { get; set; }
    }

}
