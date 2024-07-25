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

        public string SmtpServer { get; set; } = "";

        public int Port { get; set; }

        public int SecureSocketOptions { get; set; }
    }
}
