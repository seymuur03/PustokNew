using PartialView.pustok.Models;
using PartialView.pustok.ViewModels.UserAccountDetailsVM;

namespace PartialView.pustok.ViewModels.UserProfileViewModel
{
    public class UserProfileVm
    {
        public UserUpdateProfileVm UserUpdateProfileVm  { get; set; }
        public List<Order> Orders { get; set; }
    }
}
