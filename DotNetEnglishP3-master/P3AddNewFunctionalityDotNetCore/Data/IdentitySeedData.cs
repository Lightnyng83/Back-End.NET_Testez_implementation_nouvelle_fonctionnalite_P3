﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace P3Core.Data
{
    public static class IdentitySeedData
    {
        private const string AdminUser = "Admin";
        private const string AdminPassword = "P@ssword123";

        public static async Task EnsurePopulated(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var userManager = (UserManager<IdentityUser>)scope.ServiceProvider.GetService(typeof(UserManager<IdentityUser>));

            IdentityUser user = await userManager.FindByIdAsync(AdminUser);

            if (user == null)
            {
                user = new IdentityUser("Admin");
                await userManager.CreateAsync(user, AdminPassword);
            }
        }
    }
}
