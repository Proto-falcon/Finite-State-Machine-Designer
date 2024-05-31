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
    }
}
