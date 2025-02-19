using Microsoft.EntityFrameworkCore;
using Finite_State_Machine_Designer.Models.FSM;
using Finite_State_Machine_Designer.Data.Identity;

namespace Finite_State_Machine_Designer.Data
{
    public partial class ApplicationDbContext
    {
        public int FsmNameLimit { get; } = 128;
        public int FsmDescLimit { get; } = 514;
        public int FsmTextLimit { get; } = 128;

        private static void OnModelCreateFSM(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(user => user.StateMachines)
                .WithOne()
                .IsRequired();

            modelBuilder.Entity<FiniteStateMachine>(fsmBuilder =>
            {
                fsmBuilder.HasKey(fsm => fsm.Id);

                fsmBuilder.Property(fsm => fsm.Id)
                    .ValueGeneratedNever();

                fsmBuilder.Property(fsm => fsm.Name)
                    .HasMaxLength(512);

                fsmBuilder.Property(fsm => fsm.Description)
                    .HasMaxLength(2056);

                fsmBuilder.Property(fsm => fsm.TimeCreated)
                    .IsRequired();

                fsmBuilder.Property(fsm => fsm.TimeUpdated)
                    .IsRequired();

                fsmBuilder.HasMany(fsm => fsm.Transitions)
                    .WithOne()
                    .IsRequired();

                fsmBuilder.HasMany(fsm => fsm.States)
                    .WithOne()
                    .IsRequired();
            });

            modelBuilder.Entity<FiniteState>(stateBuilder =>
            {
                stateBuilder.HasKey(state => state.Id);

                stateBuilder
                    .Property(state => state.Id)
                    .ValueGeneratedNever();

                stateBuilder.Property(state => state.Text)
                    .HasMaxLength(512);
            });


            modelBuilder.Entity<Transition>(transitionBuilder =>
            {
                transitionBuilder.HasKey(transition => transition.Id);

                transitionBuilder
                    .Property(transition => transition.Id)
                    .ValueGeneratedNever();

                transitionBuilder
                    .HasOne(transition => transition.FromState)
                    .WithOne()
                    .HasForeignKey<Transition>(transition => transition.FromStateId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.NoAction);

                transitionBuilder
                    .HasOne(transition => transition.ToState)
                    .WithOne()
                    .HasForeignKey<Transition>(transition => transition.ToStateId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.NoAction);

                transitionBuilder
                    .HasIndex(transition => transition.FromStateId)
                    .IsUnique(false);

                transitionBuilder
                    .HasIndex(transition => transition.ToStateId)
                    .IsUnique(false);

                transitionBuilder.Property(transition => transition.Text)
                    .HasMaxLength(512);
            });
        }
    }
}
