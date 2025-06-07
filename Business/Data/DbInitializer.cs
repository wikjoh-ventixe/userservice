using Business.Dtos;
using Business.Interfaces;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Business.Data;

public class DbInitializer
{
    public static async Task AddDefaultAdmin(IServiceProvider serviceProvider)
    {
        var userService = serviceProvider.GetRequiredService<IUserService>();
        var userManager = serviceProvider.GetRequiredService<UserManager<UserEntity>>();
        var defaultAdmin = new CreateUserRequestDto
        {
            FirstName = "Hans",
            LastName = "Poweruser",
            Email = "hans@poweruser.ec",
            PhoneNumber = "0700000000",
            StreetAddress = "Yes",
            PostalCode = "00000",
            City = "No"
        };

        var exists = await userManager.Users.AnyAsync(x => x.Email == defaultAdmin.Email);
        if (!exists)
        {
            await userService.CreateUserWithPasswordAsync(defaultAdmin, "P@ssw0rd");
        }
    }
}
