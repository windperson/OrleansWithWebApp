using System.ComponentModel.DataAnnotations;

namespace OrleansWithWebApp.TypedOptions
{
    public class OrleansDashboardOption
    {
        public bool Enable { get; set; } = false;
        public int Port { get; set; } = 8080;
    }

    public class OrleansProviderOption
    {
        public string DefaultProvider { get; set; }

        public MongoDbProviderSettings MongoDB { get; set; }
    }

    public class MongoDbProviderSettings
    {
        public MongoDbProviderClusterSettings Cluster { get; set; }
        public MongoDbProviderStorageSettings Storage { get; set; }
        public MongoDbProviderReminderSettings Reminder { get; set; }
    }

    public class MongoDbProviderClusterSettings
    {
        [Required]
        public string DbConn { get; set; }
        [Required]
        public string DbName { get; set; }
        public string CollectionPrefix { get; set; }
    }

    public class MongoDbProviderStorageSettings
    {
        [Required]
        public string DbConn { get; set; }
        [Required]
        public string DbName { get; set; }
        public string CollectionPrefix { get; set; }
    }

    public class MongoDbProviderReminderSettings
    {
        [Required]
        public string DbConn { get; set; }
        [Required]
        public string DbName { get; set; }
        public string CollectionPrefix { get; set; }
    }

    public class SiloConfigOption
    {
        [Required]
        public string ClusterId { get; set; }
        [Required]
        public string ServiceId { get; set; }

        public string AdvertisedIp { get; set; }
        public bool ListenOnAnyHostAddress { get; set; }
        public int SiloPort { get; set; }
        public int GatewayPort { get; set; }

        public double ResponseTimeoutMinutes { get; set; } = 3.0;
    }
}