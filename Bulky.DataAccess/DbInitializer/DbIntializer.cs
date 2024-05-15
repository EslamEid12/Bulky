using BulkyBookWeb;
using BulkyBookWeb.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _uesrManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public DbInitializer(UserManager<IdentityUser> uesrManager,
            RoleManager<IdentityRole> roleManager, ApplicationDbContext db)
        {
            _uesrManager = uesrManager;
            _roleManager = roleManager;
            _db= db;
        }
        public void Initialize()
        {
            //Migration if they Are not Applied
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch(Exception ex){}
            //Create Role if they Are not Created
            if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();

                //if Role they Are not Created,thin We will Create Admin user As
                _uesrManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@dotnetmastery.com",
                    Email = "admin@dotnetmastery.com",
                    Name = "Bhrugen Patel",
                    PhoneNumber = "1112223333",
                    StreetAddress = "test 123 Ave",
                    State = "IL",
                    PostalCode = "23422",
                    City = "Chicago"
                }, "admin123*").GetAwaiter().GetResult();
                ApplicationUser User =_db.ApplicationUsers.FirstOrDefault(u=>u.Email== "admin@dotnetmastery.com");
                if(User != null) { 
                _uesrManager.AddToRoleAsync(User, SD.Role_Admin).GetAwaiter().GetResult();
                }
            }
            return;
        }
    }
}
