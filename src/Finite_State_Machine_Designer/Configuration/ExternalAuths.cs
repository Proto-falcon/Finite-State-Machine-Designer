namespace Finite_State_Machine_Designer.Configuration
{
    public class ExternalAuths
    {
        public GoogleAuth GoogleAuth { get; set; } = new();

        public SlackAuth SlackAuth { get; set; } = new();

        public GithubAuth GithubAuth { get; set; } = new();

        public DiscordAuth DiscordAuth { get; set; } = new();
    }

    public class GoogleAuth
    {
        public string ClientId { get; set; } = "";

        public string ClientSecret { get; set; } = "";

        public Uri MetaAuthUrl { get; set; }
    }

    public class SlackAuth
    {
        public string ClientId { get; set; } = "";

        public string ClientSecret { get; set; } = "";

        public Uri MetaAuthUrl { get; set; }
    }

    public class GithubAuth
    {
        public string ClientId { get; set; } = "";

        public string ClientSecret { get; set; } = "";
    }

    public class DiscordAuth
    {
        public string ClientId { get; set; } = "";

        public string ClientSecret { get; set; } = "";
    }

}
