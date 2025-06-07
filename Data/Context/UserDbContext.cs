using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data.Context;

public class UserDbContext(DbContextOptions<UserDbContext> options) : IdentityDbContext<UserEntity>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Add prefix for user-related identity tables
        builder.Entity<UserEntity>().ToTable("User_IdentityUsers");
        builder.Entity<IdentityRole>().ToTable("User_IdentityRoles");
        builder.Entity<IdentityUserRole<string>>().ToTable("User_IdentityUserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("User_IdentityUserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("User_IdentityUserLogins");
        builder.Entity<IdentityUserToken<string>>().ToTable("User_IdentityUserTokens");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("User_IdentityRoleClaims");
    }
}
