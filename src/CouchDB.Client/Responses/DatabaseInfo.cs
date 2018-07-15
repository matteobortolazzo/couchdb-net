using Newtonsoft.Json;

namespace CouchDB.Client.Responses
{
    public class Sizes
    {
        [JsonProperty("file")]
        public int File { get; internal set; }
        [JsonProperty("external")]
        public int External { get; internal set; }
        [JsonProperty("active")]
        public int Active { get; internal set; }
    }

    public class Other
    {
        [JsonProperty("data_size")]
        public int DataSize { get; internal set; }
    }

    public class Cluster
    {
        [JsonProperty("q")]
        public int Q { get; internal set; }
        [JsonProperty("n")]
        public int N { get; internal set; }
        [JsonProperty("w")]
        public int W { get; internal set; }
        [JsonProperty("r")]
        public int R { get; internal set; }
    }

    public class CouchDatabaseInfo
    {
        [JsonProperty("db_name")]
        public string DbName { get; internal set; }
        [JsonProperty("update_seq")]
        public string UpdateSeq { get; internal set; }
        [JsonProperty("sizes")]
        public Sizes Sizes { get; internal set; }
        [JsonProperty("purge_seq")]
        public int PurgeSeq { get; internal set; }
        [JsonProperty("other")]
        public Other Other { get; internal set; }
        [JsonProperty("doc_del_count")]
        public int DocDelCount { get; internal set; }
        [JsonProperty("doc_count")]
        public int DocCount { get; internal set; }
        [JsonProperty("disk_size")]
        public int DiskSize { get; internal set; }
        [JsonProperty("disk_format_version")]
        public int DiskFormatVersion { get; internal set; }
        [JsonProperty("data_size")]
        public int DataSize { get; internal set; }
        [JsonProperty("compact_running")]
        public bool CompactRunning { get; internal set; }
        [JsonProperty("cluster")]
        public Cluster Cluster { get; internal set; }
        [JsonProperty("instance_start_time")]
        public string InstanceStartTime { get; internal set; }
    }
}
