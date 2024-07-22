namespace Finite_State_Machine_Designer.Configuration
{
    public class ExternalAuths
    {
        public GoogleAuth Google { get; set; } = new();
    }

    public class GoogleAuth
    {
        public string ClientId { get; set; } = "";

        public string ClientSecret { get; set; } = "";
    }
}
