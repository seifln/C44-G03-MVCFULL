using GymManagementBLL.Services.Interfaces;
using GymManagementBLL.ViewModels.AccountViewModels;
using GymManagementDAL.Entities;
using Microsoft.AspNetCore.Identity;

namespace GymManagementBLL.Services.Classes
{
	public class AccountService : IAccountService
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;

		public AccountService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
		}
		public ApplicationUser? ValidateUser(LoginViewModel LoginViewModel)
		{
			var User = _userManager.FindByEmailAsync(LoginViewModel.Email).Result;
			if (User == null) return null;
            var isValid = _userManager.CheckPasswordAsync(User, LoginViewModel.Password).Result;

			return isValid ? User : null;
		}
	}
}
