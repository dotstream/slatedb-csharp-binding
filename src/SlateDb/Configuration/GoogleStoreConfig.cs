using System.Text.Json.Serialization;
using SlateDb.Configuration.Converter;
using SlateDb.Interop;

namespace SlateDb.Configuration;

public class GoogleStoreConfig : AbstractSlateDbConfig
{
    internal override ObjectStoreType StoreType => ObjectStoreType.GCS;
    
    [JsonPropertyName("service_account_path")]
    [SlateDbConfiguration("service_account_path")]
    public string ServiceAccountPath { get; set; }
    [JsonPropertyName("service_account_key")]
    [SlateDbConfiguration("service_account_key")]
    public string ServiceAccountKey { get; set; }
    [JsonPropertyName("bucket_name")]
    [SlateDbConfiguration("bucket_name")]
    public string BucketName { get; set; }
    [JsonPropertyName("application_credentials")]
    [SlateDbConfiguration("application_credentials")]
    public string ApplicationCredentialsPath { get; set; }
    [JsonPropertyName("google_skip_signature")]
    [SlateDbConfiguration("google_skip_signature")]
    public bool? SkipSignature { get; set; }
    [JsonPropertyName("google_allow_http")]
    [SlateDbConfiguration("google_allow_http")]
    public bool? AllowHttp { get; set; }
    [JsonPropertyName("google_allow_invalid_certificates")]
    [SlateDbConfiguration("google_allow_invalid_certificates")]
    public bool? AllowInvalidCertificates { get; set; }
    [JsonPropertyName("google_connect_timeout")]
    [SlateDbConfiguration("google_connect_timeout", typeof(TimeSpanConverter))]
    public TimeSpan? ConnectTimeout { get; set; }
    [JsonPropertyName("google_default_content_type")]
    [SlateDbConfiguration("google_default_content_type")]
    public string DefaultContentType { get; set; }
    [JsonPropertyName("google_http1_only")]
    [SlateDbConfiguration("google_http1_only")]
    public bool? HttpOnly1 { get; set; }
    [JsonPropertyName("google_http2_only")]
    [SlateDbConfiguration("google_http2_only")]
    public bool? HttpOnly2 { get; set; }
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("google_http2_keep_alive_interval")]
    [SlateDbConfiguration("google_http2_keep_alive_interval", typeof(TimeSpanConverter))]
    public TimeSpan? Http2KeepAliveInterval { get; set; }
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("google_http2_keep_alive_timeout")]
    [SlateDbConfiguration("google_http2_keep_alive_timeout", typeof(TimeSpanConverter))]
    public TimeSpan? Http2KeepAliveTimeout { get; set; }
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("google_http2_keep_alive_while_idle")]
    [SlateDbConfiguration("google_http2_keep_alive_while_idle", typeof(TimeSpanConverter))]
    public TimeSpan? Http2KeepAliveWhileIdle { get; set; }
    [JsonPropertyName("google_http2_max_frame_size")]
    [SlateDbConfiguration("google_http2_max_frame_size")]
    public Int32? Http2MaxFrameSize { get; set; }
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("google_pool_idle_timeout")]
    [SlateDbConfiguration("google_pool_idle_timeout", typeof(TimeSpanConverter))]
    public TimeSpan? PoolIdleTimeout { get; set; }
    [JsonPropertyName("google_pool_max_idle_per_host")]
    [SlateDbConfiguration("google_pool_max_idle_per_host")]
    public Int16? PoolMaxIdlePerHost { get; set; }
    [JsonPropertyName("google_proxy_url")]
    [SlateDbConfiguration("google_proxy_url")]
    public string ProxyUrl  { get; set; }
    [JsonPropertyName("google_proxy_ca_certificate")]
    [SlateDbConfiguration("google_proxy_ca_certificate")]
    public string ProxyCaCertificate  { get; set; }
    [JsonPropertyName("google_proxy_excludes")]
    [SlateDbConfiguration("google_proxy_excludes")]
    public string ProxyExcludes { get; set; }
    [JsonPropertyName("google_randomize_addresses")]
    [SlateDbConfiguration("google_randomize_addresses")]
    public bool? RandomizeAddresses  { get; set; }
    [JsonConverter(typeof(JsonTimeSpanConverter))]
    [JsonPropertyName("google_timeout")]
    [SlateDbConfiguration("google_timeout", typeof(TimeSpanConverter))]
    public TimeSpan? Timeout { get; set; }
    [JsonPropertyName("google_user_agent")]
    [SlateDbConfiguration("google_user_agent")]
    public String UserAgent { get; set; }
}