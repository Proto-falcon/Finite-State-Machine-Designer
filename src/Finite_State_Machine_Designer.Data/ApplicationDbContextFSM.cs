using Microsoft.EntityFrameworkCore;
using Finite_State_Machine_Designer.Models.FSM;
using Finite_State_Machine_Designer.Data.Identity;

namespace Finite_State_Machine_Designer.Data
{
    public partial class ApplicationDbContext
    {
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
                    .IsFixedLength()
                    .ValueGeneratedNever();

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
                    .IsFixedLength()
                    .ValueGeneratedNever();
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
            });
        }
    }
}
