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

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.ComplexProperties<CanvasCoordinate>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FiniteStateMachine>()
                .HasKey(fsm => fsm.Id);

            modelBuilder.Entity<FiniteState>()
                .HasKey(state => state.Id);
            
            modelBuilder.Entity<Transition>()
                .HasKey(transition => transition.Id);

            modelBuilder.Entity<FiniteStateMachine>()
                .Property(fsm => fsm.Id)
                .IsFixedLength()
                .HasMaxLength(36)
				.ValueGeneratedNever();

            modelBuilder.Entity<FiniteState>()
                .Property(state => state.Id)
                .IsFixedLength()
                .HasMaxLength(36)
                .ValueGeneratedNever();

			/// The length of GUIDs converted to string in terms of byte pairs is 36
			/// This due to the 4 extra hyphens '-'
			modelBuilder.Entity<Transition>()
				.Property(transition => transition.Id)
				.IsFixedLength()
				.HasMaxLength(36)
				.ValueGeneratedNever();

            modelBuilder.Entity<Transition>()
                .HasOne(transition => transition.FromState)
                .WithOne()
                .HasForeignKey<Transition>(transition => transition.FromStateId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Transition>()
                .HasOne(transition => transition.ToState)
                .WithOne()
                .HasForeignKey<Transition>(transition => transition.ToStateId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Transition>()
                .HasIndex(transition => transition.FromStateId)
                .IsUnique(false);

            modelBuilder.Entity<Transition>()
                .HasIndex(transition => transition.ToStateId)
                .IsUnique(false);

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
                .IsRequired();

            base.OnModelCreating(modelBuilder);
        }
    }
}
