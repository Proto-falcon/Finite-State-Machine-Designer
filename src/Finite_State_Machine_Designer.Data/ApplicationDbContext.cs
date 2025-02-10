using Finite_State_Machine_Designer.Data.Identity;
using Finite_State_Machine_Designer.Models.FSM;
using Microsoft.EntityFrameworkCore;

namespace Finite_State_Machine_Designer.Data
{
    public partial class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : AppIdentityDbContext<ApplicationUser,
            AppRole,
            Guid,
            AppUserClaim,
            AppUserRole,
            AppUserLogin,
            AppRoleClaim,
            AppUserToken>(options)
    {
        public DbSet<FiniteStateMachine> StateMachines { get; set; }
        public DbSet<Transition> Transitions { get; set; }
        public DbSet<FiniteState> States { get; set; }

        protected override void ConfigureConventions(
            ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.ComplexProperties<CanvasCoordinate>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUser>()
                .Property(user => user.CreationTime);

            modelBuilder.Entity<ApplicationUser>(appUserBuilder =>
            {
                // Primary key
                appUserBuilder.HasKey(u => u.Id);

                // Indexes for "normalized" username and email, to allow efficient lookups
                appUserBuilder.HasIndex(u => u.NormalizedUserName)
                    .HasDatabaseName("UserNameIndex").IsUnique();
                appUserBuilder.HasIndex(u => u.NormalizedEmail)
                    .HasDatabaseName("EmailIndex").IsUnique();

                // Maps to the AspNetUsers table
                appUserBuilder.ToTable("AspNetUsers");

                // A concurrency token for use with the optimistic concurrency checking
                appUserBuilder.Property(u => u.ConcurrencyStamp).IsConcurrencyToken();

                // Limit the size of columns to use efficient database types
                appUserBuilder.Property(u => u.UserName)
                    .HasMaxLength(256);
                appUserBuilder.Property(u => u.NormalizedUserName)
                    .HasMaxLength(256);
                appUserBuilder.Property(u => u.Email)
                    .HasMaxLength(256);
                appUserBuilder.Property(u => u.NormalizedEmail)
                    .HasMaxLength(256);

                // The relationships between User and other entity types
                // Note that these relationships are configured with no navigation properties

                // Each User can have many UserClaims
                appUserBuilder.HasMany<AppUserClaim>()
                    .WithOne()
                    .HasForeignKey(uc => uc.UserId)
                    .IsRequired();

                // Each User can have many UserLogins
                appUserBuilder.HasMany<AppUserLogin>()
                    .WithOne()
                    .HasForeignKey(ul => ul.UserId)
                    .IsRequired();

                // Each User can have many UserTokens
                appUserBuilder.HasMany<AppUserToken>()
                    .WithOne()
                    .HasForeignKey(ut => ut.UserId)
                    .IsRequired();

                // Each User can have many entries in the UserRole join table
                appUserBuilder.HasMany<AppUserRole>()
                    .WithOne()
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });

            modelBuilder.Entity<AppUserClaim>(userClaimBuilder =>
            {
                // Primary key
                userClaimBuilder.HasKey(uc => uc.Id);

                // Maps to the AspNetUserClaims table
                userClaimBuilder.ToTable("AspNetUserClaims");
            });

            modelBuilder.Entity<AppUserLogin>(userLoginBuilder =>
            {
                // Composite primary key consisting of the LoginProvider and the key to use
                // with that provider
                userLoginBuilder.HasKey(l => new { l.LoginProvider, l.ProviderKey });
                
                // Limit the size of the composite key columns due to common DB restrictions
                userLoginBuilder.Property(l => l.LoginProvider).HasMaxLength(128);
                userLoginBuilder.Property(l => l.ProviderKey).HasMaxLength(128);

                // Maps to the AspNetUserLogins table
                userLoginBuilder.ToTable("AspNetUserLogins");
            });

            modelBuilder.Entity<AppUserToken>(userTokenBuilder =>
            {
                // Composite primary key consisting of the UserId, LoginProvider and Name
                userTokenBuilder.HasKey(t => new { t.UserId, t.LoginProvider, t.Name });

                // Limit the size of the composite key columns due to common DB restrictions
                userTokenBuilder.Property(t => t.LoginProvider).HasMaxLength(221);
                userTokenBuilder.Property(t => t.Name).HasMaxLength(221);

                // Maps to the AspNetUserTokens table
                userTokenBuilder.ToTable("AspNetUserTokens");
            });

            modelBuilder.Entity<AppRole>(roleBuilder =>
            {
                // Primary key
                roleBuilder.HasKey(r => r.Id);

                // Index for "normalized" role name to allow efficient lookups
                roleBuilder.HasIndex(r => r.NormalizedName).HasDatabaseName("RoleNameIndex").IsUnique();

                // Maps to the AspNetRoles table
                roleBuilder.ToTable("AspNetRoles");

                // A concurrency token for use with the optimistic concurrency checking
                roleBuilder.Property(r => r.ConcurrencyStamp).IsConcurrencyToken();

                // Limit the size of columns to use efficient database types
                roleBuilder.Property(u => u.Name).HasMaxLength(256);
                roleBuilder.Property(u => u.NormalizedName).HasMaxLength(256);

                // The relationships between Role and other entity types
                // Note that these relationships are configured with no navigation properties

                // Each Role can have many entries in the UserRole join table
                roleBuilder.HasMany<AppUserRole>().WithOne().HasForeignKey(ur => ur.RoleId).IsRequired();

                // Each Role can have many associated RoleClaims
                roleBuilder.HasMany<AppRoleClaim>().WithOne().HasForeignKey(rc => rc.RoleId).IsRequired();
            });

            modelBuilder.Entity<AppRoleClaim>(roleClaimBuilder =>
            {
                // Primary key
                roleClaimBuilder.HasKey(rc => rc.Id);

                // Maps to the AspNetRoleClaims table
                roleClaimBuilder.ToTable("AspNetRoleClaims");
            });

            modelBuilder.Entity<AppUserRole>(userRoleBuilder =>
            {
                // Primary key
                userRoleBuilder.HasKey(r => new { r.UserId, r.RoleId });

                // Maps to the AspNetUserRoles table
                userRoleBuilder.ToTable("AspNetUserRoles");
            });

            OnModelCreateFSM(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }
    }
}
