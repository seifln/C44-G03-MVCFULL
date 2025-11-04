using GymManagementBLL;
using GymManagementBLL.Services.AttachmentService;
using GymManagementBLL.Services.Classes;
using GymManagementBLL.Services.Interfaces;
using GymManagementDAL.Data.Contexts;
using GymManagementDAL.Data.DataSeed;
using GymManagementDAL.Entities;
using GymManagementDAL.Repositories.Classes;
using GymManagementDAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

namespace GymManagementPL
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<GymDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddScoped<IMembershipRepository, MembershipRepository>();

            builder.Services.AddScoped<ISessionRepository, SessionRepository>();

            builder.Services.AddScoped<IMemberService, MemberService>();

            builder.Services.AddScoped<ITrainerService, TrainerService>();

            builder.Services.AddScoped<IPlanService, PlanService>();

            builder.Services.AddScoped<ISessionService, SessionService>();

            builder.Services.AddScoped<IMembershipService, MembershipService>();

            builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

            builder.Services.AddScoped<IBookingRepository, BookingRepository>();

            builder.Services.AddScoped<IBookingService, BookingService>();

            builder.Services.AddScoped<IAttachmentService, AttachmentService>();

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(Config =>

            {
                Config.Password.RequiredLength = 6;
                Config.Password.RequireLowercase = true;
            }).AddEntityFrameworkStores<GymDbContext>();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddIdentityCore<ApplicationUser>()
                            .AddEntityFrameworkStores<GymDbContext>();

            builder.Services.AddAutoMapper(M => M.AddProfile(new MappingProfile()));
            var app = builder.Build();

            using var Scope = app.Services.CreateScope();

            var dbContextObj = Scope.ServiceProvider.GetRequiredService<GymDbContext>();

            var roleManager = Scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var userManager = Scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();


            var PendingMigrations = dbContextObj.Database.GetPendingMigrations();

            if (PendingMigrations?.Any() ?? false)
                dbContextObj.Database.Migrate();

            GymDataSeeding.SeedData(dbContextObj);
            IdentityDataSeeding.SeedData(roleManager, userManager);

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            var provider = new FileExtensionContentTypeProvider();

            provider.Mappings[".jpeg"] = "image/jpeg";
            provider.Mappings[".jpg"] = "image/jpeg";
            provider.Mappings[".png"] = "image/png";

            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = provider
            });
            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapStaticAssets();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");
            app.Run();
        }
    }
}
