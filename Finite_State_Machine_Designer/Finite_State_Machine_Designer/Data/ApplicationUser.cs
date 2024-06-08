using Finite_State_Machine_Designer.Client.FSM;
using Microsoft.AspNetCore.Identity;

namespace Finite_State_Machine_Designer.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public List<FiniteStateMachine> StateMachines { get; set; } = [];
    }

}
