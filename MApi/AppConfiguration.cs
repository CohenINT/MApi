namespace MApi
{
    public class AppConfiguration
    {
        public Logging Logging { get; set; }
        public ApplicationConfiguration ApplicationConfiguration { get; set; }
    }

    public class Logging
    {
        public LogLevel LogLevel { get; set; }
    }

    public class LogLevel
    {
        public string Default { get; set; }
        public string MicrosoftAspNetCore { get; set; }
    }

    public class ApplicationConfiguration
    {
        public string AllowedHosts { get; set; }
        public Worker Worker { get; set; }
    }

    public class Worker
    {
        public bool Active { get; set; }
    }
}
