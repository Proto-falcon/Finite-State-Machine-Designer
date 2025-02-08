using Finite_State_Machine_Designer.Models.FSM;
using Microsoft.AspNetCore.Identity;

namespace Finite_State_Machine_Designer.Data.Identity
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser<Guid>
    {
        public ApplicationUser() : base()
        {
            Id = Guid.NewGuid();
            SecurityStamp = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Time that the account was created.
        /// </summary>
        public DateTime CreationTime { get; set; } = DateTime.UtcNow;

        public List<FiniteStateMachine> StateMachines { get; set; } = [];
    }

}
