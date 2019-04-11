using System.ComponentModel.DataAnnotations;

namespace OrleansDemoClient.TypedOptions
{
    public class ClusterInfoOption
    {
        [Required]
        public string ClusterId { get; set; }
        [Required]
        public string ServiceId { get; set; }
    }

    public class OrleansProviderOption
    {
        public string DefaultProvider { get; set; }

        public MongoDbProviderSettings MongoDB { get; set; }
    }

    public class MongoDbProviderSettings
    {
        public MongoDbProviderClusterSettings Cluster { get; set; }
    }

    public class MongoDbProviderClusterSettings
    {
        [Required]
        public string DbConn { get; set; }
        [Required]
        public string DbName { get; set; }
        public string CollectionPrefix { get; set; }
    }
}