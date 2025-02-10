using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Finite_State_Machine_Designer.Data.Identity
{
    public abstract class AppIdentityDbContext<
    TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>(
    DbContextOptions<ApplicationDbContext> options)
    : IdentityUserContext<TUser, Guid, TUserClaim, TUserLogin, TUserToken>(options)
         where TUser : IdentityUser<Guid>
         where TRole : IdentityRole<Guid>
         where TKey : IEquatable<Guid>
         where TUserClaim : IdentityUserClaim<Guid>
         where TUserRole : IdentityUserRole<Guid>
         where TUserLogin : IdentityUserLogin<Guid>
         where TRoleClaim : IdentityRoleClaim<Guid>
         where TUserToken : IdentityUserToken<Guid>
    {
    }
}
