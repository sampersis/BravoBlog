using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Common;
using Extensions.Hosting.AsyncInitialization;

namespace BlogBravo.Data
{
    public class DbInitializer : IAsyncInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private bool InitializeDb = true;
        private string[] roles = new string[] {"sysadmin","author","user","anonymous"};

        public DbInitializer(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _db = context;
            _userManager = userManager;
            _roleManager = roleManager;
            
        }

        public async Task InitializeAsync()
        {
            // Creates the database if it does not exist and applies any pending migrations
            await  _db.Database.MigrateAsync();

            // If true, initializes the database with some sample data
            if (InitializeDb)
            {
                foreach(string role in roles)
                {
                    await CreateRoleAsync(role);
                }
            }

            InitializeDb = false;
        }

        private async Task CreateRoleAsync(string roleName)
        {
            if(!await _roleManager.RoleExistsAsync(roleName))
            {
                var role = new IdentityRole
                {
                    Name = roleName
                };

                await _roleManager.CreateAsync(role);
            }
        }
    }


}
