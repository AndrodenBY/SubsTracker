namespace SubsTracker.Hangfire.Options;

public class LogstashOptions
{
    public const string SectionName = "Logstash";

    public string? Host { get; set; }
    public int Port { get; set; }
}
