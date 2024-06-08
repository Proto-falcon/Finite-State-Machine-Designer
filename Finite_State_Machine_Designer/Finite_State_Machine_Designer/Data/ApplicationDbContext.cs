using Finite_State_Machine_Designer.Client.FSM;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Finite_State_Machine_Designer.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<FiniteStateMachine> StateMachines { get; set; }
        public DbSet<Transition> Transitions { get; set; }
        public DbSet<FiniteState> States { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            base.OnConfiguring(optionsBuilder);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.ComplexProperties<CanvasCoordinate>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transition>()
                .HasMany(transition => transition.States)
                .WithMany()
                .UsingEntity(
                    "FiniteStateTransitions",
                    leftSide => leftSide.HasOne(typeof(FiniteState)).WithMany().OnDelete(DeleteBehavior.Restrict),
                    rightSide => rightSide.HasOne(typeof(Transition)).WithMany().OnDelete(DeleteBehavior.Cascade)
                );

            modelBuilder.Entity<FiniteStateMachine>()
                .HasMany(fsm => fsm.Transitions)
                .WithOne()
                .IsRequired();

            modelBuilder.Entity<FiniteStateMachine>()
                .HasMany(fsm => fsm.States)
                .WithOne()
                .IsRequired();

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(user => user.StateMachines)
                .WithOne()
                .HasForeignKey(fsm => fsm.UserId)
                .IsRequired();

            base.OnModelCreating(modelBuilder);
        }
    }
}
