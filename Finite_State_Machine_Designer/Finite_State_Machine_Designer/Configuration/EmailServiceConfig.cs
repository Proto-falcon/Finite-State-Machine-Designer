using System.ComponentModel.DataAnnotations;

namespace Finite_State_Machine_Designer.Configuration
{
    public class EmailServiceConfig
    {
        [EmailAddress(
            ErrorMessage = "Invalid Email Address",
            ErrorMessageResourceName = "InvalidDisplayEmail")]
        public string DisplayAddress { get; set; } = "";

        public string Password { get; set; } = "";
    }
}
