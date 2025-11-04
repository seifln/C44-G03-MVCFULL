using GymManagementDAL.Entities;
using Microsoft.AspNetCore.Identity;

namespace GymManagementDAL.Data.DataSeed
{
	public static class IdentityDataSeeding
	{
		public static bool SeedData(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
		{
			try
			{
				bool HasUsers = userManager.Users.Any();
				bool HasRoles = roleManager.Roles.Any();

				if (HasUsers && HasRoles) return false;

				if (!HasRoles)
				{
					var Roles = new List<IdentityRole>()
					{
						new IdentityRole(){Name = "SuperAdmin"},
						new IdentityRole(){Name = "Admin"}
					};

					foreach (var Role in Roles)
					{
						if (!roleManager.RoleExistsAsync(Role.Name!).Result)
						{
							roleManager.CreateAsync(Role).Wait();
						}
					}
				}
				if (!HasUsers)
				{
					var MainAdmin = new ApplicationUser()
					{
						FirstName = "seif",
						LastName = "khaled",
						UserName = "seifkhaled",
						Email = "seifkhaled@gmail.com",
						PhoneNumber = "01002104133"
					};

					userManager.CreateAsync(MainAdmin, "P@ssw0rd").Wait();
					userManager.AddToRoleAsync(MainAdmin, "SuperAdmin").Wait();

					var Admin01 = new ApplicationUser()
					{
						FirstName = "kareem",
						LastName = "khaled",
						UserName = "kareemkhaled",
						Email = "kareemkhaled@gmail.com",
						PhoneNumber = "01551115997"
					};

					userManager.CreateAsync(Admin01, "P@ssw0rd").Wait();
					userManager.AddToRoleAsync(Admin01, "Admin").Wait();
				}
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Seeding Failed : {ex}");
				return false;
			}
		}

	}
}
